using AutoMapper;
using Moq;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using SFA.DAS.Payments.Application.Repositories;
using SFA.DAS.Payments.EarningEvents.Messages;
using SFA.DAS.Payments.EarningEvents.Messages.Events;
using SFA.DAS.Payments.Model.Core;
using SFA.DAS.Payments.RequiredPayments.Application.Processors;
using SFA.DAS.Payments.RequiredPayments.Domain;
using SFA.DAS.Payments.RequiredPayments.Domain.Entities;
using SFA.DAS.Payments.RequiredPayments.Messages.Events;
using SFA.DAS.Payments.RequiredPayments.Model.Entities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.Payments.RequiredPayments.Application.UnitTests.Application.Processors
{
    [TestFixture]
    public class ShortCoursesEarningEventProcessorTests
    {
        private Mock<INegativeEarningService> negativeEarningServiceMock;
        private Mock<IMapper> mapperMock;
        private Mock<IDataCache<PaymentHistoryEntity[]>> paymentHistoryCacheMock;
        private ShortCoursesEarningEventProcessor processor;

        [SetUp]
        public void SetUp()
        {
            negativeEarningServiceMock = new Mock<INegativeEarningService>();
            mapperMock = new Mock<IMapper>();
            paymentHistoryCacheMock = new Mock<IDataCache<PaymentHistoryEntity[]>>();
            processor = new ShortCoursesEarningEventProcessor(negativeEarningServiceMock.Object, mapperMock.Object);
        }

        [Test]
        public async Task HandleEarningEvent_Returns_RequiredPayments_For_ValidEarnings()
        {
            // Arrange
            var earningEvent = new GSLShortCourseEarningsEvent
            {
                Earnings = new List<ShortCourseEarning>
                {
                    new ShortCourseEarning
                    {
                        Periods = new List<EarningPeriod>
                        {
                            new EarningPeriod
                            {
                                Period = 1,
                                Amount = 100m,
                                PriceEpisodeIdentifier = "PE-1",
                                AccountId = 1,
                                TransferSenderAccountId = null,
                                ApprenticeshipId  = 1
                            }
                        },
                    }
                },
                LearningAim = new LearningAim { Reference = "ZPROG001" },
                CollectionPeriod = new CollectionPeriod { AcademicYear = 2324 },
                PriceEpisodes = new List<PriceEpisode>
                {
                    new PriceEpisode
                    {
                        Identifier = "PE-2",
                        TotalNegotiatedPrice1 = 15000m,
                        TotalNegotiatedPrice2 = 2000m,
                        TotalNegotiatedPrice3 = null,
                        TotalNegotiatedPrice4 = null,
                        AgreedPrice = 17000m,
                        CourseStartDate = new DateTime(2024, 9, 1),
                        StartDate = new DateTime(2024, 9, 1),
                        EffectiveTotalNegotiatedPriceStartDate = new DateTime(2024, 9, 1),
                        PlannedEndDate = new DateTime(2026, 8, 31),
                        ActualEndDate = null,
                        NumberOfInstalments = 24,
                        InstalmentAmount = 625m,
                        CompletionAmount = 2000m,
                        Completed = false,
                        EmployerContribution = 500m,
                        CompletionHoldBackExemptionCode = null,
                        FundingLineType = "Apprenticeship Levy",
                        LearningAimSequenceNumber = 1
                    }
                },
            };

            var paymentHistoryEntities = new PaymentHistoryEntity[]
            {
                new PaymentHistoryEntity
                {
                    LearnAimReference = "ZPROG001",
                    CollectionPeriod = new CollectionPeriod { AcademicYear = 2324, Period = 1 },
                    PriceEpisodeIdentifier = "PE-1"
                }
            };

            paymentHistoryCacheMock
                .Setup(x => x.TryGet(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ConditionalValue<PaymentHistoryEntity[]>(true, paymentHistoryEntities));

            mapperMock
                .Setup(x => x.Map<PaymentHistoryEntity, Payment>(It.IsAny<PaymentHistoryEntity>()))
                .Returns((PaymentHistoryEntity phe) => new Payment
                {
                    CollectionPeriod = phe.CollectionPeriod,
                    PriceEpisodeIdentifier = phe.PriceEpisodeIdentifier
                });

            negativeEarningServiceMock
                .Setup(x => x.ProcessNegativeEarning(It.IsAny<decimal>(), It.IsAny<List<Payment>>(), It.IsAny<byte>(), It.IsAny<string>()))
                .Returns(new List<RequiredPayment>());

            // Act
            var result = await processor.HandleEarningEvent(earningEvent, paymentHistoryCacheMock.Object, CancellationToken.None);

            // Assert
            ClassicAssert.IsInstanceOf<ReadOnlyCollection<PeriodisedRequiredPaymentEvent>>(result);
        }

        [Test]
        public async Task HandleEarningEvent_Refunds_When_Payment_Not_In_Earnings()
        {
            // Arrange
            var earningEvent = new GSLShortCourseEarningsEvent
            {
                Earnings = new List<ShortCourseEarning>
                {
                    new() {
                        Periods = new List<EarningPeriod>
                        {
                            new EarningPeriod
                            {
                                Period = 1,
                                Amount = 100m,
                                PriceEpisodeIdentifier = "PE-1",
                                AccountId = 1,
                                TransferSenderAccountId = null,
                                ApprenticeshipId  = 1
                            }
                        },
                    }
                },
                LearningAim = new LearningAim { Reference = "ZPROG001" },
                CollectionPeriod = new CollectionPeriod { AcademicYear = 2324 },
                PriceEpisodes = new List<PriceEpisode>
                {
                    new()
                    {
                        Identifier = "PE-1",
                        TotalNegotiatedPrice1 = 15000m,
                        TotalNegotiatedPrice2 = 2000m,
                        TotalNegotiatedPrice3 = null,
                        TotalNegotiatedPrice4 = null,
                        AgreedPrice = 17000m,
                        CourseStartDate = new DateTime(2024, 9, 1),
                        StartDate = new DateTime(2024, 9, 1),
                        EffectiveTotalNegotiatedPriceStartDate = new DateTime(2024, 9, 1),
                        PlannedEndDate = new DateTime(2026, 8, 31),
                        ActualEndDate = null,
                        NumberOfInstalments = 24,
                        InstalmentAmount = 625m,
                        CompletionAmount = 2000m,
                        Completed = false,
                        EmployerContribution = 500m,
                        CompletionHoldBackExemptionCode = null,
                        FundingLineType = "Apprenticeship Levy",
                        LearningAimSequenceNumber = 1
                    }
                },
            };

            var paymentHistoryEntities = new PaymentHistoryEntity[]
            {
                new PaymentHistoryEntity
                {
                    LearnAimReference = "ZPROG001",
                    CollectionPeriod = new CollectionPeriod { AcademicYear = 2324, Period = 2 }, // Not matching period
                    PriceEpisodeIdentifier = "PE-1"
                }
            };

            paymentHistoryCacheMock
                .Setup(x => x.TryGet(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ConditionalValue<PaymentHistoryEntity[]>(true, paymentHistoryEntities));

            mapperMock
                .Setup(x => x.Map<PaymentHistoryEntity, Payment>(It.IsAny<PaymentHistoryEntity>()))
                .Returns((PaymentHistoryEntity phe) => new Payment
                {
                    CollectionPeriod = phe.CollectionPeriod,
                    PriceEpisodeIdentifier = phe.PriceEpisodeIdentifier,
                    Amount = 100m
                });

            negativeEarningServiceMock
                .Setup(x => x.ProcessNegativeEarning(It.IsAny<decimal>(), It.IsAny<List<Payment>>(), It.IsAny<int>(), It.IsAny<string>()))
                .Returns(new List<RequiredPayment>
                {
                    new RequiredPayment
                    {
                        Amount = -100m,
                        EarningType = EarningType.Levy,
                        PriceEpisodeIdentifier = "PE-1",
                        AccountId = 1,
                        TransferSenderAccountId = null
                    }
                });

            // Act
            var result = await processor.HandleEarningEvent(earningEvent, paymentHistoryCacheMock.Object, CancellationToken.None);

            // Assert
            ClassicAssert.IsTrue(result.Count > 0);
            var requiredPaymentEvent = result[0];
            ClassicAssert.IsTrue(requiredPaymentEvent.CompletionAmount < 0);

            // Check mapping from GSLShortCourseEarningsEvent to RequiredPaymentEvent
            ClassicAssert.AreEqual("PE-1", requiredPaymentEvent.PriceEpisodeIdentifier);
            ClassicAssert.AreEqual(1, requiredPaymentEvent.AccountId);
            ClassicAssert.IsNull(requiredPaymentEvent.TransferSenderAccountId);
        }
        [Test]
        public async Task HandleEarningEvent_Returns_Empty_When_NoEarnings()
        {
            var earningEvent = new GSLShortCourseEarningsEvent
            {
                Earnings = new List<ShortCourseEarning>(),
                LearningAim = new LearningAim { Reference = "ZPROG001" },
                CollectionPeriod = new CollectionPeriod { AcademicYear = 2324 },
                PriceEpisodes = new List<PriceEpisode>()
            };

            paymentHistoryCacheMock
                .Setup(x => x.TryGet(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ConditionalValue<PaymentHistoryEntity[]>(false, null));

            // Act
            var result = await processor.HandleEarningEvent(earningEvent, paymentHistoryCacheMock.Object, CancellationToken.None);

            // Assert
            ClassicAssert.IsNotNull(result);
            ClassicAssert.AreEqual(0, result.Count);
        }

        [Test]
        public async Task HandleEarningEvent_Returns_Empty_When_NoPaymentHistory()
        {
            var earningEvent = new GSLShortCourseEarningsEvent
            {
                Earnings = new List<ShortCourseEarning>
                {
                    new ShortCourseEarning
                    {
                        Periods = new List<EarningPeriod>
                        {
                            new EarningPeriod
                            {
                                Period = 1,
                                Amount = 100m,
                                PriceEpisodeIdentifier = "PE-1",
                                AccountId = 1,
                                TransferSenderAccountId = null,
                                ApprenticeshipId  = 1
                            }
                        },
                    }
                },
                LearningAim = new LearningAim { Reference = "ZPROG001" },
                CollectionPeriod = new CollectionPeriod { AcademicYear = 2324 },
                PriceEpisodes = new List<PriceEpisode>()
            };

            paymentHistoryCacheMock
                .Setup(x => x.TryGet(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ConditionalValue<PaymentHistoryEntity[]>(false, null));

            // Act
            var result = await processor.HandleEarningEvent(earningEvent, paymentHistoryCacheMock.Object, CancellationToken.None);

            // Assert
            ClassicAssert.IsNotNull(result);
            ClassicAssert.AreEqual(0, result.Count);
        }

        [Test]
        public async Task HandleEarningEvent_Refunds_When_Amount_Differs()
        {
            var earningEvent = new GSLShortCourseEarningsEvent
            {
                Earnings = new List<ShortCourseEarning>
                {
                    new ShortCourseEarning
                    {
                        Periods = new List<EarningPeriod>
                        {
                            new EarningPeriod
                            {
                                Period = 1,
                                Amount = 200m,
                                PriceEpisodeIdentifier = "PE-1",
                                AccountId = 1,
                                TransferSenderAccountId = null,
                                ApprenticeshipId  = 1
                            }
                        },
                    }
                },
                LearningAim = new LearningAim { Reference = "ZPROG001" },
                CollectionPeriod = new CollectionPeriod { AcademicYear = 2324 },
                PriceEpisodes = new List<PriceEpisode>
                {
                    new PriceEpisode
                    {
                        Identifier = "PE-1",
                        TotalNegotiatedPrice1 = 200m
                    }
                }
            };

            var paymentHistoryEntities = new PaymentHistoryEntity[]
            {
                new PaymentHistoryEntity
                {
                    LearnAimReference = "ZPROG001",
                    CollectionPeriod = new CollectionPeriod { AcademicYear = 2324, Period = 1 },
                    PriceEpisodeIdentifier = "PE-1"
                }
            };

            paymentHistoryCacheMock
                .Setup(x => x.TryGet(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ConditionalValue<PaymentHistoryEntity[]>(true, paymentHistoryEntities));

            mapperMock
                .Setup(x => x.Map<PaymentHistoryEntity, Payment>(It.IsAny<PaymentHistoryEntity>()))
                .Returns((PaymentHistoryEntity phe) => new Payment
                {
                    CollectionPeriod = phe.CollectionPeriod,
                    PriceEpisodeIdentifier = phe.PriceEpisodeIdentifier,
                    Amount = 100m // Simulate payment history with different amount
                });

            negativeEarningServiceMock
                .Setup(x => x.ProcessNegativeEarning(It.IsAny<decimal>(), It.IsAny<List<Payment>>(), It.IsAny<int>(), It.IsAny<string>()))
                .Returns(new List<RequiredPayment>
                {
                    new RequiredPayment
                    {
                        Amount = -100m,
                        EarningType = EarningType.Levy,
                        PriceEpisodeIdentifier = "PE-1",
                        AccountId = 1,
                        TransferSenderAccountId = null
                    }
                });

            // Act
            var result = await processor.HandleEarningEvent(earningEvent, paymentHistoryCacheMock.Object, CancellationToken.None);

            // Assert
            ClassicAssert.IsTrue(result.Count >= 2, "Should have both a refund and a new payment.");

            // Check for refund
            var refund = result[0];
            ClassicAssert.IsTrue(refund.CompletionAmount < 0, "First event should be a refund.");
            ClassicAssert.AreEqual("PE-1", refund.PriceEpisodeIdentifier);

            // Check for new payment
            var newPayment = result[1];
            ClassicAssert.IsTrue(newPayment.CompletionAmount > 0, "Second event should be a new payment.");
            ClassicAssert.AreEqual("PE-1", newPayment.PriceEpisodeIdentifier);
            ClassicAssert.AreEqual(200m, newPayment.CompletionAmount);
        }

        [Test]
        public void GenerateRefund_Calls_NegativeEarningService()
        {
            // Arrange
            var period = new EarningPeriod
            {
                Period = 1,
                Amount = 100m,
                PriceEpisodeIdentifier = "PE-1",
                AccountId = 1,
                TransferSenderAccountId = null,
                ApprenticeshipId = 1
            };
            var academicYearPayments = new List<Payment>
            {
                new Payment
                {
                    CollectionPeriod = new CollectionPeriod { AcademicYear = 2324, Period = 1 },
                    PriceEpisodeIdentifier = "PE-1"
                }
            };

            negativeEarningServiceMock
                .Setup(x => x.ProcessNegativeEarning(period.Amount, academicYearPayments, period.Period, period.PriceEpisodeIdentifier))
                .Returns(new List<RequiredPayment> { new RequiredPayment { Amount = -100m } })
                .Verifiable();

            // Use reflection to call private method
            var result = (List<RequiredPayment>)processor.GetType()
                .GetMethod("GenerateRefund", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .Invoke(processor, new object[] { period, academicYearPayments });

            // Assert
            negativeEarningServiceMock.Verify();
            ClassicAssert.IsTrue(result.Count > 0);
            ClassicAssert.IsTrue(result[0].Amount < 0);
        }


    }
}
