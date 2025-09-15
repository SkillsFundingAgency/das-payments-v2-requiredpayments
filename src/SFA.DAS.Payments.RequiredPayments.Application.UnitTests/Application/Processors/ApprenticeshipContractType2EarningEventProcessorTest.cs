using Autofac;
using Autofac.Core.Activators.Reflection;
using Autofac.Extras.Moq;
using AutoFixture.NUnit3;
using AutoMapper;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using SFA.DAS.Payments.Application.Infrastructure.Logging;
using SFA.DAS.Payments.Application.Messaging;
using SFA.DAS.Payments.Application.Repositories;
using SFA.DAS.Payments.EarningEvents.Messages.Events;
using SFA.DAS.Payments.Messages.Common.Events;
using SFA.DAS.Payments.Model.Core;
using SFA.DAS.Payments.Model.Core.Entities;
using SFA.DAS.Payments.Model.Core.Factories;
using SFA.DAS.Payments.Model.Core.OnProgramme;
using SFA.DAS.Payments.RequiredPayments.Application.Infrastructure;
using SFA.DAS.Payments.RequiredPayments.Application.Mapping;
using SFA.DAS.Payments.RequiredPayments.Application.Processors;
using SFA.DAS.Payments.RequiredPayments.Application.UnitTests.TestHelpers;
using SFA.DAS.Payments.RequiredPayments.Domain;
using SFA.DAS.Payments.RequiredPayments.Domain.Entities;
using SFA.DAS.Payments.RequiredPayments.Messages.Events;
using SFA.DAS.Payments.RequiredPayments.Model.Entities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Earning = SFA.DAS.Payments.RequiredPayments.Domain.Entities.Earning;

namespace SFA.DAS.Payments.RequiredPayments.Application.UnitTests.Application.Processors
{
    [TestFixture]
    public class ApprenticeshipContractType2EarningEventProcessorTest
    {
        private AutoMock mocker;
        private ApprenticeshipContractType2EarningEventProcessor act2EarningEventProcessor;
        private Mock<IDataCache<PaymentHistoryEntity[]>> paymentHistoryCacheMock;
        private Mock<IRequiredPaymentProcessor> requiredPaymentService;
        private Mock<INegativeEarningService> negativeEarningsService;

        private Mapper mapper;

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
            requiredPaymentService = mocker.Mock<IRequiredPaymentProcessor>();
            negativeEarningsService = mocker.Mock<INegativeEarningService>();
            mocker.Mock<IPaymentLogger>();
            mocker.Mock<IDuplicateEarningEventService>()
                .Setup(x => x.IsDuplicate(It.IsAny<IPaymentsEvent>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);
            act2EarningEventProcessor = mocker.Create<ApprenticeshipContractType2EarningEventProcessor>(
                new NamedParameter("apprenticeshipKey", "key"),
                new NamedParameter("mapper", mapper),
                new AutowiringParameter());
        }

        [TearDown]
        public void TearDown()
        {
            mocker.Dispose();
        }

        [Test]
        public async Task TestHandleNullEvent()
        {
            // arrange
            // act
            // assert
            try
            {
                await act2EarningEventProcessor.HandleEarningEvent(null, paymentHistoryCacheMock.Object, CancellationToken.None);
            }
            catch (ArgumentNullException ex)
            {
                ClassicAssert.AreEqual("earningEvent", ex.ParamName);
                return;
            }

            Assert.Fail();
        }

