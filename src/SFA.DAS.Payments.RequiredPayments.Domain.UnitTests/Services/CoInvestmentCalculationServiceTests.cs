using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.Payments.DataLocks.Messages.Events;
using SFA.DAS.Payments.Model.Core;
using SFA.DAS.Payments.Model.Core.Entities;
using SFA.DAS.Payments.RequiredPayments.Domain.Services;

namespace SFA.DAS.Payments.RequiredPayments.Domain.UnitTests.Services
{
    [TestFixture]
    public class CoInvestmentCalculationServiceTests
    {
        private ICoInvestmentCalculationService service;
        private static DateTime FundingRules2024EligibilityDate;
        private static DateTime FundingRules2026EligibilityDate;
        private PayableEarningEvent payableEvent;

        [SetUp]
        public void SetUp()
        {
            service = new CoInvestmentCalculationService();
            FundingRules2024EligibilityDate = new DateTime(2024, 4, 1);
            FundingRules2026EligibilityDate = new DateTime(2026, 8, 1);
            payableEvent = new PayableEarningEvent();

        }


        [Test]
        [TestCase(-1, false, 0.0)]
        [TestCase(0, true, 1.0)]
        [TestCase(1, true, 1.0)]
        public void IsEligibleForRecalculation_Should_Not_Allow_Recalc_Before_StartDate(int dateModifier, bool requiresRecalc, double expectedFundingPercentage)
        {
            Decimal sfaContrib = (decimal)expectedFundingPercentage;
            payableEvent.StartDate = FundingRules2024EligibilityDate.AddDays(dateModifier);
            payableEvent.AgeAtStartOfLearning = 21;

            var result = service.IsEligibleForRecalculation(payableEvent, new List<(EarningPeriod period, int type)>());

            result.Should().Be((requiresRecalc, sfaContrib));
        }

        [Test]
        [TestCase(21, true, 1.0)]
        [TestCase(22, false, 0.0)]
        [TestCase(23, false, 0.0)]
        [TestCase(null, false, 0.0)]
        public void IsEligibleForRecalculation_Should_Not_Allow_Apprentice_22_Or_Over(int? apprenticeAge, bool isCorrectAge, double expectedContribution)
        {
            decimal sfaContrib = (decimal)expectedContribution;
            payableEvent.StartDate = FundingRules2024EligibilityDate;
            payableEvent.AgeAtStartOfLearning = apprenticeAge;

            var result = service.IsEligibleForRecalculation(payableEvent, new List<(EarningPeriod period, int type)>());
            result.Should().Be((isCorrectAge, sfaContrib));
        }

        [Test]
        [TestCase(null, 21)]
        [TestCase("2024/03/02", 21)]
        [TestCase("2024/04/01", 23)]
        [TestCase("2024/04/01", null)]

        public void ProcessPeriodsForRecalculation_Should_Not_Recalc_For_Invalid_Events(DateTime? eventStartDate, int? ageAtStartOfLearning)
        {

            var dataLockFailures = new List<DataLockFailure> { new DataLockFailure() };
            var periods = new List<(EarningPeriod period, int type)>
            {
                (new EarningPeriod { ApprenticeshipId = 1234, DataLockFailures = null, SfaContributionPercentage = new decimal(0.95)} , 1)
            };

            var result = service.ProcessPeriodsForRecalculation(payableEvent, periods);

            result.FirstOrDefault().period.SfaContributionPercentage.Should().Be(new decimal(0.95));
        }

        [Test]
        [TestCase(null, false)]
        [TestCase(22, true)]
        [TestCase(null, true)]
        [TestCase(0, false)]
        public void ProcessPeriodsForRecalculation_Should_Not_Recalc_For_Null_Apprentice_Ids_Or_DataLocks(long? apprenticeId, bool DLFailure)
        {

            var dataLockFailures = new List<DataLockFailure>{new DataLockFailure()};
            var periods = new List<(EarningPeriod period, int type)>
            {
                (new EarningPeriod { ApprenticeshipId = apprenticeId, DataLockFailures = DLFailure ? dataLockFailures : null, SfaContributionPercentage = 0} , 1)
            };

            var result = service.ProcessPeriodsForRecalculation(payableEvent, periods);

            result.FirstOrDefault().period.SfaContributionPercentage.Should().Be(0);
        }

        [Test]
        [TestCase(ApprenticeshipEmployerType.Levy, 0.95)]
        public void ProcessPeriodsForRecalculation_Should_Not_Override_CoInvestmentRate_For_Levy_Employers_Before_August_2026(ApprenticeshipEmployerType apprenticeshipEmployerType, decimal? fundingPercentage)
        {
            payableEvent.StartDate = FundingRules2024EligibilityDate;
            payableEvent.AgeAtStartOfLearning = 21;
            var periods = new List<(EarningPeriod period, int type)>
            {
                (new EarningPeriod { ApprenticeshipId = 1234, ApprenticeshipEmployerType = apprenticeshipEmployerType, SfaContributionPercentage = fundingPercentage} , 1)
            };

            var result = service.ProcessPeriodsForRecalculation(payableEvent, periods);

            result.FirstOrDefault().period.SfaContributionPercentage.Should().Be(0.95m);
        }

