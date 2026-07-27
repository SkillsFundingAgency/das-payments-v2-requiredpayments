using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using SFA.DAS.Payments.Application.Infrastructure.Logging;
using SFA.DAS.Payments.Application.Infrastructure.Telemetry;
using SFA.DAS.Payments.Application.Messaging;
using SFA.DAS.Payments.Application.Repositories;
using SFA.DAS.Payments.Messages.Common.Events;
using SFA.DAS.Payments.Model.Core;
using SFA.DAS.Payments.Model.Core.Incentives;
using SFA.DAS.Payments.Model.Core.OnProgramme;
using SFA.DAS.Payments.RequiredPayments.Application.Infrastructure;
using SFA.DAS.Payments.RequiredPayments.Application.Repositories;
using SFA.DAS.Payments.RequiredPayments.Domain;
using SFA.DAS.Payments.RequiredPayments.Domain.Entities;
using SFA.DAS.Payments.RequiredPayments.Messages.Events;
using SFA.DAS.Payments.RequiredPayments.Model.Entities;
using Earning = SFA.DAS.Payments.RequiredPayments.Domain.Entities.Earning;

namespace SFA.DAS.Payments.RequiredPayments.Application.Processors
{
    public abstract class EarningEventProcessorBase<TEarningEvent> : IEarningEventProcessor<TEarningEvent>
        where TEarningEvent : IEarningEvent
    {
        private readonly IRequiredPaymentProcessor requiredPaymentProcessor;
        private readonly IMapper mapper;
        private readonly IPaymentHistoryRepository paymentHistoryRepository;
        private readonly IApprenticeshipKeyProvider apprenticeshipKeyProvider;
        private readonly INegativeEarningService negativeEarningService;
        private readonly IPaymentLogger paymentLogger;
        private readonly IDuplicateEarningEventService duplicateEarningEventService;
        private readonly ITelemetry telemetry;

        protected EarningEventProcessorBase(
            IMapper mapper,
            IRequiredPaymentProcessor requiredPaymentProcessor,
            IPaymentHistoryRepository paymentHistoryRepository,
            IApprenticeshipKeyProvider apprenticeshipKeyProvider,
            INegativeEarningService negativeEarningService,
            IPaymentLogger paymentLogger,
            IDuplicateEarningEventService duplicateEarningEventService,
            ITelemetry telemetry)
        {
            this.mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            this.requiredPaymentProcessor = requiredPaymentProcessor ?? throw new ArgumentNullException(nameof(requiredPaymentProcessor));
            this.paymentHistoryRepository = paymentHistoryRepository;
            this.apprenticeshipKeyProvider = apprenticeshipKeyProvider;
            this.negativeEarningService = negativeEarningService;
            this.paymentLogger = paymentLogger;
            this.duplicateEarningEventService = duplicateEarningEventService ?? throw new ArgumentNullException(nameof(duplicateEarningEventService));
            this.telemetry = telemetry ?? throw new ArgumentNullException(nameof(telemetry));
        }

        public async Task<ReadOnlyCollection<PeriodisedRequiredPaymentEvent>> HandleEarningEvent(TEarningEvent earningEvent, IDataCache<PaymentHistoryEntity[]> paymentHistoryCache, CancellationToken cancellationToken)
        {
            if (earningEvent == null)
                throw new ArgumentNullException(nameof(earningEvent));

            try
            {
                var result = new List<PeriodisedRequiredPaymentEvent>();
                if (await duplicateEarningEventService.IsDuplicate(earningEvent, cancellationToken)
                    .ConfigureAwait(false))
                {
                    return result.AsReadOnly();
                }
                var cachedPayments = await paymentHistoryCache.TryGet(CacheKeys.PaymentHistoryKey, cancellationToken);
                var academicYearPayments = cachedPayments.HasValue
                    ? cachedPayments.Value
                        .Where(p => p.LearnAimReference.Equals(earningEvent.LearningAim.Reference, StringComparison.OrdinalIgnoreCase))
                        .Select(p => mapper.Map<PaymentHistoryEntity, Payment>(p))
                        .ToList()
                    : new List<Payment>();

                foreach (var (period, type) in GetPeriods(earningEvent))
                {
                    if (period.Period > earningEvent.CollectionPeriod.Period) // cut off future periods
                        continue;

                    if (period.Amount != 0 && !period.SfaContributionPercentage.HasValue)
                    {
                        if (period.Amount > 0 && period.Amount < 0.01m)
                        {
                            paymentLogger.LogWarning($"Payment amount is a fraction of a penny (£{period.Amount}) for ULN {earningEvent.Learner.Uln} UKPRN {earningEvent.Ukprn}. Skipping processing.");
                            continue;
                        }

                        if (period.Amount < 0 && period.Amount > -0.01m)
                        {
                            paymentLogger.LogWarning($"Refund amount is a fraction of a penny (-£{period.Amount * -1}) for ULN {earningEvent.Learner.Uln} UKPRN {earningEvent.Ukprn}. Skipping processing.");
                            continue;
                        }

                        throw new InvalidOperationException("Non-zero amount with no Sfa Contribution");
                    }

                    var payments = academicYearPayments.Where(payment => payment.DeliveryPeriod == period.Period &&
                                                                         payment.TransactionType == type)
                        .ToList();

                    List<RequiredPayment> requiredPayments;

                    if (NegativeEarningWillResultInARefund(period, payments))
                    {
                        requiredPayments = negativeEarningService
                            .ProcessNegativeEarning(period.Amount, academicYearPayments, period.Period, period.PriceEpisodeIdentifier);
                    }
                    else
                    {
                        var earning = new Earning
                        {
                            Amount = period.Amount,
                            SfaContributionPercentage = period.SfaContributionPercentage,
                            EarningType = GetEarningType(type),
                            PriceEpisodeIdentifier = period.PriceEpisodeIdentifier,
                            AccountId = period.AccountId,
                            TransferSenderAccountId = period.TransferSenderAccountId
                        };

                        requiredPayments = requiredPaymentProcessor.GetRequiredPayments(earning, payments);
                    }

                    if (requiredPayments
                        .GroupBy(x => new { x.SfaContributionPercentage, x.AccountId })
                        .All(x => x.Sum(y => y.Amount) == 0))
                    {
                        continue;
                    }

                    foreach (var requiredPayment in requiredPayments)
                    {
                        var requiredPaymentEvent = CreateRequiredPaymentEvent(requiredPayment.EarningType, type);
                        mapper.Map(period, requiredPaymentEvent);
                        mapper.Map(earningEvent, requiredPaymentEvent);
                        mapper.Map(requiredPayment, requiredPaymentEvent);
                        AddRefundCommitmentDetails(requiredPayment, requiredPaymentEvent);
                        AddAimSeqNumber(period, earningEvent, requiredPaymentEvent);

                        var priceEpisodeIdentifier = requiredPaymentEvent.PriceEpisodeIdentifier;

                        if (earningEvent.PriceEpisodes != null && earningEvent.PriceEpisodes.Any())
                        {
                            var priceEpisode = earningEvent.PriceEpisodes.Count == 1
                                ? earningEvent.PriceEpisodes.FirstOrDefault()
                                : earningEvent.PriceEpisodes?.SingleOrDefault(x => x.Identifier == priceEpisodeIdentifier);

                            mapper.Map(priceEpisode, requiredPaymentEvent);

                            if (requiredPaymentEvent.LearningAim != null) mapper.Map(priceEpisode, requiredPaymentEvent.LearningAim);
                        }

                        result.Add(requiredPaymentEvent);
                    }
                }
                return result.AsReadOnly();

            }
            catch (Exception e)
            {
                paymentLogger.LogError($"Error while Handling EarningEvent for {earningEvent.Ukprn} ", e);
                throw;
            }
        }

        private static void AddRefundCommitmentDetails(RequiredPayment requiredPayment, PeriodisedRequiredPaymentEvent requiredPaymentEvent)
        {
            if (requiredPayment.Amount < 0)
            {
                requiredPaymentEvent.ApprenticeshipId = requiredPayment.ApprenticeshipId;
                requiredPaymentEvent.ApprenticeshipPriceEpisodeId = requiredPayment.ApprenticeshipPriceEpisodeId;
                requiredPaymentEvent.ApprenticeshipEmployerType = requiredPayment.ApprenticeshipEmployerType;
                requiredPaymentEvent.LearningStartDate = requiredPayment.LearningStartDate;
            }
        }

        private static void AddAimSeqNumber(EarningPeriod period, TEarningEvent earningEvent, PeriodisedRequiredPaymentEvent requiredPaymentEvent)
        {
            var priceEpisode = earningEvent?.PriceEpisodes.FirstOrDefault(x => x.Identifier == period.PriceEpisodeIdentifier);
            requiredPaymentEvent.LearningAimSequenceNumber = priceEpisode != null ? priceEpisode.LearningAimSequenceNumber : earningEvent.LearningAim.SequenceNumber;
        }

        private static bool NegativeEarningWillResultInARefund(EarningPeriod period, List<Payment> payments)
        {
            return period.Amount < 0 && period.Amount < payments.Sum(x => x.Amount);
        }

        protected abstract EarningType GetEarningType(int type);

        protected PeriodisedRequiredPaymentEvent CreateRequiredPaymentEvent(EarningType earningType, int transactionType)
        {
            switch (earningType)
            {
                case EarningType.CoInvested:
                    return new CalculatedRequiredCoInvestedAmount
                    {
                        OnProgrammeEarningType = (OnProgrammeEarningType)transactionType,
                    };
                case EarningType.Incentive:
                    return new CalculatedRequiredIncentiveAmount
                    {
                        Type = (IncentivePaymentType)transactionType,
                    };
                case EarningType.Levy:
                    return new CalculatedRequiredLevyAmount
                    {
                        OnProgrammeEarningType = (OnProgrammeEarningType)transactionType,
                    };
            }

            throw new NotImplementedException($"Could not create required payment for earning type: {earningType}");
        }

        protected abstract IReadOnlyCollection<(EarningPeriod period, int type)> GetPeriods(TEarningEvent earningEvent);
    }
}