using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using Reqnroll;
using SFA.DAS.Payments.AcceptanceTests.Core.Data;
using SFA.DAS.Payments.DataLocks.Messages.Events;
using SFA.DAS.Payments.EarningEvents.Messages.Events;
using SFA.DAS.Payments.Model.Core;
using SFA.DAS.Payments.Model.Core.Entities;
using SFA.DAS.Payments.Model.Core.OnProgramme;
using SFA.DAS.Payments.RequiredPayments.Tests.Specs.Handlers;

namespace SFA.DAS.Payments.RequiredPayments.Tests.Specs.StepDefinitions
{
    [Binding]
    public class CompletionPaymentSteps
    {
        private readonly ScenarioContext scenarioContext;
        private readonly MessagingContext messagingContext;
        private readonly TestSession testSession;
        private short currentAcademicYear;

        public CompletionPaymentSteps(ScenarioContext scenarioContext, MessagingContext messagingContext, TestSession testSession)
        {
            this.scenarioContext = scenarioContext;
            this.messagingContext = messagingContext;
            this.testSession = testSession;
        }

        [BeforeScenario]
        public void ClearReceivedEvents()
        {
            currentAcademicYear = new CollectionPeriodBuilder().WithDate(DateTime.Today).Build().AcademicYear;
            RequiredLevyPaymentsHandler.ReceivedEvents.Clear();
            CompletionPaymentHeldBackHandler.ReceivedEvents.Clear();
        }

        [Given("the apprentice has a learning start date before 1 August 2026")]
        public void GivenTheApprenticeHasALearningStartDateBefore1August2026()
        {
            scenarioContext["LearningStartDate"] = new DateTime(2026, 7, 31);
        }