        [Test]
        [TestCase("2026/08/01", 21, true, 1.0)]
        [TestCase("2026/07/31", 25, false, 0.95)]
        [TestCase("2026/08/01", 25, true, 0.75)]
        public void ProcessPeriodsForRecalculation_Should_Override_CoInvestmentRate_For_Levy_Employers_2026_Eligibility(DateTime eventStartDate, int ageAtStartOfLearning, bool requiresRecalc, double expectedFundingPercentage)
        {
            decimal sfaContrib = (decimal)expectedFundingPercentage;
            payableEvent.StartDate = eventStartDate;
            payableEvent.AgeAtStartOfLearning = ageAtStartOfLearning;
            var periods = new List<(EarningPeriod period, int type)>
            {
                (new EarningPeriod { ApprenticeshipId = 1234, ApprenticeshipEmployerType = ApprenticeshipEmployerType.Levy, SfaContributionPercentage = 0.95m} , 1)
            };

            var result = service.ProcessPeriodsForRecalculation(payableEvent, periods);

            result.FirstOrDefault().period.SfaContributionPercentage.Should().Be(sfaContrib);
        }

        [Test]
        [TestCase(ApprenticeshipEmployerType.NonLevy, 0.95, 1.0)]
        public void ProcessPeriodsForRecalculation_Should_Override_CoInvestmentRate_For_NonLevy_Employers(ApprenticeshipEmployerType apprenticeshipEmployerType, decimal? fundingPercentage, decimal? expectedFundingPercentage)
        {
            payableEvent.StartDate = FundingRules2024EligibilityDate;
            payableEvent.AgeAtStartOfLearning = 21;
            var periods = new List<(EarningPeriod period, int type)>
            {
                (new EarningPeriod { ApprenticeshipId = 1234, ApprenticeshipEmployerType = apprenticeshipEmployerType, SfaContributionPercentage = fundingPercentage} , 1)
            };

            var result = service.ProcessPeriodsForRecalculation(payableEvent, periods);

            result.FirstOrDefault().period.SfaContributionPercentage.Should().Be(expectedFundingPercentage);
        }

        [Test]
        [TestCase("2026/07/31", 24, false, 0.0)]
        [TestCase("2026/08/01", 24, true, 1.0)]
        [TestCase("2026/08/01", 25, false, 0.0)]
        [TestCase("2026/08/01", null, false, 0.0)]
        public void IsEligibleForRecalculation_Should_Apply_2026_Eligibility_Criteria(DateTime eventStartDate, int? apprenticeAge, bool requiresRecalc, double expectedFundingPercentage)
        {
            decimal sfaContrib = (decimal)expectedFundingPercentage;
            payableEvent.StartDate = eventStartDate;
            payableEvent.AgeAtStartOfLearning = apprenticeAge;

            var result = service.IsEligibleForRecalculation(payableEvent, new List<(EarningPeriod period, int type)>());

            result.Should().Be((requiresRecalc, sfaContrib));
        }

        [Test]
        [TestCase(ApprenticeshipEmployerType.NonLevy, 0.95,1)]
        public void ProcessPeriodsForRecalculation_Should_Override_CoInvestmentRate_For_NonLevy_Employers_Starting_After_1st_August_2026_Who_Are_Younger_Than_25(ApprenticeshipEmployerType apprenticeshipEmployerType, decimal? fundingPercentage, decimal? expectedFundingPercentage)
        {
            payableEvent.StartDate = FundingRules2026EligibilityDate;
            payableEvent.AgeAtStartOfLearning = 24;
            var periods = new List<(EarningPeriod period, int type)>
            {
                (new EarningPeriod { ApprenticeshipId = 1234, ApprenticeshipEmployerType = apprenticeshipEmployerType, SfaContributionPercentage = fundingPercentage} , 1)
            };

            var result = service.ProcessPeriodsForRecalculation(payableEvent, periods);

            result.FirstOrDefault().period.SfaContributionPercentage.Should().Be(expectedFundingPercentage);
        }

        [Test]
        [TestCase(ApprenticeshipEmployerType.Levy, 0.95, 1)]
        public void ProcessPeriodsForRecalculation_Should_Override_CoInvestmentRate_For_Levy_Employers_Starting_After_1st_August_2026_Who_Are_Younger_Than_25(ApprenticeshipEmployerType apprenticeshipEmployerType, decimal? fundingPercentage, decimal? expectedFundingPercentage)
        {
            payableEvent.StartDate = FundingRules2026EligibilityDate;
            payableEvent.AgeAtStartOfLearning = 24;
            var periods = new List<(EarningPeriod period, int type)>
            {
                (new EarningPeriod { ApprenticeshipId = 1234, ApprenticeshipEmployerType = apprenticeshipEmployerType, SfaContributionPercentage = fundingPercentage} , 1)
            };

            var result = service.ProcessPeriodsForRecalculation(payableEvent, periods);

            result.FirstOrDefault().period.SfaContributionPercentage.Should().Be(expectedFundingPercentage);
        }
    }
}
