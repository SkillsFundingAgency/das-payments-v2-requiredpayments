using Moq;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using SFA.DAS.Payments.Application.Messaging;
using SFA.DAS.Payments.Application.Repositories;
using SFA.DAS.Payments.EarningEvents.Messages;
using SFA.DAS.Payments.EarningEvents.Messages.Events;
using SFA.DAS.Payments.Model.Core;
using SFA.DAS.Payments.Model.Core.Entities;
using SFA.DAS.Payments.RequiredPayments.Application.Processors;
using SFA.DAS.Payments.RequiredPayments.Messages.Events;
using SFA.DAS.Payments.RequiredPayments.Model.Entities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.Payments.RequiredPayments.Application.UnitTests.Application.Processors
{
    internal class ShortCoursesTestValues
    {
        public new List<ShortCourseEarning> ShortCourseEarnings { get; set; }
        public short Year { get; set; }
        public byte CollectionPeriod { get; set; }
        public decimal Amount { get; set; }
        public DateTime PlannedEndDate { get; set; }
    }
    [TestFixture]
    public class ShortCoursesEarningEventProcessorTests
    {
        private Mock<IDataCache<PaymentHistoryEntity[]>> paymentHistoryCacheMock;
        private Mock<IDuplicateEarningEventService> duplicateEarningEventServiceMock;
        private ShortCoursesEarningEventProcessor processor;

        [SetUp]
        public void SetUp()
        {
            paymentHistoryCacheMock = new Mock<IDataCache<PaymentHistoryEntity[]>>();
            duplicateEarningEventServiceMock = new Mock<IDuplicateEarningEventService>();
            processor = new ShortCoursesEarningEventProcessor(duplicateEarningEventServiceMock.Object);
        }
        [TearDown]
        public void TearDown()
        {
            paymentHistoryCacheMock = new Mock<IDataCache<PaymentHistoryEntity[]>>();
            duplicateEarningEventServiceMock = new Mock<IDuplicateEarningEventService>();
            processor = new ShortCoursesEarningEventProcessor(duplicateEarningEventServiceMock.Object);
        }

        [Test]
        public async Task HandleEarningEvent_Returns_RequiredPayments_For_ValidEarnings()
        {
            // Arrange
            var period = new EarningPeriod
            {
                Period = 1,
                Amount = 100m,
                PriceEpisodeIdentifier = "PE-1",
                AccountId = 1,
                TransferSenderAccountId = null,
                ApprenticeshipId = 1,
                SfaContributionPercentage = 0m
            };

            var earningEvent = new GSLShortCourseEarningsEvent
            {
                Earnings = new List<ShortCourseEarning>
                {
                    new ShortCourseEarning
                    {
                        Periods = new List<EarningPeriod>
                        {
                            period
                        },
                    }
                },
                LearningAim = new LearningAim { Reference = "ZSC0001" },
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
                    LearnAimReference = "ZSC0001",
                    CollectionPeriod = new CollectionPeriod { AcademicYear = 2324, Period = 1 },
                    PriceEpisodeIdentifier = "PE-1"
                }
            };

            paymentHistoryCacheMock
                .Setup(x => x.TryGet(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ConditionalValue<PaymentHistoryEntity[]>(true, paymentHistoryEntities));

            // Act
            var result = await processor.HandleEarningEvent(earningEvent, paymentHistoryCacheMock.Object, CancellationToken.None);

            // Assert
            ClassicAssert.IsInstanceOf<ReadOnlyCollection<PeriodisedRequiredPaymentEvent>>(result);
        }

        [Test]
        public async Task HandleEarningEvent_Refunds_When_Payment_Not_In_Earnings()
        {
            // Arrange
            var period = new EarningPeriod
            {
                Period = 1,
                Amount = 100m,
                PriceEpisodeIdentifier = "PE-1",
                AccountId = 1,
                TransferSenderAccountId = null,
                ApprenticeshipId = 1,
                SfaContributionPercentage = 0m,
            };

            var earningEvent = new GSLShortCourseEarningsEvent
            {
                Earnings = new List<ShortCourseEarning>
                {
                    new() {
                        Periods = new List<EarningPeriod>
                        {
                            period
                        },
                        Type = ShortCourseEarningType.Completion
                    }
                },
                LearningAim = new LearningAim { Reference = "ZSC0001", LearningType = LearningType.ApprenticeshipUnit },
                CollectionPeriod = new CollectionPeriod { AcademicYear = 2324, Period = 1},
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
                IlrSubmissionDateTime = DateTime.Now
            };

            var paymentHistoryEntities = new PaymentHistoryEntity[]
            {
                new PaymentHistoryEntity
                {
                    LearnAimReference = "ZSC0001",
                    CollectionPeriod = new CollectionPeriod { AcademicYear = 2324, Period = 1 }, 
                    PriceEpisodeIdentifier = "PE-1",
                    CompletionAmount = 101m,
                    DeliveryPeriod = 1,
                    TransactionType = 2,
                    Amount = 101m
                }
            };

            paymentHistoryCacheMock
                .Setup(x => x.TryGet(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ConditionalValue<PaymentHistoryEntity[]>(true, paymentHistoryEntities));

            // Act
            var result = await processor.HandleEarningEvent(earningEvent, paymentHistoryCacheMock.Object, CancellationToken.None);

            // Assert
            ClassicAssert.IsTrue(result.Count > 0);

            // Check for refund
            var refund = result[0];
            ClassicAssert.IsTrue(refund.CompletionAmount == -101);

            var requiredPaymentEvent = result[1];

            AssertMappingFromGslShortCourseEarningsEventToRequiredPaymentEvent(earningEvent, earningEvent.PriceEpisodes[0], period, requiredPaymentEvent);
        }
        [Test]
        public async Task HandleEarningEvent_Returns_Empty_When_NoEarnings()
        {
            var earningEvent = new GSLShortCourseEarningsEvent
            {
                Earnings = new List<ShortCourseEarning>(),
                LearningAim = new LearningAim { Reference = "ZSC0001" },
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
        public async Task HandleEarningEvent_Creates_Payment_When_NoPaymentHistory()
        {
            var period = new EarningPeriod
            {
                Period = 1,
                Amount = 100m,
                PriceEpisodeIdentifier = "PE-1",
                AccountId = 1,
                TransferSenderAccountId = null,
                ApprenticeshipId = 1,
                SfaContributionPercentage = 0m,
            };

            var earningEvent = new GSLShortCourseEarningsEvent
            {
                Earnings = new List<ShortCourseEarning>
                {
                    new ShortCourseEarning
                    {
                        Periods = new List<EarningPeriod>
                        {
                            period
                        },
                        Type = ShortCourseEarningType.Completion,
                    }
                },
                LearningAim = new LearningAim { Reference = "ZSC0001" },
                CollectionPeriod = new CollectionPeriod { AcademicYear = 2324, Period = 1 },
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

            paymentHistoryCacheMock
                .Setup(x => x.TryGet(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ConditionalValue<PaymentHistoryEntity[]>(false, null));

            // Act
            var result = await processor.HandleEarningEvent(earningEvent, paymentHistoryCacheMock.Object, CancellationToken.None);

            // Assert
            ClassicAssert.IsNotNull(result);
            ClassicAssert.AreEqual(1, result.Count);
            var newPayment = result[0];
            AssertMappingFromGslShortCourseEarningsEventToRequiredPaymentEvent(earningEvent, earningEvent.PriceEpisodes[0], period, newPayment);
        }

        [Test]
        public async Task HandleEarningEvent_Refunds_When_Amount_Differs()
        {
            var period = new EarningPeriod
            {
                Period = 1,
                Amount = 200m,
                PriceEpisodeIdentifier = "PE-1",
                AccountId = 1,
                TransferSenderAccountId = null,
                ApprenticeshipId = 1,
                SfaContributionPercentage = 0m,
            };

            var earningEvent = new GSLShortCourseEarningsEvent
            {
                Earnings = new List<ShortCourseEarning>
                {
                    new ShortCourseEarning
                    {
                        Periods = new List<EarningPeriod>
                        {
                            period
                        },
                        Type = ShortCourseEarningType.Completion
                    }
                },
                LearningAim = new LearningAim { Reference = "ZSC0001", LearningType = LearningType.ApprenticeshipUnit },
                CollectionPeriod = new CollectionPeriod { AcademicYear = 2526, Period = 1},
                PriceEpisodes = new List<PriceEpisode>
                {
                    new PriceEpisode
                    {
                        Identifier = "PE-1",
                        TotalNegotiatedPrice1 = 200m,
                    }
                }
            };

            var paymentHistoryEntities = new PaymentHistoryEntity[]
            {
                new PaymentHistoryEntity
                {
                    LearnAimReference = "ZSC0001",
                    CollectionPeriod = new CollectionPeriod { AcademicYear = 2526, Period = 1 },
                    PriceEpisodeIdentifier = "PE-1",
                    TransactionType = 2,
                    DeliveryPeriod = 1,
                    Amount = 100m,
                }
            };

            paymentHistoryCacheMock
                .Setup(x => x.TryGet(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ConditionalValue<PaymentHistoryEntity[]>(true, paymentHistoryEntities));

            // Act
            var result = await processor.HandleEarningEvent(earningEvent, paymentHistoryCacheMock.Object, CancellationToken.None);

            // Assert
            ClassicAssert.IsTrue(result.Count >= 2, "Should have both a refund and a new payment.");

            // Check for refund
            var refund = result[0];
            ClassicAssert.IsTrue(refund.CompletionAmount == -100, "First event should be a refund.");
            ClassicAssert.AreEqual("PE-1", refund.PriceEpisodeIdentifier);

            // Check for new payment
            var newPayment = result[1];
            ClassicAssert.IsTrue(newPayment.CompletionAmount > 0, "Second event should be a new payment.");

            AssertMappingFromGslShortCourseEarningsEventToRequiredPaymentEvent(earningEvent, earningEvent.PriceEpisodes[0], period, newPayment);
        }

        [Test]
        public async Task Initial_Payment_For_ShortCourse_Training_Delivery()
        {
            // Arrange
            var amount = 300m;
            byte collectionPeriod = 1;
            var period = GenerateTestEarningPeriod(collectionPeriod, amount);
            var shortCourses = new List<ShortCourseEarning>
            {
                new ShortCourseEarning
                {
                    Type = ShortCourseEarningType.Milestone1,
                    Periods = new List<EarningPeriod>
                    {
                        period
                    },
                }
            };
            var earningEvent = GenerateTestShortCourseEarningsEvent(
                new ShortCoursesTestValues
                {
                    ShortCourseEarnings = shortCourses,
                    Year = 2526,
                    CollectionPeriod = collectionPeriod,
                    Amount = 1000m,
                    PlannedEndDate = new DateTime(2026, 9, 30)

                });

            var paymentHistoryEntities = Array.Empty<PaymentHistoryEntity>();

            paymentHistoryCacheMock
                .Setup(x => x.TryGet(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ConditionalValue<PaymentHistoryEntity[]>(true, paymentHistoryEntities));

            // Act
            var result = await processor.HandleEarningEvent(earningEvent, paymentHistoryCacheMock.Object,
                CancellationToken.None);

            // Assert
            ClassicAssert.IsTrue(result.Count == 1, "Should have a new payment.");

            // Check for new payment
            var newPayment = result[0];
            ValidateRequiredPaymentEvents(newPayment, amount, 1, TransactionType.Milestone1, 2526, 1);
            AssertMappingFromGslShortCourseEarningsEventToRequiredPaymentEvent(earningEvent, earningEvent.PriceEpisodes[0], period, newPayment);
        }

        [Test]
        public async Task Short_Courses_Delivery_Recorded_Completion()
        {
            // Arrange
            var deliveryPeriod1 = GenerateTestEarningPeriod(1, 300m);
            var deliveryPeriod2 = GenerateTestEarningPeriod(2, 700m);

            var shortCourses = new List<ShortCourseEarning>
            {
                new ()
                {
                    Type = ShortCourseEarningType.Milestone1,
                    Periods = new List<EarningPeriod>
                    {
                        deliveryPeriod1
                    },
                },
                new()
                {
                    Type = ShortCourseEarningType.Completion,
                    Periods = new List<EarningPeriod>
                    {
                        deliveryPeriod2
                    },
                }
            };

            var earningEvent = GenerateTestShortCourseEarningsEvent(
                new ShortCoursesTestValues
                {
                    ShortCourseEarnings = shortCourses,
                    Year = 2526,
                    CollectionPeriod = 2,
                    Amount = 1000m,
                    PlannedEndDate = new DateTime(2026, 9, 30)

                });

            var paymentHistoryEntities = new PaymentHistoryEntity[]
            {
                new PaymentHistoryEntity
                {
                    LearnAimReference = "ZSC0001",
                    CollectionPeriod = new CollectionPeriod { AcademicYear = 2526, Period = 1 },
                    PriceEpisodeIdentifier = "PE-1",
                    TransactionType = 17,
                    DeliveryPeriod = 1,
                    Amount = 300m,
                }
            };

            paymentHistoryCacheMock
                .Setup(x => x.TryGet(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ConditionalValue<PaymentHistoryEntity[]>(true, paymentHistoryEntities));

            // Act
            var result = await processor.HandleEarningEvent(earningEvent, paymentHistoryCacheMock.Object,
                CancellationToken.None);

            // Assert
            ClassicAssert.IsTrue(result.Count == 1, "Should have a new payment.");

            // Check for new payment
            var newPayment = result[0];
            ValidateRequiredPaymentEvents(newPayment, 700m, 2, TransactionType.Completion, 2526, 2);

            AssertMappingFromGslShortCourseEarningsEventToRequiredPaymentEvent(earningEvent, earningEvent.PriceEpisodes[0], deliveryPeriod2, newPayment);
        }

        [Test]
        public async Task Short_Courses_That_Starts_And_Completes_In_Same_Period()
        {
            // Arrange
            var deliveryPeriod1 = GenerateTestEarningPeriod(1, 300m);
            var deliveryPeriod2 = GenerateTestEarningPeriod(1, 700m);
            var shortCourses = new List<ShortCourseEarning>
            {
                new ()
                {
                    Type = ShortCourseEarningType.Milestone1,
                    Periods = new List<EarningPeriod>
                    {
                        deliveryPeriod1
                    },
                },
                new()
                {
                    Type = ShortCourseEarningType.Completion,
                    Periods = new List<EarningPeriod>
                    {
                        deliveryPeriod2
                    },
                }
            };

            var earningEvent = GenerateTestShortCourseEarningsEvent(
                new ShortCoursesTestValues
                {
                    ShortCourseEarnings = shortCourses,
                    Year = 2526,
                    CollectionPeriod = 1,
                    Amount = 1000m,
                    PlannedEndDate = new DateTime(2026, 8, 31)

                });

            var paymentHistoryEntities = Array.Empty<PaymentHistoryEntity>();

            paymentHistoryCacheMock
                .Setup(x => x.TryGet(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ConditionalValue<PaymentHistoryEntity[]>(true, paymentHistoryEntities));

            // Act
            var result = await processor.HandleEarningEvent(earningEvent, paymentHistoryCacheMock.Object,
                CancellationToken.None);

            // Assert
            ClassicAssert.IsTrue(result.Count == 2, "Should have 2 new payments.");

            // Check for new milestone 1 payment
            var newMilestone1 = result[0];
            ValidateRequiredPaymentEvents(newMilestone1, 300m, 1, TransactionType.Milestone1, 2526, 1);


            // Check for new completion payment
            var newCompletion = result[1];
            ValidateRequiredPaymentEvents(newCompletion, 700m, 1, TransactionType.Completion, 2526, 1);

            AssertMappingFromGslShortCourseEarningsEventToRequiredPaymentEvent(earningEvent, earningEvent.PriceEpisodes[0], deliveryPeriod1, newMilestone1);
            AssertMappingFromGslShortCourseEarningsEventToRequiredPaymentEvent(earningEvent, earningEvent.PriceEpisodes[0], deliveryPeriod2, newCompletion);
        }

        [Test]
        public async Task Provider_Retrospectively_Changes_Start_Completion_Date()
        {
            // Arrange
            var deliveryPeriod1 = GenerateTestEarningPeriod(2, 300m);
            var deliveryPeriod2 = GenerateTestEarningPeriod(2, 700m);
            var shortCourses = new List<ShortCourseEarning>
            {
                new ()
                {
                    Type = ShortCourseEarningType.Milestone1,
                    Periods = new List<EarningPeriod>
                    {
                        deliveryPeriod1
                    },
                },
                new()
                {
                    Type = ShortCourseEarningType.Completion,
                    Periods = new List<EarningPeriod>
                    {
                        deliveryPeriod2
                    },
                }
            };

            var earningEvent = GenerateTestShortCourseEarningsEvent(
                new ShortCoursesTestValues
                {
                    ShortCourseEarnings = shortCourses,
                    Year = 2526,
                    CollectionPeriod = 2,
                    Amount = 1000m,
                    PlannedEndDate = new DateTime(2026, 8, 31)

                });

            var paymentHistoryEntities = new PaymentHistoryEntity[]
            {
                new PaymentHistoryEntity
                {
                    LearnAimReference = "ZSC0001",
                    CollectionPeriod = new CollectionPeriod { AcademicYear = 2526, Period = 1 },
                    PriceEpisodeIdentifier = "PE-1",
                    TransactionType = 17,
                    DeliveryPeriod = 1,
                    Amount = 300m,
                    AccountId = 1,
                    ApprenticeshipId = 1,
                    PlannedEndDate = new DateTime(2026,08,31),
                },
                new PaymentHistoryEntity
                {
                    LearnAimReference = "ZSC0001",
                    CollectionPeriod = new CollectionPeriod { AcademicYear = 2526, Period = 1 },
                    PriceEpisodeIdentifier = "PE-1",
                    TransactionType = 2,
                    DeliveryPeriod = 1,
                    Amount = 700m,
                    AccountId = 1,
                    ApprenticeshipId = 1,
                    PlannedEndDate = new DateTime(2026,08,31),
                }
            };

            paymentHistoryCacheMock
                .Setup(x => x.TryGet(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ConditionalValue<PaymentHistoryEntity[]>(true, paymentHistoryEntities));

            // Act
            var result = await processor.HandleEarningEvent(earningEvent, paymentHistoryCacheMock.Object,
                CancellationToken.None);

            // Assert
            ClassicAssert.IsTrue(result.Count == 4);

            // Check for milestone 1 refund
            var refundMilestone1 = result[0];
            ValidateRequiredPaymentEvents(refundMilestone1, -300m, 1, TransactionType.Milestone1, 2526, 2);

            // Check for new milestone 1 payment
            var newMilestone1 = result[1];
            ValidateRequiredPaymentEvents(newMilestone1, 300m, 2, TransactionType.Milestone1, 2526, 2);

            // Check for new completion payment
            var refundCompletion = result[2];
            ValidateRequiredPaymentEvents(refundCompletion, -700m, 1, TransactionType.Completion, 2526, 2);

            // Check for new completion payment
            var newCompletion = result[3];
            ValidateRequiredPaymentEvents(newCompletion, 700m, 2, TransactionType.Completion, 2526, 2);

            AssertMappingFromGslShortCourseEarningsEventToRequiredPaymentEvent(earningEvent, earningEvent.PriceEpisodes[0], deliveryPeriod1, refundMilestone1, paymentHistoryEntities[0], true);
            AssertMappingFromGslShortCourseEarningsEventToRequiredPaymentEvent(earningEvent, earningEvent.PriceEpisodes[0], deliveryPeriod1, newMilestone1);
            AssertMappingFromGslShortCourseEarningsEventToRequiredPaymentEvent(earningEvent, earningEvent.PriceEpisodes[0], deliveryPeriod2, refundCompletion, paymentHistoryEntities[1], true);
            AssertMappingFromGslShortCourseEarningsEventToRequiredPaymentEvent(earningEvent, earningEvent.PriceEpisodes[0], deliveryPeriod2, newCompletion);
        }

        [Test]
        public async Task Learner_Withdraws_From_Course_Prior_To_Completion()
        {
            // Arrange
            var earningEvent = new GSLShortCourseEarningsEvent
            {
                Earnings = new List<ShortCourseEarning>(),
                LearningAim = new LearningAim { Reference = "ZSC0001", LearningType = LearningType.ApprenticeshipUnit },
                CollectionPeriod = new CollectionPeriod
                {
                    AcademicYear = 2526,
                    Period = 2
                },
                PriceEpisodes = new List<PriceEpisode>(),
            };

            var paymentHistoryEntities = new PaymentHistoryEntity[]
            {
                new PaymentHistoryEntity
                {
                    LearnAimReference = "ZSC0001",
                    CollectionPeriod = new CollectionPeriod { AcademicYear = 2526, Period = 1 },
                    PriceEpisodeIdentifier = "PE-1",
                    TransactionType = 17,
                    DeliveryPeriod = 1,
                    Amount = 300m,
                    PlannedEndDate = new DateTime(2026,08,31),
                },
            };

            paymentHistoryCacheMock
                .Setup(x => x.TryGet(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ConditionalValue<PaymentHistoryEntity[]>(true, paymentHistoryEntities));

            // Act
            var result = await processor.HandleEarningEvent(earningEvent, paymentHistoryCacheMock.Object,
                CancellationToken.None);

            // Assert
            ClassicAssert.IsTrue(result.Count == 1);

            // Check for milestone 1 refund
            var refundMilestone1 = result[0];
            ValidateRequiredPaymentEvents(refundMilestone1, -300m, 1, TransactionType.Milestone1, 2526, 2);
        }

        [Test]
        public async Task Learner_Withdraws_From_CoInvested_Course_Prior_To_Completion()
        {
            // Arrange
            var earningEvent = new GSLShortCourseEarningsEvent
            {
                Earnings = new List<ShortCourseEarning>(),
                LearningAim = new LearningAim { Reference = "ZSC0001", LearningType = LearningType.ApprenticeshipUnit },
                CollectionPeriod = new CollectionPeriod
                {
                    AcademicYear = 2526,
                    Period = 1
                },
                PriceEpisodes = new List<PriceEpisode>(),
            };

            var paymentHistoryEntities = new PaymentHistoryEntity[]
            {
                new PaymentHistoryEntity
                {
                    LearnAimReference = "ZSC0001",
                    CollectionPeriod = new CollectionPeriod { AcademicYear = 2526, Period = 1 },
                    PriceEpisodeIdentifier = "PE-1",
                    TransactionType = 17,
                    DeliveryPeriod = 1,
                    Amount = 285m,
                    PlannedEndDate = new DateTime(2026,08,31),
                    FundingSource = FundingSourceType.CoInvestedSfa
                },
                new PaymentHistoryEntity
                {
                    LearnAimReference = "ZSC0001",
                    CollectionPeriod = new CollectionPeriod { AcademicYear = 2526, Period = 1 },
                    PriceEpisodeIdentifier = "PE-1",
                    TransactionType = 17,
                    DeliveryPeriod = 1,
                    Amount = 15m,
                    PlannedEndDate = new DateTime(2026,08,31),
                    FundingSource = FundingSourceType.CoInvestedEmployer
                },
            };

            paymentHistoryCacheMock
                .Setup(x => x.TryGet(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ConditionalValue<PaymentHistoryEntity[]>(true, paymentHistoryEntities));

            // Act
            var result = await processor.HandleEarningEvent(earningEvent, paymentHistoryCacheMock.Object,
                CancellationToken.None);

            // Assert
            ClassicAssert.IsTrue(result.Count == 1);

            // Check for milestone 1 refund
            var refundMilestone1 = result[0];
            ValidateRequiredPaymentEvents(refundMilestone1, -300m, 1, TransactionType.Milestone1, 2526, 1);
        }

        private void ValidateRequiredPaymentEvents(PeriodisedRequiredPaymentEvent rpe, decimal amount, byte deliveryPeriod, TransactionType type, int academicYear, byte period)
        {
            ClassicAssert.IsTrue(rpe.CompletionAmount == amount);
            ClassicAssert.IsTrue(rpe.DeliveryPeriod == deliveryPeriod);
            ClassicAssert.IsTrue(rpe.TransactionType == type);
            ClassicAssert.IsTrue(rpe.CollectionPeriod.AcademicYear == academicYear);
            ClassicAssert.IsTrue(rpe.CollectionPeriod.Period == period);
        }
        private EarningPeriod GenerateTestEarningPeriod(byte period, decimal amount, decimal? sfaContribution = 0m)
        {
            return new EarningPeriod
            {
                Period = period,
                Amount = amount,
                PriceEpisodeIdentifier = "PE-1",
                TransferSenderAccountId = null,
                AccountId = 1,
                ApprenticeshipId = 1,
                SfaContributionPercentage = sfaContribution,
            };
        }

        private GSLShortCourseEarningsEvent GenerateTestShortCourseEarningsEvent(ShortCoursesTestValues testValues)
        {
            return new GSLShortCourseEarningsEvent
            {
                Earnings = testValues.ShortCourseEarnings,
                LearningAim = new LearningAim { Reference = "ZSC0001", LearningType = LearningType.ApprenticeshipUnit },
                CollectionPeriod = new CollectionPeriod { AcademicYear = testValues.Year, Period = testValues.CollectionPeriod },
                PriceEpisodes = new List<PriceEpisode>
                {
                    new PriceEpisode
                    {
                        Identifier = "PE-1",
                        TotalNegotiatedPrice1 = testValues.Amount,
                        PlannedEndDate = testValues.PlannedEndDate
                    }
                },
            };
        }
        private void AssertMappingFromGslShortCourseEarningsEventToRequiredPaymentEvent(
            GSLShortCourseEarningsEvent earningEvent,
            PriceEpisode priceEpisode,
            EarningPeriod period,
            PeriodisedRequiredPaymentEvent periodisedRequiredPaymentEvent,
            PaymentHistoryEntity phe = null,
            bool refund = false)
        {
            var actualEvent = (CalculatedRequiredLevyAmount) periodisedRequiredPaymentEvent;
            // Check mappings from EarningEvent
            ClassicAssert.AreEqual(earningEvent.AgeAtStartOfLearning, actualEvent.AgeAtStartOfLearning, "AgeAtStartOfLearning mismatch");
            ClassicAssert.AreEqual(earningEvent.LearningAim.CourseCode, actualEvent.LearningAim.CourseCode, "LearningAim CourseCode mismatch");
            ClassicAssert.AreEqual(earningEvent.LearningAim.FrameworkCode, actualEvent.LearningAim.FrameworkCode, "LearningAim FrameworkCode mismatch");
            ClassicAssert.AreEqual(priceEpisode.FundingLineType, actualEvent.LearningAim.FundingLineType, "LearningAim FundingLineType mismatch");
            ClassicAssert.AreEqual(earningEvent.LearningAim.LearningType, actualEvent.LearningAim.LearningType, "LearningAim LearningType mismatch");
            ClassicAssert.AreEqual(earningEvent.LearningAim.PathwayCode, actualEvent.LearningAim.PathwayCode, "LearningAim PathwayCode mismatch");
            ClassicAssert.AreEqual(earningEvent.LearningAim.ProgrammeType, actualEvent.LearningAim.ProgrammeType, "LearningAim ProgrammeType mismatch");
            ClassicAssert.AreEqual(earningEvent.LearningAim.Reference, actualEvent.LearningAim.Reference, "LearningAim Reference mismatch");
            ClassicAssert.AreEqual(earningEvent.LearningAim.SequenceNumber, actualEvent.LearningAim.SequenceNumber, "LearningAim SequenceNumber mismatch");
            ClassicAssert.AreEqual(earningEvent.LearningAim.StandardCode, actualEvent.LearningAim.StandardCode, "LearningAim StandardCode mismatch");
            ClassicAssert.AreEqual(earningEvent.LearningAim.StartDate, actualEvent.LearningAim.StartDate, "LearningAim StartDate mismatch");
            ClassicAssert.AreEqual(earningEvent.FundingPlatformType, actualEvent.FundingPlatformType, "FundingPlatformType mismatch");

            // Check mappings from PriceEpisode
            ClassicAssert.AreEqual(priceEpisode.LearningAimSequenceNumber, actualEvent.LearningAimSequenceNumber, "LearningAimSequenceNumber mismatch");
            ClassicAssert.AreEqual(earningEvent.LearningAim.StartDate, actualEvent.LearningStartDate, "LearningStartDate mismatch");

            // Check mappings from Period
            ClassicAssert.AreEqual(period.Period, actualEvent.CollectionPeriod.Period, "CollectionPeriod.Period mismatch");
            ClassicAssert.AreEqual(earningEvent.CollectionPeriod.AcademicYear, actualEvent.CollectionPeriod.AcademicYear, "CollectionPeriod.AcademicYear mismatch");
            ClassicAssert.AreEqual(period.AccountId, actualEvent.AccountId, "AccountId mismatch");
            ClassicAssert.AreEqual(period.TransferSenderAccountId, actualEvent.TransferSenderAccountId, "TransferSenderAccountId mismatch");
            ClassicAssert.AreEqual(period.ApprenticeshipEmployerType, actualEvent.ApprenticeshipEmployerType, "ApprenticeshipEmployerType mismatch");
            ClassicAssert.AreEqual(period.ApprenticeshipId, actualEvent.ApprenticeshipId, "ApprenticeshipId mismatch");
            ClassicAssert.AreEqual(period.ApprenticeshipPriceEpisodeId, actualEvent.ApprenticeshipPriceEpisodeId, "ApprenticeshipPriceEpisodeId mismatch");
            ClassicAssert.AreEqual(period.SfaContributionPercentage, actualEvent.SfaContributionPercentage, "SfaContributionPercentage mismatch");
            ClassicAssert.AreEqual(period.PriceEpisodeIdentifier, actualEvent.PriceEpisodeIdentifier, "PriceEpisodeIdentifier mismatch");
            ClassicAssert.AreEqual((refund ? -period.Amount : period.Amount), actualEvent.CompletionAmount, "CompletionAmount mismatch");

            // Check constant mapping
            ClassicAssert.AreEqual(CourseType.ShortCourse, actualEvent.CourseType, "CourseType mismatch");

            ClassicAssert.AreEqual(earningEvent.Learner, actualEvent.Learner, "Learner mismatch");
            ClassicAssert.AreEqual(period.AgreedOnDate, actualEvent.AgreedOnDate, "AgreedOnDate mismatch");
            ClassicAssert.AreEqual(earningEvent.EventId, actualEvent.EarningEventId, "EarningEventId mismatch");
            ClassicAssert.AreEqual((refund ? -period.Amount : period.Amount), actualEvent.AmountDue, "AmountDue mismatch");
            ClassicAssert.AreEqual(period.Period, actualEvent.CollectionPeriod.Period, "CollectionPeriod mismatch");
            ClassicAssert.AreEqual((refund ? phe?.DeliveryPeriod : period.Period), actualEvent.DeliveryPeriod, "DeliveryPeriod mismatch");
            ClassicAssert.AreEqual(priceEpisode.StartDate, actualEvent.StartDate, "StartDate mismatch");
            ClassicAssert.AreEqual(priceEpisode.PlannedEndDate, actualEvent.PlannedEndDate, "PlannedEndDate mismatch");
            ClassicAssert.AreEqual(priceEpisode.ActualEndDate, actualEvent.ActualEndDate, "ActualEndDate mismatch");
            ClassicAssert.AreEqual(priceEpisode.InstalmentAmount, actualEvent.InstalmentAmount, "InstalmentAmount mismatch");
            ClassicAssert.AreEqual((short)priceEpisode.NumberOfInstalments, actualEvent.NumberOfInstalments, "NumberOfInstalments mismatch");
            ClassicAssert.AreEqual(earningEvent.JobId, actualEvent.JobId, "JobId mismatch");
            ClassicAssert.AreNotEqual(earningEvent.EventId, actualEvent.EventId, "EventId mismatch");
            ClassicAssert.AreEqual(earningEvent.Ukprn, actualEvent.Ukprn, "Ukprn mismatch");
            ClassicAssert.AreEqual(earningEvent.IlrSubmissionDateTime, actualEvent.IlrSubmissionDateTime, "IlrSubmissionDateTime mismatch");
        }

    }
}
