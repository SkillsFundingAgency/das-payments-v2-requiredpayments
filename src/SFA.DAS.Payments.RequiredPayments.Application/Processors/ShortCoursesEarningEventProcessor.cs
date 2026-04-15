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

                    //Get list of payments from Payment history for the same transaction type and delivery period
                    var payments = academicYearPayments.Where(payment => payment.DeliveryPeriod == period.Period && payment.TransactionType == type )
                        .ToList();

             
                    if (payments.Count != 0)
                    {
                        var amountPaid = payments.Sum(x => x.Amount);
                        if (amountPaid == period.Amount)
                        {
                            continue; // payment already made for this period and type
                        }
                    }
                    //Generate new payment
                    var priceEpisode =
                        earningEvent.PriceEpisodes.FirstOrDefault(x =>
                            x.Identifier == period.PriceEpisodeIdentifier) ??
                        new PriceEpisode();
                    requiredPaymentEvents.Add(GenerateRequiredPaymentEvent(earningEvent,
                        priceEpisode, period, type));
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

        private CalculatedRequiredOnProgrammeAmount GenerateRequiredPaymentEvent(
            GSLShortCourseEarningsEvent earningEvent, PriceEpisode priceEpisode, EarningPeriod period, int type)
        {
            var requiredPayment = InitialiseRequiredPaymentEvent(type, earningEvent);
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

        protected CalculatedRequiredOnProgrammeAmount InitialiseRequiredPaymentEvent(int transactionType, GSLShortCourseEarningsEvent earningEvent)
        {
            return new CalculatedRequiredLevyAmount
            {
                OnProgrammeEarningType = (OnProgrammeEarningType)transactionType,
                CourseType = CourseType.ShortCourse,
                FundingPlatformType = earningEvent.FundingPlatformType,
            };
        }
    }
}
