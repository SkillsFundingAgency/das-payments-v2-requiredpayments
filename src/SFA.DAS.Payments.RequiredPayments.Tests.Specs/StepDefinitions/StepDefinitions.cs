using Microsoft.EntityFrameworkCore;
using Reqnroll;
using SFA.DAS.Payments.EarningEvents.Messages;
using SFA.DAS.Payments.AcceptanceTests.Core.Data;
using SFA.DAS.Payments.EarningEvents.Messages.Events;
using SFA.DAS.Payments.Model.Core;
using SFA.DAS.Payments.Model.Core.Entities;
using SFA.DAS.Payments.Model.Core.OnProgramme;
using SFA.DAS.Payments.RequiredPayments.Tests.Specs.Handlers;

namespace SFA.DAS.Payments.RequiredPayments.Tests.Specs.StepDefinitions
{
    [Binding]
    public class StepDefinitions
    {
        private readonly ScenarioContext scenarioContext;
        private readonly MessagingContext messagingContext;
        private readonly TestSession testSession;
        private CollectionPeriod collectionPeriod;
        private short currentAcademicYear;
        private GSLShortCourseEarningsEvent shortCourseEarningsEvent;
        private Guid expectedExternalEarningsId;
        private static readonly Guid ShortCourseExternalEarningsId = Guid.Parse("11111111-1111-1111-1111-111111111111");

        public StepDefinitions(ScenarioContext scenarioContext, MessagingContext messagingContext, TestSession testSession)
        {
            this.scenarioContext = scenarioContext;
            this.messagingContext = messagingContext;
            this.testSession = testSession;
        }

        protected void SetCurrentCollectionYear()
        {
            currentAcademicYear = new CollectionPeriodBuilder().WithDate(DateTime.Today).Build().AcademicYear;
        }

        [BeforeScenario]
        public void BeforeScenario()
        {
            SetCurrentCollectionYear();
            Console.WriteLine($"UKPRN : {testSession.Provider.Ukprn}, ULN: {testSession.Learner.Uln}, collection year: {currentAcademicYear}");
        }

        [AfterScenario]
        public void AfterScenario()
        {
        }

        [Given("the Co-invested payments for the apprenticeship were recorded prior to the requirement to record the Reporting Funding Line Type")]
        public async Task GivenTheCo_InvestedPaymentsForTheApprenticeshipWereRecordedPriorToTheRequirementToRecordTheReportingFundingLineType()
        {
            testSession.Learner.Course.LearningStartDate = DateTime.Now.AddYears(-1); ;
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

        [When("the learner completes the apprenticeship")]
        public async Task WhenTheLearnerCompletesTheApprenticeship()
        {
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
                StartDate = testSession.Learner.Course.LearningStartDate,
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
                    new PriceEpisode{
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
                        FundingLineType = "19+ Apprenticeship Non-Levy Contract (procured)",

                    }
                },


                OnProgrammeEarnings = new List<OnProgrammeEarning> {
                    new OnProgrammeEarning {
                        Type = OnProgrammeEarningType.Completion,
                        Periods = new List<EarningPeriod>{
                            new EarningPeriod
                            {
                                Amount = 3300,
                                SfaContributionPercentage = .9M,
                                Period = 1,
                                PriceEpisodeIdentifier = "pe-1",
                            },
                        }.AsReadOnly()
                    }
                }
            };
            await messagingContext.Send(message);
        }

        [Then("the service should allow payment of the completion payment")]
        public async Task ThenTheServiceShouldAllowPaymentOfTheCompletionPayment()
        {
            await testSession.WaitForIt(() => RequiredCoInvestedPaymentsHandler.GetEvents(testSession.Learner).Any(ev => ev.TransactionType == TransactionType.Completion), "Failed to find completion payment event");
        }

        [Given("a short course earnings event with an external earnings id")]
        public void GivenAShortCourseEarningsEventWithAnExternalEarningsId()
        {
            expectedExternalEarningsId = ShortCourseExternalEarningsId;
            shortCourseEarningsEvent = new GSLShortCourseEarningsEvent
            {
                EventId = Guid.NewGuid(),
                FundingPlatformType = FundingPlatformType.DigitalApprenticeshipService,
                ExternalEarningsId = expectedExternalEarningsId,
                JobId = testSession.JobId,
                Ukprn = testSession.Provider.Ukprn,
                CollectionPeriod = new CollectionPeriod
                {
                    AcademicYear = currentAcademicYear,
                    Period = 1
                },
                EventTime = DateTimeOffset.UtcNow,
                IlrSubmissionDateTime = DateTime.Now,
                AgeAtStartOfLearning = 19,
                Learner = new SFA.DAS.Payments.Model.Core.Learner
                {
                    Uln = testSession.Learner.Uln,
                    ReferenceNumber = testSession.Learner.LearnRefNumber
                },
                LearningAim = new LearningAim
                {
                    Reference = testSession.Learner.Course.Reference,
                    ProgrammeType = testSession.Learner.Course.ProgrammeType,
                    PathwayCode = testSession.Learner.Course.PathwayCode,
                    StandardCode = testSession.Learner.Course.StandardCode,
                    FrameworkCode = testSession.Learner.Course.FrameworkCode,
                    SequenceNumber = testSession.Learner.Course.AimSeqNumber,
                    FundingLineType = testSession.Learner.Course.FundingLineType,
                    StartDate = testSession.Learner.Course.LearningStartDate,
                    LearningType = LearningType.ApprenticeshipUnit
                },
                PriceEpisodes = new List<PriceEpisode>
                {
                    new PriceEpisode
                    {
                        Identifier = "PE-1",
                        LearningAimSequenceNumber = testSession.Learner.Course.AimSeqNumber,
                        FundingLineType = testSession.Learner.Course.FundingLineType,
                        StartDate = testSession.Learner.Course.LearningStartDate,
                        PlannedEndDate = testSession.Learner.Course.LearningPlannedEndDate.GetValueOrDefault(),
                        CompletionAmount = 1000m,
                        InstalmentAmount = 300m,
                        NumberOfInstalments = 2,
                        CourseStartDate = testSession.Learner.Course.LearningStartDate
                    }
                },
                Earnings = new List<ShortCourseEarning>
                {
                    new ShortCourseEarning
                    {
                        Type = ShortCourseEarningType.Milestone1,
                        Periods = new List<EarningPeriod>
                        {
                            new EarningPeriod
                            {
                                Period = 1,
                                Amount = 300m,
                                PriceEpisodeIdentifier = "PE-1",
                                AccountId = 1,
                                ApprenticeshipId = 1,
                                ApprenticeshipEmployerType = ApprenticeshipEmployerType.Levy
                            }
                        }
                    }
                }
            };
        }

        [When("the short course earnings event is processed")]
        public async Task WhenTheShortCourseEarningsEventIsReceived()
        {
            await messagingContext.Send(shortCourseEarningsEvent);

        }

        [Then("the published required payment event should have the same external earnings id")]
        public async Task ThenAllPublishedRequiredPaymentEventsShouldHaveTheSameExternalEarningsId()
        {

            await testSession.WaitForIt(() => PeriodisedRequiredPaymentEventHandler.GetEvents(testSession.Learner).Any(ev => ev.ExternalEarningsId == shortCourseEarningsEvent.ExternalEarningsId), "Failed to find any required payment events with the expected ExternalEarningsId");

        }
    }
}