        [Test]
        public async Task  Test()
        {
            // arrange
            var period = CollectionPeriodFactory.CreateFromAcademicYearAndPeriod(1819, 2);
            byte deliveryPeriod = 2;

            var message = "{\"IsPayable\":true,\"SfaContributionPercentage\":1,\"StartDate\":\"2021-03-15T00:00:00+00:00\",\"OnProgrammeEarnings\":[{\"Type\":3,\"Periods\":[{\"PriceEpisodeIdentifier\":null,\"Period\":1,\"Amount\":0,\"SfaContributionPercentage\":null,\"AccountId\":null,\"ApprenticeshipId\":null,\"ApprenticeshipPriceEpisodeId\":null,\"ApprenticeshipEmployerType\":0,\"TransferSenderAccountId\":null,\"Priority\":null,\"DataLockFailures\":[],\"AgreedOnDate\":null},{\"PriceEpisodeIdentifier\":null,\"Period\":2,\"Amount\":0,\"SfaContributionPercentage\":null,\"AccountId\":null,\"ApprenticeshipId\":null,\"ApprenticeshipPriceEpisodeId\":null,\"ApprenticeshipEmployerType\":0,\"TransferSenderAccountId\":null,\"Priority\":null,\"DataLockFailures\":[],\"AgreedOnDate\":null},{\"PriceEpisodeIdentifier\":null,\"Period\":3,\"Amount\":0,\"SfaContributionPercentage\":null,\"AccountId\":null,\"ApprenticeshipId\":null,\"ApprenticeshipPriceEpisodeId\":null,\"ApprenticeshipEmployerType\":0,\"TransferSenderAccountId\":null,\"Priority\":null,\"DataLockFailures\":[],\"AgreedOnDate\":null},{\"PriceEpisodeIdentifier\":null,\"Period\":4,\"Amount\":0,\"SfaContributionPercentage\":null,\"AccountId\":null,\"ApprenticeshipId\":null,\"ApprenticeshipPriceEpisodeId\":null,\"ApprenticeshipEmployerType\":0,\"TransferSenderAccountId\":null,\"Priority\":null,\"DataLockFailures\":[],\"AgreedOnDate\":null},{\"PriceEpisodeIdentifier\":null,\"Period\":5,\"Amount\":0,\"SfaContributionPercentage\":null,\"AccountId\":null,\"ApprenticeshipId\":null,\"ApprenticeshipPriceEpisodeId\":null,\"ApprenticeshipEmployerType\":0,\"TransferSenderAccountId\":null,\"Priority\":null,\"DataLockFailures\":[],\"AgreedOnDate\":null},{\"PriceEpisodeIdentifier\":null,\"Period\":6,\"Amount\":0,\"SfaContributionPercentage\":null,\"AccountId\":null,\"ApprenticeshipId\":null,\"ApprenticeshipPriceEpisodeId\":null,\"ApprenticeshipEmployerType\":0,\"TransferSenderAccountId\":null,\"Priority\":null,\"DataLockFailures\":[],\"AgreedOnDate\":null},{\"PriceEpisodeIdentifier\":null,\"Period\":7,\"Amount\":0,\"SfaContributionPercentage\":null,\"AccountId\":null,\"ApprenticeshipId\":null,\"ApprenticeshipPriceEpisodeId\":null,\"ApprenticeshipEmployerType\":0,\"TransferSenderAccountId\":null,\"Priority\":null,\"DataLockFailures\":[],\"AgreedOnDate\":null},{\"PriceEpisodeIdentifier\":null,\"Period\":8,\"Amount\":0,\"SfaContributionPercentage\":null,\"AccountId\":null,\"ApprenticeshipId\":null,\"ApprenticeshipPriceEpisodeId\":null,\"ApprenticeshipEmployerType\":0,\"TransferSenderAccountId\":null,\"Priority\":null,\"DataLockFailures\":[],\"AgreedOnDate\":null},{\"PriceEpisodeIdentifier\":null,\"Period\":9,\"Amount\":0,\"SfaContributionPercentage\":null,\"AccountId\":null,\"ApprenticeshipId\":null,\"ApprenticeshipPriceEpisodeId\":null,\"ApprenticeshipEmployerType\":0,\"TransferSenderAccountId\":null,\"Priority\":null,\"DataLockFailures\":[],\"AgreedOnDate\":null},{\"PriceEpisodeIdentifier\":null,\"Period\":10,\"Amount\":0,\"SfaContributionPercentage\":null,\"AccountId\":null,\"ApprenticeshipId\":null,\"ApprenticeshipPriceEpisodeId\":null,\"ApprenticeshipEmployerType\":0,\"TransferSenderAccountId\":null,\"Priority\":null,\"DataLockFailures\":[],\"AgreedOnDate\":null},{\"PriceEpisodeIdentifier\":null,\"Period\":11,\"Amount\":0,\"SfaContributionPercentage\":null,\"AccountId\":null,\"ApprenticeshipId\":null,\"ApprenticeshipPriceEpisodeId\":null,\"ApprenticeshipEmployerType\":0,\"TransferSenderAccountId\":null,\"Priority\":null,\"DataLockFailures\":[],\"AgreedOnDate\":null},{\"PriceEpisodeIdentifier\":null,\"Period\":12,\"Amount\":0,\"SfaContributionPercentage\":null,\"AccountId\":null,\"ApprenticeshipId\":null,\"ApprenticeshipPriceEpisodeId\":null,\"ApprenticeshipEmployerType\":0,\"TransferSenderAccountId\":null,\"Priority\":null,\"DataLockFailures\":[],\"AgreedOnDate\":null}],\"CensusDate\":\"0001-01-01T00:00:00\"},{\"Type\":2,\"Periods\":[{\"PriceEpisodeIdentifier\":null,\"Period\":1,\"Amount\":0,\"SfaContributionPercentage\":null,\"AccountId\":null,\"ApprenticeshipId\":null,\"ApprenticeshipPriceEpisodeId\":null,\"ApprenticeshipEmployerType\":0,\"TransferSenderAccountId\":null,\"Priority\":null,\"DataLockFailures\":[],\"AgreedOnDate\":null},{\"PriceEpisodeIdentifier\":null,\"Period\":2,\"Amount\":0,\"SfaContributionPercentage\":null,\"AccountId\":null,\"ApprenticeshipId\":null,\"ApprenticeshipPriceEpisodeId\":null,\"ApprenticeshipEmployerType\":0,\"TransferSenderAccountId\":null,\"Priority\":null,\"DataLockFailures\":[],\"AgreedOnDate\":null},{\"PriceEpisodeIdentifier\":null,\"Period\":3,\"Amount\":0,\"SfaContributionPercentage\":null,\"AccountId\":null,\"ApprenticeshipId\":null,\"ApprenticeshipPriceEpisodeId\":null,\"ApprenticeshipEmployerType\":0,\"TransferSenderAccountId\":null,\"Priority\":null,\"DataLockFailures\":[],\"AgreedOnDate\":null},{\"PriceEpisodeIdentifier\":null,\"Period\":4,\"Amount\":0,\"SfaContributionPercentage\":null,\"AccountId\":null,\"ApprenticeshipId\":null,\"ApprenticeshipPriceEpisodeId\":null,\"ApprenticeshipEmployerType\":0,\"TransferSenderAccountId\":null,\"Priority\":null,\"DataLockFailures\":[],\"AgreedOnDate\":null},{\"PriceEpisodeIdentifier\":null,\"Period\":5,\"Amount\":0,\"SfaContributionPercentage\":null,\"AccountId\":null,\"ApprenticeshipId\":null,\"ApprenticeshipPriceEpisodeId\":null,\"ApprenticeshipEmployerType\":0,\"TransferSenderAccountId\":null,\"Priority\":null,\"DataLockFailures\":[],\"AgreedOnDate\":null},{\"PriceEpisodeIdentifier\":null,\"Period\":6,\"Amount\":0,\"SfaContributionPercentage\":null,\"AccountId\":null,\"ApprenticeshipId\":null,\"ApprenticeshipPriceEpisodeId\":null,\"ApprenticeshipEmployerType\":0,\"TransferSenderAccountId\":null,\"Priority\":null,\"DataLockFailures\":[],\"AgreedOnDate\":null},{\"PriceEpisodeIdentifier\":null,\"Period\":7,\"Amount\":0,\"SfaContributionPercentage\":null,\"AccountId\":null,\"ApprenticeshipId\":null,\"ApprenticeshipPriceEpisodeId\":null,\"ApprenticeshipEmployerType\":0,\"TransferSenderAccountId\":null,\"Priority\":null,\"DataLockFailures\":[],\"AgreedOnDate\":null},{\"PriceEpisodeIdentifier\":null,\"Period\":8,\"Amount\":0,\"SfaContributionPercentage\":null,\"AccountId\":null,\"ApprenticeshipId\":null,\"ApprenticeshipPriceEpisodeId\":null,\"ApprenticeshipEmployerType\":0,\"TransferSenderAccountId\":null,\"Priority\":null,\"DataLockFailures\":[],\"AgreedOnDate\":null},{\"PriceEpisodeIdentifier\":null,\"Period\":9,\"Amount\":0,\"SfaContributionPercentage\":null,\"AccountId\":null,\"ApprenticeshipId\":null,\"ApprenticeshipPriceEpisodeId\":null,\"ApprenticeshipEmployerType\":0,\"TransferSenderAccountId\":null,\"Priority\":null,\"DataLockFailures\":[],\"AgreedOnDate\":null},{\"PriceEpisodeIdentifier\":null,\"Period\":10,\"Amount\":0,\"SfaContributionPercentage\":null,\"AccountId\":null,\"ApprenticeshipId\":null,\"ApprenticeshipPriceEpisodeId\":null,\"ApprenticeshipEmployerType\":0,\"TransferSenderAccountId\":null,\"Priority\":null,\"DataLockFailures\":[],\"AgreedOnDate\":null},{\"PriceEpisodeIdentifier\":null,\"Period\":11,\"Amount\":0,\"SfaContributionPercentage\":null,\"AccountId\":null,\"ApprenticeshipId\":null,\"ApprenticeshipPriceEpisodeId\":null,\"ApprenticeshipEmployerType\":0,\"TransferSenderAccountId\":null,\"Priority\":null,\"DataLockFailures\":[],\"AgreedOnDate\":null},{\"PriceEpisodeIdentifier\":null,\"Period\":12,\"Amount\":0,\"SfaContributionPercentage\":null,\"AccountId\":null,\"ApprenticeshipId\":null,\"ApprenticeshipPriceEpisodeId\":null,\"ApprenticeshipEmployerType\":0,\"TransferSenderAccountId\":null,\"Priority\":null,\"DataLockFailures\":[],\"AgreedOnDate\":null}],\"CensusDate\":\"0001-01-01T00:00:00\"},{\"Type\":1,\"Periods\":[{\"PriceEpisodeIdentifier\":\"25-5-01/08/2024\",\"Period\":1,\"Amount\":300.0,\"SfaContributionPercentage\":0.95,\"AccountId\":null,\"ApprenticeshipId\":null,\"ApprenticeshipPriceEpisodeId\":null,\"ApprenticeshipEmployerType\":0,\"TransferSenderAccountId\":null,\"Priority\":null,\"DataLockFailures\":[],\"AgreedOnDate\":null},{\"PriceEpisodeIdentifier\":\"25-5-01/08/2024\",\"Period\":2,\"Amount\":300.0,\"SfaContributionPercentage\":0.95,\"AccountId\":null,\"ApprenticeshipId\":null,\"ApprenticeshipPriceEpisodeId\":null,\"ApprenticeshipEmployerType\":0,\"TransferSenderAccountId\":null,\"Priority\":null,\"DataLockFailures\":[],\"AgreedOnDate\":null},{\"PriceEpisodeIdentifier\":\"25-5-01/08/2024\",\"Period\":3,\"Amount\":300.0,\"SfaContributionPercentage\":0.95,\"AccountId\":null,\"ApprenticeshipId\":null,\"ApprenticeshipPriceEpisodeId\":null,\"ApprenticeshipEmployerType\":0,\"TransferSenderAccountId\":null,\"Priority\":null,\"DataLockFailures\":[],\"AgreedOnDate\":null},{\"PriceEpisodeIdentifier\":\"25-5-01/08/2024\",\"Period\":4,\"Amount\":300.0,\"SfaContributionPercentage\":0.95,\"AccountId\":null,\"ApprenticeshipId\":null,\"ApprenticeshipPriceEpisodeId\":null,\"ApprenticeshipEmployerType\":0,\"TransferSenderAccountId\":null,\"Priority\":null,\"DataLockFailures\":[],\"AgreedOnDate\":null},{\"PriceEpisodeIdentifier\":\"25-5-01/08/2024\",\"Period\":5,\"Amount\":300.0,\"SfaContributionPercentage\":0.95,\"AccountId\":null,\"ApprenticeshipId\":null,\"ApprenticeshipPriceEpisodeId\":null,\"ApprenticeshipEmployerType\":0,\"TransferSenderAccountId\":null,\"Priority\":null,\"DataLockFailures\":[],\"AgreedOnDate\":null},{\"PriceEpisodeIdentifier\":\"25-5-01/08/2024\",\"Period\":6,\"Amount\":300.0,\"SfaContributionPercentage\":0.95,\"AccountId\":null,\"ApprenticeshipId\":null,\"ApprenticeshipPriceEpisodeId\":null,\"ApprenticeshipEmployerType\":0,\"TransferSenderAccountId\":null,\"Priority\":null,\"DataLockFailures\":[],\"AgreedOnDate\":null},{\"PriceEpisodeIdentifier\":\"25-5-01/08/2024\",\"Period\":7,\"Amount\":300.0,\"SfaContributionPercentage\":0.95,\"AccountId\":null,\"ApprenticeshipId\":null,\"ApprenticeshipPriceEpisodeId\":null,\"ApprenticeshipEmployerType\":0,\"TransferSenderAccountId\":null,\"Priority\":null,\"DataLockFailures\":[],\"AgreedOnDate\":null},{\"PriceEpisodeIdentifier\":null,\"Period\":8,\"Amount\":0,\"SfaContributionPercentage\":null,\"AccountId\":null,\"ApprenticeshipId\":null,\"ApprenticeshipPriceEpisodeId\":null,\"ApprenticeshipEmployerType\":0,\"TransferSenderAccountId\":null,\"Priority\":null,\"DataLockFailures\":[],\"AgreedOnDate\":null},{\"PriceEpisodeIdentifier\":null,\"Period\":9,\"Amount\":0,\"SfaContributionPercentage\":null,\"AccountId\":null,\"ApprenticeshipId\":null,\"ApprenticeshipPriceEpisodeId\":null,\"ApprenticeshipEmployerType\":0,\"TransferSenderAccountId\":null,\"Priority\":null,\"DataLockFailures\":[],\"AgreedOnDate\":null},{\"PriceEpisodeIdentifier\":null,\"Period\":10,\"Amount\":0,\"SfaContributionPercentage\":null,\"AccountId\":null,\"ApprenticeshipId\":null,\"ApprenticeshipPriceEpisodeId\":null,\"ApprenticeshipEmployerType\":0,\"TransferSenderAccountId\":null,\"Priority\":null,\"DataLockFailures\":[],\"AgreedOnDate\":null},{\"PriceEpisodeIdentifier\":null,\"Period\":11,\"Amount\":0,\"SfaContributionPercentage\":null,\"AccountId\":null,\"ApprenticeshipId\":null,\"ApprenticeshipPriceEpisodeId\":null,\"ApprenticeshipEmployerType\":0,\"TransferSenderAccountId\":null,\"Priority\":null,\"DataLockFailures\":[],\"AgreedOnDate\":null},{\"PriceEpisodeIdentifier\":null,\"Period\":12,\"Amount\":0,\"SfaContributionPercentage\":null,\"AccountId\":null,\"ApprenticeshipId\":null,\"ApprenticeshipPriceEpisodeId\":null,\"ApprenticeshipEmployerType\":0,\"TransferSenderAccountId\":null,\"Priority\":null,\"DataLockFailures\":[],\"AgreedOnDate\":null}],\"CensusDate\":\"0001-01-01T00:00:00\"}],\"IncentiveEarnings\":[{\"Type\":4,\"Periods\":[{\"PriceEpisodeIdentifier\":null,\"Period\":1,\"Amount\":0,\"SfaContributionPercentage\":null,\"AccountId\":null,\"ApprenticeshipId\":null,\"ApprenticeshipPriceEpisodeId\":null,\"ApprenticeshipEmployerType\":0,\"TransferSenderAccountId\":null,\"Priority\":null,\"DataLockFailures\":[],\"AgreedOnDate\":null},{\"PriceEpisodeIdentifier\":null,\"Period\":2,\"Amount\":0,\"SfaContributionPercentage\":null,\"AccountId\":null,\"ApprenticeshipId\":null,\"ApprenticeshipPriceEpisodeId\":null,\"ApprenticeshipEmployerType\":0,\"TransferSenderAccountId\":null,\"Priority\":null,\"DataLockFailures\":[],\"AgreedOnDate\":null},{\"PriceEpisodeIdentifier\":null,\"Period\":3,\"Amount\":0,\"SfaContributionPercentage\":null,\"AccountId\":null,\"ApprenticeshipId\":null,\"ApprenticeshipPriceEpisodeId\":null,\"ApprenticeshipEmployerType\":0,\"TransferSenderAccountId\":null,\"Priority\":null,\"DataLockFailures\":[],\"AgreedOnDate\":null},{\"PriceEpisodeIdentifier\":null,\"Period\":4,\"Amount\":0,\"SfaContributionPercentage\":null,\"AccountId\":null,\"ApprenticeshipId\":null,\"ApprenticeshipPriceEpisodeId\":null,\"ApprenticeshipEmployerType\":0,\"TransferSenderAccountId\":null,\"Priority\":null,\"DataLockFailures\":[],\"AgreedOnDate\":null},{\"PriceEpisodeIdentifier\":null,\"Period\":5,\"Amount\":0,\"SfaContributionPercentage\":null,\"AccountId\":null,\"ApprenticeshipId\":null,\"ApprenticeshipPriceEpisodeId\":null,\"ApprenticeshipEmployerType\":0,\"TransferSenderAccountId\":null,\"Priority\":null,\"DataLockFailures\":[],\"AgreedOnDate\":null},{\"PriceEpisodeIdentifier\":null,\"Period\":6,\"Amount\":0,\"SfaContributionPercentage\":null,\"AccountId\":null,\"ApprenticeshipId\":null,\"ApprenticeshipPriceEpisodeId\":null,\"ApprenticeshipEmployerType\":0,\"TransferSenderAccountId\":null,\"Priority\":null,\"DataLockFailures\":[],\"AgreedOnDate\":null},{\"PriceEpisodeIdentifier\":null,\"Period\":7,\"Amount\":0,\"SfaContributionPercentage\":null,\"AccountId\":null,\"ApprenticeshipId\":null,\"ApprenticeshipPriceEpisodeId\":null,\"ApprenticeshipEmployerType\":0,\"TransferSenderAccountId\":null,\"Priority\":null,\"DataLockFailures\":[],\"AgreedOnDate\":null},{\"PriceEpisodeIdentifier\":null,\"Period\":8,\"Amount\":0,\"SfaContributionPercentage\":null,\"AccountId\":null,\"ApprenticeshipId\":null,\"ApprenticeshipPriceEpisodeId\":null,\"ApprenticeshipEmployerType\":0,\"TransferSenderAccountId\":null,\"Priority\":null,\"DataLockFailures\":[],\"AgreedOnDate\":null},{\"PriceEpisodeIdentifier\":null,\"Period\":9,\"Amount\":0,\"SfaContributionPercentage\":null,\"AccountId\":null,\"ApprenticeshipId\":null,\"ApprenticeshipPriceEpisodeId\":null,\"ApprenticeshipEmployerType\":0,\"TransferSenderAccountId\":null,\"Priority\":null,\"DataLockFailures\":[],\"AgreedOnDate\":null},{\"PriceEpisodeIdentifier\":null,\"Period\":10,\"Amount\":0,\"SfaContributionPercentage\":null,\"AccountId\":null,\"ApprenticeshipId\":null,\"ApprenticeshipPriceEpisodeId\":null,\"ApprenticeshipEmployerType\":0,\"TransferSenderAccountId\":null,\"Priority\":null,\"DataLockFailures\":[],\"AgreedOnDate\":null},{\"PriceEpisodeIdentifier\":null,\"Period\":11,\"Amount\":0,\"SfaContributionPercentage\":null,\"AccountId\":null,\"ApprenticeshipId\":null,\"ApprenticeshipPriceEpisodeId\":null,\"ApprenticeshipEmployerType\":0,\"TransferSenderAccountId\":null,\"Priority\":null,\"DataLockFailures\":[],\"AgreedOnDate\":null},{\"PriceEpisodeIdentifier\":null,\"Period\":12,\"Amount\":0,\"SfaContributionPercentage\":null,\"AccountId\":null,\"ApprenticeshipId\":null,\"ApprenticeshipPriceEpisodeId\":null,\"ApprenticeshipEmployerType\":0,\"TransferSenderAccountId\":null,\"Priority\":null,\"DataLockFailures\":[],\"AgreedOnDate\":null}],\"CensusDate\":\"0001-01-01T00:00:00\"},{\"Type\":5,\"Periods\":[{\"PriceEpisodeIdentifier\":null,\"Period\":1,\"Amount\":0,\"SfaContributionPercentage\":null,\"AccountId\":null,\"ApprenticeshipId\":null,\"ApprenticeshipPriceEpisodeId\":null,\"ApprenticeshipEmployerType\":0,\"TransferSenderAccountId\":null,\"Priority\":null,\"DataLockFailures\":[],\"AgreedOnDate\":null},{\"PriceEpisodeIdentifier\":null,\"Period\":2,\"Amount\":0,\"SfaContributionPercentage\":null,\"AccountId\":null,\"ApprenticeshipId\":null,\"ApprenticeshipPriceEpisodeId\":null,\"ApprenticeshipEmployerType\":0,\"TransferSenderAccountId\":null,\"Priority\":null,\"DataLockFailures\":[],\"AgreedOnDate\":null},{\"PriceEpisodeIdentifier\":null,\"Period\":3,\"Amount\":0,\"SfaContributionPercentage\":null,\"AccountId\":null,\"ApprenticeshipId\":null,\"ApprenticeshipPriceEpisodeId\":null,\"ApprenticeshipEmployerType\":0,\"TransferSenderAccountId\":null,\"Priority\":null,\"DataLockFailures\":[],\"AgreedOnDate\":null},{\"PriceEpisodeIdentifier\":null,\"Period\":4,\"Amount\":0,\"SfaContributionPercentage\":null,\"AccountId\":null,\"ApprenticeshipId\":null,\"ApprenticeshipPriceEpisodeId\":null,\"ApprenticeshipEmployerType\":0,\"TransferSenderAccountId\":null,\"Priority\":null,\"DataLockFailures\":[],\"AgreedOnDate\":null},{\"PriceEpisodeIdentifier\":null,\"Period\":5,\"Amount\":0,\"SfaContributionPercentage\":null,\"AccountId\":null,\"ApprenticeshipId\":null,\"ApprenticeshipPriceEpisodeId\":null,\"ApprenticeshipEmployerType\":0,\"TransferSenderAccountId\":null,\"Priority\":null,\"DataLockFailures\":[],\"AgreedOnDate\":null},{\"PriceEpisodeIdentifier\":null,\"Period\":6,\"Amount\":0,\"SfaContributionPercentage\":null,\"AccountId\":null,\"ApprenticeshipId\":null,\"ApprenticeshipPriceEpisodeId\":null,\"ApprenticeshipEmployerType\":0,\"TransferSenderAccountId\":null,\"Priority\":null,\"DataLockFailures\":[],\"AgreedOnDate\":null},{\"PriceEpisodeIdentifier\":null,\"Period\":7,\"Amount\":0,\"SfaContributionPercentage\":null,\"AccountId\":null,\"ApprenticeshipId\":null,\"ApprenticeshipPriceEpisodeId\":null,\"ApprenticeshipEmployerType\":0,\"TransferSenderAccountId\":null,\"Priority\":null,\"DataLockFailures\":[],\"AgreedOnDate\":null},{\"PriceEpisodeIdentifier\":null,\"Period\":8,\"Amount\":0,\"SfaContributionPercentage\":null,\"AccountId\":null,\"ApprenticeshipId\":null,\"ApprenticeshipPriceEpisodeId\":null,\"ApprenticeshipEmployerType\":0,\"TransferSenderAccountId\":null,\"Priority\":null,\"DataLockFailures\":[],\"AgreedOnDate\":null},{\"PriceEpisodeIdentifier\":null,\"Period\":9,\"Amount\":0,\"SfaContributionPercentage\":null,\"AccountId\":null,\"ApprenticeshipId\":null,\"ApprenticeshipPriceEpisodeId\":null,\"ApprenticeshipEmployerType\":0,\"TransferSenderAccountId\":null,\"Priority\":null,\"DataLockFailures\":[],\"AgreedOnDate\":null},{\"PriceEpisodeIdentifier\":null,\"Period\":10,\"Amount\":0,\"SfaContributionPercentage\":null,\"AccountId\":null,\"ApprenticeshipId\":null,\"ApprenticeshipPriceEpisodeId\":null,\"ApprenticeshipEmployerType\":0,\"TransferSenderAccountId\":null,\"Priority\":null,\"DataLockFailures\":[],\"AgreedOnDate\":null},{\"PriceEpisodeIdentifier\":null,\"Period\":11,\"Amount\":0,\"SfaContributionPercentage\":null,\"AccountId\":null,\"ApprenticeshipId\":null,\"ApprenticeshipPriceEpisodeId\":null,\"ApprenticeshipEmployerType\":0,\"TransferSenderAccountId\":null,\"Priority\":null,\"DataLockFailures\":[],\"AgreedOnDate\":null},{\"PriceEpisodeIdentifier\":null,\"Period\":12,\"Amount\":0,\"SfaContributionPercentage\":null,\"AccountId\":null,\"ApprenticeshipId\":null,\"ApprenticeshipPriceEpisodeId\":null,\"ApprenticeshipEmployerType\":0,\"TransferSenderAccountId\":null,\"Priority\":null,\"DataLockFailures\":[],\"AgreedOnDate\":null}],\"CensusDate\":\"0001-01-01T00:00:00\"},{\"Type\":6,\"Periods\":[{\"PriceEpisodeIdentifier\":null,\"Period\":1,\"Amount\":0,\"SfaContributionPercentage\":null,\"AccountId\":null,\"ApprenticeshipId\":null,\"ApprenticeshipPriceEpisodeId\":null,\"ApprenticeshipEmployerType\":0,\"TransferSenderAccountId\":null,\"Priority\":null,\"DataLockFailures\":[],\"AgreedOnDate\":null},{\"PriceEpisodeIdentifier\":null,\"Period\":2,\"Amount\":0,\"SfaContributionPercentage\":null,\"AccountId\":null,\"ApprenticeshipId\":null,\"ApprenticeshipPriceEpisodeId\":null,\"ApprenticeshipEmployerType\":0,\"TransferSenderAccountId\":null,\"Priority\":null,\"DataLockFailures\":[],\"AgreedOnDate\":null},{\"PriceEpisodeIdentifier\":null,\"Period\":3,\"Amount\":0,\"SfaContributionPercentage\":null,\"AccountId\":null,\"ApprenticeshipId\":null,\"ApprenticeshipPriceEpisodeId\":null,\"ApprenticeshipEmployerType\":0,\"TransferSenderAccountId\":null,\"Priority\":null,\"DataLockFailures\":[],\"AgreedOnDate\":null},{\"PriceEpisodeIdentifier\":null,\"Period\":4,\"Amount\":0,\"SfaContributionPercentage\":null,\"AccountId\":null,\"ApprenticeshipId\":null,\"ApprenticeshipPriceEpisodeId\":null,\"ApprenticeshipEmployerType\":0,\"TransferSenderAccountId\":null,\"Priority\":null,\"DataLockFailures\":[],\"AgreedOnDate\":null},{\"PriceEpisodeIdentifier\":null,\"Period\":5,\"Amount\":0,\"SfaContributionPercentage\":null,\"AccountId\":null,\"ApprenticeshipId\":null,\"ApprenticeshipPriceEpisodeId\":null,\"ApprenticeshipEmployerType\":0,\"TransferSenderAccountId\":null,\"Priority\":null,\"DataLockFailures\":[],\"AgreedOnDate\":null},{\"PriceEpisodeIdentifier\":null,\"Period\":6,\"Amount\":0,\"SfaContributionPercentage\":null,\"AccountId\":null,\"ApprenticeshipId\":null,\"ApprenticeshipPriceEpisodeId\":null,\"ApprenticeshipEmployerType\":0,\"TransferSenderAccountId\":null,\"Priority\":null,\"DataLockFailures\":[],\"AgreedOnDate\":null},{\"PriceEpisodeIdentifier\":null,\"Period\":7,\"Amount\":0,\"SfaContributionPercentage\":null,\"AccountId\":null,\"ApprenticeshipId\":null,\"ApprenticeshipPriceEpisodeId\":null,\"ApprenticeshipEmployerType\":0,\"TransferSenderAccountId\":null,\"Priority\":null,\"DataLockFailures\":[],\"AgreedOnDate\":null},{\"PriceEpisodeIdentifier\":null,\"Period\":8,\"Amount\":0,\"SfaContributionPercentage\":null,\"AccountId\":null,\"ApprenticeshipId\":null,\"ApprenticeshipPriceEpisodeId\":null,\"ApprenticeshipEmployerType\":0,\"TransferSenderAccountId\":null,\"Priority\":null,\"DataLockFailures\":[],\"AgreedOnDate\":null},{\"PriceEpisodeIdentifier\":null,\"Period\":9,\"Amount\":0,\"SfaContributionPercentage\":null,\"AccountId\":null,\"ApprenticeshipId\":null,\"ApprenticeshipPriceEpisodeId\":null,\"ApprenticeshipEmployerType\":0,\"TransferSenderAccountId\":null,\"Priority\":null,\"DataLockFailures\":[],\"AgreedOnDate\":null},{\"PriceEpisodeIdentifier\":null,\"Period\":10,\"Amount\":0,\"SfaContributionPercentage\":null,\"AccountId\":null,\"ApprenticeshipId\":null,\"ApprenticeshipPriceEpisodeId\":null,\"ApprenticeshipEmployerType\":0,\"TransferSenderAccountId\":null,\"Priority\":null,\"DataLockFailures\":[],\"AgreedOnDate\":null},{\"PriceEpisodeIdentifier\":null,\"Period\":11,\"Amount\":0,\"SfaContributionPercentage\":null,\"AccountId\":null,\"ApprenticeshipId\":null,\"ApprenticeshipPriceEpisodeId\":null,\"ApprenticeshipEmployerType\":0,\"TransferSenderAccountId\":null,\"Priority\":null,\"DataLockFailures\":[],\"AgreedOnDate\":null},{\"PriceEpisodeIdentifier\":null,\"Period\":12,\"Amount\":0,\"SfaContributionPercentage\":null,\"AccountId\":null,\"ApprenticeshipId\":null,\"ApprenticeshipPriceEpisodeId\":null,\"ApprenticeshipEmployerType\":0,\"TransferSenderAccountId\":null,\"Priority\":null,\"DataLockFailures\":[],\"AgreedOnDate\":null}],\"CensusDate\":\"0001-01-01T00:00:00\"},{\"Type\":7,\"Periods\":[{\"PriceEpisodeIdentifier\":null,\"Period\":1,\"Amount\":0,\"SfaContributionPercentage\":null,\"AccountId\":null,\"ApprenticeshipId\":null,\"ApprenticeshipPriceEpisodeId\":null,\"ApprenticeshipEmployerType\":0,\"TransferSenderAccountId\":null,\"Priority\":null,\"DataLockFailures\":[],\"AgreedOnDate\":null},{\"PriceEpisodeIdentifier\":null,\"Period\":2,\"Amount\":0,\"SfaContributionPercentage\":null,\"AccountId\":null,\"ApprenticeshipId\":null,\"ApprenticeshipPriceEpisodeId\":null,\"ApprenticeshipEmployerType\":0,\"TransferSenderAccountId\":null,\"Priority\":null,\"DataLockFailures\":[],\"AgreedOnDate\":null},{\"PriceEpisodeIdentifier\":null,\"Period\":3,\"Amount\":0,\"SfaContributionPercentage\":null,\"AccountId\":null,\"ApprenticeshipId\":null,\"ApprenticeshipPriceEpisodeId\":null,\"ApprenticeshipEmployerType\":0,\"TransferSenderAccountId\":null,\"Priority\":null,\"DataLockFailures\":[],\"AgreedOnDate\":null},{\"PriceEpisodeIdentifier\":null,\"Period\":4,\"Amount\":0,\"SfaContributionPercentage\":null,\"AccountId\":null,\"ApprenticeshipId\":null,\"ApprenticeshipPriceEpisodeId\":null,\"ApprenticeshipEmployerType\":0,\"TransferSenderAccountId\":null,\"Priority\":null,\"DataLockFailures\":[],\"AgreedOnDate\":null},{\"PriceEpisodeIdentifier\":null,\"Period\":5,\"Amount\":0,\"SfaContributionPercentage\":null,\"AccountId\":null,\"ApprenticeshipId\":null,\"ApprenticeshipPriceEpisodeId\":null,\"ApprenticeshipEmployerType\":0,\"TransferSenderAccountId\":null,\"Priority\":null,\"DataLockFailures\":[],\"AgreedOnDate\":null},{\"PriceEpisodeIdentifier\":null,\"Period\":6,\"Amount\":0,\"SfaContributionPercentage\":null,\"AccountId\":null,\"ApprenticeshipId\":null,\"ApprenticeshipPriceEpisodeId\":null,\"ApprenticeshipEmployerType\":0,\"TransferSenderAccountId\":null,\"Priority\":null,\"DataLockFailures\":[],\"AgreedOnDate\":null},{\"PriceEpisodeIdentifier\":null,\"Period\":7,\"Amount\":0,\"SfaContributionPercentage\":null,\"AccountId\":null,\"ApprenticeshipId\":null,\"ApprenticeshipPriceEpisodeId\":null,\"ApprenticeshipEmployerType\":0,\"TransferSenderAccountId\":null,\"Priority\":null,\"DataLockFailures\":[],\"AgreedOnDate\":null},{\"PriceEpisodeIdentifier\":null,\"Period\":8,\"Amount\":0,\"SfaContributionPercentage\":null,\"AccountId\":null,\"ApprenticeshipId\":null,\"ApprenticeshipPriceEpisodeId\":null,\"ApprenticeshipEmployerType\":0,\"TransferSenderAccountId\":null,\"Priority\":null,\"DataLockFailures\":[],\"AgreedOnDate\":null},{\"PriceEpisodeIdentifier\":null,\"Period\":9,\"Amount\":0,\"SfaContributionPercentage\":null,\"AccountId\":null,\"ApprenticeshipId\":null,\"ApprenticeshipPriceEpisodeId\":null,\"ApprenticeshipEmployerType\":0,\"TransferSenderAccountId\":null,\"Priority\":null,\"DataLockFailures\":[],\"AgreedOnDate\":null},{\"PriceEpisodeIdentifier\":null,\"Period\":10,\"Amount\":0,\"SfaContributionPercentage\":null,\"AccountId\":null,\"ApprenticeshipId\":null,\"ApprenticeshipPriceEpisodeId\":null,\"ApprenticeshipEmployerType\":0,\"TransferSenderAccountId\":null,\"Priority\":null,\"DataLockFailures\":[],\"AgreedOnDate\":null},{\"PriceEpisodeIdentifier\":null,\"Period\":11,\"Amount\":0,\"SfaContributionPercentage\":null,\"AccountId\":null,\"ApprenticeshipId\":null,\"ApprenticeshipPriceEpisodeId\":null,\"ApprenticeshipEmployerType\":0,\"TransferSenderAccountId\":null,\"Priority\":null,\"DataLockFailures\":[],\"AgreedOnDate\":null},{\"PriceEpisodeIdentifier\":null,\"Period\":12,\"Amount\":0,\"SfaContributionPercentage\":null,\"AccountId\":null,\"ApprenticeshipId\":null,\"ApprenticeshipPriceEpisodeId\":null,\"ApprenticeshipEmployerType\":0,\"TransferSenderAccountId\":null,\"Priority\":null,\"DataLockFailures\":[],\"AgreedOnDate\":null}],\"CensusDate\":\"0001-01-01T00:00:00\"},{\"Type\":8,\"Periods\":[{\"PriceEpisodeIdentifier\":null,\"Period\":1,\"Amount\":0,\"SfaContributionPercentage\":null,\"AccountId\":null,\"ApprenticeshipId\":null,\"ApprenticeshipPriceEpisodeId\":null,\"ApprenticeshipEmployerType\":0,\"TransferSenderAccountId\":null,\"Priority\":null,\"DataLockFailures\":[],\"AgreedOnDate\":null},{\"PriceEpisodeIdentifier\":null,\"Period\":2,\"Amount\":0,\"SfaContributionPercentage\":null,\"AccountId\":null,\"ApprenticeshipId\":null,\"ApprenticeshipPriceEpisodeId\":null,\"ApprenticeshipEmployerType\":0,\"TransferSenderAccountId\":null,\"Priority\":null,\"DataLockFailures\":[],\"AgreedOnDate\":null},{\"PriceEpisodeIdentifier\":null,\"Period\":3,\"Amount\":0,\"SfaContributionPercentage\":null,\"AccountId\":null,\"ApprenticeshipId\":null,\"ApprenticeshipPriceEpisodeId\":null,\"ApprenticeshipEmployerType\":0,\"TransferSenderAccountId\":null,\"Priority\":null,\"DataLockFailures\":[],\"AgreedOnDate\":null},{\"PriceEpisodeIdentifier\":null,\"Period\":4,\"Amount\":0,\"SfaContributionPercentage\":null,\"AccountId\":null,\"ApprenticeshipId\":null,\"ApprenticeshipPriceEpisodeId\":null,\"ApprenticeshipEmployerType\":0,\"TransferSenderAccountId\":null,\"Priority\":null,\"DataLockFailures\":[],\"AgreedOnDate\":null},{\"PriceEpisodeIdentifier\":null,\"Period\":5,\"Amount\":0,\"SfaContributionPercentage\":null,\"AccountId\":null,\"ApprenticeshipId\":null,\"ApprenticeshipPriceEpisodeId\":null,\"ApprenticeshipEmployerType\":0,\"TransferSenderAccountId\":null,\"Priority\":null,\"DataLockFailures\":[],\"AgreedOnDate\":null},{\"PriceEpisodeIdentifier\":null,\"Period\":6,\"Amount\":0,\"SfaContributionPercentage\":null,\"AccountId\":null,\"ApprenticeshipId\":null,\"ApprenticeshipPriceEpisodeId\":null,\"ApprenticeshipEmployerType\":0,\"TransferSenderAccountId\":null,\"Priority\":null,\"DataLockFailures\":[],\"AgreedOnDate\":null},{\"PriceEpisodeIdentifier\":null,\"Period\":7,\"Amount\":0,\"SfaContributionPercentage\":null,\"AccountId\":null,\"ApprenticeshipId\":null,\"ApprenticeshipPriceEpisodeId\":null,\"ApprenticeshipEmployerType\":0,\"TransferSenderAccountId\":null,\"Priority\":null,\"DataLockFailures\":[],\"AgreedOnDate\":null},{\"PriceEpisodeIdentifier\":null,\"Period\":8,\"Amount\":0,\"SfaContributionPercentage\":null,\"AccountId\":null,\"ApprenticeshipId\":null,\"ApprenticeshipPriceEpisodeId\":null,\"ApprenticeshipEmployerType\":0,\"TransferSenderAccountId\":null,\"Priority\":null,\"DataLockFailures\":[],\"AgreedOnDate\":null},{\"PriceEpisodeIdentifier\":null,\"Period\":9,\"Amount\":0,\"SfaContributionPercentage\":null,\"AccountId\":null,\"ApprenticeshipId\":null,\"ApprenticeshipPriceEpisodeId\":null,\"ApprenticeshipEmployerType\":0,\"TransferSenderAccountId\":null,\"Priority\":null,\"DataLockFailures\":[],\"AgreedOnDate\":null},{\"PriceEpisodeIdentifier\":null,\"Period\":10,\"Amount\":0,\"SfaContributionPercentage\":null,\"AccountId\":null,\"ApprenticeshipId\":null,\"ApprenticeshipPriceEpisodeId\":null,\"ApprenticeshipEmployerType\":0,\"TransferSenderAccountId\":null,\"Priority\":null,\"DataLockFailures\":[],\"AgreedOnDate\":null},{\"PriceEpisodeIdentifier\":null,\"Period\":11,\"Amount\":0,\"SfaContributionPercentage\":null,\"AccountId\":null,\"ApprenticeshipId\":null,\"ApprenticeshipPriceEpisodeId\":null,\"ApprenticeshipEmployerType\":0,\"TransferSenderAccountId\":null,\"Priority\":null,\"DataLockFailures\":[],\"AgreedOnDate\":null},{\"PriceEpisodeIdentifier\":null,\"Period\":12,\"Amount\":0,\"SfaContributionPercentage\":null,\"AccountId\":null,\"ApprenticeshipId\":null,\"ApprenticeshipPriceEpisodeId\":null,\"ApprenticeshipEmployerType\":0,\"TransferSenderAccountId\":null,\"Priority\":null,\"DataLockFailures\":[],\"AgreedOnDate\":null}],\"CensusDate\":\"0001-01-01T00:00:00\"},{\"Type\":9,\"Periods\":[{\"PriceEpisodeIdentifier\":null,\"Period\":1,\"Amount\":0,\"SfaContributionPercentage\":null,\"AccountId\":null,\"ApprenticeshipId\":null,\"ApprenticeshipPriceEpisodeId\":null,\"ApprenticeshipEmployerType\":0,\"TransferSenderAccountId\":null,\"Priority\":null,\"DataLockFailures\":[],\"AgreedOnDate\":null},{\"PriceEpisodeIdentifier\":null,\"Period\":2,\"Amount\":0,\"SfaContributionPercentage\":null,\"AccountId\":null,\"ApprenticeshipId\":null,\"ApprenticeshipPriceEpisodeId\":null,\"ApprenticeshipEmployerType\":0,\"TransferSenderAccountId\":null,\"Priority\":null,\"DataLockFailures\":[],\"AgreedOnDate\":null},{\"PriceEpisodeIdentifier\":null,\"Period\":3,\"Amount\":0,\"SfaContributionPercentage\":null,\"AccountId\":null,\"ApprenticeshipId\":null,\"ApprenticeshipPriceEpisodeId\":null,\"ApprenticeshipEmployerType\":0,\"TransferSenderAccountId\":null,\"Priority\":null,\"DataLockFailures\":[],\"AgreedOnDate\":null},{\"PriceEpisodeIdentifier\":null,\"Period\":4,\"Amount\":0,\"SfaContributionPercentage\":null,\"AccountId\":null,\"ApprenticeshipId\":null,\"ApprenticeshipPriceEpisodeId\":null,\"ApprenticeshipEmployerType\":0,\"TransferSenderAccountId\":null,\"Priority\":null,\"DataLockFailures\":[],\"AgreedOnDate\":null},{\"PriceEpisodeIdentifier\":null,\"Period\":5,\"Amount\":0,\"SfaContributionPercentage\":null,\"AccountId\":null,\"ApprenticeshipId\":null,\"ApprenticeshipPriceEpisodeId\":null,\"ApprenticeshipEmployerType\":0,\"TransferSenderAccountId\":null,\"Priority\":null,\"DataLockFailures\":[],\"AgreedOnDate\":null},{\"PriceEpisodeIdentifier\":null,\"Period\":6,\"Amount\":0,\"SfaContributionPercentage\":null,\"AccountId\":null,\"ApprenticeshipId\":null,\"ApprenticeshipPriceEpisodeId\":null,\"ApprenticeshipEmployerType\":0,\"TransferSenderAccountId\":null,\"Priority\":null,\"DataLockFailures\":[],\"AgreedOnDate\":null},{\"PriceEpisodeIdentifier\":null,\"Period\":7,\"Amount\":0,\"SfaContributionPercentage\":null,\"AccountId\":null,\"ApprenticeshipId\":null,\"ApprenticeshipPriceEpisodeId\":null,\"ApprenticeshipEmployerType\":0,\"TransferSenderAccountId\":null,\"Priority\":null,\"DataLockFailures\":[],\"AgreedOnDate\":null},{\"PriceEpisodeIdentifier\":null,\"Period\":8,\"Amount\":0,\"SfaContributionPercentage\":null,\"AccountId\":null,\"ApprenticeshipId\":null,\"ApprenticeshipPriceEpisodeId\":null,\"ApprenticeshipEmployerType\":0,\"TransferSenderAccountId\":null,\"Priority\":null,\"DataLockFailures\":[],\"AgreedOnDate\":null},{\"PriceEpisodeIdentifier\":null,\"Period\":9,\"Amount\":0,\"SfaContributionPercentage\":null,\"AccountId\":null,\"ApprenticeshipId\":null,\"ApprenticeshipPriceEpisodeId\":null,\"ApprenticeshipEmployerType\":0,\"TransferSenderAccountId\":null,\"Priority\":null,\"DataLockFailures\":[],\"AgreedOnDate\":null},{\"PriceEpisodeIdentifier\":null,\"Period\":10,\"Amount\":0,\"SfaContributionPercentage\":null,\"AccountId\":null,\"ApprenticeshipId\":null,\"ApprenticeshipPriceEpisodeId\":null,\"ApprenticeshipEmployerType\":0,\"TransferSenderAccountId\":null,\"Priority\":null,\"DataLockFailures\":[],\"AgreedOnDate\":null},{\"PriceEpisodeIdentifier\":null,\"Period\":11,\"Amount\":0,\"SfaContributionPercentage\":null,\"AccountId\":null,\"ApprenticeshipId\":null,\"ApprenticeshipPriceEpisodeId\":null,\"ApprenticeshipEmployerType\":0,\"TransferSenderAccountId\":null,\"Priority\":null,\"DataLockFailures\":[],\"AgreedOnDate\":null},{\"PriceEpisodeIdentifier\":null,\"Period\":12,\"Amount\":0,\"SfaContributionPercentage\":null,\"AccountId\":null,\"ApprenticeshipId\":null,\"ApprenticeshipPriceEpisodeId\":null,\"ApprenticeshipEmployerType\":0,\"TransferSenderAccountId\":null,\"Priority\":null,\"DataLockFailures\":[],\"AgreedOnDate\":null}],\"CensusDate\":\"0001-01-01T00:00:00\"},{\"Type\":10,\"Periods\":[{\"PriceEpisodeIdentifier\":null,\"Period\":1,\"Amount\":0,\"SfaContributionPercentage\":null,\"AccountId\":null,\"ApprenticeshipId\":null,\"ApprenticeshipPriceEpisodeId\":null,\"ApprenticeshipEmployerType\":0,\"TransferSenderAccountId\":null,\"Priority\":null,\"DataLockFailures\":[],\"AgreedOnDate\":null},{\"PriceEpisodeIdentifier\":null,\"Period\":2,\"Amount\":0,\"SfaContributionPercentage\":null,\"AccountId\":null,\"ApprenticeshipId\":null,\"ApprenticeshipPriceEpisodeId\":null,\"ApprenticeshipEmployerType\":0,\"TransferSenderAccountId\":null,\"Priority\":null,\"DataLockFailures\":[],\"AgreedOnDate\":null},{\"PriceEpisodeIdentifier\":null,\"Period\":3,\"Amount\":0,\"SfaContributionPercentage\":null,\"AccountId\":null,\"ApprenticeshipId\":null,\"ApprenticeshipPriceEpisodeId\":null,\"ApprenticeshipEmployerType\":0,\"TransferSenderAccountId\":null,\"Priority\":null,\"DataLockFailures\":[],\"AgreedOnDate\":null},{\"PriceEpisodeIdentifier\":null,\"Period\":4,\"Amount\":0,\"SfaContributionPercentage\":null,\"AccountId\":null,\"ApprenticeshipId\":null,\"ApprenticeshipPriceEpisodeId\":null,\"ApprenticeshipEmployerType\":0,\"TransferSenderAccountId\":null,\"Priority\":null,\"DataLockFailures\":[],\"AgreedOnDate\":null},{\"PriceEpisodeIdentifier\":null,\"Period\":5,\"Amount\":0,\"SfaContributionPercentage\":null,\"AccountId\":null,\"ApprenticeshipId\":null,\"ApprenticeshipPriceEpisodeId\":null,\"ApprenticeshipEmployerType\":0,\"TransferSenderAccountId\":null,\"Priority\":null,\"DataLockFailures\":[],\"AgreedOnDate\":null},{\"PriceEpisodeIdentifier\":null,\"Period\":6,\"Amount\":0,\"SfaContributionPercentage\":null,\"AccountId\":null,\"ApprenticeshipId\":null,\"ApprenticeshipPriceEpisodeId\":null,\"ApprenticeshipEmployerType\":0,\"TransferSenderAccountId\":null,\"Priority\":null,\"DataLockFailures\":[],\"AgreedOnDate\":null},{\"PriceEpisodeIdentifier\":null,\"Period\":7,\"Amount\":0,\"SfaContributionPercentage\":null,\"AccountId\":null,\"ApprenticeshipId\":null,\"ApprenticeshipPriceEpisodeId\":null,\"ApprenticeshipEmployerType\":0,\"TransferSenderAccountId\":null,\"Priority\":null,\"DataLockFailures\":[],\"AgreedOnDate\":null},{\"PriceEpisodeIdentifier\":null,\"Period\":8,\"Amount\":0,\"SfaContributionPercentage\":null,\"AccountId\":null,\"ApprenticeshipId\":null,\"ApprenticeshipPriceEpisodeId\":null,\"ApprenticeshipEmployerType\":0,\"TransferSenderAccountId\":null,\"Priority\":null,\"DataLockFailures\":[],\"AgreedOnDate\":null},{\"PriceEpisodeIdentifier\":null,\"Period\":9,\"Amount\":0,\"SfaContributionPercentage\":null,\"AccountId\":null,\"ApprenticeshipId\":null,\"ApprenticeshipPriceEpisodeId\":null,\"ApprenticeshipEmployerType\":0,\"TransferSenderAccountId\":null,\"Priority\":null,\"DataLockFailures\":[],\"AgreedOnDate\":null},{\"PriceEpisodeIdentifier\":null,\"Period\":10,\"Amount\":0,\"SfaContributionPercentage\":null,\"AccountId\":null,\"ApprenticeshipId\":null,\"ApprenticeshipPriceEpisodeId\":null,\"ApprenticeshipEmployerType\":0,\"TransferSenderAccountId\":null,\"Priority\":null,\"DataLockFailures\":[],\"AgreedOnDate\":null},{\"PriceEpisodeIdentifier\":null,\"Period\":11,\"Amount\":0,\"SfaContributionPercentage\":null,\"AccountId\":null,\"ApprenticeshipId\":null,\"ApprenticeshipPriceEpisodeId\":null,\"ApprenticeshipEmployerType\":0,\"TransferSenderAccountId\":null,\"Priority\":null,\"DataLockFailures\":[],\"AgreedOnDate\":null},{\"PriceEpisodeIdentifier\":null,\"Period\":12,\"Amount\":0,\"SfaContributionPercentage\":null,\"AccountId\":null,\"ApprenticeshipId\":null,\"ApprenticeshipPriceEpisodeId\":null,\"ApprenticeshipEmployerType\":0,\"TransferSenderAccountId\":null,\"Priority\":null,\"DataLockFailures\":[],\"AgreedOnDate\":null}],\"CensusDate\":\"0001-01-01T00:00:00\"},{\"Type\":11,\"Periods\":[{\"PriceEpisodeIdentifier\":null,\"Period\":1,\"Amount\":0,\"SfaContributionPercentage\":null,\"AccountId\":null,\"ApprenticeshipId\":null,\"ApprenticeshipPriceEpisodeId\":null,\"ApprenticeshipEmployerType\":0,\"TransferSenderAccountId\":null,\"Priority\":null,\"DataLockFailures\":[],\"AgreedOnDate\":null},{\"PriceEpisodeIdentifier\":null,\"Period\":2,\"Amount\":0,\"SfaContributionPercentage\":null,\"AccountId\":null,\"ApprenticeshipId\":null,\"ApprenticeshipPriceEpisodeId\":null,\"ApprenticeshipEmployerType\":0,\"TransferSenderAccountId\":null,\"Priority\":null,\"DataLockFailures\":[],\"AgreedOnDate\":null},{\"PriceEpisodeIdentifier\":null,\"Period\":3,\"Amount\":0,\"SfaContributionPercentage\":null,\"AccountId\":null,\"ApprenticeshipId\":null,\"ApprenticeshipPriceEpisodeId\":null,\"ApprenticeshipEmployerType\":0,\"TransferSenderAccountId\":null,\"Priority\":null,\"DataLockFailures\":[],\"AgreedOnDate\":null},{\"PriceEpisodeIdentifier\":null,\"Period\":4,\"Amount\":0,\"SfaContributionPercentage\":null,\"AccountId\":null,\"ApprenticeshipId\":null,\"ApprenticeshipPriceEpisodeId\":null,\"ApprenticeshipEmployerType\":0,\"TransferSenderAccountId\":null,\"Priority\":null,\"DataLockFailures\":[],\"AgreedOnDate\":null},{\"PriceEpisodeIdentifier\":null,\"Period\":5,\"Amount\":0,\"SfaContributionPercentage\":null,\"AccountId\":null,\"ApprenticeshipId\":null,\"ApprenticeshipPriceEpisodeId\":null,\"ApprenticeshipEmployerType\":0,\"TransferSenderAccountId\":null,\"Priority\":null,\"DataLockFailures\":[],\"AgreedOnDate\":null},{\"PriceEpisodeIdentifier\":null,\"Period\":6,\"Amount\":0,\"SfaContributionPercentage\":null,\"AccountId\":null,\"ApprenticeshipId\":null,\"ApprenticeshipPriceEpisodeId\":null,\"ApprenticeshipEmployerType\":0,\"TransferSenderAccountId\":null,\"Priority\":null,\"DataLockFailures\":[],\"AgreedOnDate\":null},{\"PriceEpisodeIdentifier\":null,\"Period\":7,\"Amount\":0,\"SfaContributionPercentage\":null,\"AccountId\":null,\"ApprenticeshipId\":null,\"ApprenticeshipPriceEpisodeId\":null,\"ApprenticeshipEmployerType\":0,\"TransferSenderAccountId\":null,\"Priority\":null,\"DataLockFailures\":[],\"AgreedOnDate\":null},{\"PriceEpisodeIdentifier\":null,\"Period\":8,\"Amount\":0,\"SfaContributionPercentage\":null,\"AccountId\":null,\"ApprenticeshipId\":null,\"ApprenticeshipPriceEpisodeId\":null,\"ApprenticeshipEmployerType\":0,\"TransferSenderAccountId\":null,\"Priority\":null,\"DataLockFailures\":[],\"AgreedOnDate\":null},{\"PriceEpisodeIdentifier\":null,\"Period\":9,\"Amount\":0,\"SfaContributionPercentage\":null,\"AccountId\":null,\"ApprenticeshipId\":null,\"ApprenticeshipPriceEpisodeId\":null,\"ApprenticeshipEmployerType\":0,\"TransferSenderAccountId\":null,\"Priority\":null,\"DataLockFailures\":[],\"AgreedOnDate\":null},{\"PriceEpisodeIdentifier\":null,\"Period\":10,\"Amount\":0,\"SfaContributionPercentage\":null,\"AccountId\":null,\"ApprenticeshipId\":null,\"ApprenticeshipPriceEpisodeId\":null,\"ApprenticeshipEmployerType\":0,\"TransferSenderAccountId\":null,\"Priority\":null,\"DataLockFailures\":[],\"AgreedOnDate\":null},{\"PriceEpisodeIdentifier\":null,\"Period\":11,\"Amount\":0,\"SfaContributionPercentage\":null,\"AccountId\":null,\"ApprenticeshipId\":null,\"ApprenticeshipPriceEpisodeId\":null,\"ApprenticeshipEmployerType\":0,\"TransferSenderAccountId\":null,\"Priority\":null,\"DataLockFailures\":[],\"AgreedOnDate\":null},{\"PriceEpisodeIdentifier\":null,\"Period\":12,\"Amount\":0,\"SfaContributionPercentage\":null,\"AccountId\":null,\"ApprenticeshipId\":null,\"ApprenticeshipPriceEpisodeId\":null,\"ApprenticeshipEmployerType\":0,\"TransferSenderAccountId\":null,\"Priority\":null,\"DataLockFailures\":[],\"AgreedOnDate\":null}],\"CensusDate\":\"0001-01-01T00:00:00\"},{\"Type\":12,\"Periods\":[{\"PriceEpisodeIdentifier\":null,\"Period\":1,\"Amount\":0,\"SfaContributionPercentage\":null,\"AccountId\":null,\"ApprenticeshipId\":null,\"ApprenticeshipPriceEpisodeId\":null,\"ApprenticeshipEmployerType\":0,\"TransferSenderAccountId\":null,\"Priority\":null,\"DataLockFailures\":[],\"AgreedOnDate\":null},{\"PriceEpisodeIdentifier\":null,\"Period\":2,\"Amount\":0,\"SfaContributionPercentage\":null,\"AccountId\":null,\"ApprenticeshipId\":null,\"ApprenticeshipPriceEpisodeId\":null,\"ApprenticeshipEmployerType\":0,\"TransferSenderAccountId\":null,\"Priority\":null,\"DataLockFailures\":[],\"AgreedOnDate\":null},{\"PriceEpisodeIdentifier\":null,\"Period\":3,\"Amount\":0,\"SfaContributionPercentage\":null,\"AccountId\":null,\"ApprenticeshipId\":null,\"ApprenticeshipPriceEpisodeId\":null,\"ApprenticeshipEmployerType\":0,\"TransferSenderAccountId\":null,\"Priority\":null,\"DataLockFailures\":[],\"AgreedOnDate\":null},{\"PriceEpisodeIdentifier\":null,\"Period\":4,\"Amount\":0,\"SfaContributionPercentage\":null,\"AccountId\":null,\"ApprenticeshipId\":null,\"ApprenticeshipPriceEpisodeId\":null,\"ApprenticeshipEmployerType\":0,\"TransferSenderAccountId\":null,\"Priority\":null,\"DataLockFailures\":[],\"AgreedOnDate\":null},{\"PriceEpisodeIdentifier\":null,\"Period\":5,\"Amount\":0,\"SfaContributionPercentage\":null,\"AccountId\":null,\"ApprenticeshipId\":null,\"ApprenticeshipPriceEpisodeId\":null,\"ApprenticeshipEmployerType\":0,\"TransferSenderAccountId\":null,\"Priority\":null,\"DataLockFailures\":[],\"AgreedOnDate\":null},{\"PriceEpisodeIdentifier\":null,\"Period\":6,\"Amount\":0,\"SfaContributionPercentage\":null,\"AccountId\":null,\"ApprenticeshipId\":null,\"ApprenticeshipPriceEpisodeId\":null,\"ApprenticeshipEmployerType\":0,\"TransferSenderAccountId\":null,\"Priority\":null,\"DataLockFailures\":[],\"AgreedOnDate\":null},{\"PriceEpisodeIdentifier\":null,\"Period\":7,\"Amount\":0,\"SfaContributionPercentage\":null,\"AccountId\":null,\"ApprenticeshipId\":null,\"ApprenticeshipPriceEpisodeId\":null,\"ApprenticeshipEmployerType\":0,\"TransferSenderAccountId\":null,\"Priority\":null,\"DataLockFailures\":[],\"AgreedOnDate\":null},{\"PriceEpisodeIdentifier\":null,\"Period\":8,\"Amount\":0,\"SfaContributionPercentage\":null,\"AccountId\":null,\"ApprenticeshipId\":null,\"ApprenticeshipPriceEpisodeId\":null,\"ApprenticeshipEmployerType\":0,\"TransferSenderAccountId\":null,\"Priority\":null,\"DataLockFailures\":[],\"AgreedOnDate\":null},{\"PriceEpisodeIdentifier\":null,\"Period\":9,\"Amount\":0,\"SfaContributionPercentage\":null,\"AccountId\":null,\"ApprenticeshipId\":null,\"ApprenticeshipPriceEpisodeId\":null,\"ApprenticeshipEmployerType\":0,\"TransferSenderAccountId\":null,\"Priority\":null,\"DataLockFailures\":[],\"AgreedOnDate\":null},{\"PriceEpisodeIdentifier\":null,\"Period\":10,\"Amount\":0,\"SfaContributionPercentage\":null,\"AccountId\":null,\"ApprenticeshipId\":null,\"ApprenticeshipPriceEpisodeId\":null,\"ApprenticeshipEmployerType\":0,\"TransferSenderAccountId\":null,\"Priority\":null,\"DataLockFailures\":[],\"AgreedOnDate\":null},{\"PriceEpisodeIdentifier\":null,\"Period\":11,\"Amount\":0,\"SfaContributionPercentage\":null,\"AccountId\":null,\"ApprenticeshipId\":null,\"ApprenticeshipPriceEpisodeId\":null,\"ApprenticeshipEmployerType\":0,\"TransferSenderAccountId\":null,\"Priority\":null,\"DataLockFailures\":[],\"AgreedOnDate\":null},{\"PriceEpisodeIdentifier\":null,\"Period\":12,\"Amount\":0,\"SfaContributionPercentage\":null,\"AccountId\":null,\"ApprenticeshipId\":null,\"ApprenticeshipPriceEpisodeId\":null,\"ApprenticeshipEmployerType\":0,\"TransferSenderAccountId\":null,\"Priority\":null,\"DataLockFailures\":[],\"AgreedOnDate\":null}],\"CensusDate\":\"0001-01-01T00:00:00\"},{\"Type\":15,\"Periods\":[{\"PriceEpisodeIdentifier\":null,\"Period\":1,\"Amount\":0,\"SfaContributionPercentage\":null,\"AccountId\":null,\"ApprenticeshipId\":null,\"ApprenticeshipPriceEpisodeId\":null,\"ApprenticeshipEmployerType\":0,\"TransferSenderAccountId\":null,\"Priority\":null,\"DataLockFailures\":[],\"AgreedOnDate\":null},{\"PriceEpisodeIdentifier\":null,\"Period\":2,\"Amount\":0,\"SfaContributionPercentage\":null,\"AccountId\":null,\"ApprenticeshipId\":null,\"ApprenticeshipPriceEpisodeId\":null,\"ApprenticeshipEmployerType\":0,\"TransferSenderAccountId\":null,\"Priority\":null,\"DataLockFailures\":[],\"AgreedOnDate\":null},{\"PriceEpisodeIdentifier\":null,\"Period\":3,\"Amount\":0,\"SfaContributionPercentage\":null,\"AccountId\":null,\"ApprenticeshipId\":null,\"ApprenticeshipPriceEpisodeId\":null,\"ApprenticeshipEmployerType\":0,\"TransferSenderAccountId\":null,\"Priority\":null,\"DataLockFailures\":[],\"AgreedOnDate\":null},{\"PriceEpisodeIdentifier\":null,\"Period\":4,\"Amount\":0,\"SfaContributionPercentage\":null,\"AccountId\":null,\"ApprenticeshipId\":null,\"ApprenticeshipPriceEpisodeId\":null,\"ApprenticeshipEmployerType\":0,\"TransferSenderAccountId\":null,\"Priority\":null,\"DataLockFailures\":[],\"AgreedOnDate\":null},{\"PriceEpisodeIdentifier\":null,\"Period\":5,\"Amount\":0,\"SfaContributionPercentage\":null,\"AccountId\":null,\"ApprenticeshipId\":null,\"ApprenticeshipPriceEpisodeId\":null,\"ApprenticeshipEmployerType\":0,\"TransferSenderAccountId\":null,\"Priority\":null,\"DataLockFailures\":[],\"AgreedOnDate\":null},{\"PriceEpisodeIdentifier\":null,\"Period\":6,\"Amount\":0,\"SfaContributionPercentage\":null,\"AccountId\":null,\"ApprenticeshipId\":null,\"ApprenticeshipPriceEpisodeId\":null,\"ApprenticeshipEmployerType\":0,\"TransferSenderAccountId\":null,\"Priority\":null,\"DataLockFailures\":[],\"AgreedOnDate\":null},{\"PriceEpisodeIdentifier\":null,\"Period\":7,\"Amount\":0,\"SfaContributionPercentage\":null,\"AccountId\":null,\"ApprenticeshipId\":null,\"ApprenticeshipPriceEpisodeId\":null,\"ApprenticeshipEmployerType\":0,\"TransferSenderAccountId\":null,\"Priority\":null,\"DataLockFailures\":[],\"AgreedOnDate\":null},{\"PriceEpisodeIdentifier\":null,\"Period\":8,\"Amount\":0,\"SfaContributionPercentage\":null,\"AccountId\":null,\"ApprenticeshipId\":null,\"ApprenticeshipPriceEpisodeId\":null,\"ApprenticeshipEmployerType\":0,\"TransferSenderAccountId\":null,\"Priority\":null,\"DataLockFailures\":[],\"AgreedOnDate\":null},{\"PriceEpisodeIdentifier\":null,\"Period\":9,\"Amount\":0,\"SfaContributionPercentage\":null,\"AccountId\":null,\"ApprenticeshipId\":null,\"ApprenticeshipPriceEpisodeId\":null,\"ApprenticeshipEmployerType\":0,\"TransferSenderAccountId\":null,\"Priority\":null,\"DataLockFailures\":[],\"AgreedOnDate\":null},{\"PriceEpisodeIdentifier\":null,\"Period\":10,\"Amount\":0,\"SfaContributionPercentage\":null,\"AccountId\":null,\"ApprenticeshipId\":null,\"ApprenticeshipPriceEpisodeId\":null,\"ApprenticeshipEmployerType\":0,\"TransferSenderAccountId\":null,\"Priority\":null,\"DataLockFailures\":[],\"AgreedOnDate\":null},{\"PriceEpisodeIdentifier\":null,\"Period\":11,\"Amount\":0,\"SfaContributionPercentage\":null,\"AccountId\":null,\"ApprenticeshipId\":null,\"ApprenticeshipPriceEpisodeId\":null,\"ApprenticeshipEmployerType\":0,\"TransferSenderAccountId\":null,\"Priority\":null,\"DataLockFailures\":[],\"AgreedOnDate\":null},{\"PriceEpisodeIdentifier\":null,\"Period\":12,\"Amount\":0,\"SfaContributionPercentage\":null,\"AccountId\":null,\"ApprenticeshipId\":null,\"ApprenticeshipPriceEpisodeId\":null,\"ApprenticeshipEmployerType\":0,\"TransferSenderAccountId\":null,\"Priority\":null,\"DataLockFailures\":[],\"AgreedOnDate\":null}],\"CensusDate\":\"0001-01-01T00:00:00\"},{\"Type\":16,\"Periods\":[{\"PriceEpisodeIdentifier\":null,\"Period\":1,\"Amount\":0,\"SfaContributionPercentage\":null,\"AccountId\":null,\"ApprenticeshipId\":null,\"ApprenticeshipPriceEpisodeId\":null,\"ApprenticeshipEmployerType\":0,\"TransferSenderAccountId\":null,\"Priority\":null,\"DataLockFailures\":[],\"AgreedOnDate\":null},{\"PriceEpisodeIdentifier\":null,\"Period\":2,\"Amount\":0,\"SfaContributionPercentage\":null,\"AccountId\":null,\"ApprenticeshipId\":null,\"ApprenticeshipPriceEpisodeId\":null,\"ApprenticeshipEmployerType\":0,\"TransferSenderAccountId\":null,\"Priority\":null,\"DataLockFailures\":[],\"AgreedOnDate\":null},{\"PriceEpisodeIdentifier\":null,\"Period\":3,\"Amount\":0,\"SfaContributionPercentage\":null,\"AccountId\":null,\"ApprenticeshipId\":null,\"ApprenticeshipPriceEpisodeId\":null,\"ApprenticeshipEmployerType\":0,\"TransferSenderAccountId\":null,\"Priority\":null,\"DataLockFailures\":[],\"AgreedOnDate\":null},{\"PriceEpisodeIdentifier\":null,\"Period\":4,\"Amount\":0,\"SfaContributionPercentage\":null,\"AccountId\":null,\"ApprenticeshipId\":null,\"ApprenticeshipPriceEpisodeId\":null,\"ApprenticeshipEmployerType\":0,\"TransferSenderAccountId\":null,\"Priority\":null,\"DataLockFailures\":[],\"AgreedOnDate\":null},{\"PriceEpisodeIdentifier\":null,\"Period\":5,\"Amount\":0,\"SfaContributionPercentage\":null,\"AccountId\":null,\"ApprenticeshipId\":null,\"ApprenticeshipPriceEpisodeId\":null,\"ApprenticeshipEmployerType\":0,\"TransferSenderAccountId\":null,\"Priority\":null,\"DataLockFailures\":[],\"AgreedOnDate\":null},{\"PriceEpisodeIdentifier\":null,\"Period\":6,\"Amount\":0,\"SfaContributionPercentage\":null,\"AccountId\":null,\"ApprenticeshipId\":null,\"ApprenticeshipPriceEpisodeId\":null,\"ApprenticeshipEmployerType\":0,\"TransferSenderAccountId\":null,\"Priority\":null,\"DataLockFailures\":[],\"AgreedOnDate\":null},{\"PriceEpisodeIdentifier\":null,\"Period\":7,\"Amount\":0,\"SfaContributionPercentage\":null,\"AccountId\":null,\"ApprenticeshipId\":null,\"ApprenticeshipPriceEpisodeId\":null,\"ApprenticeshipEmployerType\":0,\"TransferSenderAccountId\":null,\"Priority\":null,\"DataLockFailures\":[],\"AgreedOnDate\":null},{\"PriceEpisodeIdentifier\":null,\"Period\":8,\"Amount\":0,\"SfaContributionPercentage\":null,\"AccountId\":null,\"ApprenticeshipId\":null,\"ApprenticeshipPriceEpisodeId\":null,\"ApprenticeshipEmployerType\":0,\"TransferSenderAccountId\":null,\"Priority\":null,\"DataLockFailures\":[],\"AgreedOnDate\":null},{\"PriceEpisodeIdentifier\":null,\"Period\":9,\"Amount\":0,\"SfaContributionPercentage\":null,\"AccountId\":null,\"ApprenticeshipId\":null,\"ApprenticeshipPriceEpisodeId\":null,\"ApprenticeshipEmployerType\":0,\"TransferSenderAccountId\":null,\"Priority\":null,\"DataLockFailures\":[],\"AgreedOnDate\":null},{\"PriceEpisodeIdentifier\":null,\"Period\":10,\"Amount\":0,\"SfaContributionPercentage\":null,\"AccountId\":null,\"ApprenticeshipId\":null,\"ApprenticeshipPriceEpisodeId\":null,\"ApprenticeshipEmployerType\":0,\"TransferSenderAccountId\":null,\"Priority\":null,\"DataLockFailures\":[],\"AgreedOnDate\":null},{\"PriceEpisodeIdentifier\":null,\"Period\":11,\"Amount\":0,\"SfaContributionPercentage\":null,\"AccountId\":null,\"ApprenticeshipId\":null,\"ApprenticeshipPriceEpisodeId\":null,\"ApprenticeshipEmployerType\":0,\"TransferSenderAccountId\":null,\"Priority\":null,\"DataLockFailures\":[],\"AgreedOnDate\":null},{\"PriceEpisodeIdentifier\":null,\"Period\":12,\"Amount\":0,\"SfaContributionPercentage\":null,\"AccountId\":null,\"ApprenticeshipId\":null,\"ApprenticeshipPriceEpisodeId\":null,\"ApprenticeshipEmployerType\":0,\"TransferSenderAccountId\":null,\"Priority\":null,\"DataLockFailures\":[],\"AgreedOnDate\":null}],\"CensusDate\":\"0001-01-01T00:00:00\"}],\"PriceEpisodes\":[{\"Identifier\":\"25-5-01/08/2024\",\"TotalNegotiatedPrice1\":17000.0,\"TotalNegotiatedPrice2\":1000.0,\"TotalNegotiatedPrice3\":0.0,\"TotalNegotiatedPrice4\":0.0,\"AgreedPrice\":18000.0,\"CourseStartDate\":\"2021-03-15T00:00:00+00:00\",\"StartDate\":\"2024-08-01T01:00:00+01:00\",\"EffectiveTotalNegotiatedPriceStartDate\":\"2021-03-15T00:00:00+00:00\",\"PlannedEndDate\":\"2025-03-15T00:00:00+00:00\",\"ActualEndDate\":\"2025-04-25T01:00:00+01:00\",\"NumberOfInstalments\":7,\"InstalmentAmount\":300.0,\"CompletionAmount\":3600.0,\"Completed\":false,\"EmployerContribution\":900.0,\"CompletionHoldBackExemptionCode\":0,\"FundingLineType\":\"19+ Apprenticeship Non-Levy Contract (procured)\",\"LearningAimSequenceNumber\":2}],\"CollectionYear\":2425,\"AgeAtStartOfLearning\":33,\"JobId\":1,\"EventTime\":\"2025-09-11T16:53:59.9940109+00:00\",\"EventId\":\"5f9c6173-1d5c-495b-8b4f-d747ac56558d\",\"Ukprn\":0,\"Learner\":{\"ReferenceNumber\":\"213085\",\"Uln\":6573161563},\"LearningAim\":{\"Reference\":\"ZPROG001\",\"ProgrammeType\":25,\"StandardCode\":5,\"FrameworkCode\":0,\"PathwayCode\":0,\"FundingLineType\":\"19+ Apprenticeship Non-Levy Contract (procured)\",\"SequenceNumber\":2,\"StartDate\":\"2021-03-15T00:00:00+00:00\"},\"IlrSubmissionDateTime\":\"0001-01-01T00:00:00\",\"IlrFileName\":null,\"CollectionPeriod\":{\"AcademicYear\":2425,\"Period\":1}}";
            
        }

