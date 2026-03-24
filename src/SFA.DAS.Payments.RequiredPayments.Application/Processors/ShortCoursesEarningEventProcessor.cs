using AutoMapper;
using SFA.DAS.Payments.Application.Messaging;
using SFA.DAS.Payments.Application.Repositories;
using SFA.DAS.Payments.EarningEvents.Messages.Events;
using SFA.DAS.Payments.Messages.Common.Events;
using SFA.DAS.Payments.Model.Core;
using SFA.DAS.Payments.Model.Core.Entities;
using SFA.DAS.Payments.Model.Core.OnProgramme;
using SFA.DAS.Payments.RequiredPayments.Application.Infrastructure;
using SFA.DAS.Payments.RequiredPayments.Domain;
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
        private readonly INegativeEarningService negativeEarningService;
        private readonly IMapper mapper;
        private readonly IDuplicateEarningEventService duplicateEarningEventService;

        public ShortCoursesEarningEventProcessor(INegativeEarningService negativeEarningService, IMapper mapper, IDuplicateEarningEventService duplicateEarningEventService)
        {
            this.negativeEarningService = negativeEarningService;
            this.mapper = mapper;
            this.duplicateEarningEventService = duplicateEarningEventService;
        }
        public async Task<ReadOnlyCollection<PeriodisedRequiredPaymentEvent>> HandleEarningEvent(
            GSLShortCourseEarningsEvent earningEvent, IDataCache<PaymentHistoryEntity[]> paymentHistoryCache,
            CancellationToken cancellationToken)
        {
            try
            {
                var requiredPaymentEvents = new List<PeriodisedRequiredPaymentEvent>();

                if (await duplicateEarningEventService.IsDuplicate(earningEvent, cancellationToken)
                        .ConfigureAwait(false))
                {
                    return requiredPaymentEvents.AsReadOnly();
                }

                var allPeriods = earningEvent.Earnings
                    .SelectMany(e => e.Periods)
                    .ToList();

                //Get payment history for short course, minus the current period
                var cachedPayments = await paymentHistoryCache.TryGet(CacheKeys.PaymentHistoryKey, cancellationToken);
                var academicYearPayments = cachedPayments.HasValue
                    ? cachedPayments.Value
                        .Where(p => p.LearnAimReference.Equals(earningEvent.LearningAim.Reference, StringComparison.OrdinalIgnoreCase) && p.CollectionPeriod.AcademicYear == earningEvent.CollectionPeriod.AcademicYear)
                        .Select(p => mapper.Map<PaymentHistoryEntity, Payment>(p))
                        .ToList()
                    : new List<Payment>();

                foreach (var (period, type) in GetPeriods(earningEvent))
                {
                    if (period.Period > earningEvent.CollectionPeriod.Period) // cut off future periods
                        continue;

                    //Get list of payments from Payment history for the same period and transaction type
                    var payments = academicYearPayments.Where(payment => payment.DeliveryPeriod == period.Period &&
                                                                         payment.TransactionType == type)
                        .ToList();

                    //Generate new payment
                    if (payments.Count == 0)
                    {
                        requiredPaymentEvents.Add(GenerateShortCoursesPayment(period, earningEvent));
                        continue;
                    }
                    //For existing payments, check if the delivery period matches the earning event,
                    //if not generate a refund for the original payment and a new payment for the amount in the earning event.
                    requiredPaymentEvents.AddRange(CheckDeliveryPeriodAgainstPayments(period, payments, earningEvent));

                }
                requiredPaymentEvents.AddRange(CheckPaymentsAgainstDeliveryPeriods(allPeriods, earningEvent, academicYearPayments));

                return new ReadOnlyCollection<PeriodisedRequiredPaymentEvent>(requiredPaymentEvents);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error processing GSLShortCourseEarningsEvent for LearningAimReference: {earningEvent.LearningAim.Reference}, " +
                    $"CollectionPeriod: Year: {earningEvent.CollectionPeriod.AcademicYear} Period: {earningEvent.CollectionPeriod.Period}", ex);
            }

        }

        private PeriodisedRequiredPaymentEvent GenerateShortCoursesPayment(EarningPeriod period, GSLShortCourseEarningsEvent earningEvent)
        {
            var priceEpisode = earningEvent.PriceEpisodes.FirstOrDefault(x => x.Identifier == period.PriceEpisodeIdentifier);
            var requiredPayment = GenerateRequiredPayment(priceEpisode, period);
            return GenerateRequiredPaymentEvent(requiredPayment, earningEvent, priceEpisode, period);

        }
        private List<PeriodisedRequiredPaymentEvent> CheckPaymentsAgainstDeliveryPeriods(List<EarningPeriod> allPeriods, GSLShortCourseEarningsEvent earningEvent,
            List<Payment> academicYearPayments)
        {
            var requiredPayments = new List<RequiredPayment>();
            var requiredPaymentEvents = new List<PeriodisedRequiredPaymentEvent>();

            //If a payment exists for a delivery period that doesn't exist in the earning event, then refund the original payment.
            foreach (var payment in academicYearPayments)
            {
                foreach (var period in allPeriods)
                {
                    var paymentPeriod = allPeriods.FirstOrDefault(p => p.Period == payment.CollectionPeriod.Period);

                    if (paymentPeriod == null)
                    {
                        var priceEpisode = earningEvent.PriceEpisodes.FirstOrDefault(x => x.Identifier == period.PriceEpisodeIdentifier);
                        //Generate Refund
                        requiredPayments.AddRange(negativeEarningService
                            .ProcessNegativeEarning(payment.Amount, academicYearPayments, payment.CollectionPeriod.Period, payment.PriceEpisodeIdentifier));
                        foreach (var requiredPayment in requiredPayments)
                        {
                            requiredPaymentEvents.Add(GenerateRequiredPaymentEvent(requiredPayment, earningEvent, priceEpisode, period));
                        }
                    }

                }


            }

            return requiredPaymentEvents;
        }

        private List<PeriodisedRequiredPaymentEvent> CheckDeliveryPeriodAgainstPayments(EarningPeriod period, List<Payment> payments, GSLShortCourseEarningsEvent earningEvent)
        {
            var requiredPayments = new List<RequiredPayment>();
            var requiredPaymentEvents = new List<PeriodisedRequiredPaymentEvent>();
            //if the value in the EarningEvent is different to the payment history:
            // 1. Original payment should be refunded
            // 2. New payment made to the amount requested in the earnings

            foreach (var payment in payments)
            {
                if(payment.DeliveryPeriod != period.Period)
                {
                    continue;
                }
                var priceEpisode = earningEvent.PriceEpisodes.FirstOrDefault(x => x.Identifier == period.PriceEpisodeIdentifier);


                if (priceEpisode != null)
                {
                    if (payment.Amount != period.Amount)
                    {
                        //Generate Refund
                        requiredPayments.AddRange(negativeEarningService
                            .ProcessNegativeEarning(period.Amount, payments, period.Period, period.PriceEpisodeIdentifier));

                        //Generate new payment 
                        requiredPayments.Add(GenerateRequiredPayment(priceEpisode, period));
                    }

                    foreach (var requiredPayment in requiredPayments)
                    {
                        requiredPaymentEvents.Add(GenerateRequiredPaymentEvent(requiredPayment, earningEvent, priceEpisode, period));
                    }
                }
            }

            return requiredPaymentEvents;
        }

        private PeriodisedRequiredPaymentEvent GenerateRequiredPaymentEvent(RequiredPayment requiredPayment, GSLShortCourseEarningsEvent earningEvent, PriceEpisode priceEpisode, EarningPeriod period)
        {
            return new CalculatedRequiredLevyAmount
            {
                OnProgrammeEarningType = (OnProgrammeEarningType)requiredPayment.EarningType,
                AccountId = requiredPayment.AccountId,
                TransferSenderAccountId = requiredPayment.TransferSenderAccountId,
                ApprenticeshipEmployerType = requiredPayment.ApprenticeshipEmployerType,
                ApprenticeshipId = requiredPayment.ApprenticeshipId,
                ApprenticeshipPriceEpisodeId = requiredPayment.ApprenticeshipPriceEpisodeId,
                AgeAtStartOfLearning = earningEvent.AgeAtStartOfLearning,
                LearningAim = earningEvent.LearningAim,
                LearningStartDate = requiredPayment.LearningStartDate,
                LearningAimSequenceNumber = priceEpisode.LearningAimSequenceNumber,
                CompletionAmount = requiredPayment.Amount,
                SfaContributionPercentage = requiredPayment.SfaContributionPercentage,
                PriceEpisodeIdentifier = requiredPayment.PriceEpisodeIdentifier,
                CollectionPeriod = new CollectionPeriod { AcademicYear = earningEvent.CollectionPeriod.AcademicYear, Period = period.Period },
                FundingPlatformType = earningEvent.FundingPlatformType,
                CourseType = CourseType.ShortCourse,
            };
        }
        private RequiredPayment GenerateRequiredPayment(PriceEpisode priceEpisode, EarningPeriod period)
        {
            return new RequiredPayment
            {
                Amount = period.Amount,
                EarningType = EarningType.Levy,
                PriceEpisodeIdentifier = period.PriceEpisodeIdentifier,
                AccountId = period.AccountId,
                TransferSenderAccountId = period.TransferSenderAccountId,
                ApprenticeshipEmployerType = period.ApprenticeshipEmployerType,
                ApprenticeshipId = period.ApprenticeshipId,
                ApprenticeshipPriceEpisodeId = period.ApprenticeshipPriceEpisodeId,
                SfaContributionPercentage = period.SfaContributionPercentage ?? 0,
                LearningStartDate = priceEpisode.CourseStartDate,

            };
        }
        private IReadOnlyCollection<(EarningPeriod period, int type)> GetPeriods(GSLShortCourseEarningsEvent earningEvent)
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
    }
}
