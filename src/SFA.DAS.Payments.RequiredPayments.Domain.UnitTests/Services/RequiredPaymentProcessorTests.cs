using System;
using System.Collections.Generic;
using System.Linq;
using AutoFixture.NUnit3;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SFA.DAS.Payments.Application.Infrastructure.Logging;
using SFA.DAS.Payments.Model.Core.Entities;
using SFA.DAS.Payments.RequiredPayments.Domain.Entities;
using SFA.DAS.Payments.RequiredPayments.Domain.Services;

namespace SFA.DAS.Payments.RequiredPayments.Domain.UnitTests.Services
{
    [TestFixture]
    public class RequiredPaymentProcessorTests
    {
        protected RequiredPaymentProcessor sut;
        protected List<Payment> paymentHistory;
        protected decimal expectedAmount;
        protected Earning testEarning;
        protected Mock<IPaymentLogger> paymentLogger;
        
        [SetUp]
        public void Setup()
        {
            paymentHistory = new List<Payment>();

            testEarning = new Earning
            {
                Amount = 50,
                PriceEpisodeIdentifier = string.Empty,
                SfaContributionPercentage = 0,
                EarningType = EarningType.Levy,
            };

            paymentLogger = new Mock<IPaymentLogger>();

            sut = new RequiredPaymentProcessor(new PaymentDueProcessor(), new RefundService(), paymentLogger.Object);
            expectedAmount = 50;
        }
        
        [TestFixture]
        public class WhenAmountIsMoreThanTotalAmountInHistory : RequiredPaymentProcessorTests
        {
            [Test]
            public void ThenRequiredPaymentHasCorrectAmount()
            {
                var actual = sut.GetRequiredPayments(testEarning, paymentHistory);

                actual.Single().Amount.Should().Be(expectedAmount);
            }

            [Test, AutoData]
            public void ThenRequiredPaymentHasSfaContributionPercentageOfInput(decimal expectedSfaContribution)
            {
                testEarning.SfaContributionPercentage = expectedSfaContribution;

                var actual = sut.GetRequiredPayments(testEarning, paymentHistory);

                actual.Single().SfaContributionPercentage.Should().Be(expectedSfaContribution);
            }

            [Test]
            [TestCase(EarningType.CoInvested)]
            [TestCase(EarningType.Levy)]
            [TestCase(EarningType.Incentive)]
            public void ThenRequiredPaymentHasEarningTypeOfInput(EarningType expectedEarningType)
            {
                testEarning.EarningType = expectedEarningType;

                var actual = sut.GetRequiredPayments(testEarning, paymentHistory);

                actual.Single().EarningType.Should().Be(expectedEarningType);
            }

            [Test]
            public void AndThereIsNoSfaContributionPercentage_ThenAnExceptionIsThrown()
            {
                testEarning.SfaContributionPercentage = null;

                sut.Invoking(x => x.GetRequiredPayments(testEarning, paymentHistory))
                    .Should().Throw<ArgumentException>();
            }
        }

        [TestFixture]
        public class WhenAmountIsEqualToTotalAmountInHistory : RequiredPaymentProcessorTests
        {
            [Test]
            public void ThenThenNothingIsReturned()
            {
                testEarning.Amount = 0;

                var actual = sut.GetRequiredPayments(testEarning, paymentHistory);

                actual.Should().BeEmpty();
            }
        }

        [TestFixture]
        public class WhenAmountIsLessThanTotalAmountInHistory : RequiredPaymentProcessorTests
        {
            [Test]
            public void ThenRefundIsCreated()
            {
                testEarning.Amount = 0;
                paymentHistory.Add(new Payment { Amount = 50, SfaContributionPercentage = 0.9m, FundingSource = FundingSourceType.Levy});
                var actual = sut.GetRequiredPayments(testEarning, paymentHistory);

                actual.Should().HaveCount(1);
                actual.All(rp => rp.SfaContributionPercentage == .9m && rp.Amount == -50).Should().BeTrue();
            }

            [Test, InlineAutoData]
            public void ThenTheApprenticeshipIdIsCorrectInTheRequiredPayment(long? testApprenticeshipId)
            {
                testEarning.Amount = 0;
                paymentHistory.Add(new Payment
                {
                    Amount = 50, 
                    SfaContributionPercentage = 0.9m, 
                    FundingSource = FundingSourceType.Levy, 
                    ApprenticeshipId = testApprenticeshipId,
                });

                var actual = sut.GetRequiredPayments(testEarning, paymentHistory);
                actual.Should().BeEquivalentTo(new
                {
                    ApprenticeshipId = testApprenticeshipId,
                });
            }

            [Test, AutoData]
            public void ThenTheApprenticeshipPriceEpisodeIdIsCorrectInTheRequiredPayment(
                long? testApprenticeshipPriceEpisodeId)
            {
                testEarning.Amount = 0;
                paymentHistory.Add(new Payment
                {
                    Amount = 50, 
                    SfaContributionPercentage = 0.9m, 
                    FundingSource = FundingSourceType.Levy, 
                    ApprenticeshipPriceEpisodeId = testApprenticeshipPriceEpisodeId,
                });

                var actual = sut.GetRequiredPayments(testEarning, paymentHistory);
                actual.Should().BeEquivalentTo(new
                {
                    ApprenticeshipPriceEpisodeId = testApprenticeshipPriceEpisodeId,
                });
            }