        [Test]
        public async Task TestHandleNormalEvent()
        {
            // arrange
            var period = CollectionPeriodFactory.CreateFromAcademicYearAndPeriod(1819, 2);
            byte deliveryPeriod = 2;

            var earningEvent = new ApprenticeshipContractType2EarningEvent
            {
                Ukprn = 1,
                CollectionPeriod = period,
                CollectionYear = period.AcademicYear,
                Learner = EarningEventDataHelper.CreateLearner(),
                LearningAim = EarningEventDataHelper.CreateLearningAim(),
                OnProgrammeEarnings = new List<OnProgrammeEarning>()
                {
                    new OnProgrammeEarning
                    {
                        Type = OnProgrammeEarningType.Learning,
                        Periods = new ReadOnlyCollection<EarningPeriod>(new List<EarningPeriod>()
                        {
                            new EarningPeriod
                            {
                                Amount = 100,
                                Period = deliveryPeriod,
                                PriceEpisodeIdentifier = "2",
                                SfaContributionPercentage = 0.9m,
                            }
                        })
                    }
                },
                PriceEpisodes = new List<PriceEpisode>
                {
                    new PriceEpisode
                    {
                        LearningAimSequenceNumber = 2,
                        Identifier = "2"
                    }
                }
            };

            var requiredPayments = new List<RequiredPayment>
            {
                new RequiredPayment
                {
                    Amount = 1,
                    EarningType = EarningType.CoInvested,
                },
            };

            var paymentHistoryEntities = new[] { new PaymentHistoryEntity
            {
                CollectionPeriod = CollectionPeriodFactory.CreateFromAcademicYearAndPeriod(1819, 2),
                DeliveryPeriod = 2,
                LearnAimReference = earningEvent.LearningAim.Reference,
                TransactionType = (int)OnProgrammeEarningType.Learning
            } };

            var paymentHistory = new ConditionalValue<PaymentHistoryEntity[]>(true, paymentHistoryEntities);
            paymentHistoryCacheMock.Setup(c => c.TryGet(It.Is<string>(key => key == CacheKeys.PaymentHistoryKey), It.IsAny<CancellationToken>())).ReturnsAsync(paymentHistory).Verifiable();
            requiredPaymentService
                .Setup(p => p.GetRequiredPayments(It.Is<Earning>(x => x.Amount == 100), It.Is<List<Payment>>(x => x.Count == 1)))
                .Returns(requiredPayments)
                .Verifiable();

            // act
            var actualRequiredPayment = await act2EarningEventProcessor.HandleEarningEvent(earningEvent, paymentHistoryCacheMock.Object, CancellationToken.None);

            // assert
            ClassicAssert.IsNotNull(actualRequiredPayment);
            ClassicAssert.AreEqual(1, actualRequiredPayment.Count);
            ClassicAssert.AreEqual(1, actualRequiredPayment.First().AmountDue);
            ClassicAssert.AreEqual(earningEvent.LearningAim.Reference, actualRequiredPayment.First().LearningAim.Reference);
            ClassicAssert.AreEqual(2, actualRequiredPayment.First().LearningAimSequenceNumber);
            ClassicAssert.AreEqual(earningEvent.StartDate, actualRequiredPayment.First().LearningStartDate);
        }

