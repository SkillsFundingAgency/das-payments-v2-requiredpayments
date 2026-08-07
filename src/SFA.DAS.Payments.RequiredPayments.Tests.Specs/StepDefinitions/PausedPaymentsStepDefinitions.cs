using SFA.DAS.Payments.EarningEvents.Messages;
using SFA.DAS.Payments.EarningEvents.Messages.Events;
using SFA.DAS.Payments.Model.Core;
using SFA.DAS.Payments.Model.Core.Entities;
using SFA.DAS.Payments.RequiredPayments.Tests.Specs.Handlers;

namespace SFA.DAS.Payments.RequiredPayments.Tests.Specs.StepDefinitions
{
    public class PausedPaymentsStepDefinitions : StepDefinitions
    {
        private GSLShortCourseEarningsEvent shortCourseEarningsEvent;

        public PausedPaymentsStepDefinitions(ScenarioContext scenarioContext, MessagingContext messagingContext, TestSession testSession)
            : base(scenarioContext, messagingContext, testSession)
        {
        }


        [Given("the employer has paused payments for the course")]
        [When("the employer has paused payments for the course")]
        [Given("the employer has subsequently paused payments")]        
        public void GivenTheEmployerHasPausedPaymentsForTheCourse()
        {
            shortCourseEarningsEvent = new GSLShortCourseEarningsEvent
            {
                EventId = Guid.NewGuid(),
                FundingPlatformType = FundingPlatformType.DigitalApprenticeshipService,
                ExternalEarningsId = Guid.NewGuid(),
                JobId = testSession.JobId,
                Ukprn = testSession.Provider.Ukprn,
                CollectionPeriod = new CollectionPeriod
                {
                    AcademicYear = currentAcademicYear,
                    Period = 3
                },
                EventTime = DateTimeOffset.UtcNow,
                IlrSubmissionDateTime = DateTime.Now,
                AgeAtStartOfLearning = 19,
                Earnings = new List<ShortCourseEarning>
                {
                    new ShortCourseEarning
                    {
                         Type = ShortCourseEarningType.Milestone1,
                         Periods = new List<EarningPeriod>
                         {
                             new EarningPeriod
                             {                 
                                Period = 3,
                                Amount = 300m,
                                PriceEpisodeIdentifier = "PE-1",
                                AccountId = 1,
                                ApprenticeshipId = 1,
                                ApprenticeshipEmployerType = ApprenticeshipEmployerType.Levy,
                                IsPaymentPaused = true
                            }
                        }
                    }
                }
            };
        }

        [When("we receive the earnings for the course for the first time")]
        public async Task WhenWeReceiveTheEarningsForTheCourseForTheFirstTime()
        {
            await messagingContext.Send(shortCourseEarningsEvent);
        }

        [When("the provider now states that the learner has withdrawn from the course and we received amended earnings")]
        public async Task WhenTheProviderNowStatesThatTheLearnerHasWithdrawnFromTheCourseAndWeReceivedAmendedEarnings()
        {
            shortCourseEarningsEvent.Earnings = new List<ShortCourseEarning>();
            shortCourseEarningsEvent.PriceEpisodes = new List<PriceEpisode>();
            await messagingContext.Send(shortCourseEarningsEvent);
        }

        [When("we receive the new earnings for the course with the milestone payment made in the current collection period")]
        public async Task WhenWeReceiveTheNewEarningsForTheCourseWithTheMilestonePaymentMadeInTheCurrentCollectionPeriod()
        {
            shortCourseEarningsEvent.CollectionPeriod.Period++;
            await messagingContext.Send(shortCourseEarningsEvent);
        }

        [Then("no required payments should be generated")]
        public async Task ThenNoRequiredPaymentsShouldBeGenerated()
        {
            await testSession.WaitForItAndFail(() => RequiredLevyPaymentsHandler.GetEvents(testSession.Learner).Any(ev => ev.FundingPlatformType == FundingPlatformType.DigitalApprenticeshipService), "Required payments were created when payments paused for learner and course");
        }

