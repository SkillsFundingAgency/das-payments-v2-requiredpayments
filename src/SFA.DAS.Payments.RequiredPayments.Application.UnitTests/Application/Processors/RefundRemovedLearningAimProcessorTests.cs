﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Autofac.Extras.Moq;
using AutoMapper;
using FluentAssertions;
using FluentAssertions.Execution;
using Moq;
using NUnit.Framework;
using SFA.DAS.Payments.Application.Infrastructure.Logging;
using SFA.DAS.Payments.Application.Repositories;
using SFA.DAS.Payments.Model.Core;
using SFA.DAS.Payments.Model.Core.Entities;
using SFA.DAS.Payments.Model.Core.Incentives;
using SFA.DAS.Payments.Model.Core.OnProgramme;
using SFA.DAS.Payments.RequiredPayments.Application.Mapping;
using SFA.DAS.Payments.RequiredPayments.Application.Processors;
using SFA.DAS.Payments.RequiredPayments.Application.UnitTests.TestHelpers;
using SFA.DAS.Payments.RequiredPayments.Domain;
using SFA.DAS.Payments.RequiredPayments.Domain.Entities;
using SFA.DAS.Payments.RequiredPayments.Domain.Services;
using SFA.DAS.Payments.RequiredPayments.Messages.Events;
using SFA.DAS.Payments.RequiredPayments.Model.Entities;

namespace SFA.DAS.Payments.RequiredPayments.Application.UnitTests.Application.Processors
{
    [TestFixture]
    public class RefundRemovedLearningAimProcessorTests
    {
        private AutoMock mocker;
        private List<PaymentHistoryEntity> history;
        private IdentifiedRemovedLearningAim identifiedLearner;

        [SetUp]
        public void SetUp()
        {
            mocker = AutoMock.GetLoose();
            history = new List<PaymentHistoryEntity>();
            var config = new MapperConfiguration(cfg => cfg.AddProfile<RequiredPaymentsProfile>());
            config.AssertConfigurationIsValid();
            var mapper = new Mapper(config);
            mocker.Provide<IMapper>(mapper);
            identifiedLearner = identifiedLearner = new IdentifiedRemovedLearningAim
            {
                CollectionPeriod = new CollectionPeriod
                {
                    AcademicYear = 1819,
                    Period = 1
                },
                EventId = Guid.NewGuid(),
                EventTime = DateTimeOffset.UtcNow,
                IlrSubmissionDateTime = DateTime.Now,
                JobId = 1,
                Learner = new Learner
                {
                    ReferenceNumber = "learner-ref-123",
                    Uln = 2
                },
                LearningAim = new LearningAim
                {
                    FrameworkCode = 3,
                    FundingLineType = "funding line type",
                    PathwayCode = 4,
                    ProgrammeType = 5,
                    Reference = "learning-ref-456",
                    StandardCode = 6
                },
                Ukprn = 7
            };
        }

        private PaymentHistoryEntity CreatePaymentHistoryEntity(FundingSourceType fundingSource, byte collectionPeriod,
            byte deliveryPeriod, TransactionType transactionType = TransactionType.Learning)
        {
            return new PaymentHistoryEntity
            {
                Amount = 10,
                SfaContributionPercentage = .9M,
                TransactionType = (int) transactionType,
                CollectionPeriod = new CollectionPeriod
                {
                    AcademicYear = 1819,
                    Period = collectionPeriod
                },
                LearnAimReference = "aim-ref-123",
                LearnerReferenceNumber = "learning-ref-456",
                PriceEpisodeIdentifier = "pe-1",
                DeliveryPeriod = deliveryPeriod,
                Ukprn = 7,
                ActualEndDate = null,
                CompletionAmount = 3000,
                CompletionStatus = 1,
                ExternalId = Guid.NewGuid(),
                FundingSource = fundingSource,
                InstalmentAmount = 1000,
                NumberOfInstalments = 12,
                PlannedEndDate = DateTime.Today,
                StartDate = DateTime.Today.AddMonths(-12)
            };
        }


