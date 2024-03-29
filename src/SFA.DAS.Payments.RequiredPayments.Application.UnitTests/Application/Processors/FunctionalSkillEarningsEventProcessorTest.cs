﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Autofac.Core.Activators.Reflection;
using Autofac.Extras.Moq;
using AutoMapper;
using Moq;
using NUnit.Framework;
using SFA.DAS.Payments.Application.Messaging;
using SFA.DAS.Payments.Application.Repositories;
using SFA.DAS.Payments.EarningEvents.Messages.Events;
using SFA.DAS.Payments.Messages.Common.Events;
using SFA.DAS.Payments.Model.Core;
using SFA.DAS.Payments.Model.Core.Factories;
using SFA.DAS.Payments.Model.Core.Incentives;
using SFA.DAS.Payments.RequiredPayments.Application.Infrastructure;
using SFA.DAS.Payments.RequiredPayments.Application.Mapping;
using SFA.DAS.Payments.RequiredPayments.Application.Processors;
using SFA.DAS.Payments.RequiredPayments.Application.UnitTests.TestHelpers;
using SFA.DAS.Payments.RequiredPayments.Domain;
using SFA.DAS.Payments.RequiredPayments.Domain.Entities;
using SFA.DAS.Payments.RequiredPayments.Model.Entities;

namespace SFA.DAS.Payments.RequiredPayments.Application.UnitTests.Application.Processors
{
    [TestFixture]
    public class FunctionalSkillEarningsEventProcessorTest
    {
        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            var config = new MapperConfiguration(cfg => cfg.AddProfile<RequiredPaymentsProfile>());
            config.AssertConfigurationIsValid();
            mapper = new Mapper(config);
        }

