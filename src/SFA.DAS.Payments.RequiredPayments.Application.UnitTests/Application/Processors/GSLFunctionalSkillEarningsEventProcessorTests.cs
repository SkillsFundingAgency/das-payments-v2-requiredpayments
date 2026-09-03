using Moq;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using SFA.DAS.Payments.Application.Repositories;
using SFA.DAS.Payments.EarningEvents.Messages.Events;
using SFA.DAS.Payments.Model.Core;
using SFA.DAS.Payments.Model.Core.Entities;
using SFA.DAS.Payments.Model.Core.Incentives;
using SFA.DAS.Payments.RequiredPayments.Application.Processors;
using SFA.DAS.Payments.RequiredPayments.Domain;
using SFA.DAS.Payments.RequiredPayments.Messages.Events;
using SFA.DAS.Payments.RequiredPayments.Model.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.Payments.RequiredPayments.Application.UnitTests.Application.Processors
{
    [TestFixture]
    public class GSLFunctionalSkillEarningsEventProcessorTests
    {
        private Mock<IDataCache<PaymentHistoryEntity[]>> paymentHistoryCacheMock;
        private Mock<IDuplicateGSLFunctionalSkillEarningEventService> duplicateEarningEventServiceMock;
        private GSLFunctionalSkillEarningsEventProcessor processor;

        [SetUp]
        public void SetUp()
        {
            paymentHistoryCacheMock =
                new Mock<IDataCache<PaymentHistoryEntity[]>>();

            duplicateEarningEventServiceMock =
                new Mock<IDuplicateGSLFunctionalSkillEarningEventService>();

            duplicateEarningEventServiceMock
                .Setup(x => x.IsDuplicate(
                    It.IsAny<GSLFunctionalSkillEarningsEvent>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            processor =
                new GSLFunctionalSkillEarningsEventProcessor(
                    duplicateEarningEventServiceMock.Object);
        }

        [Test]
        public async Task HandleEarningEvent_ReturnsEmpty_WhenThereAreNoEarnings()
        {
            var earningEvent =
                CreateEarningEvent(new List<FunctionalSkillEarning>());

            SetPaymentHistory(Array.Empty<PaymentHistoryEntity>());

            var result = await processor.HandleEarningEvent(
                earningEvent,
                paymentHistoryCacheMock.Object,
                CancellationToken.None);

            ClassicAssert.IsNotNull(result);
            ClassicAssert.IsEmpty(result);
        }

        [Test]
        public async Task HandleEarningEvent_ReturnsRequiredPayment_WhenThereIsNoPaymentHistory()
        {
            var earningPeriod =
                CreateEarningPeriod(1, 100m);

            var earningEvent =
                CreateEarningEvent(
                    new List<FunctionalSkillEarning>
                    {
                        CreateFunctionalSkillEarning(earningPeriod)
                    });

            SetPaymentHistory(Array.Empty<PaymentHistoryEntity>());

            var result = await processor.HandleEarningEvent(
                earningEvent,
                paymentHistoryCacheMock.Object,
                CancellationToken.None);

            ClassicAssert.AreEqual(1, result.Count);

            var requiredPayment =
                result.Single();

            ClassicAssert.AreEqual(100m, requiredPayment.AmountDue);
            ClassicAssert.AreEqual(1, requiredPayment.DeliveryPeriod);
            ClassicAssert.AreEqual(
                TransactionType.OnProgrammeMathsAndEnglish,
                requiredPayment.TransactionType);
            ClassicAssert.AreEqual(
                earningPeriod.PriceEpisodeIdentifier,
                requiredPayment.PriceEpisodeIdentifier);
            ClassicAssert.AreEqual(
                2526,
                requiredPayment.CollectionPeriod.AcademicYear);
            ClassicAssert.AreEqual(
                1,
                requiredPayment.CollectionPeriod.Period);
        }

        [Test]
        public async Task HandleEarningEvent_ReturnsEmpty_WhenPaymentHistoryMatchesCurrentEarning()
        {
            var earningPeriod =
                CreateEarningPeriod(1, 100m);

            var earningEvent =
                CreateEarningEvent(
                    new List<FunctionalSkillEarning>
                    {
                        CreateFunctionalSkillEarning(earningPeriod)
                    });

            SetPaymentHistory(
                new[]
                {
                    CreatePaymentHistory(
                        deliveryPeriod: 1,
                        amount: 100m)
                });

            var result = await processor.HandleEarningEvent(
                earningEvent,
                paymentHistoryCacheMock.Object,
                CancellationToken.None);

            ClassicAssert.IsEmpty(result);
        }

        [Test]
        public async Task HandleEarningEvent_ReturnsRefundAndNewPayment_WhenHistoricalAmountDiffers()
        {
            var earningPeriod =
                CreateEarningPeriod(1, 100m);

            var earningEvent =
                CreateEarningEvent(
                    new List<FunctionalSkillEarning>
                    {
                        CreateFunctionalSkillEarning(earningPeriod)
                    });

            SetPaymentHistory(
                new[]
                {
                    CreatePaymentHistory(
                        deliveryPeriod: 1,
                        amount: 125m)
                });

            var result = await processor.HandleEarningEvent(
                earningEvent,
                paymentHistoryCacheMock.Object,
                CancellationToken.None);

            ClassicAssert.AreEqual(2, result.Count);

            var refund =
                result.Single(x => x.AmountDue < 0);

            var newPayment =
                result.Single(x => x.AmountDue > 0);

            ClassicAssert.AreEqual(-125m, refund.AmountDue);
            ClassicAssert.AreEqual(1, refund.DeliveryPeriod);
            ClassicAssert.AreEqual(
                TransactionType.OnProgrammeMathsAndEnglish,
                refund.TransactionType);

            ClassicAssert.AreEqual(100m, newPayment.AmountDue);
            ClassicAssert.AreEqual(1, newPayment.DeliveryPeriod);
            ClassicAssert.AreEqual(
                TransactionType.OnProgrammeMathsAndEnglish,
                newPayment.TransactionType);
        }

        [Test]
        public async Task HandleEarningEvent_ReturnsRefund_WhenHistoricalPaymentIsNoLongerPresent()
        {
            var earningEvent =
                CreateEarningEvent(new List<FunctionalSkillEarning>());

            earningEvent.PriceEpisodes =
                new List<PriceEpisode>();

            SetPaymentHistory(
                new[]
                {
                    CreatePaymentHistory(
                        deliveryPeriod: 1,
                        amount: 100m)
                });

            var result = await processor.HandleEarningEvent(
                earningEvent,
                paymentHistoryCacheMock.Object,
                CancellationToken.None);

            ClassicAssert.AreEqual(1, result.Count);

            var refund =
                result.Single();

            ClassicAssert.AreEqual(-100m, refund.AmountDue);
            ClassicAssert.AreEqual(1, refund.DeliveryPeriod);
            ClassicAssert.AreEqual(
                TransactionType.OnProgrammeMathsAndEnglish,
                refund.TransactionType);
            ClassicAssert.AreEqual(
                "Funding Line",
                refund.LearningAim.FundingLineType);
        }

        [Test]
        public async Task HandleEarningEvent_DoesNotCreatePaymentForFutureDeliveryPeriod()
        {
            var earningPeriod =
                CreateEarningPeriod(2, 100m);

            var earningEvent =
                CreateEarningEvent(
                    new List<FunctionalSkillEarning>
                    {
                        CreateFunctionalSkillEarning(earningPeriod)
                    });

            earningEvent.CollectionPeriod.Period = 1;

            SetPaymentHistory(Array.Empty<PaymentHistoryEntity>());

            var result = await processor.HandleEarningEvent(
                earningEvent,
                paymentHistoryCacheMock.Object,
                CancellationToken.None);

            ClassicAssert.IsEmpty(result);
        }

        [Test]
        public async Task HandleEarningEvent_ReturnsEmpty_WhenEventIsDuplicate()
        {
            var earningEvent =
                CreateEarningEvent(new List<FunctionalSkillEarning>());

            duplicateEarningEventServiceMock
                .Setup(x => x.IsDuplicate(
                    earningEvent,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var result = await processor.HandleEarningEvent(
                earningEvent,
                paymentHistoryCacheMock.Object,
                CancellationToken.None);

            ClassicAssert.IsEmpty(result);

            paymentHistoryCacheMock.Verify(
                x => x.TryGet(
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [TestCase(FunctionalSkillType.OnProgrammeMathsAndEnglish, 125, 2)]
        [TestCase(FunctionalSkillType.BalancingMathsAndEnglish, 250, 3)]
        public async Task HandleEarningEvent_ProcessesMathsAndEnglishEarnings_AsCalculatedRequiredLevyAmount(
            FunctionalSkillType earningType,
            int amount,
            byte deliveryPeriod)
        {
            var earningPeriod = new EarningPeriod
            {
                Period = deliveryPeriod,
                Amount = amount,
                PriceEpisodeIdentifier = "PE-1",
                AccountId = 1,
                ApprenticeshipId = 1,
                ApprenticeshipEmployerType =
                    ApprenticeshipEmployerType.Levy,
                SfaContributionPercentage = 1m
            };

            var earningEvent = new GSLFunctionalSkillEarningsEvent
            {
                Earnings = new List<FunctionalSkillEarning>
                {
                    new FunctionalSkillEarning
                    {
                        Type = earningType,
                        Periods = new List<EarningPeriod>
                        {
                            earningPeriod
                        }.AsReadOnly()
                    }
                }.AsReadOnly(),
                LearningAim = new LearningAim
                {
                    Reference = "ZFS0001",
                    LearningType = LearningType.MathsAndEnglish
                },
                CollectionPeriod = new CollectionPeriod
                {
                    AcademicYear = 2526,
                    Period = 3
                },
                PriceEpisodes = new List<PriceEpisode>
                {
                    new PriceEpisode
                    {
                        Identifier = "PE-1",
                        FundingLineType = "Funding Line"
                    }
                }
            };

            SetPaymentHistory(Array.Empty<PaymentHistoryEntity>());

            var result = await processor.HandleEarningEvent(
                earningEvent,
                paymentHistoryCacheMock.Object,
                CancellationToken.None);

            ClassicAssert.AreEqual(1, result.Count);

            var requiredPayment = result.Single();

            ClassicAssert.IsInstanceOf<CalculatedRequiredLevyAmount>(requiredPayment);

            var levyPayment = (CalculatedRequiredLevyAmount)requiredPayment;

            ClassicAssert.AreEqual(CourseType.FunctionalSkill, levyPayment.CourseType);

            ClassicAssert.AreEqual(LearningType.MathsAndEnglish, levyPayment.LearningAim.LearningType);

            ClassicAssert.AreEqual((int)earningType, (int)levyPayment.TransactionType);

            ClassicAssert.AreEqual((decimal)amount, levyPayment.AmountDue);

            ClassicAssert.AreEqual(earningEvent.CollectionPeriod.AcademicYear, levyPayment.CollectionPeriod.AcademicYear);

            ClassicAssert.AreEqual(deliveryPeriod, levyPayment.DeliveryPeriod);
        }

        [Test]
        public async Task HandleEarningEvent_ReturnsCoInvestedRefund_WhenHistoricalPaymentWasCoInvested()
        {
            var earningEvent =
                CreateEarningEvent(new List<FunctionalSkillEarning>());

            earningEvent.PriceEpisodes =
                new List<PriceEpisode>();

            SetPaymentHistory(
                new[]
                {
            new PaymentHistoryEntity
            {
                LearnAimReference = "ZFS0001",
                CollectionPeriod = new CollectionPeriod
                {
                    AcademicYear = 2526,
                    Period = 1
                },
                DeliveryPeriod = 1,
                TransactionType =
                    (int)FunctionalSkillType.OnProgrammeMathsAndEnglish,
                Amount = 95m,
                FundingSource = FundingSourceType.CoInvestedSfa,
                PriceEpisodeIdentifier = "PE-1",
                LearningAimFundingLineType = "Funding Line"
            },
            new PaymentHistoryEntity
            {
                LearnAimReference = "ZFS0001",
                CollectionPeriod = new CollectionPeriod
                {
                    AcademicYear = 2526,
                    Period = 1
                },
                DeliveryPeriod = 1,
                TransactionType =
                    (int)FunctionalSkillType.OnProgrammeMathsAndEnglish,
                Amount = 5m,
                FundingSource = FundingSourceType.CoInvestedEmployer,
                PriceEpisodeIdentifier = "PE-1",
                LearningAimFundingLineType = "Funding Line"
            }
                });

            var result = await processor.HandleEarningEvent(
                earningEvent,
                paymentHistoryCacheMock.Object,
                CancellationToken.None);

            ClassicAssert.AreEqual(1, result.Count);

            var refund = result.Single();

            ClassicAssert.IsInstanceOf<CalculatedRequiredCoInvestedAmount>(refund);

            ClassicAssert.AreEqual(-100m, refund.AmountDue);
            ClassicAssert.AreEqual(1, refund.DeliveryPeriod);
            ClassicAssert.AreEqual(
                TransactionType.OnProgrammeMathsAndEnglish,
                refund.TransactionType);
        }

        [Test]
        public async Task HandleEarningEvent_IgnoresPaymentsFromDifferentAcademicYear()
        {
            var earningPeriod =
                CreateEarningPeriod(1, 100m);

            var earningEvent =
                CreateEarningEvent(
                    new List<FunctionalSkillEarning>
                    {
                CreateFunctionalSkillEarning(earningPeriod)
                    });

            SetPaymentHistory(
                new[]
                {
            new PaymentHistoryEntity
            {
                LearnAimReference = "ZFS0001",
                CollectionPeriod = new CollectionPeriod
                {
                    AcademicYear = 2425,
                    Period = 1
                },
                DeliveryPeriod = 1,
                TransactionType =
                    (int)FunctionalSkillType.OnProgrammeMathsAndEnglish,
                Amount = 100m,
                PriceEpisodeIdentifier = "PE-1",
                LearningAimFundingLineType = "Funding Line"
            }
                });

            var result = await processor.HandleEarningEvent(
                earningEvent,
                paymentHistoryCacheMock.Object,
                CancellationToken.None);

            ClassicAssert.AreEqual(1, result.Count);

            var payment = result.Single();

            ClassicAssert.AreEqual(100m, payment.AmountDue);
            ClassicAssert.AreEqual(1, payment.DeliveryPeriod);
        }

        [Test]
        public async Task HandleEarningEvent_IgnoresPaymentsForDifferentLearningAim()
        {
            var earningPeriod =
                CreateEarningPeriod(1, 100m);

            var earningEvent =
                CreateEarningEvent(
                    new List<FunctionalSkillEarning>
                    {
                CreateFunctionalSkillEarning(earningPeriod)
                    });

            SetPaymentHistory(
                new[]
                {
            new PaymentHistoryEntity
            {
                LearnAimReference = "DIFFERENT-AIM",
                CollectionPeriod = new CollectionPeriod
                {
                    AcademicYear = 2526,
                    Period = 1
                },
                DeliveryPeriod = 1,
                TransactionType =
                    (int)FunctionalSkillType.OnProgrammeMathsAndEnglish,
                Amount = 100m,
                PriceEpisodeIdentifier = "PE-1",
                LearningAimFundingLineType = "Funding Line"
            }
                });

            var result = await processor.HandleEarningEvent(
                earningEvent,
                paymentHistoryCacheMock.Object,
                CancellationToken.None);

            ClassicAssert.AreEqual(1, result.Count);

            var payment = result.Single();

            ClassicAssert.AreEqual(100m, payment.AmountDue);
            ClassicAssert.AreEqual(1, payment.DeliveryPeriod);
        }

        [Test]
        public void HandleEarningEvent_WrapsUnhandledExceptions()
        {
            var earningEvent =
                CreateEarningEvent(
                    new List<FunctionalSkillEarning>());

            duplicateEarningEventServiceMock
                .Setup(x => x.IsDuplicate(
                    It.IsAny<GSLFunctionalSkillEarningsEvent>(),
                    It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("Boom"));

            var exception =
                Assert.ThrowsAsync<Exception>(
                    async () => await processor.HandleEarningEvent(
                        earningEvent,
                        paymentHistoryCacheMock.Object,
                        CancellationToken.None));

            ClassicAssert.IsNotNull(exception);

            StringAssert.Contains(
                "Error processing GSLFunctionalSkillEarningsEvent",
                exception.Message);

            ClassicAssert.IsInstanceOf<InvalidOperationException>(
                exception.InnerException);
        }

        private void SetPaymentHistory(
            PaymentHistoryEntity[] paymentHistory)
        {
            paymentHistoryCacheMock
                .Setup(x => x.TryGet(
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(
                    new ConditionalValue<PaymentHistoryEntity[]>(
                        true,
                        paymentHistory));
        }

        private static FunctionalSkillEarning CreateFunctionalSkillEarning(
            EarningPeriod earningPeriod)
        {
            return new FunctionalSkillEarning
            {
                Type = FunctionalSkillType.OnProgrammeMathsAndEnglish,
                Periods = new List<EarningPeriod>
                {
                    earningPeriod
                }.AsReadOnly()
            };
        }

        private static EarningPeriod CreateEarningPeriod(
            byte period,
            decimal amount)
        {
            return new EarningPeriod
            {
                Period = period,
                Amount = amount,
                PriceEpisodeIdentifier = "PE-1",
                AccountId = 1,
                ApprenticeshipId = 1,
                SfaContributionPercentage = 1m,
                ApprenticeshipEmployerType =
                    ApprenticeshipEmployerType.Levy
            };
        }

        private static PaymentHistoryEntity CreatePaymentHistory(
            byte deliveryPeriod,
            decimal amount)
        {
            return new PaymentHistoryEntity
            {
                LearnAimReference = "ZFS0001",
                CollectionPeriod = new CollectionPeriod
                {
                    AcademicYear = 2526,
                    Period = 1
                },
                DeliveryPeriod = deliveryPeriod,
                TransactionType =
                    (int)FunctionalSkillType.OnProgrammeMathsAndEnglish,
                Amount = amount,
                PriceEpisodeIdentifier = "PE-1",
                LearningAimFundingLineType = "Funding Line"
            };
        }

        private static GSLFunctionalSkillEarningsEvent CreateEarningEvent(
            List<FunctionalSkillEarning> earnings)
        {
            return new GSLFunctionalSkillEarningsEvent
            {
                Earnings = earnings.AsReadOnly(),
                LearningAim = new LearningAim
                {
                    Reference = "ZFS0001",
                    LearningType =
                        LearningType.ApprenticeshipUnit
                },
                CollectionPeriod = new CollectionPeriod
                {
                    AcademicYear = 2526,
                    Period = 1
                },
                PriceEpisodes = new List<PriceEpisode>
                {
                    new PriceEpisode
                    {
                        Identifier = "PE-1",
                        FundingLineType = "Funding Line"
                    }
                }
            };
        }
    }
}