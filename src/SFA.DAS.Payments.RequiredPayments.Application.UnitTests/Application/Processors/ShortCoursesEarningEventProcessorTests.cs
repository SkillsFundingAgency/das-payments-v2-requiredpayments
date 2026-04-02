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
using FluentAssertions;
using SFA.DAS.Payments.Application.Messaging;
using SFA.DAS.Payments.Model.Core.Entities;

namespace SFA.DAS.Payments.RequiredPayments.Application.UnitTests.Application.Processors
{
    [TestFixture]
    public class ShortCoursesEarningEventProcessorTests
    {
        private Mock<INegativeEarningService> negativeEarningServiceMock;
        private Mock<IMapper> mapperMock;
        private Mock<IDataCache<PaymentHistoryEntity[]>> paymentHistoryCacheMock;
        private Mock<IDuplicateEarningEventService> duplicateEarningEventServiceMock;
        private ShortCoursesEarningEventProcessor processor;

        [SetUp]
        public void SetUp()
        {
            negativeEarningServiceMock = new Mock<INegativeEarningService>();
            mapperMock = new Mock<IMapper>();
            paymentHistoryCacheMock = new Mock<IDataCache<PaymentHistoryEntity[]>>();
            duplicateEarningEventServiceMock = new Mock<IDuplicateEarningEventService>();
            processor = new ShortCoursesEarningEventProcessor(negativeEarningServiceMock.Object, mapperMock.Object, duplicateEarningEventServiceMock.Object);
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
                    TransactionType = 2
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
                    Amount = 101m,
                    DeliveryPeriod = phe.DeliveryPeriod,
                    TransactionType = phe.TransactionType
                });

            negativeEarningServiceMock
                .Setup(x => x.ProcessNegativeEarning(It.IsAny<decimal>(), It.IsAny<List<Payment>>(), It.IsAny<int>(), It.IsAny<string>()))
                .Returns(new List<RequiredPayment>
                {
                    new RequiredPayment
                    {
                        Amount = -101m,
                        EarningType = EarningType.Levy,
                        PriceEpisodeIdentifier = "PE-1",
                        AccountId = 1,
                        TransferSenderAccountId = null,
                        LearningStartDate = new DateTime(2024, 9, 1),
                        ApprenticeshipId = 1,
                    }
                }).Verifiable();

            // Act
            var result = await processor.HandleEarningEvent(earningEvent, paymentHistoryCacheMock.Object, CancellationToken.None);

            // Assert
            ClassicAssert.IsTrue(result.Count > 0);
            negativeEarningServiceMock.Verify(x => x.ProcessNegativeEarning(It.IsAny<decimal>(), It.IsAny<List<Payment>>(), It.IsAny<int>(), It.IsAny<string>()), Times.Once);

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
                CollectionPeriod = new CollectionPeriod { AcademicYear = 2324, Period = 1},
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
                    LearnAimReference = "ZSC0001",
                    CollectionPeriod = new CollectionPeriod { AcademicYear = 2324, Period = 1 },
                    PriceEpisodeIdentifier = "PE-1",
                    TransactionType = 2,
                    DeliveryPeriod = 1,
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
                    Amount = 100m, // Simulate payment history with different amount
                    TransactionType = phe.TransactionType,
                    DeliveryPeriod = phe.DeliveryPeriod
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
                }).Verifiable();

            // Act
            var result = await processor.HandleEarningEvent(earningEvent, paymentHistoryCacheMock.Object, CancellationToken.None);

            // Assert
            ClassicAssert.IsTrue(result.Count >= 2, "Should have both a refund and a new payment.");

            // Check for refund
            var refund = result[0];
            ClassicAssert.IsTrue(refund.CompletionAmount == -100, "First event should be a refund.");
            ClassicAssert.AreEqual("PE-1", refund.PriceEpisodeIdentifier);
            negativeEarningServiceMock.Verify(x => x.ProcessNegativeEarning(It.IsAny<decimal>(), It.IsAny<List<Payment>>(), It.IsAny<int>(), It.IsAny<string>()), Times.Once);

            // Check for new payment
            var newPayment = result[1];
            ClassicAssert.IsTrue(newPayment.CompletionAmount > 0, "Second event should be a new payment.");

            AssertMappingFromGslShortCourseEarningsEventToRequiredPaymentEvent(earningEvent, earningEvent.PriceEpisodes[0], period, newPayment);
        }

        private void AssertMappingFromGslShortCourseEarningsEventToRequiredPaymentEvent(
            GSLShortCourseEarningsEvent earningEvent,
            PriceEpisode priceEpisode,
            EarningPeriod period,
            PeriodisedRequiredPaymentEvent periodisedRequiredPaymentEvent)
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
            ClassicAssert.AreEqual(period.Amount, actualEvent.CompletionAmount, "CompletionAmount mismatch");

            // Check constant mapping
            ClassicAssert.AreEqual(CourseType.ShortCourse, actualEvent.CourseType, "CourseType mismatch");

            ClassicAssert.AreEqual(earningEvent.Learner, actualEvent.Learner, "Learner mismatch");
            ClassicAssert.AreEqual(period.AgreedOnDate, actualEvent.AgreedOnDate, "AgreedOnDate mismatch");
            ClassicAssert.AreEqual(earningEvent.EventId, actualEvent.EarningEventId, "EarningEventId mismatch");
            ClassicAssert.AreEqual(period.Amount, actualEvent.AmountDue, "AmountDue mismatch");
            ClassicAssert.AreEqual(period.Period, actualEvent.DeliveryPeriod, "DeliveryPeriod mismatch");
            ClassicAssert.AreEqual(priceEpisode.StartDate, actualEvent.StartDate, "StartDate mismatch");
            ClassicAssert.AreEqual(priceEpisode.PlannedEndDate, actualEvent.PlannedEndDate, "PlannedEndDate mismatch");
            ClassicAssert.AreEqual(priceEpisode.ActualEndDate, actualEvent.ActualEndDate, "ActualEndDate mismatch");
            ClassicAssert.AreEqual(priceEpisode.InstalmentAmount, actualEvent.InstalmentAmount, "InstalmentAmount mismatch");
            ClassicAssert.AreEqual((short)priceEpisode.NumberOfInstalments, actualEvent.NumberOfInstalments, "NumberOfInstalments mismatch");
            ClassicAssert.AreEqual(earningEvent.JobId, actualEvent.JobId, "JobId mismatch");
            ClassicAssert.AreNotEqual(earningEvent.EventId, actualEvent.EventId, "EventId mismatch");
            ClassicAssert.AreEqual(earningEvent.Ukprn, actualEvent.Ukprn, "Ukprn mismatch");
        }

    }
}