        [Given("the Co-invested payments for the apprenticeship were recorded prior to the requirement to record the Reporting Funding Line Type")]
        public async Task GivenTheCo_InvestedPaymentsForTheApprenticeshipWereRecordedPriorToTheRequirementToRecordTheReportingFundingLineType()
        {
            var learningStartDate = scenarioContext.Get<DateTime>("LearningStartDate");
            testSession.Learner.Course.LearningStartDate = learningStartDate;
            testSession.DataContext.Payment.Add(new PaymentModel
            {
                Ukprn = testSession.Provider.Ukprn,
                LearnerUln = testSession.Learner.Uln,
                CollectionPeriod = new CollectionPeriod { AcademicYear = (short)(currentAcademicYear - 101), Period = 1 },
                DeliveryPeriod = 1,
                ContractType = ContractType.Act2,
                TransactionType = TransactionType.Learning,
                FundingSource = FundingSourceType.CoInvestedSfa,
                LearnerReferenceNumber = testSession.Learner.LearnRefNumber,
                LearningAimStandardCode = testSession.Learner.Course.StandardCode,
                LearningAimProgrammeType = testSession.Learner.Course.ProgrammeType,
                CompletionAmount = 3600,
                Amount = 270,
                IlrSubmissionDateTime = testSession.Learner.Course.LearningStartDate,
                CompletionStatus = 1,
                InstalmentAmount = 300,
                LearningAimReference = "ZPROG001",
                LearningAimFundingLineType = "16-18 Apprenticeship Non-Levy Contract (procured)",
                SfaContributionPercentage = 0.9M,
                LearningStartDate = testSession.Learner.Course.LearningStartDate,
                StartDate = testSession.Learner.Course.LearningStartDate,
                ActualEndDate = null,
                PlannedEndDate = DateTime.Now,
                NumberOfInstalments = 48,
                ApprenticeshipEmployerType = ApprenticeshipEmployerType.NonLevy,
                PriceEpisodeIdentifier = "pe-1",
                ReportingAimFundingLineType = string.Empty,
                EventTime = DateTime.Now,
                EventId = Guid.NewGuid()
            });
            testSession.DataContext.Payment.Add(new PaymentModel
            {
                Ukprn = testSession.Provider.Ukprn,
                LearnerUln = testSession.Learner.Uln,
                CollectionPeriod = new CollectionPeriod { AcademicYear = (short)(currentAcademicYear - 101), Period = 1 },
                DeliveryPeriod = 1,
                ContractType = ContractType.Act2,
                TransactionType = TransactionType.Learning,
                FundingSource = FundingSourceType.CoInvestedEmployer,
                LearnerReferenceNumber = testSession.Learner.LearnRefNumber,
                LearningAimStandardCode = testSession.Learner.Course.StandardCode,
                LearningAimProgrammeType = testSession.Learner.Course.ProgrammeType,
                CompletionAmount = 3600,
                Amount = 30,
                IlrSubmissionDateTime = testSession.Learner.Course.LearningStartDate,
                CompletionStatus = 1,
                InstalmentAmount = 300,
                LearningAimReference = "ZPROG001",
                LearningAimFundingLineType = "16-18 Apprenticeship Non-Levy Contract (procured)",
                SfaContributionPercentage = 0.9M,
                LearningStartDate = testSession.Learner.Course.LearningStartDate,
                StartDate = testSession.Learner.Course.LearningStartDate,
                ActualEndDate = null,
                PlannedEndDate = DateTime.Now,
                NumberOfInstalments = 48,
                ApprenticeshipEmployerType = ApprenticeshipEmployerType.NonLevy,
                PriceEpisodeIdentifier = "pe-1",
                ReportingAimFundingLineType = string.Empty,
                EventTime = DateTime.Now,
                EventId = Guid.NewGuid()
            });
            testSession.DataContext.Payment.Add(new PaymentModel
            {
                Ukprn = testSession.Provider.Ukprn,
                LearnerUln = testSession.Learner.Uln,
                CollectionPeriod = new CollectionPeriod { AcademicYear = (short)(currentAcademicYear - 101), Period = 2 },
                DeliveryPeriod = 2,
                ContractType = ContractType.Act2,
                TransactionType = TransactionType.Learning,
                FundingSource = FundingSourceType.CoInvestedSfa,
                LearnerReferenceNumber = testSession.Learner.LearnRefNumber,
                LearningAimStandardCode = testSession.Learner.Course.StandardCode,
                LearningAimProgrammeType = testSession.Learner.Course.ProgrammeType,
                CompletionAmount = 3600,
                Amount = 270,
                IlrSubmissionDateTime = testSession.Learner.Course.LearningStartDate,
                CompletionStatus = 1,
                InstalmentAmount = 300,
                LearningAimReference = "ZPROG001",
                LearningAimFundingLineType = "16-18 Apprenticeship Non-Levy Contract (procured)",
                SfaContributionPercentage = 0.9M,
                LearningStartDate = testSession.Learner.Course.LearningStartDate,
                StartDate = testSession.Learner.Course.LearningStartDate,
                ActualEndDate = null,
                PlannedEndDate = DateTime.Now,
                NumberOfInstalments = 48,
                ApprenticeshipEmployerType = ApprenticeshipEmployerType.NonLevy,
                PriceEpisodeIdentifier = "pe-1",
                ReportingAimFundingLineType = string.Empty,
                EventTime = DateTime.Now,
                EventId = Guid.NewGuid()
            });
            testSession.DataContext.Payment.Add(new PaymentModel
            {
                Ukprn = testSession.Provider.Ukprn,
                LearnerUln = testSession.Learner.Uln,
                CollectionPeriod = new CollectionPeriod { AcademicYear = (short)(currentAcademicYear - 101), Period = 2 },
                DeliveryPeriod = 2,
                ContractType = ContractType.Act2,
                TransactionType = TransactionType.Learning,
                FundingSource = FundingSourceType.CoInvestedEmployer,
                LearnerReferenceNumber = testSession.Learner.LearnRefNumber,
                LearningAimStandardCode = testSession.Learner.Course.StandardCode,
                LearningAimProgrammeType = testSession.Learner.Course.ProgrammeType,
                CompletionAmount = 3600,
                Amount = 30,
                IlrSubmissionDateTime = testSession.Learner.Course.LearningStartDate,
                CompletionStatus = 1,
                InstalmentAmount = 300,
                LearningAimReference = "ZPROG001",
                LearningAimFundingLineType = "16-18 Apprenticeship Non-Levy Contract (procured)",
                SfaContributionPercentage = 0.9M,
                LearningStartDate = testSession.Learner.Course.LearningStartDate,
                StartDate = testSession.Learner.Course.LearningStartDate,
                ActualEndDate = null,
                PlannedEndDate = DateTime.Now,
                NumberOfInstalments = 48,
                ApprenticeshipEmployerType = ApprenticeshipEmployerType.NonLevy,
                PriceEpisodeIdentifier = "pe-1",
                ReportingAimFundingLineType = string.Empty,
                EventTime = DateTime.Now,
                EventId = Guid.NewGuid()
            });
            testSession.DataContext.Payment.Add(new PaymentModel
            {
                Ukprn = testSession.Provider.Ukprn,
                LearnerUln = testSession.Learner.Uln,
                CollectionPeriod = new CollectionPeriod { AcademicYear = (short)(currentAcademicYear - 101), Period = 3 },
                DeliveryPeriod = 3,
                ContractType = ContractType.Act2,
                TransactionType = TransactionType.Learning,
                FundingSource = FundingSourceType.CoInvestedSfa,
                LearnerReferenceNumber = testSession.Learner.LearnRefNumber,
                LearningAimStandardCode = testSession.Learner.Course.StandardCode,
                LearningAimProgrammeType = testSession.Learner.Course.ProgrammeType,
                CompletionAmount = 3600,
                Amount = 270,
                IlrSubmissionDateTime = testSession.Learner.Course.LearningStartDate,
                CompletionStatus = 1,
                InstalmentAmount = 300,
                LearningAimReference = "ZPROG001",
                LearningAimFundingLineType = "16-18 Apprenticeship Non-Levy Contract (procured)",
                SfaContributionPercentage = 0.9M,
                LearningStartDate = testSession.Learner.Course.LearningStartDate,
                StartDate = testSession.Learner.Course.LearningStartDate,
                ActualEndDate = null,
                PlannedEndDate = DateTime.Now,
                NumberOfInstalments = 48,
                ApprenticeshipEmployerType = ApprenticeshipEmployerType.NonLevy,
                PriceEpisodeIdentifier = "pe-1",
                ReportingAimFundingLineType = string.Empty,
                EventTime = DateTime.Now,
                EventId = Guid.NewGuid()
            });
            testSession.DataContext.Payment.Add(new PaymentModel
            {
                Ukprn = testSession.Provider.Ukprn,
                LearnerUln = testSession.Learner.Uln,
                CollectionPeriod = new CollectionPeriod { AcademicYear = (short)(currentAcademicYear - 101), Period = 3 },
                DeliveryPeriod = 3,
                ContractType = ContractType.Act2,
                TransactionType = TransactionType.Learning,
                FundingSource = FundingSourceType.CoInvestedEmployer,
                LearnerReferenceNumber = testSession.Learner.LearnRefNumber,
                LearningAimStandardCode = testSession.Learner.Course.StandardCode,
                LearningAimProgrammeType = testSession.Learner.Course.ProgrammeType,
                CompletionAmount = 3600,
                Amount = 30,
                IlrSubmissionDateTime = testSession.Learner.Course.LearningStartDate,
                CompletionStatus = 1,
                InstalmentAmount = 300,
                LearningAimReference = "ZPROG001",
                LearningAimFundingLineType = "16-18 Apprenticeship Non-Levy Contract (procured)",
                SfaContributionPercentage = 0.9M,
                LearningStartDate = testSession.Learner.Course.LearningStartDate,
                StartDate = testSession.Learner.Course.LearningStartDate,
                ActualEndDate = null,
                PlannedEndDate = DateTime.Now,
                NumberOfInstalments = 48,
                ApprenticeshipEmployerType = ApprenticeshipEmployerType.NonLevy,
                PriceEpisodeIdentifier = "pe-1",
                ReportingAimFundingLineType = string.Empty,
                EventTime = DateTime.Now,
                EventId = Guid.NewGuid()
            });

            await testSession.DataContext.SaveChangesAsync();
            var sql = $"update Payments2.Payment set ReportingAimFundingLineType = null where Ukprn = {testSession.Provider.Ukprn} and LearnerUln = {testSession.Learner.Uln}";
            Console.WriteLine(sql);
            await testSession.DataContext.Database.ExecuteSqlRawAsync(sql);
            await testSession.DataContext.SaveChangesAsync();

        }


