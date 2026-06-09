using Moq;
using NUnit.Framework;
using NUnit.Framework.Legacy;
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
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SFA.DAS.Payments.RequiredPayments.Domain;

namespace SFA.DAS.Payments.RequiredPayments.Application.UnitTests.Application.Processors
{
    internal class ShortCoursesTestValues
    {
        public new List<ShortCourseEarning> ShortCourseEarnings { get; set; }
        public short Year { get; set; }
        public byte CollectionPeriod { get; set; }
        public DateTime PlannedEndDate { get; set; }
    }
    [TestFixture]
    public class ShortCoursesEarningEventProcessorTests
    {
        private Mock<IDataCache<PaymentHistoryEntity[]>> paymentHistoryCacheMock;
        private Mock<IDuplicateShortCoursesEarningEventService> duplicateEarningEventServiceMock;
        private ShortCoursesEarningEventProcessor processor;

        [SetUp]
        public void SetUp()
        {
            paymentHistoryCacheMock = new Mock<IDataCache<PaymentHistoryEntity[]>>();
            duplicateEarningEventServiceMock = new Mock<IDuplicateShortCoursesEarningEventService>();
            processor = new ShortCoursesEarningEventProcessor(duplicateEarningEventServiceMock.Object);
        }
        [TearDown]
        public void TearDown()
        {
            paymentHistoryCacheMock = new Mock<IDataCache<PaymentHistoryEntity[]>>();
            duplicateEarningEventServiceMock = new Mock<IDuplicateShortCoursesEarningEventService>();
            processor = new ShortCoursesEarningEventProcessor(duplicateEarningEventServiceMock.Object);
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
        public async Task Initial_Payment_For_ShortCourse_Training_Delivery()
        {
            // Arrange
            var completionAmount = 1000m;
            var amount = 300m;
            byte collectionPeriod = 1;
            var period = GenerateTestEarningPeriod(collectionPeriod, amount, ApprenticeshipEmployerType.Levy);
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
            var newPayment = result.FirstOrDefault(x => x.TransactionType == TransactionType.Milestone1);
            ValidateRequiredPaymentEvents(newPayment, amount, 1, TransactionType.Milestone1, 2526, 1);
            AssertMappingFromGslShortCourseEarningsEventToRequiredPaymentEvent(earningEvent, earningEvent.PriceEpisodes[0], period, newPayment);
        }

        [Test]
        public async Task Short_Courses_Delivery_Recorded_Completion()
        {
            // Arrange
            var deliveryPeriod1 = GenerateTestEarningPeriod(1, 300m, ApprenticeshipEmployerType.Levy);
            var deliveryPeriod2 = GenerateTestEarningPeriod(2, 700m, ApprenticeshipEmployerType.Levy);

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
                    PlannedEndDate = new DateTime(2026, 9, 30)

                });

