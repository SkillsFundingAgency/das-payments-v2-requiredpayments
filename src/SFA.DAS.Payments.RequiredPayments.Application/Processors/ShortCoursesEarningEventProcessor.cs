using SFA.DAS.Payments.Application.Messaging;
using SFA.DAS.Payments.Application.Repositories;
using SFA.DAS.Payments.EarningEvents.Messages;
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
            public bool CompletionPayment { get; set; }
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

                //If Learner has withdrawn, refund all previous payments for the academic year in EarningEvent
                if (!earningEvent.Earnings.Any())
                {
                    //If the course was Co-Invested

                    var coInvestedPayments = academicYearPayments.Where(x =>
                        x.FundingSource is FundingSourceType.CoInvestedSfa or FundingSourceType.CoInvestedEmployer).ToList();

                    if (coInvestedPayments.Any())
                    {
                        //Refund Milestone 1 payments
                        var milestone1Payment = coInvestedPayments.FirstOrDefault(x => x.TransactionType == (int)TransactionType.Milestone1);
                        if (milestone1Payment != null)
                        {
                            var paymentPriceEpisode = MapPaymentHistoryToPriceEpisode(milestone1Payment);
                            var paymentEarningPeriod = MapPaymentHistoryToEarningPeriodForRefund(milestone1Payment);

                            var requiredPayment = GenerateRequiredPayment(earningEvent,
                                paymentPriceEpisode,
                                paymentEarningPeriod, false);

                            requiredPayment.Amount = CreateNegativeAmount(coInvestedPayments
                                .Where(x => x.TransactionType == (int)TransactionType.Milestone1).Sum(y => y.Amount));

                            requiredPaymentEvents.Add(GenerateRequiredPaymentEvent(requiredPayment, earningEvent, paymentPriceEpisode,
                                paymentEarningPeriod));
                        }
                        //Refund Completion payments
                        var completionPayments = coInvestedPayments.FirstOrDefault(x => x.TransactionType == (int)TransactionType.Completion);
                        if (completionPayments != null)
                        {
                            var paymentPriceEpisode = MapPaymentHistoryToPriceEpisode(completionPayments);
                            var paymentEarningPeriod = MapPaymentHistoryToEarningPeriodForRefund(completionPayments);

                            var requiredPayment = GenerateRequiredPayment(earningEvent,
                                paymentPriceEpisode,
                                paymentEarningPeriod, false);

                            requiredPayment.Amount = CreateNegativeAmount(coInvestedPayments
                                .Where(x => x.TransactionType == (int)TransactionType.Completion).Sum(y => y.Amount));

                            requiredPaymentEvents.Add(GenerateRequiredPaymentEvent(requiredPayment, earningEvent, paymentPriceEpisode,
                                paymentEarningPeriod));
                        }

                        return new ReadOnlyCollection<PeriodisedRequiredPaymentEvent>(requiredPaymentEvents);
                    }

                    foreach (var payment in academicYearPayments)
                    {
                        var paymentPriceEpisode = MapPaymentHistoryToPriceEpisode(payment);
                        var paymentEarningPeriod = MapPaymentHistoryToEarningPeriodForRefund(payment);

                        // Generate refund for each payment
                        var requiredPayment = GenerateRequiredPayment(earningEvent, paymentPriceEpisode, paymentEarningPeriod, payment.TransactionType == (int)ShortCourseEarningType.Completion);
                        requiredPaymentEvents.Add(GenerateRequiredPaymentEvent(requiredPayment, earningEvent, paymentPriceEpisode, paymentEarningPeriod));
                    }
                    return new ReadOnlyCollection<PeriodisedRequiredPaymentEvent>(requiredPaymentEvents);
                }

                foreach (var (period, type) in GetPeriods(earningEvent))
                {
                    if (period.Period > earningEvent.CollectionPeriod.Period) // cut off future periods
                        continue;

                    //Get list of payments from Payment history for the same transaction type
                    var payments = academicYearPayments.Where(payment => payment.TransactionType == type)
                        .ToList();

                    var completionPayment = type == (int)ShortCourseEarningType.Completion;

                    //Generate new payment
                    if (payments.Count == 0)
                    {
                        var priceEpisode =
                            earningEvent.PriceEpisodes.FirstOrDefault(x => x.Identifier == period.PriceEpisodeIdentifier) ??
                            new PriceEpisode();
                        var requiredPayment = GenerateRequiredPayment(earningEvent, priceEpisode, period, completionPayment);
                        requiredPaymentEvents.Add(GenerateRequiredPaymentEvent(requiredPayment, earningEvent, priceEpisode, period));
                        continue;
                    }

                    //For existing payments, check if the delivery period and payment matches the earning event,
                    //if not generate a refund for the original payment and a new payment for the amount in the earning event.
                    requiredPaymentEvents.AddRange(
                        CheckDeliveryPeriodAgainstPayments(period, payments, earningEvent, completionPayment));

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

        private List<PeriodisedRequiredPaymentEvent> CheckDeliveryPeriodAgainstPayments(EarningPeriod period,
            List<PaymentHistoryEntity> payments, GSLShortCourseEarningsEvent earningEvent,
            bool completionPayment = false)
        {
            var requiredPayments = new List<ShortCourseRequiredPayment>();
            var requiredPaymentEvents = new List<PeriodisedRequiredPaymentEvent>();
            //if the value in the EarningEvent is different to the payment history:
            // 1. Original payment should be refunded
            // 2. New payment made to the amount requested in the earnings

            var priceEpisode =
                earningEvent.PriceEpisodes.FirstOrDefault(x => x.Identifier == period.PriceEpisodeIdentifier);

            if (priceEpisode != null)
            {
                foreach (var payment in payments)
                {
                    if (payment.DeliveryPeriod != period.Period || payment.Amount != period.Amount)
                    {
                        requiredPayments.AddRange(GenerateRefundAndNewPayment(earningEvent, priceEpisode, period, payment, completionPayment));
                    }
                }
            }

            foreach (var requiredPayment in requiredPayments)
            {
                requiredPaymentEvents.Add(GenerateRequiredPaymentEvent(requiredPayment, earningEvent,
                    priceEpisode, period));
            }

            return requiredPaymentEvents;
        }

        private List<ShortCourseRequiredPayment> GenerateRefundAndNewPayment(GSLShortCourseEarningsEvent earningEvent, PriceEpisode priceEpisode, EarningPeriod period, PaymentHistoryEntity payment, bool completionPayment = false)
        {
            var requiredPayments = new List<ShortCourseRequiredPayment>();
            //Generate Refund
            requiredPayments.Add(GenerateRequiredPayment(earningEvent, MapPaymentHistoryToPriceEpisode(payment), MapPaymentHistoryToEarningPeriodForRefund(payment), completionPayment));
            //Generate new payment 
            requiredPayments.Add(GenerateRequiredPayment(earningEvent, priceEpisode, period, completionPayment));
            return requiredPayments;
        }
        private CalculatedRequiredOnProgrammeAmount GenerateRequiredPaymentEvent(ShortCourseRequiredPayment requiredPayment,
            GSLShortCourseEarningsEvent earningEvent, PriceEpisode priceEpisode, EarningPeriod period, bool isCoInvested = false)
        {
            CalculatedRequiredOnProgrammeAmount paymentEvent = isCoInvested
                ? new CalculatedRequiredCoInvestedAmount()
                : new CalculatedRequiredLevyAmount();

            paymentEvent.OnProgrammeEarningType = requiredPayment.CompletionPayment
                ? OnProgrammeEarningType.Completion
                : OnProgrammeEarningType.Milestone1;
            paymentEvent.AccountId = requiredPayment.AccountId;
            paymentEvent.TransferSenderAccountId = requiredPayment.TransferSenderAccountId;
            paymentEvent.ApprenticeshipEmployerType = requiredPayment.ApprenticeshipEmployerType;
            paymentEvent.ApprenticeshipId = requiredPayment.ApprenticeshipId;
            paymentEvent.ApprenticeshipPriceEpisodeId = requiredPayment.ApprenticeshipPriceEpisodeId;
            paymentEvent.AgeAtStartOfLearning = earningEvent.AgeAtStartOfLearning;
            paymentEvent.LearningAim = new LearningAim
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
            paymentEvent.LearningStartDate = requiredPayment.LearningStartDate;
            paymentEvent.LearningAimSequenceNumber = priceEpisode.LearningAimSequenceNumber;
            paymentEvent.CompletionAmount = requiredPayment.Amount;
            paymentEvent.SfaContributionPercentage = requiredPayment.SfaContributionPercentage;
            paymentEvent.PriceEpisodeIdentifier = requiredPayment.PriceEpisodeIdentifier;
            paymentEvent.CollectionPeriod = new CollectionPeriod
            {
                AcademicYear = earningEvent.CollectionPeriod.AcademicYear,
                Period = earningEvent.CollectionPeriod.Period
            };
            if (paymentEvent is CalculatedRequiredLevyAmount levyAmount)
            {
                levyAmount.CourseType = CourseType.ShortCourse;
                levyAmount.FundingPlatformType = earningEvent.FundingPlatformType;
            }
            paymentEvent.ContractType = ContractType.Act1;
            paymentEvent.Learner = earningEvent.Learner;
            paymentEvent.EarningEventId = earningEvent.EventId;
            paymentEvent.AmountDue = requiredPayment.Amount;
            paymentEvent.DeliveryPeriod = requiredPayment.DeliveryPeriod;
            paymentEvent.StartDate = priceEpisode.StartDate;
            paymentEvent.PlannedEndDate = priceEpisode.PlannedEndDate;
            paymentEvent.ActualEndDate = priceEpisode.ActualEndDate;
            paymentEvent.CompletionStatus = requiredPayment.CompletionPayment ? (byte)1 : (byte)0;
            paymentEvent.InstalmentAmount = priceEpisode.InstalmentAmount;
            paymentEvent.NumberOfInstalments = (short)priceEpisode.NumberOfInstalments;
            paymentEvent.JobId = earningEvent.JobId;
            paymentEvent.EventId = Guid.NewGuid();
            paymentEvent.Ukprn = earningEvent.Ukprn;
            paymentEvent.IlrSubmissionDateTime = earningEvent.IlrSubmissionDateTime;

            return paymentEvent;
        }

        private ShortCourseRequiredPayment GenerateRequiredPayment(GSLShortCourseEarningsEvent earningEvent,
            PriceEpisode priceEpisode, EarningPeriod period, bool completionPayment)
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
                CompletionPayment = completionPayment,
                DeliveryPeriod = period.Period
            };
        }


        private PriceEpisode MapPaymentHistoryToPriceEpisode(PaymentHistoryEntity payment)
        {
            return new PriceEpisode
            {
                // Dates
                StartDate = payment.StartDate,
                CourseStartDate = payment.StartDate,
                PlannedEndDate = payment.PlannedEndDate ?? default,
                ActualEndDate = payment.ActualEndDate,
                // Identifiers
                Identifier = payment.PriceEpisodeIdentifier,
                // Funding and Amounts
                FundingLineType = payment.LearningAimFundingLineType,
                InstalmentAmount = payment.InstalmentAmount,
                NumberOfInstalments = payment.NumberOfInstalments,
                AgreedPrice = payment.Amount,
                CompletionAmount = payment.CompletionAmount,
                // Sequence
                LearningAimSequenceNumber = 0, // Not present in PaymentHistoryEntity, set to 0 or default
            };
        }
        private EarningPeriod MapPaymentHistoryToEarningPeriodForRefund(PaymentHistoryEntity payment)
        {
            return new EarningPeriod
            {
                Period = payment.DeliveryPeriod,
                PriceEpisodeIdentifier = payment.PriceEpisodeIdentifier,
                Amount = CreateNegativeAmount(payment.Amount),
                SfaContributionPercentage = payment.SfaContributionPercentage,
                AccountId = payment.AccountId,
                TransferSenderAccountId = payment.TransferSenderAccountId,
                ApprenticeshipEmployerType = payment.ApprenticeshipEmployerType,
                ApprenticeshipId = payment.ApprenticeshipId,
                ApprenticeshipPriceEpisodeId = payment.ApprenticeshipPriceEpisodeId,
                AgreedOnDate = null, // Not present in PaymentHistoryEntity
                Priority = null,     // Not present in PaymentHistoryEntity
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

        private decimal CreateNegativeAmount(decimal amount)
        {
            return -amount;
        }
    }
}