        [Given("a (.*) employer with an apprentice")]
        public void GivenAnEmployerWithAnApprentice(string employerType)
        {
            testSession.Learner.IsLevyLearner = employerType.Equals("Levy");
            scenarioContext["EmployerType"] = employerType;
        }

        [Given("the apprentice has a completion date on or after 1 August 2026")]
        public void GivenTheApprenticeHasACompletionDateOnOrAfter1August2026()
        {
            scenarioContext["CompletionDate"] = new DateTime(2026, 8, 1);
        }

        [Given("the completion payment is due for payment")]
        public async Task GivenTheCompletionPaymentIsDueForPayment()
        {
            var collectionPeriod = new CollectionPeriodBuilder().WithDate(DateTime.Today).Build();
            testSession.DataContext.Payment.Add(new PaymentModel
            {
                Ukprn = testSession.Provider.Ukprn,
                LearnerUln = testSession.Learner.Uln,
                CollectionPeriod = collectionPeriod,
                DeliveryPeriod = 1,
                ContractType = ContractType.Act1,
                TransactionType = TransactionType.Learning,
                FundingSource = FundingSourceType.CoInvestedEmployer,
                JobId = testSession.JobId,
                LearnerReferenceNumber = testSession.Learner.LearnRefNumber,
                LearningAimReference = testSession.Learner.Course.Reference,
                LearningAimFrameworkCode = testSession.Learner.Course.FrameworkCode,
                LearningAimPathwayCode = testSession.Learner.Course.PathwayCode,
                LearningAimStandardCode = testSession.Learner.Course.StandardCode,
                LearningAimProgrammeType = testSession.Learner.Course.ProgrammeType,
                LearningAimFundingLineType = testSession.Learner.IsLevyLearner
                    ? "19+ Apprenticeship Levy Contract"
                    : "19+ Apprenticeship Non-Levy Contract (procured)",
                CompletionAmount = 100,
                Amount = 100,
                LearningStartDate = testSession.Learner.Course.LearningStartDate,
                StartDate = testSession.Learner.Course.LearningStartDate,
                ApprenticeshipEmployerType = testSession.Learner.IsLevyLearner
                    ? ApprenticeshipEmployerType.Levy
                    : ApprenticeshipEmployerType.NonLevy,
                PriceEpisodeIdentifier = "pe-1",
                ReportingAimFundingLineType = string.Empty,
                EventId = Guid.NewGuid()
            });

            await testSession.DataContext.SaveChangesAsync();
        }