            var paymentHistoryEntities = new PaymentHistoryEntity[]
            {
                new PaymentHistoryEntity
                {
                    LearnAimReference = "ZSC0001",
                    CollectionPeriod = new CollectionPeriod { AcademicYear = 2526, Period = 1 },
                    PriceEpisodeIdentifier = "PE-1",
                    TransactionType = (int)TransactionType.Milestone1,
                    DeliveryPeriod = 1,
                    Amount = 285m,
                    LearningAimFundingLineType = "funding line type"
                },
                new PaymentHistoryEntity
                {
                    LearnAimReference = "ZSC0001",
                    CollectionPeriod = new CollectionPeriod { AcademicYear = 2526, Period = 1 },
                    PriceEpisodeIdentifier = "PE-1",
                    TransactionType = (int)TransactionType.Milestone1,
                    DeliveryPeriod = 1,
                    Amount = 15m,
                    LearningAimFundingLineType = "funding line type"
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
            var newPayment = result.FirstOrDefault(x => x.TransactionType == TransactionType.Completion);
            ValidateRequiredPaymentEvents(newPayment, 700m, 2, TransactionType.Completion, 2526, 2);

            AssertMappingFromGslShortCourseEarningsEventToRequiredPaymentEvent(earningEvent, earningEvent.PriceEpisodes[0], deliveryPeriod2, newPayment);
        }


        [Test]
        public async Task Short_Courses_That_Starts_And_Completes_In_Same_Period()
        {
            // Arrange
            var deliveryPeriod1 = GenerateTestEarningPeriod(1, 300m, ApprenticeshipEmployerType.Levy);
            var deliveryPeriod2 = GenerateTestEarningPeriod(1, 700m, ApprenticeshipEmployerType.Levy);
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
            var newMilestone1 = result.FirstOrDefault(x => x.TransactionType == TransactionType.Milestone1);
            ValidateRequiredPaymentEvents(newMilestone1, 300m, 1, TransactionType.Milestone1, 2526, 1);


            // Check for new completion payment
            var newCompletion = result.FirstOrDefault(x => x.TransactionType == TransactionType.Completion);
            ValidateRequiredPaymentEvents(newCompletion, 700m, 1, TransactionType.Completion, 2526, 1);

            AssertMappingFromGslShortCourseEarningsEventToRequiredPaymentEvent(earningEvent, earningEvent.PriceEpisodes[0], deliveryPeriod1, newMilestone1);
            AssertMappingFromGslShortCourseEarningsEventToRequiredPaymentEvent(earningEvent, earningEvent.PriceEpisodes[0], deliveryPeriod2, newCompletion);
        }

        [Test]
        public async Task Skip_Funding_For_Delivered_Training_Levy_Employer_No_Balance()
        {
            // Arrange
            var deliveryPeriod1 = GenerateTestEarningPeriod(1, 300m, ApprenticeshipEmployerType.NonLevy);

            var shortCourses = new List<ShortCourseEarning>
            {
                new ()
                {
                    Type = ShortCourseEarningType.Milestone1,
                    Periods = new List<EarningPeriod>
                    {
                        deliveryPeriod1
                    },
                }
            };

            var earningEvent = GenerateTestShortCourseEarningsEvent(
                new ShortCoursesTestValues
                {
                    ShortCourseEarnings = shortCourses,
                    Year = 2526,
                    CollectionPeriod = 1,
                    PlannedEndDate = new DateTime(2026, 9, 30)

                });

            var paymentHistoryEntities = new PaymentHistoryEntity[]
            {
                new PaymentHistoryEntity
                {
                    LearnAimReference = "ZSC0001",
                    CollectionPeriod = new CollectionPeriod { AcademicYear = 2526, Period = 1 },
                    PriceEpisodeIdentifier = "PE-1",
                    TransactionType = (int)TransactionType.Milestone1,
                    DeliveryPeriod = 1,
                    Amount = 285m,
                    FundingSource = FundingSourceType.CoInvestedSfa,
                    LearningAimFundingLineType = "funding line type"
                },
                new PaymentHistoryEntity
                {
                    LearnAimReference = "ZSC0001",
                    CollectionPeriod = new CollectionPeriod { AcademicYear = 2526, Period = 1 },
                    PriceEpisodeIdentifier = "PE-1",
                    TransactionType = (int)TransactionType.Milestone1,
                    DeliveryPeriod = 1,
                    Amount = 15m,
                    FundingSource = FundingSourceType.CoInvestedEmployer,
                    LearningAimFundingLineType = "funding line type"
                }
            };

            paymentHistoryCacheMock
                .Setup(x => x.TryGet(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ConditionalValue<PaymentHistoryEntity[]>(true, paymentHistoryEntities));

            // Act
            var result = await processor.HandleEarningEvent(earningEvent, paymentHistoryCacheMock.Object,
                CancellationToken.None);

            // Assert
            ClassicAssert.IsTrue(result.Count == 0, "Should have no payment.");
        }

        [Test]
        public async Task Payments_Made_In_Different_Delivery_Period_Generates_New_Payment()
        {
            // Arrange
            var deliveryPeriod1 = GenerateTestEarningPeriod(1, 300m, ApprenticeshipEmployerType.Levy);
            var deliveryPeriod2 = GenerateTestEarningPeriod(2, 300m, ApprenticeshipEmployerType.Levy);

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
                new ()
                {
                    Type = ShortCourseEarningType.Milestone1,
                    Periods = new List<EarningPeriod>
                    {
                        deliveryPeriod2
                    },
                },
            };

            var earningEvent = GenerateTestShortCourseEarningsEvent(
                new ShortCoursesTestValues
                {
                    ShortCourseEarnings = shortCourses,
                    Year = 2526,
                    CollectionPeriod = 2,
                    PlannedEndDate = new DateTime(2026, 9, 30)

                });

            var paymentHistoryEntities = new PaymentHistoryEntity[]
            {
                new PaymentHistoryEntity
                {
                    LearnAimReference = "ZSC0001",
                    CollectionPeriod = new CollectionPeriod { AcademicYear = 2526, Period = 1 },
                    PriceEpisodeIdentifier = "PE-1",
                    TransactionType = (int)TransactionType.Milestone1,
                    DeliveryPeriod = 1,
                    Amount = 285m,
                    LearningAimFundingLineType = "funding line type"
                },
                new PaymentHistoryEntity
                {
                    LearnAimReference = "ZSC0001",
                    CollectionPeriod = new CollectionPeriod { AcademicYear = 2526, Period = 1 },
                    PriceEpisodeIdentifier = "PE-1",
                    TransactionType = (int)TransactionType.Milestone1,
                    DeliveryPeriod = 1,
                    Amount = 15m,
                    LearningAimFundingLineType = "funding line type"
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
            var newPayment = result.FirstOrDefault(x => x.TransactionType == TransactionType.Milestone1);
            ValidateRequiredPaymentEvents(newPayment, 300m, 2, TransactionType.Milestone1, 2526, 2);

            AssertMappingFromGslShortCourseEarningsEventToRequiredPaymentEvent(earningEvent, earningEvent.PriceEpisodes[0], deliveryPeriod2, newPayment);
        }

        [Test]
        public async Task Change_Of_Circumstances_Updated_Start_Date_Generates_Refund_And_New_Payment()
        {
            // Arrange
            var deliveryPeriod1 = GenerateTestEarningPeriod(2, 300m, ApprenticeshipEmployerType.Levy);
            
            var shortCourses = new List<ShortCourseEarning>
            {
                new ()
                {
                    Type = ShortCourseEarningType.Milestone1,
                    Periods = new List<EarningPeriod>
                    {
                        deliveryPeriod1
                    },
                }
            };

            var earningEvent = GenerateTestShortCourseEarningsEvent(
                new ShortCoursesTestValues
                {
                    ShortCourseEarnings = shortCourses,
                    Year = 2526,
                    CollectionPeriod = 2,
                    PlannedEndDate = new DateTime(2026, 9, 30)

                });

            var paymentHistoryEntities = new PaymentHistoryEntity[]
            {
                new PaymentHistoryEntity
                {
                    LearnAimReference = "ZSC0001",
                    CollectionPeriod = new CollectionPeriod { AcademicYear = 2526, Period = 1 },
                    PriceEpisodeIdentifier = "PE-1",
                    TransactionType = (int)TransactionType.Milestone1,
                    DeliveryPeriod = 1,
                    Amount = 300m,
                    LearningAimFundingLineType = "funding line type",
                    AccountId = 12345,
                    TransferSenderAccountId = 23456,
                    SfaContributionPercentage = 0.95m,
                    ApprenticeshipEmployerType = ApprenticeshipEmployerType.Levy,
                    ApprenticeshipId = 123456789
                }
            };

            paymentHistoryCacheMock
                .Setup(x => x.TryGet(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ConditionalValue<PaymentHistoryEntity[]>(true, paymentHistoryEntities));

            // Act
            var result = await processor.HandleEarningEvent(earningEvent, paymentHistoryCacheMock.Object,
                CancellationToken.None);

            // Assert
            ClassicAssert.IsTrue(result.Count == 2, "Should have refund and new payment for Milestone1.");

            // Check payments and refunds
            var refundMilestone1Payment = result.FirstOrDefault(x =>
                x.TransactionType == TransactionType.Milestone1 && x.AmountDue < 0m);
            var newMilestone1Payment = result.FirstOrDefault(x =>
                x.TransactionType == TransactionType.Milestone1 && x.AmountDue > 0m);
            ClassicAssert.IsNotNull(refundMilestone1Payment);
            ClassicAssert.IsNotNull(newMilestone1Payment);
            ValidateRequiredPaymentEvents(refundMilestone1Payment, -300m, 1, TransactionType.Milestone1, 2526, 2);
            ValidateRequiredPaymentEvents(newMilestone1Payment, 300m, 2, TransactionType.Milestone1, 2526, 2);
            var calculatedrequiredLevyAmount = refundMilestone1Payment as CalculatedRequiredLevyAmount;
            ClassicAssert.IsTrue(calculatedrequiredLevyAmount.AccountId == paymentHistoryEntities[0].AccountId);
            ClassicAssert.IsTrue(calculatedrequiredLevyAmount.SfaContributionPercentage == paymentHistoryEntities[0].SfaContributionPercentage);
            ClassicAssert.IsTrue(calculatedrequiredLevyAmount.TransferSenderAccountId == paymentHistoryEntities[0].TransferSenderAccountId);
            ClassicAssert.IsTrue(calculatedrequiredLevyAmount.ApprenticeshipEmployerType == paymentHistoryEntities[0].ApprenticeshipEmployerType);
            ClassicAssert.IsTrue(calculatedrequiredLevyAmount.ApprenticeshipId == paymentHistoryEntities[0].ApprenticeshipId);
        }

        [Test]
        public async Task Change_Of_Circumstances_Updated_Start_And_Completion_Dates_Generate_Refunds_And_New_Payments()
        {
            // Arrange
            var deliveryPeriod1 = GenerateTestEarningPeriod(2, 300m, ApprenticeshipEmployerType.Levy);
            var deliveryPeriod2 = GenerateTestEarningPeriod(2, 700m, ApprenticeshipEmployerType.Levy);

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
                    PlannedEndDate = new DateTime(2026, 9, 30)

                });
            
            var paymentHistoryEntities = new PaymentHistoryEntity[]
            {
                new PaymentHistoryEntity
                {
                    LearnAimReference = "ZSC0001",
                    CollectionPeriod = new CollectionPeriod { AcademicYear = 2526, Period = 1 },
                    PriceEpisodeIdentifier = "PE-1",
                    TransactionType = (int)TransactionType.Milestone1,
                    DeliveryPeriod = 1,
                    Amount = 300m,
                    LearningAimFundingLineType = "funding line type"
                },
                new PaymentHistoryEntity
                {
                    LearnAimReference = "ZSC0001",
                    CollectionPeriod = new CollectionPeriod { AcademicYear = 2526, Period = 1 },
                    PriceEpisodeIdentifier = "PE-1",
                    TransactionType = (int)TransactionType.Completion,
                    DeliveryPeriod = 1,
                    Amount = 700m,
                    LearningAimFundingLineType = "funding line type"
                }
            };

            paymentHistoryCacheMock
                .Setup(x => x.TryGet(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ConditionalValue<PaymentHistoryEntity[]>(true, paymentHistoryEntities));

            // Act
            var result = await processor.HandleEarningEvent(earningEvent, paymentHistoryCacheMock.Object,
                CancellationToken.None);

            // Assert
            ClassicAssert.IsTrue(result.Count == 4, "Should have refunds and new payments for Milestone1 and Completion.");

            // Check payments and refunds
            var refundMilestone1Payment = result.FirstOrDefault(x =>
                x.TransactionType == TransactionType.Milestone1 && x.AmountDue < 0m);
            var refundCompletionPayment = result.FirstOrDefault(x =>
                x.TransactionType == TransactionType.Completion && x.AmountDue < 0m);
            var newMilestone1Payment = result.FirstOrDefault(x =>
                x.TransactionType == TransactionType.Milestone1 && x.AmountDue > 0m);
            var newCompletionPayment = result.FirstOrDefault(x =>
                x.TransactionType == TransactionType.Completion && x.AmountDue > 0m);
            ClassicAssert.IsNotNull(refundMilestone1Payment);
            ClassicAssert.IsNotNull(refundCompletionPayment);
            ClassicAssert.IsNotNull(newMilestone1Payment);
            ClassicAssert.IsNotNull(newCompletionPayment);
            ValidateRequiredPaymentEvents(refundMilestone1Payment, -300m, 1, TransactionType.Milestone1, 2526, 2);
            ValidateRequiredPaymentEvents(refundCompletionPayment, -700m, 1, TransactionType.Completion, 2526, 2);
            ValidateRequiredPaymentEvents(newMilestone1Payment, 300m, 2, TransactionType.Milestone1, 2526, 2);
            ValidateRequiredPaymentEvents(newCompletionPayment, 700m, 2, TransactionType.Completion, 2526, 2);
        }

        [Test]
        public async Task Change_Of_Circumstances_Learner_Withdrawn_After_Milestone_Payment_Made_Generates_Refund()
        {
            // Arrange
            var earningEvent = GenerateTestShortCourseEarningsEvent(
                new ShortCoursesTestValues
                {
                    ShortCourseEarnings = new List<ShortCourseEarning>(),
                    Year = 2526,
                    CollectionPeriod = 2,
                    PlannedEndDate = new DateTime(2026, 9, 30)
                });
            earningEvent.PriceEpisodes = new List<PriceEpisode>();

            var paymentHistoryEntities = new PaymentHistoryEntity[]
            {
                new PaymentHistoryEntity
                {
                    LearnAimReference = "ZSC0001",
                    CollectionPeriod = new CollectionPeriod { AcademicYear = 2526, Period = 1 },
                    PriceEpisodeIdentifier = "PE-1",
                    TransactionType = (int)TransactionType.Milestone1,
                    DeliveryPeriod = 1,
                    Amount = 300m,
                    LearningAimFundingLineType = "funding line type"
                }
            };

            paymentHistoryCacheMock
                .Setup(x => x.TryGet(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ConditionalValue<PaymentHistoryEntity[]>(true, paymentHistoryEntities));

            // Act
            var result = await processor.HandleEarningEvent(earningEvent, paymentHistoryCacheMock.Object,
                CancellationToken.None);

            // Assert
            ClassicAssert.IsTrue(result.Count == 1, "Should have refund for Milestone1.");

            // Check refunds
            var refundMilestone1Payment = result.FirstOrDefault(x =>
                x.TransactionType == TransactionType.Milestone1 && x.AmountDue < 0m);

            ClassicAssert.IsNotNull(refundMilestone1Payment);

            ValidateRequiredPaymentEvents(refundMilestone1Payment, -300m, 1, TransactionType.Milestone1, 2526, 2);
        }

        [Test]
        public async Task Change_Of_Circumstances_Learner_Withdrawn_After_Milestone_CoInvested_Payment_Made_Generates_Refund()
        {
            // Arrange
            var earningEvent = GenerateTestShortCourseEarningsEvent(
                new ShortCoursesTestValues
                {
                    ShortCourseEarnings = new List<ShortCourseEarning>(),
                    Year = 2526,
                    CollectionPeriod = 2,
                    PlannedEndDate = new DateTime(2026, 9, 30)

                });
            earningEvent.PriceEpisodes = new List<PriceEpisode>();

            var paymentHistoryEntities = new PaymentHistoryEntity[]
            {
                new PaymentHistoryEntity
                {
                    LearnAimReference = "ZSC0001",
                    CollectionPeriod = new CollectionPeriod { AcademicYear = 2526, Period = 1 },
                    PriceEpisodeIdentifier = "PE-1",
                    TransactionType = (int)TransactionType.Milestone1,
                    DeliveryPeriod = 1,
                    Amount = 285m,
                    FundingSource = FundingSourceType.CoInvestedSfa,
                    LearningAimFundingLineType = "funding line type"
                },
                new PaymentHistoryEntity
                {
                    LearnAimReference = "ZSC0001",
                    CollectionPeriod = new CollectionPeriod { AcademicYear = 2526, Period = 1 },
                    PriceEpisodeIdentifier = "PE-1",
                    TransactionType = (int)TransactionType.Milestone1,
                    DeliveryPeriod = 1,
                    Amount = 15m,
                    FundingSource = FundingSourceType.CoInvestedEmployer,
                    LearningAimFundingLineType = "funding line type"
                }
            };

            paymentHistoryCacheMock
                .Setup(x => x.TryGet(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ConditionalValue<PaymentHistoryEntity[]>(true, paymentHistoryEntities));

            // Act
            var result = await processor.HandleEarningEvent(earningEvent, paymentHistoryCacheMock.Object,
                CancellationToken.None);

            // Assert
            ClassicAssert.IsTrue(result.Count == 1, "Should have refund for Milestone1 co-invested payments.");

            // Check refunds
            var refundMilestone1Payment = result.FirstOrDefault(x =>
                x.TransactionType == TransactionType.Milestone1 && x.AmountDue == -300m);
            
            ClassicAssert.IsNotNull(refundMilestone1Payment);
            ClassicAssert.IsInstanceOf<CalculatedRequiredCoInvestedAmount>(refundMilestone1Payment);

            ValidateRequiredPaymentEvents(refundMilestone1Payment, -300m, 1, TransactionType.Milestone1, 2526, 2);
        }

        [Test]
        public async Task Change_Of_Milestone_Payment_Price_Generates_Refund_Of_Original_Payment_And_New_Payment()
        {
            // Arrange
            var deliveryPeriod1 = GenerateTestEarningPeriod(2, 330m, ApprenticeshipEmployerType.Levy);
            
            var shortCourses = new List<ShortCourseEarning>
            {
                new ()
                {
                    Type = ShortCourseEarningType.Milestone1,
                    Periods = new List<EarningPeriod>
                    {
                        deliveryPeriod1
                    },
                }
            };

            var earningEvent = GenerateTestShortCourseEarningsEvent(
                new ShortCoursesTestValues
                {
                    ShortCourseEarnings = shortCourses,
                    Year = 2526,
                    CollectionPeriod = 2,
                    PlannedEndDate = new DateTime(2026, 9, 30)

                });

            var paymentHistoryEntities = new PaymentHistoryEntity[]
            {
                new PaymentHistoryEntity
                {
                    LearnAimReference = "ZSC0001",
                    CollectionPeriod = new CollectionPeriod { AcademicYear = 2526, Period = 1 },
                    PriceEpisodeIdentifier = "PE-1",
                    TransactionType = (int)TransactionType.Milestone1,
                    DeliveryPeriod = 1,
                    Amount = 300m,
                    LearningAimFundingLineType = "funding line type"
                }
            };

            paymentHistoryCacheMock
                .Setup(x => x.TryGet(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ConditionalValue<PaymentHistoryEntity[]>(true, paymentHistoryEntities));

            // Act
            var result = await processor.HandleEarningEvent(earningEvent, paymentHistoryCacheMock.Object,
                CancellationToken.None);

            // Assert
            ClassicAssert.IsTrue(result.Count == 2, "Should have refund and new payment for Milestone1.");

            // Check payments and refunds
            var refundMilestone1Payment = result.FirstOrDefault(x =>
                x.TransactionType == TransactionType.Milestone1 && x.AmountDue < 0m);
            var newMilestone1Payment = result.FirstOrDefault(x =>
                x.TransactionType == TransactionType.Milestone1 && x.AmountDue > 0m);
            
            ClassicAssert.IsNotNull(refundMilestone1Payment);
            ClassicAssert.IsNotNull(newMilestone1Payment);
            
            ValidateRequiredPaymentEvents(refundMilestone1Payment, -300m, 1, TransactionType.Milestone1, 2526, 2);
            ValidateRequiredPaymentEvents(newMilestone1Payment, 330m, 2, TransactionType.Milestone1, 2526, 2);
        }

        [Test]
        public async Task Change_Of_Circumstances_Learner_Withdrawn_After_Milestone_Payment_Made_But_Before_Completion_Payment()
        {
            // Arrange
            var deliveryPeriod1 = GenerateTestEarningPeriod(1, 300m, ApprenticeshipEmployerType.Levy);
            
            var shortCourses = new List<ShortCourseEarning>
            {
                new ()
                {
                    Type = ShortCourseEarningType.Milestone1,
                    Periods = new List<EarningPeriod>
                    {
                        deliveryPeriod1
                    }
                }
            };

            var earningEvent = GenerateTestShortCourseEarningsEvent(
                new ShortCoursesTestValues
                {
                    ShortCourseEarnings = shortCourses,
                    Year = 2526,
                    CollectionPeriod = 2,
                    PlannedEndDate = new DateTime(2026, 9, 30)

                });
            earningEvent.PriceEpisodes = new List<PriceEpisode>();

            var paymentHistoryEntities = new PaymentHistoryEntity[]
            {
                new PaymentHistoryEntity
                {
                    LearnAimReference = "ZSC0001",
                    CollectionPeriod = new CollectionPeriod { AcademicYear = 2526, Period = 1 },
                    PriceEpisodeIdentifier = "PE-1",
                    TransactionType = (int)TransactionType.Milestone1,
                    DeliveryPeriod = 1,
                    Amount = 300m,
                    LearningAimFundingLineType = "funding line type"
                },
                new PaymentHistoryEntity
                {
                    LearnAimReference = "ZSC0001",
                    CollectionPeriod = new CollectionPeriod { AcademicYear = 2526, Period = 1 },
                    PriceEpisodeIdentifier = "PE-1",
                    TransactionType = (int)TransactionType.Completion,
                    DeliveryPeriod = 1,
                    Amount = 700m,
                    LearningAimFundingLineType = "funding line type"
                }
            };

            paymentHistoryCacheMock
                .Setup(x => x.TryGet(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ConditionalValue<PaymentHistoryEntity[]>(true, paymentHistoryEntities));

            // Act
            var result = await processor.HandleEarningEvent(earningEvent, paymentHistoryCacheMock.Object,
                CancellationToken.None);

            // Assert
            ClassicAssert.IsTrue(result.Count == 1, "Should have refund for Completion payment.");

            // Check refunds
            var refundCompletionPayment = result.FirstOrDefault(x =>
                x.TransactionType == TransactionType.Completion && x.AmountDue < 0m);
            ClassicAssert.IsNotNull(refundCompletionPayment);
            ValidateRequiredPaymentEvents(refundCompletionPayment, -700m, 1, TransactionType.Completion, 2526, 2);
        }

        [Test]
        public async Task Change_Of_Circumstances_Learner_Withdrawn_Date_Changed_To_Date_Before_Milestone_And_Completion_Payments_Made()
        {
            // Arrange
            var earningEvent = GenerateTestShortCourseEarningsEvent(
                new ShortCoursesTestValues
                {
                    ShortCourseEarnings = new List<ShortCourseEarning>(),
                    Year = 2526,
                    CollectionPeriod = 2,
                    PlannedEndDate = new DateTime(2026, 9, 30)
                });
            earningEvent.PriceEpisodes = new List<PriceEpisode>();

            var paymentHistoryEntities = new PaymentHistoryEntity[]
            {
                new PaymentHistoryEntity
                {
                    LearnAimReference = "ZSC0001",
                    CollectionPeriod = new CollectionPeriod { AcademicYear = 2526, Period = 1 },
                    PriceEpisodeIdentifier = "PE-1",
                    TransactionType = (int)TransactionType.Milestone1,
                    DeliveryPeriod = 1,
                    Amount = 300m,
                    LearningAimFundingLineType = "funding line type"
                },
                new PaymentHistoryEntity
                {
                    LearnAimReference = "ZSC0001",
                    CollectionPeriod = new CollectionPeriod { AcademicYear = 2526, Period = 1 },
                    PriceEpisodeIdentifier = "PE-1",
                    TransactionType = (int)TransactionType.Completion,
                    DeliveryPeriod = 1,
                    Amount = 700m,
                    LearningAimFundingLineType = "funding line type"
                }
            };

            paymentHistoryCacheMock
                .Setup(x => x.TryGet(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ConditionalValue<PaymentHistoryEntity[]>(true, paymentHistoryEntities));

            // Act
            var result = await processor.HandleEarningEvent(earningEvent, paymentHistoryCacheMock.Object,
                CancellationToken.None);

            // Assert
            ClassicAssert.IsTrue(result.Count == 2, "Should have refunds for Milestone 1 and Completion payments.");

            // Check refunds
            var refundMilestone1Payment = result.FirstOrDefault(x =>
                x.TransactionType == TransactionType.Milestone1 && x.AmountDue < 0m);
            var refundCompletionPayment = result.FirstOrDefault(x =>
                x.TransactionType == TransactionType.Completion && x.AmountDue < 0m);
            ClassicAssert.IsNotNull(refundMilestone1Payment);
            ClassicAssert.IsNotNull(refundCompletionPayment);

            ValidateRequiredPaymentEvents(refundMilestone1Payment, -300m, 1, TransactionType.Milestone1, 2526, 2);
            ValidateRequiredPaymentEvents(refundCompletionPayment, -700m, 1, TransactionType.Completion, 2526, 2);
        }

        private void ValidateRequiredPaymentEvents(PeriodisedRequiredPaymentEvent rpe, decimal amount, byte deliveryPeriod, TransactionType type, int academicYear, byte period)
        {
            ClassicAssert.IsTrue(rpe.AmountDue == amount, "Amount mismatch");
            ClassicAssert.IsTrue(rpe.DeliveryPeriod == deliveryPeriod, "Delivery period mismatch");
            ClassicAssert.IsTrue(rpe.TransactionType == type, "Transaction type mismatch");
            ClassicAssert.IsTrue(rpe.CollectionPeriod.AcademicYear == academicYear, "Academic year mismatch");
            ClassicAssert.IsTrue(rpe.CollectionPeriod.Period == period, "Collection period mismatch");
            ClassicAssert.IsNotNull(rpe.LearningAim.FundingLineType, "Invalid LearningAim FundingLineType");
        }

        private EarningPeriod GenerateTestEarningPeriod(byte period, decimal amount, ApprenticeshipEmployerType employerType, decimal? sfaContribution = 0m)
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
                ApprenticeshipEmployerType = employerType,
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
                        TotalNegotiatedPrice1 = 1000m,
                        PlannedEndDate = testValues.PlannedEndDate,
                        FundingLineType = "funding line type"
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
            var actualEvent = (CalculatedRequiredOnProgrammeAmount) periodisedRequiredPaymentEvent;

            //Levy specific checks
            if (periodisedRequiredPaymentEvent.ApprenticeshipEmployerType == ApprenticeshipEmployerType.Levy)
            {
                var levyEvent = (CalculatedRequiredLevyAmount)periodisedRequiredPaymentEvent;
                ClassicAssert.AreEqual(earningEvent.FundingPlatformType, levyEvent.FundingPlatformType, "FundingPlatformType mismatch");
                ClassicAssert.AreEqual(CourseType.ShortCourse, levyEvent.CourseType, "CourseType mismatch");
                ClassicAssert.AreEqual(period.AgreedOnDate, levyEvent.AgreedOnDate, "AgreedOnDate mismatch");
            }

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
            ClassicAssert.AreEqual(priceEpisode.CompletionAmount, actualEvent.CompletionAmount, "CompletionAmount mismatch");

            ClassicAssert.AreEqual(earningEvent.Learner, actualEvent.Learner, "Learner mismatch");
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