        [Test]
        public async Task TestHandleZeroEvent()
        {
            // arrange
            var period = CollectionPeriodFactory.CreateFromAcademicYearAndPeriod(1819, 2);
            byte deliveryPeriod = 2;

            var earningEvent = new ApprenticeshipContractType2EarningEvent
            {
                Ukprn = 1,
                CollectionPeriod = period,
                CollectionYear = period.AcademicYear,
                Learner = EarningEventDataHelper.CreateLearner(),
                LearningAim = EarningEventDataHelper.CreateLearningAim(),
                OnProgrammeEarnings = new List<OnProgrammeEarning>()
                {
                    new OnProgrammeEarning
                    {
                        Type = OnProgrammeEarningType.Completion,
                        Periods = new ReadOnlyCollection<EarningPeriod>(new List<EarningPeriod>()
                        {
                            new EarningPeriod
                            {
                                Amount = 0,
                                Period = deliveryPeriod,
                                PriceEpisodeIdentifier = null,
                                SfaContributionPercentage = 0m,
                            }
                        })
                    }
                }
            };

            var requiredPayments = new List<RequiredPayment>();

            var paymentHistoryEntities = new PaymentHistoryEntity[0];

            var paymentHistory = new ConditionalValue<PaymentHistoryEntity[]>(true, paymentHistoryEntities);
            paymentHistoryCacheMock.Setup(c => c.TryGet(It.Is<string>(key => key == CacheKeys.PaymentHistoryKey), It.IsAny<CancellationToken>())).ReturnsAsync(paymentHistory).Verifiable();
            requiredPaymentService
                .Setup(p => p.GetRequiredPayments(It.Is<Earning>(x => x.Amount == 0), It.Is<List<Payment>>(x => x.Count == 0)))
                .Returns(requiredPayments)
                .Verifiable();

            // act
            var actualRequiredPayment = await act2EarningEventProcessor.HandleEarningEvent(earningEvent, paymentHistoryCacheMock.Object, CancellationToken.None);

            // assert
            ClassicAssert.IsNotNull(actualRequiredPayment);
            actualRequiredPayment.Should().BeEmpty();
        }