        [When("the completion ILR is submitted")]
        public async Task WhenTheCompletionIlrIsSubmitted()
        {
            var completionDate = scenarioContext.Get<DateTime>("CompletionDate");
            var employerType = scenarioContext.Get<string>("EmployerType");
            var collectionPeriod = new CollectionPeriodBuilder().WithDate(DateTime.Today).Build();
            var isLevy = employerType.Equals("Levy");
            var fundingLineType = isLevy
                ? "19+ Apprenticeship Levy Contract"
                : "19+ Apprenticeship Non-Levy Contract (procured)";

            var message = new PayableEarningEvent
            {
                CollectionPeriod = collectionPeriod,
                CollectionYear = collectionPeriod.AcademicYear,
                Ukprn = testSession.Provider.Ukprn,
                JobId = testSession.JobId,
                Learner = new SFA.DAS.Payments.Model.Core.Learner
                {
                    Uln = testSession.Learner.Uln,
                    ReferenceNumber = testSession.Learner.LearnRefNumber
                },
                StartDate = testSession.Learner.Course.LearningStartDate,
                IlrSubmissionDateTime = DateTime.Now,
                AgeAtStartOfLearning = 21,
                LearningAim = new LearningAim
                {
                    Reference = testSession.Learner.Course.Reference,
                    ProgrammeType = testSession.Learner.Course.ProgrammeType,
                    FrameworkCode = testSession.Learner.Course.FrameworkCode,
                    PathwayCode = testSession.Learner.Course.PathwayCode,
                    StandardCode = testSession.Learner.Course.StandardCode,
                    FundingLineType = fundingLineType,
                },
                PriceEpisodes = new List<PriceEpisode>
                {
                    new PriceEpisode
                    {
                        Identifier = "pe-1",
                        LearningAimSequenceNumber = 1,
                        NumberOfInstalments = 12,
                        InstalmentAmount = 100,
                        CompletionAmount = 1800,
                        CompletionHoldBackExemptionCode = 0,
                        ActualEndDate = completionDate,
                        FundingLineType = fundingLineType,
                    }
                },
                OnProgrammeEarnings = new List<OnProgrammeEarning>
                {
                    new OnProgrammeEarning
                    {
                        Type = OnProgrammeEarningType.Completion,
                        Periods = new List<EarningPeriod>
                        {
                            new EarningPeriod
                            {
                                Amount = 1800,
                                Period = 1,
                                PriceEpisodeIdentifier = "pe-1",
                                ApprenticeshipId = 12345,
                                ApprenticeshipEmployerType = isLevy
                                    ? ApprenticeshipEmployerType.Levy
                                    : ApprenticeshipEmployerType.NonLevy,
                            }
                        }.AsReadOnly()
                    }
                }
            };

            await testSession.DataContext.SaveChangesAsync();
            await messagingContext.Send(message);
        }

