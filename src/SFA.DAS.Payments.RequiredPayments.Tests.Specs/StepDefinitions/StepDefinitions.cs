using System.Collections.ObjectModel;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using Reqnroll;
using SFA.DAS.Payments.AcceptanceTests.Core.Data;
using SFA.DAS.Payments.DataLocks.Messages.Events;
using SFA.DAS.Payments.EarningEvents.Messages.Events;
using SFA.DAS.Payments.Model.Core;
using SFA.DAS.Payments.Model.Core.Entities;
using SFA.DAS.Payments.Model.Core.Incentives;
using SFA.DAS.Payments.Model.Core.OnProgramme;
using SFA.DAS.Payments.RequiredPayments.Messages.Events;
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
        private FunctionalSkillType functionalSkillType;
        private List<EarningPeriod> periods;


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
            periods = new List<EarningPeriod>();
            Console.WriteLine($"UKPRN : {testSession.Provider.Ukprn}, ULN: {testSession.Learner.Uln}, collection year: {currentAcademicYear}");
        }

        [AfterScenario]
        public void AfterScenario()
        {
            ResetCapturedEvents();
        }

        private static void ResetCapturedEvents()
        {
            RequiredIncentivePaymentHandler.ReceivedEvents.Clear();
            RequiredLevyPaymentsHandler.ReceivedEvents.Clear();
            RequiredCoInvestedPaymentsHandler.ReceivedEvents.Clear();
        }

        [Given("a Levy employer with an Apprentice")]
        public void GivenALevyEmployerWithAnApprentice()
        {
            testSession.Learner.IsLevyLearner = true;
            periods = new List<EarningPeriod>
            {
                new EarningPeriod
                {
                    Amount = 100,
                    SfaContributionPercentage = 0.95m,
                    Period = 1,
                    PriceEpisodeIdentifier = "pe-1",
                    ApprenticeshipId = 12345,
                    ApprenticeshipEmployerType = ApprenticeshipEmployerType.Levy,
                },
            };
        }

        [Given("a Non-levy employer with an Apprentice")]
        public void GivenANonLevyEmployerWithAnApprentice()
        {
            testSession.Learner.IsLevyLearner = false;

            periods = new List<EarningPeriod>
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
            };
        }


        [Given("an apprentice changes from a Non-Levy to a Levy employer")]
        public void GivenAnApprenticeChangesFromNonLevyToALevyEmployer()
        {
            testSession.Learner.IsLevyLearner = true;
            periods = new List<EarningPeriod>
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
                new EarningPeriod
                {
                    Amount = 100,
                    SfaContributionPercentage = 0.95m,
                    Period = 1,
                    PriceEpisodeIdentifier = "pe-1",
                    ApprenticeshipId = 12345,
                    ApprenticeshipEmployerType = ApprenticeshipEmployerType.Levy,
                },
            };
        }

        [Given("an apprentice changes from a Levy to a Non-Levy employer")]
        public void GivenAnApprenticeChangesFromLevyToANonLevyEmployer()
        {
            testSession.Learner.IsLevyLearner = false;
            periods = new List<EarningPeriod>
            {
                new EarningPeriod
                {
                    Amount = 100,
                    SfaContributionPercentage = 0.95m,
                    Period = 1,
                    PriceEpisodeIdentifier = "pe-1",
                    ApprenticeshipId = 12345,
                    ApprenticeshipEmployerType = ApprenticeshipEmployerType.Levy,
                },
                new EarningPeriod
                {
                    Amount = 100,
                    SfaContributionPercentage = 0.95m,
                    Period = 1,
                    PriceEpisodeIdentifier = "pe-1",
                    ApprenticeshipId = 12345,
                    ApprenticeshipEmployerType = ApprenticeshipEmployerType.NonLevy,
                },
            };
        }

        [Given("the SFA contribution percentage was previously calculated to be 90%")]
        public void GivenTheSFAContributionPercentageWasPreviouslyCalculatedToBe90Percent()
        {
            testSession.Learner.IsLevyLearner = false;

            periods = new List<EarningPeriod>
            {
                new EarningPeriod
                {
                    Amount = 100,
                    SfaContributionPercentage = 0.9m,
                    Period = 1,
                    PriceEpisodeIdentifier = "pe-1",
                    ApprenticeshipId = 12345,
                    ApprenticeshipEmployerType = ApprenticeshipEmployerType.NonLevy,
                },
            };
        }

        [Given("the learning start date is on or after 1 August 2026")]
        public void GivenTheLearningStartDateIsOnOrAfter1August2026()
        {
            ilrLearningStartDate = new DateTime(2026, 8, 1);
        }

        [Given("the Levy Employer has insufficient balance")]
        [Given("the Levy Employer has zero balance")]
        public void GivenTheLevyEmployerHasInsufficientBalance()
        {
        }

        [Given("the learning start date is before 1 August 2026")]
        [Given("the learning start date is before 1 August 2026 and after 1 Apr 2024")]
        public void GivenTheLearningStartDateIsBefore1August2026AndAfter1Apr2024()
        {
            ilrLearningStartDate = new DateTime(2026, 7, 31);
        }

        [Given("the learning start date is before 1 April 2024")]
        public void GivenTheLearningStartDateIsBefore1April2024()
        {
            ilrLearningStartDate = new DateTime(2024, 3, 31);
        }

        // The actual number needs to be decided
        [Given(@"the learner is aged between (\d+) and (\d+) on the start date")]
        public void GivenTheLearnerIsAgedBetweenOnTheStartDate(int minimumAge, int maximumAge)
        {
            ageAtStartOfLearning = maximumAge;
        }

        [Given("the learner is aged 25 or over on the start date")]
        public void GivenTheLearnerIsAged25OrOverOnTheStartDate()
        {
            ageAtStartOfLearning = 25;
        }

        [Given("the learner is aged under 25")]
        [Given("the learner is aged under 25 on the start date")]
        public void GivenTheLearnerIsAgedUnder25()
        {
            ageAtStartOfLearning = 24;
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

        [Given("the Required Payments Service receives a GSL Functional Skills Earnings Event")]
        [Given("the event represents CourseType = Functional Skill")]
        [Given("the event represents LearningType = Maths and English")]
        public void GivenTheRequiredPaymentsServiceReceivesAGSLFunctionalSkillsEarningsEvent()
        {
            var message = new GSLFunctionalSkillEarningsEvent
            {
                CollectionPeriod = new CollectionPeriod { AcademicYear = currentAcademicYear, Period = 3 },
                CollectionYear = currentAcademicYear,
                Ukprn = testSession.Provider.Ukprn,
                JobId = testSession.JobId,
                ContractType = ContractType.Act1,
                Learner = new SFA.DAS.Payments.Model.Core.Learner
                {
                    Uln = testSession.Learner.Uln,
                    ReferenceNumber = testSession.Learner.LearnRefNumber
                },
                StartDate = testSession.Learner.Course.LearningStartDate,
                IlrSubmissionDateTime = DateTime.Now,
                AgeAtStartOfLearning = 33,
                LearningAim = new LearningAim
                {
                    Reference = testSession.Learner.Course.Reference,
                    CourseCode = testSession.Learner.Course.LearnAimRef,
                    SequenceNumber = testSession.Learner.Course.AimSeqNumber,
                    ProgrammeType = testSession.Learner.Course.ProgrammeType,
                    FrameworkCode = testSession.Learner.Course.FrameworkCode,
                    PathwayCode = testSession.Learner.Course.PathwayCode,
                    StandardCode = testSession.Learner.Course.StandardCode,
                    FundingLineType = "19+ Apprenticeship (Employer on App Service)",
                    LearningType = LearningType.MathsAndEnglish
                },
                PriceEpisodes = new List<PriceEpisode>
                {
                    new PriceEpisode{
                        Identifier = "pe-1",
                        LearningAimSequenceNumber = testSession.Learner.Course.AimSeqNumber,
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
                        FundingLineType = "19+ Apprenticeship (Employer on App Service)",

                    }
                },
            };

            scenarioContext["FunctionalSkillEarningsEvent"] = message;
        }

        [Given("the event contains (.*) earnings")]
        public void GivenTheEventContainsOn_ProgrammeMathsAndEnglishEarnings(String earningType)
        {
            functionalSkillType = ParseFunctionalSkillType(earningType);
            var message = scenarioContext.Get<GSLFunctionalSkillEarningsEvent>("FunctionalSkillEarningsEvent");

            message.Earnings = new ReadOnlyCollection<FunctionalSkillEarning>(new List<FunctionalSkillEarning>()
                {
                    new FunctionalSkillEarning
                    {
                        Type = functionalSkillType,
                        Periods = new ReadOnlyCollection<EarningPeriod>(new List<EarningPeriod>
                        {
                            new EarningPeriod
                            {
                                Period = 1,
                                Amount = 100,
                                PriceEpisodeIdentifier = "pe-1",
                                SfaContributionPercentage = 0.9m,
                            }
                        })
                    }
                }
            );
        }


        [When("the event is processed by the Required Payments")]
        public async Task WhenTheEventIsProcessedByTheRequiredPayments()
        {
            var message = (GSLFunctionalSkillEarningsEvent) scenarioContext["FunctionalSkillEarningsEvent"];
            await messagingContext.Send(message);
        }


        [When("the ILR is submitted - Levy")]
        public async Task WhenTheIlrIsSubmittedLevy()
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
                    FundingLineType = "19+ Apprenticeship Levy Contract",
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
                        FundingLineType = "19+ Apprenticeship Levy Contract",
                    }
                },
                OnProgrammeEarnings = new List<OnProgrammeEarning>
                {
                    new OnProgrammeEarning
                    {
                        Type = onProgrammeEarningType,
                        Periods = periods.AsReadOnly(),
                    }
                },
            };

            await messagingContext.Send(message);
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
                        Periods = periods.AsReadOnly(),
                    }
                },
            };

            await messagingContext.Send(message);
        }

        [Then(@"the payment is fully funded by SFA \(100%\)")]
        public async Task ThenPaymentLineIsGeneratedForSfaCoInvestment()
        {
            var events = await WaitForRequiredLevyPayments();
            Assert.That(events.Count, Is.EqualTo(1));
            Assert.That(events[0].SfaContributionPercentage, Is.EqualTo(1m));
        }

        [Then(@"the payment funding is split between 'SFA co-investment' \(95%\) and 'Employer co-investment' \(5%\)")]
        public async Task ThenPaymentLinesAreGenerated95SplitBetweenSfaCoInvestmentAndEmployerCoInvestment()
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

        [Then("the (.*) earnings should be processed successfully")]
        public async Task ThenTheEarningTypeEarningsShouldBeProcessedSuccessfully(string earningType)
        {
            var expectedFunctionalSkillType = ParseFunctionalSkillType(earningType);
            await testSession.WaitForIt(
                () => RequiredIncentivePaymentHandler.GetEvents(testSession.Learner)
                    .Any(ev => ev.TransactionType == (TransactionType)expectedFunctionalSkillType),
                "Failed to find GSL Functional Skills event");
        }

        [Then("the incoming Maths and English earnings should be mapped to the outgoing Calculated Required Incentive Amount message")]
        [Then("the earning type, amount, academic year and delivery period should match the values received in the incoming event")]
        [Then("the Calculated Required Incentive Amount message should be published for downstream processing")]
        public async Task ThenTheIncomingMathsAndEnglishEarningsShouldBeMappedToTheOutgoingCalculatedRequiredIncentiveAmountMessage()
        {
            var incomingEvent = (GSLFunctionalSkillEarningsEvent)scenarioContext["FunctionalSkillEarningsEvent"];
            var requiredIncentivePayment = RequiredIncentivePaymentHandler
                .GetEvents(testSession.Learner)
                .Single();

            Assert.That(requiredIncentivePayment.Ukprn, Is.EqualTo(incomingEvent.Ukprn));
            Assert.That(requiredIncentivePayment.JobId, Is.EqualTo(incomingEvent.JobId));
            Assert.That(requiredIncentivePayment.CollectionPeriod.AcademicYear, Is.EqualTo(incomingEvent.CollectionPeriod.AcademicYear));
            Assert.That(requiredIncentivePayment.CollectionPeriod.Period, Is.EqualTo(incomingEvent.CollectionPeriod.Period));
            Assert.That(requiredIncentivePayment.Learner.Uln, Is.EqualTo(incomingEvent.Learner.Uln));
            Assert.That(requiredIncentivePayment.Learner.ReferenceNumber, Is.EqualTo(incomingEvent.Learner.ReferenceNumber));
            Assert.That(requiredIncentivePayment.LearningAim.Reference, Is.EqualTo(incomingEvent.LearningAim.Reference));
            Assert.That(requiredIncentivePayment.LearningAim.ProgrammeType, Is.EqualTo(incomingEvent.LearningAim.ProgrammeType));
            Assert.That(requiredIncentivePayment.LearningAim.StandardCode, Is.EqualTo(incomingEvent.LearningAim.StandardCode));
            Assert.That(requiredIncentivePayment.LearningAim.FundingLineType, Is.EqualTo("19+ Apprenticeship (Employer on App Service)"));
            Assert.That(requiredIncentivePayment.TransactionType, Is.EqualTo((TransactionType)functionalSkillType));
            Assert.That(requiredIncentivePayment.Type, Is.EqualTo((IncentivePaymentType)functionalSkillType));
            Assert.That(requiredIncentivePayment.AmountDue, Is.EqualTo(100m));
            Assert.That(requiredIncentivePayment.DeliveryPeriod, Is.EqualTo(1));
            Assert.That(requiredIncentivePayment.EventId, Is.Not.EqualTo(Guid.Empty));
        }

        [Then(@"the payment funding is split between 'SFA co-investment' \(75%\) and 'Employer co-investment' \(25%\)")]
        public async Task ThenPaymentLinesAreGenerated75SplitBetweenSfaCoInvestmentAndEmployerCoInvestment()
        {

            var events = await WaitForRequiredLevyPayments();
            Assert.That(events.Count, Is.EqualTo(1));

            var requiredPayment = events.Single();
            Assert.That(requiredPayment.SfaContributionPercentage, Is.EqualTo(0.75m));

            var sfaAmount = requiredPayment.AmountDue * requiredPayment.SfaContributionPercentage;
            var employerAmount = requiredPayment.AmountDue - sfaAmount;
            Assert.That(sfaAmount, Is.EqualTo(75m));
            Assert.That(employerAmount, Is.EqualTo(25m));
        }


        [Then(@"then two payments are generated for 'Non-Levy' \(95%\) and 'Levy' \(75%\)")]
        [Then(@"then two payments are generated for 'Levy' \(75%\) and 'Non-Levy' \(95%\)")]
        public async Task Then2PaymentLinesAreGeneratedForNonLevyAndLevy()
        {

            var events = await WaitForRequiredLevyPayments();
            Assert.That(events.Count, Is.EqualTo(2));

            var nonLevyPayment = events.FirstOrDefault(x => x.employerType == ApprenticeshipEmployerType.NonLevy);
            AssertRequiredPaymentAndSfaPercentage(nonLevyPayment.AmountDue, nonLevyPayment.SfaContributionPercentage, 95m, 5m);
            var levyPayment = events.FirstOrDefault(x => x.employerType == ApprenticeshipEmployerType.Levy);
            AssertRequiredPaymentAndSfaPercentage(levyPayment.AmountDue, levyPayment.SfaContributionPercentage, 75m, 25m);
        }

        [Then(@"the payment funding is split between 'SFA co-investment' \(90%\) and 'Employer co-investment' \(10%\)")]
        public async Task ThenPaymentLinesAreGenerated95SplitBetween90PercentSfaCoInvestmentAnd10PercentEmployerCoInvestment()
        {

            var events = await WaitForRequiredLevyPayments();
            Assert.That(events.Count, Is.EqualTo(1));

            var requiredPayment = events.Single();
            Assert.That(requiredPayment.SfaContributionPercentage, Is.EqualTo(0.9m));

            var sfaAmount = requiredPayment.AmountDue * requiredPayment.SfaContributionPercentage;
            var employerAmount = requiredPayment.AmountDue - sfaAmount;
            Assert.That(sfaAmount, Is.EqualTo(90m));
            Assert.That(employerAmount, Is.EqualTo(10m));
        }

        private void AssertRequiredPaymentAndSfaPercentage(decimal amountDue, decimal sfaContributionPercentage, decimal expectedSfa, decimal expectedEmployerAmount)
        {
            var sfaAmount = amountDue * sfaContributionPercentage;
            var employerAmount = amountDue - sfaAmount;
            Assert.That(sfaAmount, Is.EqualTo(expectedSfa));
            Assert.That(employerAmount, Is.EqualTo(expectedEmployerAmount));
        }
        private async Task<List<(decimal AmountDue, decimal SfaContributionPercentage, ApprenticeshipEmployerType employerType)>> WaitForRequiredLevyPayments()
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
                .Select(ev => (ev.AmountDue, ev.SfaContributionPercentage, ev.ApprenticeshipEmployerType))
                .ToList();
        }

        private static FunctionalSkillType ParseFunctionalSkillType(string earningType)
        {
            ArgumentNullException.ThrowIfNull(earningType);
            var normalisedEarningType = earningType.Trim().ToLowerInvariant();

            return normalisedEarningType switch
            {
                "on-programme maths and english" => FunctionalSkillType.OnProgrammeMathsAndEnglish,
                "balancing maths and english" => FunctionalSkillType.BalancingMathsAndEnglish,
                "learning support" => FunctionalSkillType.LearningSupport,
                _ => throw new ArgumentOutOfRangeException(nameof(earningType), earningType, "Unsupported functional skill earning type")
            };
        }
    }
}
