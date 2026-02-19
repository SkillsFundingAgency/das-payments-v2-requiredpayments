using AutoMapper;
using Moq;
using NUnit.Framework;
using SFA.DAS.Payments.Application.Repositories;
using SFA.DAS.Payments.EarningEvents.Messages;
using SFA.DAS.Payments.EarningEvents.Messages.Events;
using SFA.DAS.Payments.Model.Core;
using SFA.DAS.Payments.RequiredPayments.Application.Processors;
using SFA.DAS.Payments.RequiredPayments.Domain;
using SFA.DAS.Payments.RequiredPayments.Domain.Entities;
using SFA.DAS.Payments.RequiredPayments.Messages.Events;
using SFA.DAS.Payments.RequiredPayments.Model.Entities;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework.Legacy;

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
                                TransferSenderAccountId = null
                            }
                        },
                    }
                },
                LearningAim = new LearningAim { Reference = "ZPROG001" },
                CollectionPeriod = new CollectionPeriod { AcademicYear = 2324 }
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
                Earnings = new List<ShortCourseEarning>(),
                LearningAim = new LearningAim { Reference = "ZPROG001" },
                CollectionPeriod = new CollectionPeriod { AcademicYear = 2324 }
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
                .Returns(new List<RequiredPayment> { new RequiredPayment { Amount = -100m } });

            // Act
            var result = await processor.HandleEarningEvent(earningEvent, paymentHistoryCacheMock.Object, CancellationToken.None);

            // Assert
            ClassicAssert.IsTrue(result.Count > 0);
            ClassicAssert.IsTrue(result[0].CompletionAmount < 0);
        }
    }
}
