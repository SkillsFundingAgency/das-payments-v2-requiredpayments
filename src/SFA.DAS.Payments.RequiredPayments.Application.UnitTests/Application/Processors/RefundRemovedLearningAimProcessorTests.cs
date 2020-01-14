﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Autofac.Extras.Moq;
using AutoMapper;
using FluentAssertions;
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
            
            mocker.Provide<IRefundRemovedLearningAimService>(new RefundRemovedLearningAimService());
            mocker.Provide<IPeriodisedRequiredPaymentEventFactory>(new PeriodisedRequiredPaymentEventFactory());
            
            identifiedLearner  = new IdentifiedRemovedLearningAim
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

        private PaymentHistoryEntity CreatePaymentHistoryEntity(FundingSourceType fundingSource, byte deliveryPeriod)
        {
            return new PaymentHistoryEntity
            {
                Amount = 10,
                SfaContributionPercentage = .9M,
                TransactionType = (int)OnProgrammeEarningType.Learning,
                CollectionPeriod = new CollectionPeriod
                {
                    AcademicYear = 1819,
                    Period = 1
                },
                LearnAimReference = "ZPROG001",
                LearnerReferenceNumber = "learning-ref-456",
                PriceEpisodeIdentifier = "pe-1",
                DeliveryPeriod = deliveryPeriod,
                Ukprn = 7,
                LearnerUln = 5,
                ActualEndDate = null,
                CompletionAmount = 3000,
                CompletionStatus = 1,
                ExternalId = Guid.NewGuid(),
                FundingSource = fundingSource,
                InstalmentAmount = 1000,
                NumberOfInstalments = 12,
                PlannedEndDate = DateTime.Today,
                StartDate = DateTime.Today.AddMonths(-12),
            };
        }

        [Test]
        public async Task Refunds_Required_Levy_Payments()
        {
            var historicPayment = CreatePaymentHistoryEntity(FundingSourceType.Levy, 1);
            history.Add(historicPayment);

            mocker.Mock<IDataCache<PaymentHistoryEntity[]>>()
                .Setup(x => x.TryGet(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ConditionalValue<PaymentHistoryEntity[]>(true, history.ToArray()));
        
            var processor = mocker.Create<RefundRemovedLearningAimProcessor>();
            var refunds = await processor.RefundLearningAim(identifiedLearner, mocker.Mock<IDataCache<PaymentHistoryEntity[]>>().Object, CancellationToken.None)
                .ConfigureAwait(false);

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
            var historicPayment = CreatePaymentHistoryEntity(FundingSourceType.CoInvestedSfa, 2);
            history.Add(historicPayment);

            mocker.Mock<IDataCache<PaymentHistoryEntity[]>>()
                .Setup(x => x.TryGet(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ConditionalValue<PaymentHistoryEntity[]>(true, history.ToArray()));
         

            var processor = mocker.Create<RefundRemovedLearningAimProcessor>();
            var refunds = await processor.RefundLearningAim(identifiedLearner, mocker.Mock<IDataCache<PaymentHistoryEntity[]>>().Object, CancellationToken.None).ConfigureAwait(false);
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
            var historicPayment = CreatePaymentHistoryEntity(FundingSourceType.FullyFundedSfa, 3);
            history.Add(historicPayment);
            mocker.Mock<IDataCache<PaymentHistoryEntity[]>>()
                .Setup(x => x.TryGet(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ConditionalValue<PaymentHistoryEntity[]>(true, history.ToArray()));
          
            var processor = mocker.Create<RefundRemovedLearningAimProcessor>();
            var refunds = await processor.RefundLearningAim(identifiedLearner, mocker.Mock<IDataCache<PaymentHistoryEntity[]>>().Object, CancellationToken.None).ConfigureAwait(false);
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
            var historicLevyPayment = CreatePaymentHistoryEntity(FundingSourceType.Levy, 4);
            historicLevyPayment.Amount = 100;
            var historicCoInvestedPayment = CreatePaymentHistoryEntity(FundingSourceType.CoInvestedSfa, 4);
            historicCoInvestedPayment.Amount = 50;
            var historicIncentivePayment = CreatePaymentHistoryEntity(FundingSourceType.FullyFundedSfa, 4);
            historicIncentivePayment.Amount = 25;
            history.Add(historicLevyPayment);
            history.Add(historicCoInvestedPayment);
            history.Add(historicIncentivePayment);

            mocker.Mock<IDataCache<PaymentHistoryEntity[]>>()
                .Setup(x => x.TryGet(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ConditionalValue<PaymentHistoryEntity[]>(true, history.ToArray()));
          
            var processor = mocker.Create<RefundRemovedLearningAimProcessor>();
            var refunds = await processor.RefundLearningAim(identifiedLearner, mocker.Mock<IDataCache<PaymentHistoryEntity[]>>().Object, CancellationToken.None).ConfigureAwait(false);
            refunds.Count.Should().Be(3);
            refunds.Count(refund => refund.AmountDue == historicLevyPayment.Amount * -1 && refund is CalculatedRequiredLevyAmount).Should().Be(1);
            refunds.Count(refund => refund.AmountDue == historicCoInvestedPayment.Amount * -1 && refund is CalculatedRequiredCoInvestedAmount).Should().Be(1);
            refunds.Count(refund => refund.AmountDue == historicIncentivePayment.Amount * -1 && refund is CalculatedRequiredIncentiveAmount).Should().Be(1);
            refunds.Count(refund => refund.DeliveryPeriod == 4).Should().Be(3);
        }


        [Test]
        public async Task Generate_Valid_SfaContribution_For_CoInvested_Refund_Payment()
        {
            var historicIncentivePayment = CreatePaymentHistoryEntity(FundingSourceType.FullyFundedSfa, 2);
            historicIncentivePayment.Amount = 500m;
            historicIncentivePayment.SfaContributionPercentage = 0m;
            historicIncentivePayment.ContractType = ContractType.Act2;
            historicIncentivePayment.TransactionType = (int)TransactionType.Second16To18EmployerIncentive;
            historicIncentivePayment.CollectionPeriod = new CollectionPeriod
            {
                AcademicYear = 1920,
                Period = 3
            };
            history.Add(historicIncentivePayment);

            var historicCoInvestedPayment = CreatePaymentHistoryEntity(FundingSourceType.CoInvestedSfa, 2);
            historicCoInvestedPayment.Amount = 160;
            historicCoInvestedPayment.SfaContributionPercentage = 1m;
            historicCoInvestedPayment.ContractType = ContractType.Act2;
            historicCoInvestedPayment.TransactionType = (int)TransactionType.Learning;
            historicCoInvestedPayment.CollectionPeriod = new CollectionPeriod
            {
                AcademicYear = 1920,
                Period = 3
            };
            history.Add(historicCoInvestedPayment);


            identifiedLearner.CollectionPeriod = new CollectionPeriod
            {
                AcademicYear = 1920,
                Period = 5
            };
            
            mocker.Mock<IDataCache<PaymentHistoryEntity[]>>()
                .Setup(x => x.TryGet(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ConditionalValue<PaymentHistoryEntity[]>(true, history.ToArray()));

            var processor = mocker.Create<RefundRemovedLearningAimProcessor>();
            var refunds = await processor.RefundLearningAim(identifiedLearner,
                mocker.Mock<IDataCache<PaymentHistoryEntity[]>>().Object,
                CancellationToken.None)
                .ConfigureAwait(false);

            var refund = refunds.FirstOrDefault(o => o is CalculatedRequiredCoInvestedAmount);
            refund.Should().NotBeNull();

            var coInvestedAmountPayment = refund as CalculatedRequiredCoInvestedAmount;
            coInvestedAmountPayment.SfaContributionPercentage.Should().Be(1m);
        }

    }
}