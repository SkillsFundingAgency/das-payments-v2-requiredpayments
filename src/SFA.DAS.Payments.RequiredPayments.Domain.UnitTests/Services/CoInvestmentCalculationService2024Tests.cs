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
    public class CoInvestmentCalculationService2024Tests
    {
        private ICoInvestmentCalculationService2024 service2024;
        private ICoInvestmentCalculationService service2026;
        private PayableEarningEvent payableEvent;
        private decimal defaultFundingPercentage;

        [SetUp]
        public void SetUp()
        {
            service2024 = new CoInvestmentCalculationService2024();
            service2026 = new CoInvestmentCalculationService();
            payableEvent = new PayableEarningEvent();
            defaultFundingPercentage = 0.5m; // dummy value to demonstrate that 100% funding not applied by rules
        }

        [Test]
        public void If_Over_21_And_Start_Date_After_Threshold_Then_Retain_Existing_Coinvestment_Rate_For_2024_Rules()
        {
            payableEvent.AgeAtStartOfLearning = 22;
            payableEvent.StartDate = new DateTime(2024, 04, 01);

            var periods = new List<(EarningPeriod period, int type)>
            {
                (new EarningPeriod { ApprenticeshipId = 1234, ApprenticeshipEmployerType = ApprenticeshipEmployerType.NonLevy, SfaContributionPercentage = defaultFundingPercentage} , 1)
            };

            var result = service2024.ProcessPeriodsForRecalculation(payableEvent, periods);

            foreach (var earningPeriod in result)
            {
                earningPeriod.period.SfaContributionPercentage.Should().Be(defaultFundingPercentage);
            }
            
        }

        [Test]
        public void If_Under_21_And_Start_Date_Before_Threshold_Then_Retain_Existing_Coinvestment_Rate_For_2024_Rules()
        {
            payableEvent.AgeAtStartOfLearning = 20;
            payableEvent.StartDate = new DateTime(2024, 03, 31);

            var periods = new List<(EarningPeriod period, int type)>
            {
                (new EarningPeriod { ApprenticeshipId = 1234, ApprenticeshipEmployerType = ApprenticeshipEmployerType.NonLevy, SfaContributionPercentage = defaultFundingPercentage} , 1)
            };

            var result = service2024.ProcessPeriodsForRecalculation(payableEvent, periods);

            foreach (var earningPeriod in result)
            {
                earningPeriod.period.SfaContributionPercentage.Should().Be(defaultFundingPercentage);
            }
        }

        [Test]
        public void If_Over_25_And_Start_Date_After_Threshold_Then_Retain_Existing_Coinvestment_Rate_For_2026_Rules()
        {
            payableEvent.AgeAtStartOfLearning = 26;
            payableEvent.StartDate = new DateTime(2024, 04, 01);

            var periods = new List<(EarningPeriod period, int type)>
            {
                (new EarningPeriod { ApprenticeshipId = 1234, ApprenticeshipEmployerType = ApprenticeshipEmployerType.NonLevy, SfaContributionPercentage = defaultFundingPercentage} , 1)
            };

            var result = service2026.ProcessPeriodsForRecalculation(payableEvent, periods);

            foreach (var earningPeriod in result)
            {
                earningPeriod.period.SfaContributionPercentage.Should().Be(defaultFundingPercentage);
            }
        }

        [Test]
        public void If_Under_25_And_Start_Before_Threshold_Then_Retain_Existing_Coinvestment_Rate_For_2026_Rules()
        {
            payableEvent.AgeAtStartOfLearning = 24;
            payableEvent.StartDate = new DateTime(2024, 03, 31);

            var periods = new List<(EarningPeriod period, int type)>
            {
                (new EarningPeriod { ApprenticeshipId = 1234, ApprenticeshipEmployerType = ApprenticeshipEmployerType.NonLevy, SfaContributionPercentage = defaultFundingPercentage} , 1)
            };

            var result = service2026.ProcessPeriodsForRecalculation(payableEvent, periods);

            foreach (var earningPeriod in result)
            {
                earningPeriod.period.SfaContributionPercentage.Should().Be(defaultFundingPercentage);
            }
        }

        [Test]
        public void If_16_to_18_And_Start_Before_2024_Threshold_Then_Retain_Existing_Coinvestment_Rate_For_2024_Rules()
        {
            payableEvent.AgeAtStartOfLearning = 17;
            payableEvent.StartDate = new DateTime(2024, 03, 31);

            var periods = new List<(EarningPeriod period, int type)>
            {
                (new EarningPeriod { ApprenticeshipId = 1234, ApprenticeshipEmployerType = ApprenticeshipEmployerType.NonLevy, SfaContributionPercentage = defaultFundingPercentage} , 1)
            };

            var result = service2024.ProcessPeriodsForRecalculation(payableEvent, periods);

            foreach (var earningPeriod in result)
            {
                earningPeriod.period.SfaContributionPercentage.Should().Be(defaultFundingPercentage);
            }
        }

        [Test]
        public void If_16_to_18_And_Start_Before_2024_Threshold_Then_Retain_Existing_Coinvestment_Rate_For_2026_Rules()
        {
            payableEvent.AgeAtStartOfLearning = 17;
            payableEvent.StartDate = new DateTime(2024, 03, 31);

            var periods = new List<(EarningPeriod period, int type)>
            {
                (new EarningPeriod { ApprenticeshipId = 1234, ApprenticeshipEmployerType = ApprenticeshipEmployerType.NonLevy, SfaContributionPercentage = defaultFundingPercentage} , 1)
            };

            var result = service2026.ProcessPeriodsForRecalculation(payableEvent, periods);

            foreach (var earningPeriod in result)
            {
                earningPeriod.period.SfaContributionPercentage.Should().Be(defaultFundingPercentage);
            }
        }

    }
}
