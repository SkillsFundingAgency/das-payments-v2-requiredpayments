using SFA.DAS.Payments.Application.Messaging;
using SFA.DAS.Payments.Application.Repositories;
using SFA.DAS.Payments.EarningEvents.Messages.Events;
using SFA.DAS.Payments.Messages.Common.Events;
using SFA.DAS.Payments.Model.Core;
using SFA.DAS.Payments.Model.Core.Entities;
using SFA.DAS.Payments.Model.Core.Incentives;
using SFA.DAS.Payments.Model.Core.OnProgramme;
using SFA.DAS.Payments.RequiredPayments.Application.Infrastructure;
using SFA.DAS.Payments.RequiredPayments.Domain.Entities;
using SFA.DAS.Payments.RequiredPayments.Messages.Events;
using SFA.DAS.Payments.RequiredPayments.Model.Entities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.Payments.RequiredPayments.Application.Processors
{
    public class ShortCoursesEarningEventProcessor : IShortCoursesEarningEventProcessor
    {
        private readonly IDuplicateEarningEventService duplicateEarningEventService;

        public ShortCoursesEarningEventProcessor(IDuplicateEarningEventService duplicateEarningEventService)
        {
            this.duplicateEarningEventService = duplicateEarningEventService;
        }

        internal class ShortCourseRequiredPayment : RequiredPayment
        {
            public int Type { get; set; }
            public byte DeliveryPeriod { get; set; }
        }

        public async Task<ReadOnlyCollection<PeriodisedRequiredPaymentEvent>> HandleEarningEvent(GSLShortCourseEarningsEvent earningEvent,
                            IDataCache<PaymentHistoryEntity[]> paymentHistoryCache,
                            CancellationToken cancellationToken)
        {
            try
            {
                var requiredPaymentEvents = new List<PeriodisedRequiredPaymentEvent>();

                if (await duplicateEarningEventService
                        .IsDuplicate(earningEvent, cancellationToken)
                        .ConfigureAwait(false))
                {
                    return requiredPaymentEvents.AsReadOnly();
                }
                //Gets all historical payments for this learner and academic year.This give milestone payments, completion payments, previous collection period payments.
                var academicYearPayments =
                    await GetAcademicYearPayments(
                        earningEvent,
                        paymentHistoryCache,
                        cancellationToken);

                //Extracting the latest earnings from the incoming submission.
                var currentEarnings = GetPeriods(earningEvent).ToList();

                GenerateRefundPayments(
                    earningEvent,
                    requiredPaymentEvents,
                    academicYearPayments,
                    currentEarnings);

                GenerateNewRequiredPayments(
                    earningEvent,
                    requiredPaymentEvents,
                    academicYearPayments,
                    currentEarnings);

                return new ReadOnlyCollection<PeriodisedRequiredPaymentEvent>(requiredPaymentEvents);
            }
            catch (Exception ex)
            {
                throw new Exception(
                    $"Error processing GSLShortCourseEarningsEvent for LearningAimReference: {earningEvent.LearningAim.Reference}, " +
                    $"CollectionPeriod: Year: {earningEvent.CollectionPeriod.AcademicYear} Period: {earningEvent.CollectionPeriod.Period}. Exception: {ex.Message}",
                    ex);
            }
        }

        private static async Task<List<PaymentHistoryEntity>> GetAcademicYearPayments(GSLShortCourseEarningsEvent earningEvent,
            IDataCache<PaymentHistoryEntity[]> paymentHistoryCache,
            CancellationToken cancellationToken)
        {
            var cachedPayments =
                await paymentHistoryCache.TryGet(
                    CacheKeys.PaymentHistoryKey,
                    cancellationToken);

            return cachedPayments.HasValue
                ? cachedPayments.Value
                    .Where(x =>
                        x.LearnAimReference.Equals(
                            earningEvent.LearningAim.Reference,
                            StringComparison.OrdinalIgnoreCase)
                        && x.CollectionPeriod.AcademicYear ==
                           earningEvent.CollectionPeriod.AcademicYear)
                    .ToList()
                : new List<PaymentHistoryEntity>();
        }

        private void GenerateRefundPayments(
                        GSLShortCourseEarningsEvent earningEvent,
                        List<PeriodisedRequiredPaymentEvent> requiredPaymentEvents,
                        List<PaymentHistoryEntity> academicYearPayments,
                        List<(EarningPeriod period, int type)> currentEarnings)
        {
            foreach (var historicGroup in academicYearPayments.GroupBy(x => new
            {
                x.TransactionType,
                x.DeliveryPeriod
            }))
            {
                var historicPayments = historicGroup.ToList();

                var historicAmount = historicPayments.Sum(x => x.Amount);

                var currentMatch = currentEarnings.FirstOrDefault(x =>
                    x.type == historicGroup.Key.TransactionType &&
                    x.period.Period == historicGroup.Key.DeliveryPeriod);

                var requiresRefund =
                    currentMatch.period == null ||
                    currentMatch.period.Amount != historicAmount;

                if (!requiresRefund)
                {
                    continue;
                }

                var firstHistoricPayment = historicPayments.First();

                var refundPeriod = new EarningPeriod
                {
                    Period = firstHistoricPayment.DeliveryPeriod,
                    Amount = -historicAmount,
                    PriceEpisodeIdentifier = firstHistoricPayment.PriceEpisodeIdentifier
                };

                requiredPaymentEvents.Add(
                    GenerateRequiredPaymentEvent(
                        earningEvent,
                        earningEvent.PriceEpisodes.FirstOrDefault()
                            ?? new PriceEpisode(),
                        refundPeriod,
                        firstHistoricPayment.TransactionType,
                        IsCoInvested(historicPayments)));
            }
        }

        private void GenerateNewRequiredPayments(
                        GSLShortCourseEarningsEvent earningEvent,
                        List<PeriodisedRequiredPaymentEvent> requiredPaymentEvents,
                        List<PaymentHistoryEntity> academicYearPayments,
                        List<(EarningPeriod period, int type)> currentEarnings)
        {
            foreach (var (period, type) in currentEarnings)
            {
                if (period.Period > earningEvent.CollectionPeriod.Period)
                {
                    continue;
                }

                var historicPayments = academicYearPayments
                    .Where(x =>
                        x.DeliveryPeriod == period.Period &&
                        x.TransactionType == type)
                    .ToList();

                if (historicPayments.Any())
                {
                    var historicAmount = historicPayments.Sum(x => x.Amount);

                    if (historicAmount == period.Amount)
                    {
                        continue;
                    }
                }

                var priceEpisode =
                    earningEvent.PriceEpisodes.FirstOrDefault(x =>
                        x.Identifier == period.PriceEpisodeIdentifier)
                    ?? new PriceEpisode();

                requiredPaymentEvents.Add(
                    GenerateRequiredPaymentEvent(
                        earningEvent,
                        priceEpisode,
                        period,
                        type,
                        IsCoInvested(historicPayments)));
            }
        }

        private static bool IsCoInvested(IEnumerable<PaymentHistoryEntity> payments)
        {
            return payments.Any(x =>
                x.FundingSource == FundingSourceType.CoInvestedSfa ||
                x.FundingSource == FundingSourceType.CoInvestedEmployer);
        }

        private PeriodisedRequiredPaymentEvent GenerateRequiredPaymentEvent(
                 GSLShortCourseEarningsEvent earningEvent, PriceEpisode priceEpisode, EarningPeriod period, int type, bool isCoInvested)
        {
            var requiredPayment = InitialiseRequiredPaymentEvent(type, earningEvent, isCoInvested);
            // replicate logic in RequiredPaymentsProfile
            var learningStartDate = priceEpisode.CourseStartDate;
            if (earningEvent.PriceEpisodes.All(x =>
                    x.LearningAimSequenceNumber != earningEvent.LearningAim.SequenceNumber))
            {
                learningStartDate = earningEvent.LearningAim.StartDate;
            }

            requiredPayment.AccountId = period.AccountId;
            requiredPayment.TransferSenderAccountId = period.TransferSenderAccountId;
            requiredPayment.ApprenticeshipEmployerType = period.ApprenticeshipEmployerType;
            requiredPayment.ApprenticeshipId = period.ApprenticeshipId;
            requiredPayment.ApprenticeshipPriceEpisodeId = period.ApprenticeshipPriceEpisodeId;
            requiredPayment.LearningAim = new LearningAim
            {
                CourseCode = earningEvent.LearningAim.CourseCode,
                FrameworkCode = earningEvent.LearningAim.FrameworkCode,
                FundingLineType = priceEpisode.FundingLineType,
                LearningType = earningEvent.LearningAim.LearningType,
                PathwayCode = earningEvent.LearningAim.PathwayCode,
                ProgrammeType = earningEvent.LearningAim.ProgrammeType,
                Reference = earningEvent.LearningAim.Reference,
                SequenceNumber = earningEvent.LearningAim.SequenceNumber,
                StandardCode = earningEvent.LearningAim.StandardCode,
                StartDate = earningEvent.LearningAim.StartDate
            };
            requiredPayment.LearningStartDate = learningStartDate;
            requiredPayment.LearningAimSequenceNumber = priceEpisode.LearningAimSequenceNumber;
            requiredPayment.CompletionAmount = priceEpisode.CompletionAmount;
            requiredPayment.SfaContributionPercentage = period.SfaContributionPercentage ?? 0;
            requiredPayment.PriceEpisodeIdentifier = period.PriceEpisodeIdentifier;
            requiredPayment.AgeAtStartOfLearning = earningEvent.AgeAtStartOfLearning;
            requiredPayment.CollectionPeriod = new CollectionPeriod
                                                    {
                                                        AcademicYear = earningEvent.CollectionPeriod.AcademicYear,
                                                        Period = earningEvent.CollectionPeriod.Period
                                                    };
            requiredPayment.ContractType = ContractType.Act1;
            requiredPayment.Learner = earningEvent.Learner;
            requiredPayment.EarningEventId = earningEvent.EventId;
            requiredPayment.AmountDue = period.Amount;
            requiredPayment.DeliveryPeriod = period.Period;
            requiredPayment.StartDate = priceEpisode.StartDate;
            requiredPayment.PlannedEndDate = priceEpisode.PlannedEndDate;
            requiredPayment.ActualEndDate = priceEpisode.ActualEndDate;
            requiredPayment.CompletionStatus = (OnProgrammeEarningType)type == OnProgrammeEarningType.Completion ? (byte)1 : (byte)0;
            requiredPayment.InstalmentAmount = priceEpisode.InstalmentAmount;
            requiredPayment.NumberOfInstalments = (short)priceEpisode.NumberOfInstalments;
            requiredPayment.JobId = earningEvent.JobId;
            requiredPayment.EventId = Guid.NewGuid();
            requiredPayment.Ukprn = earningEvent.Ukprn;
            requiredPayment.IlrSubmissionDateTime = earningEvent.IlrSubmissionDateTime;

            return requiredPayment;
        }

        private IReadOnlyCollection<(EarningPeriod period, int type)> GetPeriods(
            GSLShortCourseEarningsEvent earningEvent)
        {
            var result = new List<(EarningPeriod period, int type)>();

            foreach (var earning in earningEvent.Earnings)
            {
                foreach (var period in earning.Periods)
                {
                    result.Add((period, (int)earning.Type));
                }
            }

            return result;
        }

        protected CalculatedRequiredOnProgrammeAmount InitialiseRequiredPaymentEvent(
                        int transactionType,
                        GSLShortCourseEarningsEvent earningEvent,
                        bool isCoInvested)
        {
            var earningType = (OnProgrammeEarningType)transactionType;

            return isCoInvested
                ? new CalculatedRequiredCoInvestedAmount
                {
                    OnProgrammeEarningType = earningType,
                    CourseType = CourseType.ShortCourse,
                    FundingPlatformType = earningEvent.FundingPlatformType,
                }
                : new CalculatedRequiredLevyAmount
                {
                    OnProgrammeEarningType = earningType,
                    CourseType = CourseType.ShortCourse,
                    FundingPlatformType = earningEvent.FundingPlatformType,
                };
        }
    }
}