        [Test]
        public async Task TestHandleNegativeEarningEvent()
        {
            // arrange
            var period = CollectionPeriodFactory.CreateFromAcademicYearAndPeriod(1819, 2);
            byte deliveryPeriod = 2;

            var earningEvent = new ApprenticeshipContractType2EarningEvent
            {
                Ukprn = 1,
                CollectionPeriod = period,
                CollectionYear = period.AcademicYear,
                Learner = EarningEventDataHelper.CreateLearner(),
                LearningAim = EarningEventDataHelper.CreateLearningAim(),
                OnProgrammeEarnings = new List<OnProgrammeEarning>()
                {
                    new OnProgrammeEarning
                    {
                        Type = OnProgrammeEarningType.Learning,
                        Periods = new ReadOnlyCollection<EarningPeriod>(new List<EarningPeriod>()
                        {
                            new EarningPeriod
                            {
                                Amount = -100,
                                Period = deliveryPeriod,
                                PriceEpisodeIdentifier = "2",
                                SfaContributionPercentage = 0.9m,
                            }
                        })
                    }
                },
                PriceEpisodes = new List<PriceEpisode>
                {
                    new PriceEpisode
                    {
                        LearningAimSequenceNumber = 1234
                    }
                }
            };

            var requiredPayments = new List<RequiredPayment>
            {
                new RequiredPayment
                {
                    Amount = 1,
                    EarningType = EarningType.CoInvested,
                },
            };

            var paymentHistoryEntities = new[] { new PaymentHistoryEntity
            {
                CollectionPeriod = CollectionPeriodFactory.CreateFromAcademicYearAndPeriod(1819, 2),
                DeliveryPeriod = 1,
                LearnAimReference = earningEvent.LearningAim.Reference,
                TransactionType = (int)OnProgrammeEarningType.Learning
            } };

            var paymentHistory = new ConditionalValue<PaymentHistoryEntity[]>(true, paymentHistoryEntities);
            paymentHistoryCacheMock.Setup(c => c.TryGet(CacheKeys.PaymentHistoryKey, It.IsAny<CancellationToken>())).ReturnsAsync(paymentHistory).Verifiable();

            negativeEarningsService.Setup(x => x.ProcessNegativeEarning(-100, It.Is<List<Payment>>(y => y.Count == 1), 2, It.IsAny<string>()))
                .Returns(requiredPayments);

            // act
            var actualRequiredPayment = await act2EarningEventProcessor.HandleEarningEvent(earningEvent, paymentHistoryCacheMock.Object, CancellationToken.None);

            // assert
            ClassicAssert.IsNotNull(actualRequiredPayment);
            ClassicAssert.AreEqual(1, actualRequiredPayment.Count);
            ClassicAssert.AreEqual(earningEvent.LearningAim.Reference, actualRequiredPayment.First().LearningAim.Reference);
        }

