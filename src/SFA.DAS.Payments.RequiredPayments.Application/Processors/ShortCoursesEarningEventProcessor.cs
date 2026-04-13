using SFA.DAS.Payments.Application.Messaging;
using SFA.DAS.Payments.Application.Repositories;
using SFA.DAS.Payments.EarningEvents.Messages.Events;
using SFA.DAS.Payments.Model.Core;
using SFA.DAS.Payments.Model.Core.Entities;
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
                //Get payment history for short course, minus the current period
                var cachedPayments = await paymentHistoryCache.TryGet(CacheKeys.PaymentHistoryKey, cancellationToken);
                var academicYearPayments = cachedPayments.HasValue
                    ? cachedPayments.Value
                        .Where(p =>
                            p.LearnAimReference.Equals(earningEvent.LearningAim.Reference,
                                StringComparison.OrdinalIgnoreCase) && p.CollectionPeriod.AcademicYear ==
                            earningEvent.CollectionPeriod.AcademicYear)
                        .ToList()
                    : new List<PaymentHistoryEntity>();

                foreach (var (period, type) in GetPeriods(earningEvent))
                {
                    if (period.Period > earningEvent.CollectionPeriod.Period) // cut off future periods
                        continue;

                    //Get list of payments from Payment history for the same transaction type
                    var payments = academicYearPayments.Where(payment => payment.TransactionType == type)
                        .ToList();

                    if (payments.Count == 0)
                    {
                        //Generate new payment
                        var priceEpisode =
                            earningEvent.PriceEpisodes.FirstOrDefault(x =>
                                x.Identifier == period.PriceEpisodeIdentifier) ??
                            new PriceEpisode();
                        var requiredPayment = GenerateRequiredPayment(earningEvent, priceEpisode, period, type);
                        requiredPaymentEvents.Add(GenerateRequiredPaymentEvent(requiredPayment, earningEvent,
                            priceEpisode));
                    }
                }

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

        private CalculatedRequiredLevyAmount GenerateRequiredPaymentEvent(ShortCourseRequiredPayment requiredPayment,
            GSLShortCourseEarningsEvent earningEvent, PriceEpisode priceEpisode)
        {
            return new CalculatedRequiredLevyAmount
            {
                OnProgrammeEarningType = (OnProgrammeEarningType) requiredPayment.Type,
                AccountId = requiredPayment.AccountId,
                TransferSenderAccountId = requiredPayment.TransferSenderAccountId,
                ApprenticeshipEmployerType = requiredPayment.ApprenticeshipEmployerType,
                ApprenticeshipId = requiredPayment.ApprenticeshipId,
                ApprenticeshipPriceEpisodeId = requiredPayment.ApprenticeshipPriceEpisodeId,
                AgeAtStartOfLearning = earningEvent.AgeAtStartOfLearning,
                LearningAim = new LearningAim
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
                },
                LearningStartDate = requiredPayment.LearningStartDate,
                LearningAimSequenceNumber = priceEpisode.LearningAimSequenceNumber,
                CompletionAmount = requiredPayment.Amount,
                SfaContributionPercentage = requiredPayment.SfaContributionPercentage,
                PriceEpisodeIdentifier = requiredPayment.PriceEpisodeIdentifier,
                CollectionPeriod = new CollectionPeriod
                {
                    AcademicYear = earningEvent.CollectionPeriod.AcademicYear,
                    Period = earningEvent.CollectionPeriod.Period
                },
                CourseType = CourseType.ShortCourse,
                FundingPlatformType = earningEvent.FundingPlatformType,
                ContractType = ContractType.Act1,
                Learner = earningEvent.Learner,
                EarningEventId = earningEvent.EventId,
                AmountDue = requiredPayment.Amount,
                DeliveryPeriod = requiredPayment.DeliveryPeriod,
                StartDate = priceEpisode.StartDate,
                PlannedEndDate = priceEpisode.PlannedEndDate,
                ActualEndDate = priceEpisode.ActualEndDate,
                CompletionStatus = (OnProgrammeEarningType)requiredPayment.Type == OnProgrammeEarningType.Completion ? (byte)1 : (byte)0,
                InstalmentAmount = priceEpisode.InstalmentAmount,
                NumberOfInstalments = (short)priceEpisode.NumberOfInstalments,
                JobId = earningEvent.JobId,
                EventId = Guid.NewGuid(),
                Ukprn = earningEvent.Ukprn,
                IlrSubmissionDateTime = earningEvent.IlrSubmissionDateTime
                };
        }

        private ShortCourseRequiredPayment GenerateRequiredPayment(GSLShortCourseEarningsEvent earningEvent,
            PriceEpisode priceEpisode, EarningPeriod period, int type)
        {
            // replicate logic in RequiredPaymentsProfile
            var learningStartDate = priceEpisode.CourseStartDate;
            if (earningEvent.PriceEpisodes.All(x =>
                    x.LearningAimSequenceNumber != earningEvent.LearningAim.SequenceNumber))
            {
                learningStartDate = earningEvent.LearningAim.StartDate;
            }

            return new ShortCourseRequiredPayment
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
                LearningStartDate = learningStartDate,
                Type = type,
                DeliveryPeriod = period.Period
            };
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
    }
}