        [When("the learner completes the apprenticeship")]
        public async Task WhenTheLearnerCompletesTheApprenticeship()
        {
            var learningStartDate = scenarioContext.Get<DateTime>("LearningStartDate");
            var message = new ApprenticeshipContractType2EarningEvent
            {
                CollectionPeriod = new CollectionPeriod { AcademicYear = currentAcademicYear, Period = 3 },
                CollectionYear = currentAcademicYear,
                Ukprn = testSession.Provider.Ukprn,
                JobId = testSession.JobId,
                Learner = new SFA.DAS.Payments.Model.Core.Learner
                {
                    Uln = testSession.Learner.Uln,
                    ReferenceNumber = testSession.Learner.LearnRefNumber
                },
                StartDate = learningStartDate,
                SfaContributionPercentage = 1M,
                IlrSubmissionDateTime = DateTime.Now,
                AgeAtStartOfLearning = 33,
                LearningAim = new LearningAim
                {
                    Reference = testSession.Learner.Course.Reference,
                    ProgrammeType = testSession.Learner.Course.ProgrammeType,
                    StandardCode = testSession.Learner.Course.StandardCode,
                    FundingLineType = "19+ Apprenticeship Non-Levy Contract (procured)"
                },
                PriceEpisodes = new List<PriceEpisode>
                {
                    new PriceEpisode
                    {
                        Identifier = "pe-1",
                        TotalNegotiatedPrice1 = 17000,
                        TotalNegotiatedPrice2 = 1000,
                        AgreedPrice = 18000,
                        ActualEndDate = DateTime.Now,
                        NumberOfInstalments = 7,
                        InstalmentAmount = 300,
                        CompletionAmount = 3600,
                        Completed = false,
                        EmployerContribution = 900,
                        CompletionHoldBackExemptionCode = 0,
                        FundingLineType = "19+ Apprenticeship Non-Levy Contract (procured)"
                    }
                },
                OnProgrammeEarnings = new List<OnProgrammeEarning>
                {
                    new OnProgrammeEarning
                    {
                        Type = OnProgrammeEarningType.Completion,
                        Periods = new List<EarningPeriod>
                        {
                            new EarningPeriod
                            {
                                Amount = 3300,
                                SfaContributionPercentage = .9M,
                                Period = 1,
                                PriceEpisodeIdentifier = "pe-1"
                            }
                        }.AsReadOnly()
                    }
                }
            };

            await messagingContext.Send(message);
        }

        [Then("the service should allow payment of the completion payment")]
        public async Task ThenTheServiceShouldAllowPaymentOfTheCompletionPayment()
        {
            await testSession.WaitForIt(
                () => RequiredCoInvestedPaymentsHandler.GetEvents(testSession.Learner).Any(ev => ev.TransactionType == TransactionType.Completion),
                "Failed to find completion payment event");
        }

        [Then("the completion payment is generated")]
        public async Task ThenTheCompletionPaymentIsGenerated()
        {
            await testSession.WaitForIt(
                HasGeneratedCompletionPayment,
                $"Failed to find completion payment event. Levy events: {RequiredLevyPaymentsHandler.GetEvents(testSession.Learner).Count()}, Held-back events: {CompletionPaymentHeldBackHandler.ReceivedEvents.Count}");
        }

        [Then("the completion payment does not depend on co-investment collection")]
        public void ThenTheCompletionPaymentDoesNotDependOnCoInvestmentCollection()
        {
            Assert.That(HasHeldBackCompletionPayment(), Is.False, "The completion payment was held back");
        }

        [Then("co-investment collection continues independently of completion payment generation")]
        public Task ThenCoInvestmentCollectionContinuesIndependentlyOfCompletionPaymentGeneration()
        {
            return Task.CompletedTask;
        }

        private bool HasGeneratedCompletionPayment()
        {
            return RequiredLevyPaymentsHandler.GetEvents(testSession.Learner)
                .Any(payment => payment.TransactionType == TransactionType.Completion);
        }

        private bool HasHeldBackCompletionPayment()
        {
            return CompletionPaymentHeldBackHandler.GetEvents(testSession.Learner)
                .Any(payment => payment.TransactionType == TransactionType.Completion);
        }
    }
}