        [Test]
        public async Task TestNoEventProducedWhenZeroToPay()
        {
            // arrange
            var period = CollectionPeriodFactory.CreateFromAcademicYearAndPeriod(1819, 2);
            byte deliveryPeriod = 2;

            var earningEvent = new ApprenticeshipContractType2EarningEvent
            {
                Ukprn = 1,
                CollectionPeriod = period,
                CollectionYear = period.AcademicYear,
                Learner = EarningEventDataHelper.CreateLearner(),
                LearningAim = EarningEventDataHelper.CreateLearningAim(),
                OnProgrammeEarnings = new List<OnProgrammeEarning>()
                {
                    new OnProgrammeEarning
                    {
                        Type = OnProgrammeEarningType.Learning,
                        Periods = new ReadOnlyCollection<EarningPeriod>(new List<EarningPeriod>()
                        {
                            new EarningPeriod
                            {
                                Amount = 100,
                                Period = deliveryPeriod,
                                PriceEpisodeIdentifier = "2",
                                SfaContributionPercentage = 0.9m,
                            }
                        })
                    }
                }
            };

            var requiredPayments = new List<RequiredPayment>();

            var paymentHistoryEntities = new PaymentHistoryEntity[0];

            var paymentHistory = new ConditionalValue<PaymentHistoryEntity[]>(true, paymentHistoryEntities);
            paymentHistoryCacheMock.Setup(c => c.TryGet(It.Is<string>(key => key == CacheKeys.PaymentHistoryKey), It.IsAny<CancellationToken>())).ReturnsAsync(paymentHistory).Verifiable();

            requiredPaymentService
                .Setup(p => p.GetRequiredPayments(It.Is<Earning>(x => x.Amount == 100), It.Is<List<Payment>>(x => x.Count == 0)))
                .Returns(requiredPayments)
                .Verifiable();

            // act
            var actualRequiredPayment = await act2EarningEventProcessor.HandleEarningEvent(earningEvent, paymentHistoryCacheMock.Object, CancellationToken.None);

            // assert
            ClassicAssert.AreEqual(0, actualRequiredPayment.Count);
        }

