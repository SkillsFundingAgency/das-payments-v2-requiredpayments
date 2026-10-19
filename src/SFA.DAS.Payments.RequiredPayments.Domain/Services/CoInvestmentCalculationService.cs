using SFA.DAS.Payments.DataLocks.Messages.Events;
using SFA.DAS.Payments.Model.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using SFA.DAS.Payments.Model.Core.Entities;

namespace SFA.DAS.Payments.RequiredPayments.Domain.Services
{
    public interface ICoInvestmentCalculationService
    {
        decimal? CalculateSfaContributionPercentage(PayableEarningEvent payableEarningEvent, ApprenticeshipEmployerType employerType);

        IReadOnlyCollection<(EarningPeriod period, int type)> ProcessPeriodsForRecalculation(PayableEarningEvent earningEvent,
            IReadOnlyCollection<(EarningPeriod period, int type)> periods);
    }

    public class CoInvestmentCalculationService : ICoInvestmentCalculationService
    {
        private const int FundingRules2024AgeThreshold = 22;
        private const int FundingRules2026AgeThreshold = 25;
        public static readonly DateTime FundingRules2024EligibilityDate = new(2024, 4, 1);
        public static readonly DateTime FundingRules2026EligibilityDate = new(2026, 8, 1);

        public decimal? CalculateSfaContributionPercentage(PayableEarningEvent payableEarningEvent,
            ApprenticeshipEmployerType employerType)
        {
            if (payableEarningEvent.AgeAtStartOfLearning is null) return null;

            // If the earning event is for a levy employer and the start date is before the 2026 eligibility date, it is not eligible for recalculation.
            if (payableEarningEvent.StartDate < FundingRules2026EligibilityDate && employerType == ApprenticeshipEmployerType.Levy)
            {
                return null;
            }

            var meets2024FullEligibilityCriteria = payableEarningEvent.StartDate >= FundingRules2024EligibilityDate
                                               && payableEarningEvent.AgeAtStartOfLearning <
                                               FundingRules2024AgeThreshold;

            var meets2026FullEligibilityCriteria = payableEarningEvent.StartDate >= FundingRules2026EligibilityDate
                                               && payableEarningEvent.AgeAtStartOfLearning <
                                               FundingRules2026AgeThreshold;

            if (meets2024FullEligibilityCriteria || meets2026FullEligibilityCriteria)
            {
                return new decimal(1.0);
            }

            //If date after 1/8/26 & learner is 25 or over, and the employer is a levy employer, set contribution to 75%
            if (payableEarningEvent.StartDate >= FundingRules2026EligibilityDate && payableEarningEvent.AgeAtStartOfLearning >= FundingRules2026AgeThreshold)
            {
                //If Levy
                if (employerType == ApprenticeshipEmployerType.Levy)
                {
                    return new decimal(0.75);
                }
            }

            return null;
        }

        public IReadOnlyCollection<(EarningPeriod period, int type)> ProcessPeriodsForRecalculation(PayableEarningEvent earningEvent, IReadOnlyCollection<(EarningPeriod period, int type)> periods)
        {
            foreach (var earningPeriod in periods)
            {
                if (earningPeriod.period.ApprenticeshipId is null or 0) continue;

                if (earningPeriod.period.DataLockFailures is not null && earningPeriod.period.DataLockFailures.Any()) continue;

                var recalculatedSfaContributionPercentage = CalculateSfaContributionPercentage(earningEvent, earningPeriod.period.ApprenticeshipEmployerType);

                if (recalculatedSfaContributionPercentage.HasValue)
                {
                    earningPeriod.period.SfaContributionPercentage = recalculatedSfaContributionPercentage.Value;
                }
            }
            

            return periods;
        }
    }
}