        [Given("the provider originally stated the learner started and completed the course")]
        public async Task GivenTheProviderOriginallyStatedTheLearnerStartedAndCompletedTheCourse()
        {
            testSession.DataContext.Payment.Add(new PaymentModel
            {
                Ukprn = testSession.Provider.Ukprn,
                LearnerUln = testSession.Learner.Uln,
                CollectionPeriod = new CollectionPeriod { AcademicYear = (short)currentAcademicYear, Period = 1 },
                DeliveryPeriod = 1,
                ContractType = ContractType.Act1,
                TransactionType = TransactionType.Milestone1,
                FundingSource = FundingSourceType.CoInvestedSfa,
                LearnerReferenceNumber = testSession.Learner.LearnRefNumber,
                LearningAimStandardCode = testSession.Learner.Course.StandardCode,
                LearningAimProgrammeType = testSession.Learner.Course.ProgrammeType,
                CompletionAmount = 0,
                Amount = 300,
                IlrSubmissionDateTime = testSession.Learner.Course.LearningStartDate,
                CompletionStatus = 1,
                InstalmentAmount = 300,
                LearningAimReference = "ZSC0001",
                LearningAimFundingLineType = "GSO Short Courses (Apprenticeship Units) Non-Levy",
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
                CollectionPeriod = new CollectionPeriod { AcademicYear = (short)currentAcademicYear, Period = 2 },
                DeliveryPeriod = 1,
                ContractType = ContractType.Act1,
                TransactionType = TransactionType.Completion,
                FundingSource = FundingSourceType.CoInvestedSfa,
                LearnerReferenceNumber = testSession.Learner.LearnRefNumber,
                LearningAimStandardCode = testSession.Learner.Course.StandardCode,
                LearningAimProgrammeType = testSession.Learner.Course.ProgrammeType,
                CompletionAmount = 0,
                Amount = 300,
                IlrSubmissionDateTime = testSession.Learner.Course.LearningStartDate,
                CompletionStatus = 1,
                InstalmentAmount = 300,
                LearningAimReference = "ZSC0001",
                LearningAimFundingLineType = "GSO Short Courses (Apprenticeship Units) Non-Levy",
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
        }

        [Then("the required payments should be generated to process refunds for the previous payments made")]
        public async Task ThenTheRequiredPaymentsShouldBeGeneratedToProcessRefundsForThePreviousPaymentsMode()
        {
            await testSession.WaitForIt(() => RequiredLevyPaymentsHandler.GetEvents(testSession.Learner)
                    .Count(ev =>
                        ev.AmountDue < 0m &&
                        (ev.TransactionType == TransactionType.Milestone1 || ev.TransactionType == TransactionType.Completion)
                    ) >= 2,
                "Refunds for Milestone1 and Completion payments not generated");
        }

        [Given("the milestone payment was made in a previous collection period")]
        public async Task GivenTheMilestonePaymentWasMadeInAPreviousCollectionPeriod()
        {
            testSession.DataContext.Payment.Add(new PaymentModel
            {
                Ukprn = testSession.Provider.Ukprn,
                LearnerUln = testSession.Learner.Uln,
                CollectionPeriod = new CollectionPeriod { AcademicYear = (short)currentAcademicYear, Period = 1 },
                DeliveryPeriod = 1,
                ContractType = ContractType.Act1,
                TransactionType = TransactionType.Milestone1,
                FundingSource = FundingSourceType.CoInvestedSfa,
                LearnerReferenceNumber = testSession.Learner.LearnRefNumber,
                LearningAimStandardCode = testSession.Learner.Course.StandardCode,
                LearningAimProgrammeType = testSession.Learner.Course.ProgrammeType,
                CompletionAmount = 0,
                Amount = 300,
                IlrSubmissionDateTime = testSession.Learner.Course.LearningStartDate,
                CompletionStatus = 1,
                InstalmentAmount = 300,
                LearningAimReference = "ZSC0001",
                LearningAimFundingLineType = "GSO Short Courses (Apprenticeship Units) Non-Levy",
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
        }
    }
}
