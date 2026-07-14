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
        private static readonly DateTime FundingRules2024EligibilityDate = new(2024, 4, 1);
        private static readonly DateTime FundingRules2026EligibilityDate = new(2026, 8, 1);
        private PayableEarningEvent payableEvent;

        [SetUp]
        public void SetUp()
        {
            service = new CoInvestmentCalculationService();
            payableEvent = new PayableEarningEvent();
        }

        //General

        [Test]
        public void IsEligibleForRecalculation_Should_Not_Allow_Recalc_When_Age_Is_Null()
        {
            // Arrange
            SetPayableEvent(FundingRules2024EligibilityDate, null);

            // Act
            var result = service.IsEligibleForRecalculation(payableEvent);

            // Assert
            result.Should().BeFalse();
        }

        [Test]
        [TestCase(null)]
        [TestCase(0)]
        public void ProcessPeriodsForRecalculation_Should_Not_Override_CoInvestmentRate_for_EarningPeriods_With_Null_Or_Zero_Apprentice_Ids(long? apprenticeId)
        {
            // Arrange
            SetPayableEvent(FundingRules2024EligibilityDate, 21);
            var periods = new List<(EarningPeriod period, int type)>
            {
                (new EarningPeriod
                {
                    ApprenticeshipId = apprenticeId,
                    DataLockFailures = null,
                    ApprenticeshipEmployerType = ApprenticeshipEmployerType.NonLevy,
                    SfaContributionPercentage = 0.95m
                }, 1)
            };

            // Act
            var result = service.ProcessPeriodsForRecalculation(payableEvent, periods);

            // Assert
            result.Should().ContainSingle();
            result.Single().period.SfaContributionPercentage.Should().Be(0.95m);
        }

        [Test]
        public void ProcessPeriodsForRecalculation_Should_Not_Override_CoInvestmentRate_for_EarningPeriods_With_DataLock_Failures()
        {
            // Arrange
            SetPayableEvent(FundingRules2024EligibilityDate, 21);
            var dataLockFailures = new List<DataLockFailure> { new DataLockFailure() };
            var periods = new List<(EarningPeriod period, int type)>
            {
                (new EarningPeriod
                {
                    ApprenticeshipId = 22,
                    DataLockFailures = dataLockFailures,
                    ApprenticeshipEmployerType = ApprenticeshipEmployerType.NonLevy,
                    SfaContributionPercentage = 0.95m
                }, 1)
            };

            // Act
            var result = service.ProcessPeriodsForRecalculation(payableEvent, periods);

            // Assert
            result.Should().ContainSingle();
            result.Single().period.SfaContributionPercentage.Should().Be(0.95m);
        }

        [Test]
        public void ProcessPeriodsForRecalculation_Should_Not_Override_CoInvestmentRate_For_Levy_Employers()
        {
            // Arrange
            SetPayableEvent(FundingRules2024EligibilityDate, 21);
            var periods = new List<(EarningPeriod period, int type)>
            {
                (new EarningPeriod
                {
                    ApprenticeshipId = 1234,
                    ApprenticeshipEmployerType = ApprenticeshipEmployerType.Levy,
                    SfaContributionPercentage = 0.95m
                }, 1)
            };

            // Act
            var result = service.ProcessPeriodsForRecalculation(payableEvent, periods);

            // Assert
            result.Should().ContainSingle();
            result.Single().period.SfaContributionPercentage.Should().Be(0.95m);
        }

        //2024

        [Test]
        public void IsEligibleForRecalculation_Should_Not_Allow_Recalc_For_Age_21_Before_2024_Eligibility_Threshold()
        {
            // Arrange
            SetPayableEvent(FundingRules2024EligibilityDate.AddDays(-1), 21);

            // Act
            var result = service.IsEligibleForRecalculation(payableEvent);

            // Assert
            result.Should().BeFalse();
        }

        [Test]
        [TestCase(0)]
        [TestCase(1)]
        public void IsEligibleForRecalculation_Should_Allow_Recalc_For_Age_21_On_And_After_2024_Eligibility_Threshold(int dateModifier)
        {
            // Arrange
            SetPayableEvent(FundingRules2024EligibilityDate.AddDays(dateModifier), 21);

            // Act
            var result = service.IsEligibleForRecalculation(payableEvent);

            // Assert
            result.Should().BeTrue();
        }


        [Test]
        [TestCase(22)]
        [TestCase(23)]
        public void IsEligibleForRecalculation_Should_Not_Allow_Recalc_For_Age_22_Or_Over_On_And_After_2024_Eligibility_Threshold(int apprenticeAge)
        {
            // Arrange
            SetPayableEvent(FundingRules2024EligibilityDate, apprenticeAge);

            // Act
            var result = service.IsEligibleForRecalculation(payableEvent);

            // Assert
            result.Should().BeFalse();
        }

        [Test]
        [TestCase(20)]
        [TestCase(21)]
        public void IsEligibleForRecalculation_Should_Allow_Recalc_For_Age_21_Or_Under_On_And_After_2024_Eligibility_Threshold(int apprenticeAge)
        {
            // Arrange
            SetPayableEvent(FundingRules2024EligibilityDate, apprenticeAge);

            // Act
            var result = service.IsEligibleForRecalculation(payableEvent);

            // Assert
            result.Should().BeTrue();
        }

        [Test]
        public void ProcessPeriodsForRecalculation_Should_Not_Override_CoInvestmentRate_For_NonLevy_Employers_For_Age_21_Before_2024_Eligibility_Threshold()
        {
            // Arrange
            SetPayableEvent(FundingRules2024EligibilityDate.AddDays(-1), 21);
            var periods = new List<(EarningPeriod period, int type)>
            {
                (new EarningPeriod
                {
                    ApprenticeshipId = 1234,
                    ApprenticeshipEmployerType = ApprenticeshipEmployerType.NonLevy,
                    SfaContributionPercentage = 0.95m
                }, 1)
            };

            // Act
            var result = service.ProcessPeriodsForRecalculation(payableEvent, periods);

            // Assert
            result.Should().ContainSingle();
            result.Single().period.SfaContributionPercentage.Should().Be(0.95m);
        }

        [Test]
        public void ProcessPeriodsForRecalculation_Should_Not_Override_CoInvestmentRate_For_NonLevy_Employers_For_Age_22_And_On_2024_Eligibility_Threshold()
        {
            // Arrange
            SetPayableEvent(FundingRules2024EligibilityDate, 22);
            var periods = new List<(EarningPeriod period, int type)>
            {
                (new EarningPeriod
                {
                    ApprenticeshipId = 1234,
                    ApprenticeshipEmployerType = ApprenticeshipEmployerType.NonLevy,
                    SfaContributionPercentage = 0.95m
                }, 1)
            };

            // Act
            var result = service.ProcessPeriodsForRecalculation(payableEvent, periods);

            // Assert
            result.Should().ContainSingle();
            result.Single().period.SfaContributionPercentage.Should().Be(0.95m);
        }

        [Test]
        public void ProcessPeriodsForRecalculation_Should_Override_CoInvestmentRate_For_NonLevy_Employers_For_Age_21_And_On_2024_Eligibility_Threshold()
        {
            // Arrange
            SetPayableEvent(FundingRules2024EligibilityDate, 21);
            var periods = new List<(EarningPeriod period, int type)>
            {
                (new EarningPeriod
                {
                    ApprenticeshipId = 1234,
                    ApprenticeshipEmployerType = ApprenticeshipEmployerType.NonLevy,
                    SfaContributionPercentage = 0.95m
                }, 1)
            };

            // Act
            var result = service.ProcessPeriodsForRecalculation(payableEvent, periods);

            // Assert
            result.Should().ContainSingle();
            result.Single().period.SfaContributionPercentage.Should().Be(1m);
        }


        //2026

        [Test]
        public void IsEligibleForRecalculation_Should_Not_Allow_Recalc_For_Age_24_Before_2026_Eligibility_Threshold()
        {
            // Arrange
            SetPayableEvent(FundingRules2026EligibilityDate.AddDays(-1), 24);

            // Act
            var result = service.IsEligibleForRecalculation(payableEvent);

            // Assert
            result.Should().BeFalse();
        }


        [Test]
        [TestCase(0)]
        [TestCase(1)]
        public void IsEligibleForRecalculation_Should_Allow_Recalc_For_Age_24_On_And_After_2026_Eligibility_Threshold(int dateModifier)
        {
            // Arrange
            SetPayableEvent(FundingRules2026EligibilityDate.AddDays(dateModifier), 24);

            // Act
            var result = service.IsEligibleForRecalculation(payableEvent);

            // Assert
            result.Should().BeTrue();
        }


        [Test]
        [TestCase(25)]
        [TestCase(26)]
        public void IsEligibleForRecalculation_Should_Not_Allow_Recalc_For_Age_25_Or_Over_On_And_After_2026_Eligibility_Threshold(int? apprenticeAge)
        {
            // Arrange
            SetPayableEvent(FundingRules2026EligibilityDate, apprenticeAge);

            // Act
            var result = service.IsEligibleForRecalculation(payableEvent);

            // Assert
            result.Should().BeFalse();
        }

        [Test]
        [TestCase(23)]
        [TestCase(24)]
        public void IsEligibleForRecalculation_Should_Allow_Recalc_For_Age_24_Or_Under_On_And_After_2026_Eligibility_Threshold(int? apprenticeAge)
        {
            // Arrange
            SetPayableEvent(FundingRules2026EligibilityDate, apprenticeAge);

            // Act
            var result = service.IsEligibleForRecalculation(payableEvent);

            // Assert
            result.Should().BeTrue();
        }

        [Test]
        public void ProcessPeriodsForRecalculation_Should_Not_Override_CoInvestmentRate_For_NonLevy_Employers_For_Age_24_Before_2026_Eligibility_Threshold()
        {
            // Arrange
            SetPayableEvent(FundingRules2026EligibilityDate.AddDays(-1), 24);
            var periods = new List<(EarningPeriod period, int type)>
            {
                (new EarningPeriod
                {
                    ApprenticeshipId = 1234,
                    ApprenticeshipEmployerType = ApprenticeshipEmployerType.NonLevy,
                    SfaContributionPercentage = 0.95m
                }, 1)
            };

            // Act
            var result = service.ProcessPeriodsForRecalculation(payableEvent, periods);

            // Assert
            result.Should().ContainSingle();
            result.Single().period.SfaContributionPercentage.Should().Be(0.95m);
        }

        [Test]
        public void ProcessPeriodsForRecalculation_Should_Not_Override_CoInvestmentRate_For_NonLevy_Employers_For_Age_25_And_On_2026_Eligibility_Threshold()
        {
            // Arrange
            SetPayableEvent(FundingRules2026EligibilityDate, 25);
            var periods = new List<(EarningPeriod period, int type)>
            {
                (new EarningPeriod
                {
                    ApprenticeshipId = 1234,
                    ApprenticeshipEmployerType = ApprenticeshipEmployerType.NonLevy,
                    SfaContributionPercentage = 0.95m
                }, 1)
            };

            // Act
            var result = service.ProcessPeriodsForRecalculation(payableEvent, periods);

            // Assert
            result.Should().ContainSingle();
            result.Single().period.SfaContributionPercentage.Should().Be(0.95m);
        }

        [Test]
        public void ProcessPeriodsForRecalculation_Should_Override_CoInvestmentRate_For_NonLevy_Employers_For_Age_24_And_On_2026_Eligibility_Threshold()
        {
            // Arrange
            SetPayableEvent(FundingRules2026EligibilityDate, 24);
            var periods = new List<(EarningPeriod period, int type)>
            {
                (new EarningPeriod
                {
                    ApprenticeshipId = 1234,
                    ApprenticeshipEmployerType = ApprenticeshipEmployerType.NonLevy,
                    SfaContributionPercentage = 0.95m
                }, 1)
            };

            // Act
            var result = service.ProcessPeriodsForRecalculation(payableEvent, periods);

            // Assert
            result.Should().ContainSingle();
            result.Single().period.SfaContributionPercentage.Should().Be(1m);
        }


        private void SetPayableEvent(DateTime startDate, int? ageAtStartOfLearning)
        {
            payableEvent.StartDate = startDate;
            payableEvent.AgeAtStartOfLearning = ageAtStartOfLearning;
        }
    }
}