        [Test]
        public async Task RefundsCorrectTypesBasedOnHistory()
        {
            var historicalCompletionPayment =
                CreatePaymentHistoryEntity(FundingSourceType.CoInvestedSfa, 3, 1, TransactionType.Completion);
            historicalCompletionPayment.Amount = 1000;

            var historicalBalancingPayment =
                CreatePaymentHistoryEntity(FundingSourceType.CoInvestedSfa, 3, 1, TransactionType.Balancing);
            historicalBalancingPayment.Amount = 166.66m;

            var historicalCompletionUplift = CreatePaymentHistoryEntity(FundingSourceType.FullyFundedSfa, 3, 1,
                TransactionType.Completion16To18FrameworkUplift);
            historicalCompletionUplift.Amount = 200m;

            var historicalBalancingUpliftPayment = CreatePaymentHistoryEntity(FundingSourceType.FullyFundedSfa, 3, 1,
                TransactionType.Balancing16To18FrameworkUplift);
            historicalBalancingUpliftPayment.Amount = 33.33m;

            history.Add(historicalCompletionPayment);
            history.Add(historicalBalancingPayment);
            history.Add(historicalCompletionUplift);
            history.Add(historicalBalancingUpliftPayment);
            mocker.Mock<IDataCache<PaymentHistoryEntity[]>>()
                .Setup(x => x.TryGet(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ConditionalValue<PaymentHistoryEntity[]>(true, history.ToArray()));
           
            mocker.Provide<IPeriodisedRequiredPaymentEventFactory, PeriodisedRequiredPaymentEventFactory>();
            mocker.Provide<IRefundService, RefundService>();
            mocker.Provide<IPaymentDueProcessor, PaymentDueProcessor>();
            mocker.Provide<IRefundRemovedLearningAimService, RefundRemovedLearningAimService>();

            var processor = mocker.Create<RefundRemovedLearningAimProcessor>();
            var refunds = await processor.RefundLearningAim(identifiedLearner,
                mocker.Mock<IDataCache<PaymentHistoryEntity[]>>().Object, CancellationToken.None).ConfigureAwait(false);
            refunds.Count.Should().Be(4);
            refunds[0].Should().BeOfType<CalculatedRequiredCoInvestedAmount>();
            refunds[0].AmountDue.Should().Be(-1 * historicalCompletionPayment.Amount);
            refunds[1].Should().BeOfType<CalculatedRequiredCoInvestedAmount>();
            refunds[1].AmountDue.Should().Be(-1 * historicalBalancingPayment.Amount);
            refunds[2].Should().BeOfType<CalculatedRequiredIncentiveAmount>();
            refunds[2].AmountDue.Should().Be(-1 * historicalCompletionUplift.Amount);
            refunds[3].Should().BeOfType<CalculatedRequiredIncentiveAmount>();
            refunds[3].AmountDue.Should().Be(-1 * historicalBalancingUpliftPayment.Amount);
        }

          [Test]
        public async Task ReturnsNullAndLogsWhenInvalidTransactionTypeForFundingSource()
        {
           var mockLogger = mocker.Mock<IPaymentLogger>();
           mockLogger.Setup(x => x.LogWarning(It.IsAny<string>(),  It.IsAny<object[]>(),
                It.IsAny<long>(),
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>())).Verifiable();

            history.Add( CreatePaymentHistoryEntity(FundingSourceType.FullyFundedSfa, 3, 1, TransactionType.Completion));
          
            mocker.Mock<IDataCache<PaymentHistoryEntity[]>>()
                .Setup(x => x.TryGet(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ConditionalValue<PaymentHistoryEntity[]>(true, history.ToArray()));

            mocker.Provide<IPeriodisedRequiredPaymentEventFactory, PeriodisedRequiredPaymentEventFactory>();
            mocker.Provide<IRefundService, RefundService>();
            mocker.Provide<IPaymentDueProcessor, PaymentDueProcessor>();
            mocker.Provide<IRefundRemovedLearningAimService, RefundRemovedLearningAimService>();

            var processor = mocker.Create<RefundRemovedLearningAimProcessor>();
            var refunds = await processor.RefundLearningAim(identifiedLearner,
                mocker.Mock<IDataCache<PaymentHistoryEntity[]>>().Object, CancellationToken.None).ConfigureAwait(false);

            refunds.Should().HaveCount(0);

            mockLogger.Verify(x => x.LogWarning(It.IsAny<string>(),  It.IsAny<object[]>(),
                It.IsAny<long>(),
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()),Times.Once);;

        }




        [Test]
        public async Task Refunds_Required_Levy_Payments()
        {
            var historicPayment = CreatePaymentHistoryEntity(FundingSourceType.FullyFundedSfa, 1, 1);
            history.Add(historicPayment);
            mocker.Mock<IDataCache<PaymentHistoryEntity[]>>()
                .Setup(x => x.TryGet(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ConditionalValue<PaymentHistoryEntity[]>(true, history.ToArray()));
            mocker.Mock<IRefundRemovedLearningAimService>()
                .Setup(x => x.RefundLearningAim(It.IsAny<List<Payment>>()))
                .Returns(new List<(byte deliveryPeriod, RequiredPayment payment)>
                {
                    (1,
                        new RequiredPayment
                        {
                            Amount = -10, SfaContributionPercentage = .95M, EarningType = EarningType.Levy,
                            PriceEpisodeIdentifier = "pe-1"
                        })
                });
            mocker.Mock<IPeriodisedRequiredPaymentEventFactory>()
                .Setup(x => x.Create(It.IsAny<EarningType>(), It.IsAny<int>()))
                .Returns(new CalculatedRequiredLevyAmount
                {
                    OnProgrammeEarningType = OnProgrammeEarningType.Learning,
                });

            var processor = mocker.Create<RefundRemovedLearningAimProcessor>();
            var refunds = await processor.RefundLearningAim(identifiedLearner,
                mocker.Mock<IDataCache<PaymentHistoryEntity[]>>().Object, CancellationToken.None).ConfigureAwait(false);
            refunds.Count.Should().Be(1);
            var refund = refunds.First();
            var levyRefund = refund as CalculatedRequiredLevyAmount;
            levyRefund.Should().NotBeNull();
            levyRefund.AmountDue.Should().Be(-10);
            levyRefund.ShouldBeMappedTo(historicPayment);
            levyRefund.ShouldBeMappedTo(identifiedLearner);
            levyRefund.DeliveryPeriod.Should().Be(1);
        }

        [Test]
        public async Task Refunds_Required_CoInvested_Payments()
        {
            var historicPayment = CreatePaymentHistoryEntity(FundingSourceType.CoInvestedSfa, 1, 2);
            history.Add(historicPayment);
            mocker.Mock<IDataCache<PaymentHistoryEntity[]>>()
                .Setup(x => x.TryGet(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ConditionalValue<PaymentHistoryEntity[]>(true, history.ToArray()));
            mocker.Mock<IRefundRemovedLearningAimService>()
                .Setup(x => x.RefundLearningAim(It.IsAny<List<Payment>>()))
                .Returns(new List<(byte deliveryPeriod, RequiredPayment payment)>
                {
                    (2,
                        new RequiredPayment
                        {
                            Amount = -10, SfaContributionPercentage = .95M, EarningType = EarningType.CoInvested,
                            PriceEpisodeIdentifier = "pe-1"
                        })
                });
            mocker.Mock<IPeriodisedRequiredPaymentEventFactory>()
                .Setup(x => x.Create(It.IsAny<EarningType>(), It.IsAny<int>()))
                .Returns(new CalculatedRequiredCoInvestedAmount
                {
                    OnProgrammeEarningType = OnProgrammeEarningType.Learning
                });

            var processor = mocker.Create<RefundRemovedLearningAimProcessor>();
            var refunds = await processor.RefundLearningAim(identifiedLearner,
                mocker.Mock<IDataCache<PaymentHistoryEntity[]>>().Object, CancellationToken.None).ConfigureAwait(false);
            refunds.Count.Should().Be(1);
            var refund = refunds.First();
            var conInvestRefund = refund as CalculatedRequiredCoInvestedAmount;
            conInvestRefund.Should().NotBeNull();
            conInvestRefund.AmountDue.Should().Be(-10);
            conInvestRefund.ShouldBeMappedTo(historicPayment);
            conInvestRefund.ShouldBeMappedTo(identifiedLearner);
            conInvestRefund.DeliveryPeriod.Should().Be(2);
        }

        [Test]
        public async Task Refunds_Required_Incentive_Payments()
        {
            var historicPayment = CreatePaymentHistoryEntity(FundingSourceType.FullyFundedSfa, 1, 3);
            history.Add(historicPayment);
            mocker.Mock<IDataCache<PaymentHistoryEntity[]>>()
                .Setup(x => x.TryGet(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ConditionalValue<PaymentHistoryEntity[]>(true, history.ToArray()));
            mocker.Mock<IRefundRemovedLearningAimService>()
                .Setup(x => x.RefundLearningAim(It.IsAny<List<Payment>>()))
                .Returns(new List<(byte deliveryPeriod, RequiredPayment payment)>
                {
                    (3,
                        new RequiredPayment
                        {
                            Amount = -10, SfaContributionPercentage = .95M, EarningType = EarningType.Incentive,
                            PriceEpisodeIdentifier = "pe-1"
                        })
                });
            mocker.Mock<IPeriodisedRequiredPaymentEventFactory>()
                .Setup(x => x.Create(It.IsAny<EarningType>(), It.IsAny<int>()))
                .Returns(new CalculatedRequiredIncentiveAmount
                {
                    Type = IncentivePaymentType.Balancing16To18FrameworkUplift
                });

            var processor = mocker.Create<RefundRemovedLearningAimProcessor>();
            var refunds = await processor.RefundLearningAim(identifiedLearner,
                mocker.Mock<IDataCache<PaymentHistoryEntity[]>>().Object, CancellationToken.None).ConfigureAwait(false);
            refunds.Count.Should().Be(1);
            var refund = refunds.First();
            var incentiveRefund = refund as CalculatedRequiredIncentiveAmount;
            incentiveRefund.Should().NotBeNull();
            incentiveRefund.AmountDue.Should().Be(-10);
            incentiveRefund.ShouldBeMappedTo(historicPayment);
            incentiveRefund.ShouldBeMappedTo(identifiedLearner);
            incentiveRefund.DeliveryPeriod.Should().Be(3);
        }

        [Test]
        public async Task Refunds_All_Required_Payments()
        {
            var historicLevyPayment = CreatePaymentHistoryEntity(FundingSourceType.Levy, 1, 4);
            historicLevyPayment.Amount = 100;
            var historicCoInvestedPayment = CreatePaymentHistoryEntity(FundingSourceType.CoInvestedSfa, 1, 4);
            historicCoInvestedPayment.Amount = 50;
            var historicIncentivePayment = CreatePaymentHistoryEntity(FundingSourceType.FullyFundedSfa, 1, 4);
            historicIncentivePayment.Amount = 25;
            history.Add(historicLevyPayment);
            history.Add(historicCoInvestedPayment);
            history.Add(historicIncentivePayment);
            mocker.Mock<IDataCache<PaymentHistoryEntity[]>>()
                .Setup(x => x.TryGet(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ConditionalValue<PaymentHistoryEntity[]>(true, history.ToArray()));
            mocker.Mock<IRefundRemovedLearningAimService>()
                .Setup(x => x.RefundLearningAim(It.IsAny<List<Payment>>()))
                .Returns(new List<(byte deliveryPeriod, RequiredPayment payment)>
                {
                    (4,
                        new RequiredPayment
                        {
                            Amount = historicLevyPayment.Amount * -1, SfaContributionPercentage = .95M,
                            EarningType = EarningType.Levy, PriceEpisodeIdentifier = "pe-1"
                        }),
                    (4,
                        new RequiredPayment
                        {
                            Amount = historicCoInvestedPayment.Amount * -1, SfaContributionPercentage = .95M,
                            EarningType = EarningType.CoInvested, PriceEpisodeIdentifier = "pe-1"
                        }),
                    (4,
                        new RequiredPayment
                        {
                            Amount = historicIncentivePayment.Amount * -1, SfaContributionPercentage = .95M,
                            EarningType = EarningType.Incentive, PriceEpisodeIdentifier = "pe-1"
                        })
                });

            mocker.Mock<IPeriodisedRequiredPaymentEventFactory>()
                .Setup(x => x.Create(It.Is<EarningType>(et => et == EarningType.Levy), It.IsAny<int>()))
                .Returns(new CalculatedRequiredLevyAmount
                {
                    OnProgrammeEarningType = OnProgrammeEarningType.Learning,
                });

            mocker.Mock<IPeriodisedRequiredPaymentEventFactory>()
                .Setup(x => x.Create(It.Is<EarningType>(et => et == EarningType.CoInvested), It.IsAny<int>()))
                .Returns(new CalculatedRequiredCoInvestedAmount
                {
                    OnProgrammeEarningType = OnProgrammeEarningType.Learning
                });

            mocker.Mock<IPeriodisedRequiredPaymentEventFactory>()
                .Setup(x => x.Create(It.Is<EarningType>(et => et == EarningType.Incentive), It.IsAny<int>()))
                .Returns(new CalculatedRequiredIncentiveAmount
                {
                    Type = IncentivePaymentType.Balancing16To18FrameworkUplift
                });

            var processor = mocker.Create<RefundRemovedLearningAimProcessor>();
            var refunds = await processor.RefundLearningAim(identifiedLearner,
                mocker.Mock<IDataCache<PaymentHistoryEntity[]>>().Object, CancellationToken.None).ConfigureAwait(false);
            refunds.Count.Should().Be(3);
            refunds.Count(refund =>
                    refund.AmountDue == historicLevyPayment.Amount * -1 && refund is CalculatedRequiredLevyAmount)
                .Should()
                .Be(1);
            refunds.Count(refund =>
                refund.AmountDue == historicCoInvestedPayment.Amount * -1 &&
                refund is CalculatedRequiredCoInvestedAmount).Should().Be(1);
            refunds.Count(refund =>
                    refund.AmountDue == historicIncentivePayment.Amount * -1 &&
                    refund is CalculatedRequiredIncentiveAmount)
                .Should().Be(1);
            refunds.Count(refund => refund.DeliveryPeriod == 4).Should().Be(3);
        }

        [Test]
        public async Task Clawback_ME_OnProgramme_And_Balancing_Payments()
        {
            // arrange
            mocker.Provide<IRefundService, RefundService>();
            mocker.Provide<IPaymentDueProcessor, PaymentDueProcessor>();
            mocker.Provide<IPeriodisedRequiredPaymentEventFactory, PeriodisedRequiredPaymentEventFactory>();
            mocker.Provide<IRefundRemovedLearningAimService, RefundRemovedLearningAimService>();

            var historicOnProgrammeMEPayment = CreatePaymentHistoryEntity(FundingSourceType.FullyFundedSfa, 2, 1);
            historicOnProgrammeMEPayment.Amount = 39.25m;
            historicOnProgrammeMEPayment.TransactionType = (int) TransactionType.OnProgrammeMathsAndEnglish;
            historicOnProgrammeMEPayment.PriceEpisodeIdentifier = null;
            history.Add(historicOnProgrammeMEPayment);

            var historicBalancingMEPayment = CreatePaymentHistoryEntity(FundingSourceType.FullyFundedSfa, 2, 2);
            historicBalancingMEPayment.Amount = 39.25m;
            historicBalancingMEPayment.TransactionType = (int) TransactionType.BalancingMathsAndEnglish;
            historicBalancingMEPayment.PriceEpisodeIdentifier = null;
            history.Add(historicBalancingMEPayment);

            var mockPaymentHistoryCache = mocker.Mock<IDataCache<PaymentHistoryEntity[]>>();
            mockPaymentHistoryCache.Setup(x => x.TryGet(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ConditionalValue<PaymentHistoryEntity[]>(true, history.ToArray()));
            identifiedLearner.CollectionPeriod.Period = 3;


            // act
            var processor = mocker.Create<RefundRemovedLearningAimProcessor>();
            var refunds = await processor
                .RefundLearningAim(identifiedLearner,
                    mockPaymentHistoryCache.Object,
                    CancellationToken.None)
                .ConfigureAwait(false);

            // assert
            using (new AssertionScope())
            {
                refunds.Should().HaveCount(2);
                refunds.Cast<CalculatedRequiredIncentiveAmount>().Should()
                    .ContainSingle(x => x.Type == IncentivePaymentType.BalancingMathsAndEnglish);
                refunds.Cast<CalculatedRequiredIncentiveAmount>().Should()
                    .ContainSingle(x => x.Type == IncentivePaymentType.OnProgrammeMathsAndEnglish);
            }
        }

        [Test]
        public async Task Clawback_OnProgramme_And_Completion_Payments_WhenLearnerRefNumber_Changes()
        {
            // arrange
            mocker.Provide<IRefundService, RefundService>();
            mocker.Provide<IPaymentDueProcessor, PaymentDueProcessor>();
            mocker.Provide<IPeriodisedRequiredPaymentEventFactory, PeriodisedRequiredPaymentEventFactory>();
            mocker.Provide<IRefundRemovedLearningAimService, RefundRemovedLearningAimService>();

            var historicOnProgrammePayment = CreatePaymentHistoryEntity(FundingSourceType.Levy, 2, 1);
            historicOnProgrammePayment.Amount = 133.33333m;
            history.Add(historicOnProgrammePayment);

            var historicCompletionPayment = CreatePaymentHistoryEntity(FundingSourceType.Levy, 2, 2);
            historicCompletionPayment.Amount = 400.0m;
            historicCompletionPayment.TransactionType = (int) TransactionType.Completion;
            history.Add(historicCompletionPayment);

            var mockPaymentHistoryCache = mocker.Mock<IDataCache<PaymentHistoryEntity[]>>();
            mockPaymentHistoryCache.Setup(x => x.TryGet(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ConditionalValue<PaymentHistoryEntity[]>(true, history.ToArray()));
            identifiedLearner.CollectionPeriod.Period = 3;
            identifiedLearner.Learner.ReferenceNumber = "KEY053";


            // act
            var processor = mocker.Create<RefundRemovedLearningAimProcessor>();
            var refunds = await processor
                .RefundLearningAim(identifiedLearner,
                    mockPaymentHistoryCache.Object,
                    CancellationToken.None)
                .ConfigureAwait(false);

            // assert
            using (new AssertionScope())
            {
                refunds.Should().HaveCount(2);
                refunds.Cast<CalculatedRequiredLevyAmount>().Should().ContainSingle(x =>
                    x.OnProgrammeEarningType == OnProgrammeEarningType.Learning);
                refunds.Cast<CalculatedRequiredLevyAmount>().Should().ContainSingle(x =>
                    x.OnProgrammeEarningType == OnProgrammeEarningType.Completion);
            }
        }
    }
}