            [Test, AutoData]
            public void ThenTheApprenticeshipEmployerTypeIsCorrectInTheRequiredPayment(
                ApprenticeshipEmployerType testApprenticeshipEmployerType)
            {
                testEarning.Amount = 0;
                paymentHistory.Add(new Payment
                {
                    Amount = 50,
                    SfaContributionPercentage = 0.9m,
                    FundingSource = FundingSourceType.Levy,
                    ApprenticeshipEmployerType = testApprenticeshipEmployerType,
                });

                var actual = sut.GetRequiredPayments(testEarning, paymentHistory);
                actual.Should().BeEquivalentTo(new
                {
                    ApprenticeshipEmployerType = testApprenticeshipEmployerType,
                });
            }
        }

        [TestFixture]
        public class WhenAmountIsZero : RequiredPaymentProcessorTests
        {
            [Test]
            public void ThenFullRefundIsCreated()
            {
                testEarning.SfaContributionPercentage = null;
                testEarning.Amount = 0;

                paymentHistory.Add(new Payment { SfaContributionPercentage = .9m, Amount = 10, FundingSource = FundingSourceType.Levy});
                paymentHistory.Add(new Payment { SfaContributionPercentage = 1m, Amount = 20, FundingSource = FundingSourceType.Levy });
                paymentHistory.Add(new Payment { SfaContributionPercentage = .95m, Amount = 30, FundingSource = FundingSourceType.Levy });

                var actual = sut.GetRequiredPayments(testEarning, paymentHistory);

                actual.Sum(x => x.Amount).Should().Be(-60);
            }
        }

        [TestFixture]
        public class WhenHistoricPaymentsUsedADifferentSfaContribution : RequiredPaymentProcessorTests
        {
            [Test]
            public void ThenHistoricPaymentsWithDifferentSfaContributionShouldNotBeIncludedInRequiredAmountCalculation()
            {
                testEarning.Amount = 0;
                testEarning.SfaContributionPercentage = 0.9m;

                paymentHistory.Add(new Payment { SfaContributionPercentage = .9m, Amount = 10, FundingSource = FundingSourceType.Levy});
                paymentHistory.Add(new Payment { SfaContributionPercentage = 1m, Amount = 10, FundingSource = FundingSourceType.Levy });
                paymentHistory.Add(new Payment { SfaContributionPercentage = .95m, Amount = 10, FundingSource = FundingSourceType.Levy});
                
                var actual = sut.GetRequiredPayments(testEarning, paymentHistory);

                actual.Sum(x => x.Amount).Should().Be(-30);
            }

            [Test]
            public void ThenTheHistoricPaymentsWithADifferentSfaContributionPercentageShouldBeRefunded()
            {
                testEarning.Amount = 5;
                testEarning.SfaContributionPercentage = 0.9m;

                paymentHistory.Add(new Payment { SfaContributionPercentage = .9m, Amount = 5, FundingSource = FundingSourceType.Levy});
                paymentHistory.Add(new Payment { SfaContributionPercentage = 1m, Amount = 10, FundingSource = FundingSourceType.Levy });
                paymentHistory.Add(new Payment { SfaContributionPercentage = .95m, Amount = 20, FundingSource = FundingSourceType.Levy });
                
                var requiredPayments = sut.GetRequiredPayments(testEarning, paymentHistory);
                requiredPayments.Count.Should().Be(2);
                requiredPayments.All(rp => rp.SfaContributionPercentage != .9m).Should().BeTrue();
                requiredPayments.Select(rp => rp.Amount).Sum().Should().Be(-30m);
            }

            [Test]
            public void ThenShouldRefundHistoricPaymentsWithADifferentSfaContributionAndCreateRequiredPositivePayment()
            {
                testEarning.SfaContributionPercentage = 0.9m;
                testEarning.Amount = 45;

                expectedAmount = 45;
                paymentHistory.Add(new Payment { SfaContributionPercentage = .9m, Amount = 5, DeliveryPeriod = 1, FundingSource = FundingSourceType.Levy});
                paymentHistory.Add(new Payment { SfaContributionPercentage = 1m, Amount = 10, DeliveryPeriod = 1, FundingSource = FundingSourceType.Levy });
                paymentHistory.Add(new Payment { SfaContributionPercentage = .95m, Amount = 20, DeliveryPeriod = 1, FundingSource = FundingSourceType.Levy });
                
                var requiredPayments = sut.GetRequiredPayments(testEarning, paymentHistory);
                requiredPayments.Count.Should().Be(3);
                requiredPayments.Count(rp => rp.SfaContributionPercentage == .9m).Should().Be(1);
                requiredPayments.Count(rp => rp.SfaContributionPercentage == .95m).Should().Be(1);
                requiredPayments.Count(rp => rp.SfaContributionPercentage == 1m).Should().Be(1);
                requiredPayments.Where(x => x.Amount < 0).Sum(x => x.Amount).Should().Be(-30);
                requiredPayments.Where(x => x.Amount > 0).Sum(x => x.Amount).Should().Be(40);
            }