        [Test]
        [TestCase(0, "1")]
        [TestCase(0, null)]
        [TestCase(50, "1")]
        public async Task TestPriceEpisodeIdentifierPickedFromHistoryForRefunds(decimal amount, string priceEpisodeIdentifier)
        {
            // arrange
            var period = CollectionPeriodFactory.CreateFromAcademicYearAndPeriod(1819, 3);
            var deliveryPeriod = (byte)2;

            var earningEvent = new ApprenticeshipContractType2EarningEvent
            {
                Ukprn = 1,
                CollectionPeriod = period,
                CollectionYear = period.AcademicYear,
                Learner = EarningEventDataHelper.CreateLearner(),
                LearningAim = EarningEventDataHelper.CreateLearningAim(),
                OnProgrammeEarnings = new List<OnProgrammeEarning>()
                {
                    new OnProgrammeEarning
                    {
                        Type = OnProgrammeEarningType.Balancing,
                        Periods = new ReadOnlyCollection<EarningPeriod>(new List<EarningPeriod>()
                        {
                            new EarningPeriod
                            {
                                Amount = amount,
                                Period = deliveryPeriod,
                                PriceEpisodeIdentifier = priceEpisodeIdentifier,
                                SfaContributionPercentage = 0.9m,
                            }
                        })
                    }
                },
                PriceEpisodes = new List<PriceEpisode>
                {
                    new PriceEpisode
                    {
                        LearningAimSequenceNumber = 1234
                    }
                }
            };

            var paymentHistoryEntities = new PaymentHistoryEntity[0];
            var requiredPayments = new List<RequiredPayment>
            {
                new RequiredPayment
                {
                    Amount = 1,
                    EarningType = EarningType.CoInvested,
                    SfaContributionPercentage = 0.8m,
                    PriceEpisodeIdentifier = "2",
                },
            };
            var paymentHistory = new ConditionalValue<PaymentHistoryEntity[]>(true, paymentHistoryEntities);
            paymentHistoryCacheMock.Setup(c => c.TryGet(It.Is<string>(key => key == CacheKeys.PaymentHistoryKey), It.IsAny<CancellationToken>())).ReturnsAsync(paymentHistory).Verifiable();

            requiredPaymentService
                .Setup(p => p.GetRequiredPayments(It.Is<Earning>(x => x.Amount == amount), It.Is<List<Payment>>(x => x.Count == 0)))
                .Returns(requiredPayments)
                .Verifiable();

            // act
            var actualRequiredPayment = await act2EarningEventProcessor.HandleEarningEvent(earningEvent, paymentHistoryCacheMock.Object, CancellationToken.None);

            // assert
            ClassicAssert.IsNotNull(actualRequiredPayment);
            ClassicAssert.AreEqual("2", actualRequiredPayment.First().PriceEpisodeIdentifier);
        }

