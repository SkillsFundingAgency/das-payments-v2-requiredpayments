using SFA.DAS.Payments.DataLocks.Messages.Events;
using SFA.DAS.Payments.Model.Core;
using SFA.DAS.Payments.Model.Core.OnProgramme;
using SFA.DAS.Payments.RequiredPayments.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using SFA.DAS.Payments.Model.Core.Entities;

namespace SFA.DAS.Payments.RequiredPayments.Domain.Services
{
    public interface ICoInvestmentCalculationService
    {
        bool IsEligibleForRecalculation(PayableEarningEvent payableEarningEvent);

        IReadOnlyCollection<(EarningPeriod period, int type)> ProcessPeriodsForRecalculation(PayableEarningEvent earningEvent,
            IReadOnlyCollection<(EarningPeriod period, int type)> periods);
    }

    public class CoInvestmentCalculationService : ICoInvestmentCalculationService
    {
        private const int FundingRules2024AgeThreshold = 22;
        private const int FundingRules2026AgeThreshold = 25;
        public static readonly DateTime FundingRules2024EligibilityDate = new(2024, 4, 1);
        public static readonly DateTime FundingRules2026EligibilityDate = new(2026, 8, 1);

        public bool IsEligibleForRecalculation(PayableEarningEvent payableEarningEvent)
        {
            if (payableEarningEvent.AgeAtStartOfLearning is null) return false;

            var meets2024EligibilityCriteria = payableEarningEvent.StartDate >= FundingRules2024EligibilityDate
                                    && payableEarningEvent.AgeAtStartOfLearning < FundingRules2024AgeThreshold;

            var meets2026EligibilityCriteria = payableEarningEvent.StartDate >= FundingRules2026EligibilityDate
                                    && payableEarningEvent.AgeAtStartOfLearning < FundingRules2026AgeThreshold;

            return meets2024EligibilityCriteria || meets2026EligibilityCriteria;
        }

        public IReadOnlyCollection<(EarningPeriod period, int type)> ProcessPeriodsForRecalculation(PayableEarningEvent earningEvent, IReadOnlyCollection<(EarningPeriod period, int type)> periods)
        {
            var requiresRecalculation = IsEligibleForRecalculation(earningEvent);

            if (requiresRecalculation)
            {
                foreach (var earningPeriod in periods)
                {
                    if (earningPeriod.period.ApprenticeshipId is null or 0) continue;

                    if (earningPeriod.period.DataLockFailures is not null && earningPeriod.period.DataLockFailures.Any()) continue;

                    if (earningPeriod.period.ApprenticeshipEmployerType == ApprenticeshipEmployerType.NonLevy)
                    {
                        earningPeriod.period.SfaContributionPercentage = new decimal(1.0);
                    }
                }
            }

            return periods;
        }
    }
}