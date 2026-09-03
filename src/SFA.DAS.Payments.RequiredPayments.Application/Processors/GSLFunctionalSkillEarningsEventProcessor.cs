using SFA.DAS.Payments.Application.Repositories;
using SFA.DAS.Payments.EarningEvents.Messages.Events;
using SFA.DAS.Payments.Model.Core;
using SFA.DAS.Payments.Model.Core.Entities;
using SFA.DAS.Payments.Model.Core.OnProgramme;
using SFA.DAS.Payments.RequiredPayments.Application.Infrastructure;
using SFA.DAS.Payments.RequiredPayments.Domain;
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
    public class GSLFunctionalSkillEarningsEventProcessor : GSLEarningsEventProcessorBase, IGSLFunctionalSkillEarningsEventProcessor
    {
        private readonly IDuplicateGSLFunctionalSkillEarningEventService duplicateGSLFunctionalSkillEarningEventService;

        public GSLFunctionalSkillEarningsEventProcessor(IDuplicateGSLFunctionalSkillEarningEventService duplicateGSLFunctionalSkillEarningEventService)
        {
            this.duplicateGSLFunctionalSkillEarningEventService = duplicateGSLFunctionalSkillEarningEventService;
        }

        public async Task<ReadOnlyCollection<PeriodisedRequiredPaymentEvent>> HandleEarningEvent(
            GSLFunctionalSkillEarningsEvent earningEvent,
            IDataCache<PaymentHistoryEntity[]> paymentHistoryCache,
            CancellationToken cancellationToken)
        {
            try
            {
                var requiredPaymentEvents = new List<PeriodisedRequiredPaymentEvent>();

                if (await duplicateGSLFunctionalSkillEarningEventService.IsDuplicate(earningEvent, cancellationToken)
                        .ConfigureAwait(false))
                {
                    return requiredPaymentEvents.AsReadOnly();
                }

                //Gets all historical payments for this learner and academic year.
                //This give milestone payments, completion payments, previous collection period payments.
                var academicYearPayments = await GetAcademicYearPayments(earningEvent, paymentHistoryCache, cancellationToken);

                //Extracting the latest earnings from the incoming submission.
                var currentEarnings = GetPeriods(earningEvent).ToList();

                GenerateRefundPayments(earningEvent, requiredPaymentEvents, academicYearPayments, currentEarnings, GenerateRequiredPaymentEvent);

                GenerateNewRequiredPayments(earningEvent, requiredPaymentEvents, academicYearPayments, currentEarnings, GenerateRequiredPaymentEvent);

                return new ReadOnlyCollection<PeriodisedRequiredPaymentEvent>(requiredPaymentEvents);
            }
            catch (Exception ex)
            {
                throw new Exception(
                    $"Error processing GSLFunctionalSkillEarningsEvent for LearningAimReference: {earningEvent.LearningAim.Reference}, " +
                    $"CollectionPeriod: Year: {earningEvent.CollectionPeriod.AcademicYear} Period: {earningEvent.CollectionPeriod.Period}. Exception: {ex.Message}",
                    ex);
            }
        }

        protected CalculatedRequiredOnProgrammeAmount InitialiseRequiredPaymentEvent(
                int transactionType, GSLFunctionalSkillEarningsEvent earningEvent, bool isCoInvested)
        {
            var earningType = (OnProgrammeEarningType)transactionType;

            return isCoInvested
                ? new CalculatedRequiredCoInvestedAmount
                {
                    OnProgrammeEarningType = earningType,
                    CourseType = CourseType.FunctionalSkill,
                    FundingPlatformType = earningEvent.FundingPlatformType,
                }
                : new CalculatedRequiredLevyAmount
                {
                    OnProgrammeEarningType = earningType,
                    CourseType = CourseType.FunctionalSkill,
                    FundingPlatformType = earningEvent.FundingPlatformType,
                };
        }

        private PeriodisedRequiredPaymentEvent GenerateRequiredPaymentEvent(
                 GSLFunctionalSkillEarningsEvent earningEvent,
                 PriceEpisode priceEpisode,
                 EarningPeriod period, int type, bool isCoInvested)
        {
            var requiredPayment = InitialiseRequiredPaymentEvent(type, earningEvent, isCoInvested);
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
            requiredPayment.CompletionStatus =
                (OnProgrammeEarningType)type == OnProgrammeEarningType.Completion ? (byte)1 : (byte)0;
            requiredPayment.InstalmentAmount = priceEpisode.InstalmentAmount;
            requiredPayment.NumberOfInstalments = (short)priceEpisode.NumberOfInstalments;
            requiredPayment.JobId = earningEvent.JobId;
            requiredPayment.EventId = Guid.NewGuid();
            requiredPayment.Ukprn = earningEvent.Ukprn;
            requiredPayment.IlrSubmissionDateTime = earningEvent.IlrSubmissionDateTime;

            return requiredPayment;
        }

        private static async Task<List<PaymentHistoryEntity>> GetAcademicYearPayments(GSLFunctionalSkillEarningsEvent earningEvent,
            IDataCache<PaymentHistoryEntity[]> paymentHistoryCache,
            CancellationToken cancellationToken)
        {
            var cachedPayments = await paymentHistoryCache.TryGet(CacheKeys.PaymentHistoryKey, cancellationToken);

            return cachedPayments.HasValue
                ? cachedPayments.Value.Where(x =>
                        x.LearnAimReference.Equals(earningEvent.LearningAim.Reference, StringComparison.OrdinalIgnoreCase)
                        && x.CollectionPeriod.AcademicYear == earningEvent.CollectionPeriod.AcademicYear)
                    .ToList()
                : new List<PaymentHistoryEntity>();
        }

        private IReadOnlyCollection<(EarningPeriod period, int type)> GetPeriods(GSLFunctionalSkillEarningsEvent earningEvent)
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