        [Test]
        [InlineAutoData(ApprenticeshipEmployerType.Levy)]
        [InlineAutoData(ApprenticeshipEmployerType.NonLevy)]
        public async Task TestPriceEpisodeIdentifierPickedFromHistoryForRefunds2(ApprenticeshipEmployerType employerType, OnProgrammeEarning earning, PaymentHistoryEntity previousPayment)
        {
            // arrange
            var period = CollectionPeriodFactory.CreateFromAcademicYearAndPeriod(1819, 3);
            var deliveryPeriod = (byte)2;

            var earningEvent = new ApprenticeshipContractType2EarningEvent
            {
                Ukprn = 1,
                CollectionPeriod = period,
                CollectionYear = period.AcademicYear,
                Learner = EarningEventDataHelper.CreateLearner(),
                LearningAim = EarningEventDataHelper.CreateLearningAim(),
                OnProgrammeEarnings = new List<OnProgrammeEarning>()
                {
                    earning,
                },
                PriceEpisodes = new List<PriceEpisode>
                {
                    new PriceEpisode
                    {
                        LearningAimSequenceNumber = 1234
                    }
                }
            };

            earning.Periods[0].ApprenticeshipEmployerType
                = previousPayment.ApprenticeshipEmployerType
                = employerType;

            earning.Type = OnProgrammeEarningType.Balancing;
            earning.Periods = earning.Periods.Take(1).ToList().AsReadOnly();
            earning.Periods[0].Period = deliveryPeriod;
            earning.Periods[0].Amount = previousPayment.Amount - 1;

            var requiredPayments = new List<RequiredPayment>
            {
                new RequiredPayment
                {
                    Amount = -1,
                    EarningType = EarningType.CoInvested,
                    ApprenticeshipEmployerType = employerType,
                    ApprenticeshipId = previousPayment.ApprenticeshipId,
                    ApprenticeshipPriceEpisodeId = previousPayment.ApprenticeshipPriceEpisodeId,
                },
            };

            previousPayment.LearnAimReference = earningEvent.LearningAim.Reference;
            previousPayment.DeliveryPeriod = deliveryPeriod;
            previousPayment.TransactionType = (int)earning.Type;

            var paymentHistory = ConditionalValue.WithArray(previousPayment);
            paymentHistoryCacheMock
                .Setup(c => c.TryGet(It.Is<string>(key => key == CacheKeys.PaymentHistoryKey), It.IsAny<CancellationToken>()))
                .ReturnsAsync(paymentHistory);

            requiredPaymentService
                .Setup(p => p.GetRequiredPayments(It.IsAny<Earning>(), It.IsAny<List<Payment>>()))
                .Returns(requiredPayments);

            // act
            var actualRequiredPayment =
                await act2EarningEventProcessor.HandleEarningEvent(earningEvent, paymentHistoryCacheMock.Object, CancellationToken.None);

            // assert
            actualRequiredPayment.Should().BeEquivalentTo(
            new
            {
                previousPayment.ApprenticeshipEmployerType,
                previousPayment.ApprenticeshipId,
                previousPayment.ApprenticeshipPriceEpisodeId,
            });
        }

        [Test]
        public async Task TestSfaContributionIsCalculatedForZeroEarningRefunds()
        {
            // arrange
            var period = CollectionPeriodFactory.CreateFromAcademicYearAndPeriod(1819, 3);
            byte deliveryPeriod = 2;

            var earningEvent = new ApprenticeshipContractType2EarningEvent
            {
                Ukprn = 1,
                SfaContributionPercentage = 0,
                CollectionPeriod = period,
                CollectionYear = period.AcademicYear,
                Learner = EarningEventDataHelper.CreateLearner(),
                LearningAim = EarningEventDataHelper.CreateLearningAim(),
                OnProgrammeEarnings = new List<OnProgrammeEarning>
                {
                    new OnProgrammeEarning
                    {
                        Type = OnProgrammeEarningType.Balancing,
                        Periods = new ReadOnlyCollection<EarningPeriod>(new List<EarningPeriod>
                        {
                            new EarningPeriod
                            {
                                Period = deliveryPeriod,
                                Amount = 0,
                                PriceEpisodeIdentifier = "priceEpisodeIdentifier",
                                SfaContributionPercentage = 0
                            }
                        })
                    }
                },
                PriceEpisodes = new List<PriceEpisode>
                {
                    new PriceEpisode
                    {
                        LearningAimSequenceNumber = 1234
                    }
                }
            };

            var requiredPayments = new List<RequiredPayment>
            {
                new RequiredPayment
                {
                    Amount = 1,
                    EarningType = EarningType.CoInvested,
                    SfaContributionPercentage = 0.77m,
                },
            };

            var paymentHistoryEntities = new List<PaymentHistoryEntity>();
            var paymentHistory = new ConditionalValue<PaymentHistoryEntity[]>(true, paymentHistoryEntities.ToArray());
            paymentHistoryCacheMock.Setup(c => c.TryGet(It.Is<string>(key => key == CacheKeys.PaymentHistoryKey), It.IsAny<CancellationToken>())).ReturnsAsync(paymentHistory).Verifiable();
            requiredPaymentService
                .Setup(p => p.GetRequiredPayments(It.Is<Earning>(x => x.Amount == 0), It.Is<List<Payment>>(x => x.Count == 0)))
                .Returns(requiredPayments)
                .Verifiable();

            // act
            var actualRequiredPayment = await act2EarningEventProcessor.HandleEarningEvent(earningEvent, paymentHistoryCacheMock.Object, CancellationToken.None);

            // assert
            ClassicAssert.IsNotEmpty(actualRequiredPayment);
            ClassicAssert.AreEqual(.77m, ((CalculatedRequiredCoInvestedAmount)actualRequiredPayment[0]).SfaContributionPercentage);
        }

        [Test]
        public async Task TestFuturePeriodsCutOff()
        {
            // arrange
            var period = CollectionPeriodFactory.CreateFromAcademicYearAndPeriod(1819, 2);

            var earningEvent = new ApprenticeshipContractType2EarningEvent
            {
                Ukprn = 1,
                CollectionPeriod = period,
                CollectionYear = period.AcademicYear,
                Learner = EarningEventDataHelper.CreateLearner(),
                LearningAim = EarningEventDataHelper.CreateLearningAim(),
                OnProgrammeEarnings = new List<OnProgrammeEarning>()
                {
                    new OnProgrammeEarning
                    {
                        Type = OnProgrammeEarningType.Learning,
                        Periods = new ReadOnlyCollection<EarningPeriod>(new List<EarningPeriod>()
                        {
                            new EarningPeriod
                            {
                                Amount = 100,
                                Period = period.Period,
                                PriceEpisodeIdentifier = "2",
                                SfaContributionPercentage = 0.9m,
                            },
                            new EarningPeriod
                            {
                                Amount = 200,
                                Period = (byte)(period.Period + 1),
                                PriceEpisodeIdentifier = "2",
                                SfaContributionPercentage = 0.9m,
                            }
                        })
                    }
                },
                PriceEpisodes = new List<PriceEpisode>
                {
                    new PriceEpisode
                    {
                        LearningAimSequenceNumber = 1234
                    }
                }
            };

            var requiredPayments = new List<RequiredPayment>
            {
                new RequiredPayment
                {
                    Amount = 100,
                    EarningType = EarningType.CoInvested,
                },
            };

            var paymentHistoryEntities = new PaymentHistoryEntity[0];
            var paymentHistory = new ConditionalValue<PaymentHistoryEntity[]>(true, paymentHistoryEntities);
            paymentHistoryCacheMock.Setup(c => c.TryGet(It.Is<string>(key => key == CacheKeys.PaymentHistoryKey), It.IsAny<CancellationToken>())).ReturnsAsync(paymentHistory).Verifiable();
            requiredPaymentService
                .Setup(p => p.GetRequiredPayments(It.Is<Earning>(x => x.Amount == 100), It.Is<List<Payment>>(x => x.Count == 0)))
                .Returns(requiredPayments)
                .Verifiable();

            // act
            var actualRequiredPayment = await act2EarningEventProcessor.HandleEarningEvent(earningEvent, paymentHistoryCacheMock.Object, CancellationToken.None);

            // assert
            ClassicAssert.IsNotNull(actualRequiredPayment);
            ClassicAssert.AreEqual(1, actualRequiredPayment.Count);
            ClassicAssert.AreEqual(100, actualRequiredPayment.First().AmountDue);
            ClassicAssert.AreEqual(earningEvent.LearningAim.Reference, actualRequiredPayment.First().LearningAim.Reference);
        }

        [Test]
        public async Task IgnoresDuplicateEvents()
        {
            // arrange
            var period = CollectionPeriodFactory.CreateFromAcademicYearAndPeriod(1819, 2);
            byte deliveryPeriod = 2;

            var earningEvent = new ApprenticeshipContractType2EarningEvent
            {
                Ukprn = 1,
                CollectionPeriod = period,
                CollectionYear = period.AcademicYear,
                Learner = EarningEventDataHelper.CreateLearner(),
                LearningAim = EarningEventDataHelper.CreateLearningAim(),
                OnProgrammeEarnings = new List<OnProgrammeEarning>()
                {
                    new OnProgrammeEarning
                    {
                        Type = OnProgrammeEarningType.Learning,
                        Periods = new ReadOnlyCollection<EarningPeriod>(new List<EarningPeriod>()
                        {
                            new EarningPeriod
                            {
                                Amount = 100,
                                Period = deliveryPeriod,
                                PriceEpisodeIdentifier = "2",
                                SfaContributionPercentage = 0.9m,
                            }
                        })
                    }
                }
            };

            var requiredPayments = new List<RequiredPayment>
            {
                new RequiredPayment
                {
                    Amount = 1,
                    EarningType = EarningType.CoInvested,
                },
            };

            var paymentHistoryEntities = new[] { new PaymentHistoryEntity
            {
                CollectionPeriod = CollectionPeriodFactory.CreateFromAcademicYearAndPeriod(1819, 2),
                DeliveryPeriod = 2,
                LearnAimReference = earningEvent.LearningAim.Reference,
                TransactionType = (int)OnProgrammeEarningType.Learning
            } };

            var paymentHistory = new ConditionalValue<PaymentHistoryEntity[]>(true, paymentHistoryEntities);
            paymentHistoryCacheMock.Setup(c => c.TryGet(It.Is<string>(key => key == CacheKeys.PaymentHistoryKey), It.IsAny<CancellationToken>())).ReturnsAsync(paymentHistory);
            mocker.Mock<IDuplicateEarningEventService>()
                .Setup(x => x.IsDuplicate(It.IsAny<IPaymentsEvent>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);
            requiredPaymentService
                 .Setup(p => p.GetRequiredPayments(It.Is<Earning>(x => x.Amount == 100), It.Is<List<Payment>>(x => x.Count == 1)))
                .Returns(requiredPayments);

            // act
            var actualRequiredPayment = await act2EarningEventProcessor.HandleEarningEvent(earningEvent, paymentHistoryCacheMock.Object, CancellationToken.None);

            // assert
            ClassicAssert.IsNotNull(actualRequiredPayment);
            ClassicAssert.IsFalse(actualRequiredPayment.Any());
        }
    }
}