            [Test]
            public void ThenShouldRefundHistoricPaymentsWithADifferentSfaContributionAndRefundRequiredPayment()
            {
                testEarning.SfaContributionPercentage = 0.9m;
                testEarning.Amount = 0;

                paymentHistory.Add(new Payment { SfaContributionPercentage = .9m, Amount = 5, FundingSource = FundingSourceType.Levy});
                paymentHistory.Add(new Payment { SfaContributionPercentage = 1m, Amount = 10, FundingSource = FundingSourceType.Levy });
                paymentHistory.Add(new Payment { SfaContributionPercentage = .95m, Amount = 20, FundingSource = FundingSourceType.Levy });
                
                var requiredPayments = sut.GetRequiredPayments(testEarning, paymentHistory);
                requiredPayments.Count.Should().Be(3);
                requiredPayments.Count(rp => rp.SfaContributionPercentage == .9M).Should().Be(1);
                requiredPayments.Count(rp => rp.SfaContributionPercentage == .95M).Should().Be(1);
                requiredPayments.Count(rp => rp.SfaContributionPercentage == 1M).Should().Be(1);
                requiredPayments.Sum(rp => rp.Amount).Should().Be(-35);
            }
        }

        [TestFixture]
        public class WhenRoundingErrorInPreviousRefundGeneratesAPaymentDueThatIsLessThanOnePence : RequiredPaymentProcessorTests
        {
            [Test]
            public void ThenThePaymentShouldNotBeGeneratedAndAWarningLoggedIfSfaContributionIsNull()
            {
                testEarning.SfaContributionPercentage = null;
                testEarning.Amount = 0.004128m;

                paymentHistory.Add(new Payment { SfaContributionPercentage = .9m, Amount = 514.285872m, FundingSource = FundingSourceType.Levy });
                paymentHistory.Add(new Payment { SfaContributionPercentage = .9m, Amount = -514.29000m, FundingSource = FundingSourceType.Levy });
                
                paymentLogger.Setup(x => x.LogWarning(It.IsAny<string>(), It.IsAny<object[]>(), It.IsAny<long>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()))
                    .Verifiable();

                var requiredPayments = sut.GetRequiredPayments(testEarning, paymentHistory);
                requiredPayments.Count.Should().Be(0);
                paymentLogger.Verify(x => x.LogWarning(It.Is<string>(y => y.StartsWith($"Payment amount is a fraction of a penny")), It.IsAny<object[]>(), It.IsAny<long>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()), Times.Once);
            }

            [Test]
            public void ThenThePaymentShouldNotBeGeneratedAndAWarningLoggedIfSfaContributionIsNotNull()
            {
                testEarning.SfaContributionPercentage = 0.9m;
                testEarning.Amount = 0.004128m;

                paymentHistory.Add(new Payment { SfaContributionPercentage = .9m, Amount = 514.285872m, FundingSource = FundingSourceType.Levy });
                paymentHistory.Add(new Payment { SfaContributionPercentage = .9m, Amount = -514.29000m, FundingSource = FundingSourceType.Levy });

                paymentLogger.Setup(x => x.LogWarning(It.IsAny<string>(), It.IsAny<object[]>(), It.IsAny<long>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()))
                    .Verifiable();

                var requiredPayments = sut.GetRequiredPayments(testEarning, paymentHistory);
                requiredPayments.Count.Should().Be(0);
                paymentLogger.Verify(x => x.LogWarning(It.Is<string>(y => y.StartsWith($"Payment amount is a fraction of a penny")), It.IsAny<object[]>(), It.IsAny<long>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()), Times.Once);
            }


            [Test]
            public void ThenTheRefundShouldNotBeGeneratedAndAWarningLogged()
            {
                testEarning.SfaContributionPercentage = null;
                testEarning.Amount = -0.005128m;

                paymentHistory.Add(new Payment { SfaContributionPercentage = .9m, Amount = 514.285872m, FundingSource = FundingSourceType.Levy });
                paymentHistory.Add(new Payment { SfaContributionPercentage = .9m, Amount = -514.29000m, FundingSource = FundingSourceType.Levy });

                paymentLogger.Setup(x => x.LogWarning(It.IsAny<string>(), It.IsAny<object[]>(), It.IsAny<long>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()))
                    .Verifiable();

                var requiredPayments = sut.GetRequiredPayments(testEarning, paymentHistory);
                requiredPayments.Count.Should().Be(0);
                paymentLogger.Verify(x => x.LogWarning(It.Is<string>(y => y.StartsWith($"Refund amount is a fraction of a penny")), It.IsAny<object[]>(), It.IsAny<long>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()), Times.Once);
            }
        }
    }
}
