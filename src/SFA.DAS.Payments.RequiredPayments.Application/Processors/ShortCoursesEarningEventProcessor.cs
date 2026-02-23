using AutoMapper;
using SFA.DAS.Payments.Application.Repositories;
using SFA.DAS.Payments.EarningEvents.Messages.Events;
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
using SFA.DAS.Payments.Model.Core;
using SFA.DAS.Payments.Model.Core.OnProgramme;

namespace SFA.DAS.Payments.RequiredPayments.Application.Processors
{
    public class ShortCoursesEarningEventProcessor : IShortCoursesEarningEventProcessor
    {
        public readonly INegativeEarningService negativeEarningService;
        private readonly IMapper mapper;
        public ShortCoursesEarningEventProcessor(INegativeEarningService negativeEarningService, IMapper mapper)
        {
            this.negativeEarningService = negativeEarningService;
            this.mapper = mapper;
        }
        public async Task<ReadOnlyCollection<PeriodisedRequiredPaymentEvent>> HandleEarningEvent(
            GSLShortCourseEarningsEvent earningEvent, IDataCache<PaymentHistoryEntity[]> paymentHistoryCache,
            CancellationToken cancellationToken)
        {
            var requiredPaymentEvents = new List<PeriodisedRequiredPaymentEvent>();

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

            requiredPaymentEvents.AddRange(CheckDeliveryPeriodAgainstPayments(allPeriods, earningEvent, academicYearPayments));
            requiredPaymentEvents.AddRange(CheckPaymentsAgainstDeliveryPeriods(allPeriods, earningEvent, academicYearPayments));

            return new ReadOnlyCollection<PeriodisedRequiredPaymentEvent>(requiredPaymentEvents);
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

        private List<PeriodisedRequiredPaymentEvent> CheckDeliveryPeriodAgainstPayments(List<EarningPeriod> allPeriods, GSLShortCourseEarningsEvent earningEvent,
            List<Payment> academicYearPayments)
        {
            var requiredPayments = new List<RequiredPayment>();
            var requiredPaymentEvents = new List<PeriodisedRequiredPaymentEvent>();
            //Loop through all delivery periods (excluding current) specified in earning event
            //if the value in the earningevent is different to the payment history:
            // 1. Original payment should be refunded
            // 2. New payment made to the amount requested in the earnings

            foreach (var period in allPeriods)
            {
                var priceEpisode = earningEvent.PriceEpisodes.FirstOrDefault(x => x.Identifier == period.PriceEpisodeIdentifier);

                var payment = academicYearPayments.FirstOrDefault(x => x.CollectionPeriod.Period == period.Period);

                if (priceEpisode != null && payment != null)
                {
                    if (payment.Amount != period.Amount)
                    {
                        //Generate Refund
                        requiredPayments.AddRange(negativeEarningService
                            .ProcessNegativeEarning(period.Amount, academicYearPayments, period.Period, period.PriceEpisodeIdentifier));

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
                LearningType = LearningType.Apprenticeship
            };
        }
        private RequiredPayment GenerateRequiredPayment(PriceEpisode priceEpisode, EarningPeriod period)
        {
            return new RequiredPayment
            {
                Amount = priceEpisode.TotalNegotiatedPrice1,
                EarningType = EarningType.Levy,
                PriceEpisodeIdentifier = period.PriceEpisodeIdentifier,
                AccountId = period.AccountId,
                TransferSenderAccountId = period.TransferSenderAccountId,
                ApprenticeshipEmployerType = period.ApprenticeshipEmployerType,
                ApprenticeshipId = period.ApprenticeshipId,
                ApprenticeshipPriceEpisodeId = period.ApprenticeshipPriceEpisodeId,
                SfaContributionPercentage = period.SfaContributionPercentage ?? 0,
                LearningStartDate = priceEpisode.CourseStartDate

            };
        }

        private List<RequiredPayment> GenerateRefund(EarningPeriod period, List<Payment> academicYearPayments)
        {
            return negativeEarningService
                .ProcessNegativeEarning(period.Amount, academicYearPayments, period.Period, period.PriceEpisodeIdentifier);
        }
    }
}