        [SetUp]
        public void SetUp()
        {
            mocker = AutoMock.GetLoose();
            paymentHistoryCacheMock = mocker.Mock<IDataCache<PaymentHistoryEntity[]>>();
            requiredPaymentsService = mocker.Mock<IRequiredPaymentProcessor>();
            mocker.Mock<IDuplicateEarningEventService>()
                .Setup(x => x.IsDuplicate(It.IsAny<IPaymentsEvent>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);
            eventProcessor = mocker.Create<FunctionalSkillEarningsEventProcessor>(
                new NamedParameter("apprenticeshipKey", "key"),
                new NamedParameter("mapper", mapper),
                new AutowiringParameter());
        }

        [TearDown]
        public void TearDown()
        {
            mocker.Dispose();
        }

        private AutoMock mocker;
        private FunctionalSkillEarningsEventProcessor eventProcessor;
        private Mock<IDataCache<PaymentHistoryEntity[]>> paymentHistoryCacheMock;
        private Mock<IRequiredPaymentProcessor> requiredPaymentsService;
        private Mapper mapper;

        [Test]
        public async Task TestHandleNullEvent()
        {
            // arrange            
            // act
            // assert
            try
            {
                await eventProcessor.HandleEarningEvent(null, null, CancellationToken.None);
            }
            catch (ArgumentNullException ex)
            {
                Assert.AreEqual("earningEvent", ex.ParamName);
                return;
            }

            Assert.Fail();
        }

        [Test]
        public async Task TestHandleNormalEvent()
        {
            // arrange
            var period = CollectionPeriodFactory.CreateFromAcademicYearAndPeriod(1819, 2);
            byte deliveryPeriod = 2;

            var learningAim = EarningEventDataHelper.CreateLearningAim();
            learningAim.SequenceNumber = 2;

            var earningEvent = new Act2FunctionalSkillEarningsEvent
            {
                Ukprn = 1,
                CollectionPeriod = period,
                CollectionYear = period.AcademicYear,
                Learner = EarningEventDataHelper.CreateLearner(),
                LearningAim = learningAim,
                Earnings = new ReadOnlyCollection<FunctionalSkillEarning>(new List<FunctionalSkillEarning>
                {
                    new FunctionalSkillEarning
                    {
                        Type = FunctionalSkillType.BalancingMathsAndEnglish,
                        Periods = new ReadOnlyCollection<EarningPeriod>(new List<EarningPeriod>
                        {
                            new EarningPeriod
                            {
                                Period = deliveryPeriod,
                                Amount = 100,
                                PriceEpisodeIdentifier = "2",
                                SfaContributionPercentage = 0.9m
                            }
                        })
                    }
                }),
                PriceEpisodes = new List<PriceEpisode>()
            };

            var requiredPayments = new List<RequiredPayment>
            {
                new RequiredPayment
                {
                    Amount = 100,
                    EarningType = EarningType.Incentive,
                    SfaContributionPercentage = 0.9m,
                    PriceEpisodeIdentifier = "2"
                }
            };

            var paymentHistoryEntities = new[]
            {
                new PaymentHistoryEntity
                {
                    CollectionPeriod = CollectionPeriodFactory.CreateFromAcademicYearAndPeriod(1819, 2),
                    DeliveryPeriod = 2,
                    LearnAimReference = earningEvent.LearningAim.Reference,
                    TransactionType = (int)FunctionalSkillType.BalancingMathsAndEnglish
                }
            };

            paymentHistoryCacheMock.Setup(c =>
                    c.TryGet(It.Is<string>(key => key == CacheKeys.PaymentHistoryKey), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ConditionalValue<PaymentHistoryEntity[]>(true, paymentHistoryEntities))
                .Verifiable();

            requiredPaymentsService.Setup(p =>
                    p.GetRequiredPayments(It.Is<Earning>(x => x.Amount == 100), It.IsAny<List<Payment>>()))
                .Returns(requiredPayments)
                .Verifiable();

            // act
            var actualRequiredPayment = await eventProcessor.HandleEarningEvent(earningEvent,
                paymentHistoryCacheMock.Object, CancellationToken.None);

            // assert
            Assert.IsNotNull(actualRequiredPayment);
            Assert.AreEqual(1, actualRequiredPayment.Count);
            Assert.AreEqual(100, actualRequiredPayment.First().AmountDue);
            Assert.AreEqual(earningEvent.LearningAim.Reference, actualRequiredPayment.First().LearningAim.Reference);
            Assert.AreEqual("2", actualRequiredPayment.First().PriceEpisodeIdentifier);
            Assert.AreEqual(2, actualRequiredPayment.First().LearningAimSequenceNumber);
        }

        [Test]
        public async Task TestNoEventProducedWhenZeroToPay()
        {
            // arrange
            var period = CollectionPeriodFactory.CreateFromAcademicYearAndPeriod(1819, 2);

            var earningEvent = new Act2FunctionalSkillEarningsEvent
            {
                Ukprn = 1,
                CollectionPeriod = period,
                CollectionYear = period.AcademicYear,
                Learner = EarningEventDataHelper.CreateLearner(),
                LearningAim = EarningEventDataHelper.CreateLearningAim(),
                Earnings = new ReadOnlyCollection<FunctionalSkillEarning>(new List<FunctionalSkillEarning>
                {
                    new FunctionalSkillEarning
                    {
                        Type = FunctionalSkillType.OnProgrammeMathsAndEnglish,
                        Periods = new ReadOnlyCollection<EarningPeriod>(new List<EarningPeriod>
                        {
                            new EarningPeriod
                            {
                                Period = 2,
                                Amount = 100,
                                PriceEpisodeIdentifier = "2",
                                SfaContributionPercentage = 0.8m
                            }
                        })
                    }
                })
            };

            var paymentHistoryEntities = new PaymentHistoryEntity[0];

            paymentHistoryCacheMock.Setup(c =>
                    c.TryGet(It.Is<string>(key => key == CacheKeys.PaymentHistoryKey), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ConditionalValue<PaymentHistoryEntity[]>(true, paymentHistoryEntities))
                .Verifiable();
            requiredPaymentsService.Setup(p =>
                    p.GetRequiredPayments(It.Is<Earning>(x => x.Amount == 100), It.IsAny<List<Payment>>()))
                .Returns(new List<RequiredPayment>())
                .Verifiable();

            // act
            var actualRequiredPayment = await eventProcessor.HandleEarningEvent(earningEvent,
                paymentHistoryCacheMock.Object, CancellationToken.None);

            // assert
            Assert.AreEqual(0, actualRequiredPayment.Count);
        }

        [Test]
        public async Task IgnoresDuplicateEvents()
        {
            // arrange
            var period = CollectionPeriodFactory.CreateFromAcademicYearAndPeriod(1819, 2);
            byte deliveryPeriod = 2;

            var earningEvent = new Act2FunctionalSkillEarningsEvent
            {
                Ukprn = 1,
                CollectionPeriod = period,
                CollectionYear = period.AcademicYear,
                Learner = EarningEventDataHelper.CreateLearner(),
                LearningAim = EarningEventDataHelper.CreateLearningAim(),
                Earnings = new ReadOnlyCollection<FunctionalSkillEarning>(new List<FunctionalSkillEarning>
                {
                    new FunctionalSkillEarning
                    {
                        Type = FunctionalSkillType.BalancingMathsAndEnglish,
                        Periods = new ReadOnlyCollection<EarningPeriod>(new List<EarningPeriod>
                        {
                            new EarningPeriod
                            {
                                Period = deliveryPeriod,
                                Amount = 100,
                                PriceEpisodeIdentifier = "2",
                                SfaContributionPercentage = 0.9m
                            }
                        })
                    }
                })
            };

            var requiredPayments = new List<RequiredPayment>
            {
                new RequiredPayment
                {
                    Amount = 100,
                    EarningType = EarningType.Incentive,
                    SfaContributionPercentage = 0.9m,
                    PriceEpisodeIdentifier = "2"
                }
            };

            var paymentHistoryEntities = new[]
            {
                new PaymentHistoryEntity
                {
                    CollectionPeriod = CollectionPeriodFactory.CreateFromAcademicYearAndPeriod(1819, 2),
                    DeliveryPeriod = 2,
                    LearnAimReference = earningEvent.LearningAim.Reference,
                    TransactionType = (int)FunctionalSkillType.BalancingMathsAndEnglish
                }
            };

            paymentHistoryCacheMock.Setup(c =>
                    c.TryGet(It.Is<string>(key => key == CacheKeys.PaymentHistoryKey), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ConditionalValue<PaymentHistoryEntity[]>(true, paymentHistoryEntities));

            requiredPaymentsService.Setup(p =>
                    p.GetRequiredPayments(It.Is<Earning>(x => x.Amount == 100), It.IsAny<List<Payment>>()))
                .Returns(requiredPayments);

            mocker.Mock<IDuplicateEarningEventService>()
                .Setup(x => x.IsDuplicate(It.IsAny<IPaymentsEvent>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            // act
            var actualRequiredPayment = await eventProcessor.HandleEarningEvent(earningEvent,
                paymentHistoryCacheMock.Object, CancellationToken.None);

            // assert
            Assert.IsNotNull(actualRequiredPayment);
            Assert.AreEqual(0, actualRequiredPayment.Count);
        }
    }
}