using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SFA.DAS.Payments.Model.Core;
using SFA.DAS.Payments.Model.Core.Entities;
using SFA.DAS.Payments.RequiredPayments.Application.Processors;
using SFA.DAS.Payments.RequiredPayments.Application.Repositories;
using SFA.DAS.Payments.RequiredPayments.Messages.Events;

namespace SFA.DAS.Payments.RequiredPayments.Application.UnitTests.Application.Processors
{
    [TestFixture]
    public class RemovedLearnerAimIdentificationServiceTest
    {
        [Test]
        public async Task TestServiceReturnsRemovedAims()
        {
            // arrange
            var repositoryMock = new Mock<IPaymentHistoryRepository>(MockBehavior.Strict);
            var service = new RemovedLearnerAimIdentificationService(repositoryMock.Object);

            var expectedAims = new List<IdentifiedRemovedLearningAim>()
            {
                new IdentifiedRemovedLearningAim
                {
                    CollectionPeriod = new CollectionPeriod {AcademicYear = 1, Period = 1},
                    Ukprn = 3,
                },
                new IdentifiedRemovedLearningAim
                {
                    CollectionPeriod = new CollectionPeriod {AcademicYear = 1, Period = 1},
                    Ukprn = 3,
                }
            };

            repositoryMock.Setup(r => r.IdentifyRemovedLearnerAims(1, 2, 3, DateTime.Today, CancellationToken.None)).ReturnsAsync(expectedAims).Verifiable();

            // act
            var actualAims = await service.IdentifyRemovedLearnerAims(1, 2, 3, DateTime.Today, CancellationToken.None).ConfigureAwait(false);

            // assert
            actualAims.Should().BeSameAs(expectedAims);

            repositoryMock.Verify();
        }

        [Test]
        public async Task Test_ServiceReturnsRemovedAims_Fo_Ineligible_Aim()
        {
            // arrange
            var repositoryMock = new Mock<IPaymentHistoryRepository>();
            var service = new RemovedLearnerAimIdentificationService(repositoryMock.Object);


            var learner = new Learner
            {
                ReferenceNumber = "ref-123",
                Uln = 12345678
            };
            var learningAim = new LearningAim
            {
                Reference = "ref-345",
                StandardCode = 5,
                ProgrammeType = 25,
                FrameworkCode = 0,
                PathwayCode = 0,
                FundingLineType = "fs-1"
            };
            var ilrSubmissionTime = DateTime.Now;
            repositoryMock.Setup(r => r.FindRemovedAimContractTypes(2425, 3, It.IsAny<Learner>(), It.IsAny<LearningAim>(), It.IsAny<CancellationToken>())).ReturnsAsync(new List<ContractType> { ContractType.Act1}).Verifiable();
            var expectedAims = new List<IdentifiedRemovedLearningAim>()
            {
                new IdentifiedRemovedLearningAim
                {
                    CollectionPeriod = new CollectionPeriod {AcademicYear = 2425, Period = 1},
                    Ukprn = 3,
                    Learner = learner,
                    LearningAim = learningAim,
                    ContractType = ContractType.Act1,
                    JobId = 5, 
                    IlrSubmissionDateTime = ilrSubmissionTime
                }
            };

            // act
            var actualAims = await service.IdentifyRemovedLearnerAims(2425, 1, 3, learner,learningAim,5, ilrSubmissionTime, CancellationToken.None).ConfigureAwait(false);

            // assert
            actualAims.Count.Should().Be(1);
            actualAims.FirstOrDefault().ContractType.Should().Be(ContractType.Act1);
            actualAims.FirstOrDefault().CollectionPeriod.AcademicYear.Should().Be(2425);
            actualAims.FirstOrDefault().Ukprn.Should().Be(3);
            actualAims.FirstOrDefault().JobId.Should().Be(5);
            actualAims.FirstOrDefault().Learner.Should().BeSameAs(learner);
            actualAims.FirstOrDefault().LearningAim.Should().BeSameAs(learningAim);
        }
    }
}
