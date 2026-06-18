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
    public class StepDefinitions
    {
        private readonly ScenarioContext scenarioContext;
        private readonly MessagingContext messagingContext;
        private readonly TestSession testSession;
        private CollectionPeriod collectionPeriod;
        private short currentAcademicYear;
        private DateTime ilrLearningStartDate;
        private int ageAtStartOfLearning;
        private OnProgrammeEarningType onProgrammeEarningType;

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
            ageAtStartOfLearning = 21;
            onProgrammeEarningType = OnProgrammeEarningType.Learning;
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

        [Given("a Non-levy employer with an Apprentice")]
        public void GivenANonLevyEmployerWithAnApprentice()
        {
            testSession.Learner.IsLevyLearner = false;
        }

        [Given("the learning start date is on or after 1 August 2026")]
        public void GivenTheLearningStartDateIsOnOrAfter1August2026()
        {
            ilrLearningStartDate = new DateTime(2026, 8, 1);
        }

        [Given("the learning start date is before 1 August 2026 and after 1 Apr 2024")]
        public void GivenTheLearningStartDateIsBefore1August2026AndAfter1Apr2024()
        {
            ilrLearningStartDate = new DateTime(2026, 7, 31);
        }

        // The actual number needs to be decided
        [Given(@"the learner is aged between (\d+) and (\d+) on the start date")]
        public void GivenTheLearnerIsAgedBetweenOnTheStartDate(int minimumAge, int maximumAge)
        {
            ageAtStartOfLearning = maximumAge;
        }

        [Given("the transaction type is a {word} payment")]
        public void GivenTheTransactionTypeIsAPayment(string transactionType)
        {
            if (!Enum.TryParse(transactionType, true, out OnProgrammeEarningType parsedOnProgrammeEarningType) ||
                parsedOnProgrammeEarningType is not (OnProgrammeEarningType.Learning
                    or OnProgrammeEarningType.Completion
                    or OnProgrammeEarningType.Balancing))
            {
                Assert.Fail($"Unsupported transaction type: {transactionType}");
            }

            onProgrammeEarningType = parsedOnProgrammeEarningType;
        }


        [When("the ILR is submitted")]
        public async Task WhenTheIlrIsSubmitted()
        {
            testSession.Learner.Course.LearningStartDate = ilrLearningStartDate;

            var message = new PayableEarningEvent
            {
                CollectionPeriod = new CollectionPeriod { AcademicYear = currentAcademicYear, Period = 1 },
                CollectionYear = currentAcademicYear,
                Ukprn = testSession.Provider.Ukprn,
                JobId = testSession.JobId,
                Learner = new SFA.DAS.Payments.Model.Core.Learner
                {
                    Uln = testSession.Learner.Uln,
                    ReferenceNumber = testSession.Learner.LearnRefNumber
                },
                StartDate = ilrLearningStartDate,
                IlrSubmissionDateTime = DateTime.Now,
                AgeAtStartOfLearning = ageAtStartOfLearning,
                LearningAim = new LearningAim
                {
                    Reference = testSession.Learner.Course.Reference,
                    ProgrammeType = testSession.Learner.Course.ProgrammeType,
                    StandardCode = testSession.Learner.Course.StandardCode,
                    FundingLineType = "19+ Apprenticeship Non-Levy Contract (procured)",
                },
                PriceEpisodes = new List<PriceEpisode>
                {
                    new PriceEpisode
                    {
                        Identifier = "pe-1",
                        LearningAimSequenceNumber = 1,
                        NumberOfInstalments = 12,
                        InstalmentAmount = 100,
                        CompletionAmount = 1200,
                        CompletionHoldBackExemptionCode = 0,
                        EmployerContribution = 0,
                        FundingLineType = "19+ Apprenticeship Non-Levy Contract (procured)",
                    }
                },
                OnProgrammeEarnings = new List<OnProgrammeEarning>
                {
                    new OnProgrammeEarning
                    {
                        Type = onProgrammeEarningType,
                        Periods = new List<EarningPeriod>
                        {
                            new EarningPeriod
                            {
                                Amount = 100,
                                SfaContributionPercentage = 0.95m,
                                Period = 1,
                                PriceEpisodeIdentifier = "pe-1",
                                ApprenticeshipId = 12345,
                                ApprenticeshipEmployerType = ApprenticeshipEmployerType.NonLevy,
                            },
                        }.AsReadOnly(),
                    }
                },
            };

            await messagingContext.Send(message);
        }

        [Then(@"1 payment line is generated for 'SFA co-investment' \(100%\)")]
        public async Task ThenPaymentLineIsGeneratedForSfaCoInvestment()
        {
            var events = await WaitForRequiredLevyPayments();
            Assert.That(events.Count, Is.EqualTo(1));
            Assert.That(events[0].SfaContributionPercentage, Is.EqualTo(1m));
        }

        //Need to verify what it means by 2 payments lines - is it 2 separate events with the different contribution percentages, or a single event with the split contribution percentage?
        [Then(@"2 payment lines are generated split between 'SFA co-investment' \(95%\) and 'Employer co-investment' \(5%\)")]
        public async Task ThenPaymentLinesAreGeneratedSplitBetweenSfaCoInvestmentAndEmployerCoInvestment()
        {

            var events = await WaitForRequiredLevyPayments();
            Assert.That(events.Count, Is.EqualTo(1));
            
            var requiredPayment = events.Single();
            Assert.That(requiredPayment.SfaContributionPercentage, Is.EqualTo(0.95m));

            var sfaAmount = requiredPayment.AmountDue * requiredPayment.SfaContributionPercentage;
            var employerAmount = requiredPayment.AmountDue - sfaAmount;
            Assert.That(sfaAmount, Is.EqualTo(95m));
            Assert.That(employerAmount, Is.EqualTo(5m)); //double check this, payment line wise
        }

        private async Task<List<(decimal AmountDue, decimal SfaContributionPercentage)>> WaitForRequiredLevyPayments()
        {
            var expectedTransactionType = onProgrammeEarningType switch
            {
                OnProgrammeEarningType.Learning => TransactionType.Learning,
                OnProgrammeEarningType.Completion => TransactionType.Completion,
                OnProgrammeEarningType.Balancing => TransactionType.Balancing,
                _ => throw new ArgumentOutOfRangeException(nameof(onProgrammeEarningType), onProgrammeEarningType, "Unsupported on-programme earning type for transaction type")
            };
            await testSession.WaitForIt(
                () => RequiredLevyPaymentsHandler.GetEvents(testSession.Learner)
                    .Any(ev => ev.TransactionType == expectedTransactionType),
                "Failed to find levy required payment event");

            return RequiredLevyPaymentsHandler
                .GetEvents(testSession.Learner)
                .Where(ev => ev.TransactionType == expectedTransactionType)
                .Select(ev => (ev.AmountDue, ev.SfaContributionPercentage))
                .ToList();
        }
    }
}
