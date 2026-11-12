using NUnit.Framework;
using SFA.DAS.Payments.EarningEvents.Messages;
using SFA.DAS.Payments.EarningEvents.Messages.Events;
using SFA.DAS.Payments.Model.Core;
using SFA.DAS.Payments.Model.Core.Entities;
using SFA.DAS.Payments.RequiredPayments.Messages.Events;
using SFA.DAS.Payments.RequiredPayments.Tests.Specs.Handlers;

namespace SFA.DAS.Payments.RequiredPayments.Tests.Specs.StepDefinitions
{
    [Binding]
    public class ShortCourseRefundSteps
    {
        private const string CourseCode = "ZSC00001";
        private const string PriceEpisodeIdentifier = "pe-1";
        private const byte PaymentHistoryCollectionPeriod = 2;
        private const byte WithdrawalCollectionPeriod = 3;
        private const decimal CompletionAmount = 700m;
        private const decimal InstalmentAmount = 300m;
        private const short NumberOfInstalments = 1;

        private readonly MessagingContext messagingContext;
        private readonly TestSession testSession;
        private readonly short academicYear = new CollectionPeriodBuilder().WithDate(DateTime.Today).Build().AcademicYear;
        private readonly DateTime startDate = new DateTime(2026, 9, 1);
        private readonly DateTime plannedEndDate = new DateTime(2026, 10, 31);
        private readonly DateTime actualEndDate = new DateTime(2026, 9, 20);
        private readonly long accountId;
        private readonly long apprenticeshipId;
        private ApprenticeshipEmployerType employerType;
        private decimal sfaContributionPercentage;
        private List<PeriodisedRequiredPaymentEvent> refunds = new List<PeriodisedRequiredPaymentEvent>();

        public ShortCourseRefundSteps(MessagingContext messagingContext, TestSession testSession)
        {
            this.messagingContext = messagingContext;
            this.testSession = testSession;
            accountId = testSession.GenerateId();
            apprenticeshipId = testSession.GenerateId();
        }

        private string FundingLineType => employerType == ApprenticeshipEmployerType.Levy
            ? "GSO Short Courses (Apprenticeship Units) Levy"
            : "GSO Short Courses (Apprenticeship Units) Non-Levy";

        [Given("an Apprenticeship Unit learner with a (Levy|Non-Levy) employer who started training on 1 September 2026")]
        public void GivenAnApprenticeshipUnitLearnerWithAnEmployerWhoStartedTrainingOn1September2026(string employer)
        {
            employerType = employer == "Levy" ? ApprenticeshipEmployerType.Levy : ApprenticeshipEmployerType.NonLevy;
            testSession.Learner.IsLevyLearner = employerType == ApprenticeshipEmployerType.Levy;
        }

        [Given(@"the SFA contribution percentage for the Apprenticeship Unit is (\d+)%")]
        public void GivenTheSfaContributionPercentageForTheApprenticeshipUnitIs(int percentage)
        {
            sfaContributionPercentage = percentage / 100m;
        }

        [Given("the following short course payments were made in a previous collection period")]
        public async Task GivenTheFollowingShortCoursePaymentsWereMadeInAPreviousCollectionPeriod(Table table)
        {
            foreach (var row in table.Rows)
            {
                var deliveryPeriod = byte.Parse(row["Delivery Period"]);
                var amount = decimal.Parse(row["Amount"]);
                var transactionType = Enum.Parse<TransactionType>(row["Earning Type"]);

                if (employerType == ApprenticeshipEmployerType.Levy)
                {
                    testSession.DataContext.Payment.Add(CreatePayment(deliveryPeriod, transactionType, FundingSourceType.Levy, amount));
                    continue;
                }

                var sfaAmount = amount * sfaContributionPercentage;
                testSession.DataContext.Payment.Add(CreatePayment(deliveryPeriod, transactionType, FundingSourceType.CoInvestedSfa, sfaAmount));
                testSession.DataContext.Payment.Add(CreatePayment(deliveryPeriod, transactionType, FundingSourceType.CoInvestedEmployer, amount - sfaAmount));
            }

            await testSession.DataContext.SaveChangesAsync();
        }

        [When("the provider reports that the learner has been withdrawn with no earnings")]
        public async Task WhenTheProviderReportsThatTheLearnerHasBeenWithdrawnWithNoEarnings()
        {
            var withdrawal = new GSLShortCourseEarningsEvent
            {
                EventId = Guid.NewGuid(),
                EventTime = DateTimeOffset.UtcNow,
                ExternalEarningsId = Guid.NewGuid(),
                FundingPlatformType = FundingPlatformType.DigitalApprenticeshipService,
                CollectionPeriod = new CollectionPeriod { AcademicYear = academicYear, Period = WithdrawalCollectionPeriod },
                Ukprn = testSession.Provider.Ukprn,
                JobId = testSession.JobId,
                Learner = new SFA.DAS.Payments.Model.Core.Learner
                {
                    Uln = testSession.Learner.Uln,
                    ReferenceNumber = testSession.Learner.LearnRefNumber
                },
                IlrSubmissionDateTime = DateTime.UtcNow,
                AgeAtStartOfLearning = 21,
                LearningAim = new LearningAim
                {
                    Reference = CourseCode,
                    CourseCode = CourseCode,
                    ProgrammeType = 0,
                    StandardCode = 0,
                    FrameworkCode = 0,
                    PathwayCode = 0,
                    FundingLineType = string.Empty,
                    SequenceNumber = 0,
                    StartDate = startDate,
                    LearningType = LearningType.ApprenticeshipUnit
                },
                Earnings = new List<ShortCourseEarning>(),
                PriceEpisodes = new List<PriceEpisode>()
            };

            Console.WriteLine($"Sending the full withdrawal short course earnings event.  Event: {withdrawal.EventId}");
            await messagingContext.Send<GSLShortCourseEarningsEvent>(System.Text.Json.JsonSerializer.Serialize(withdrawal));
        }

        [Then("the following (Levy|Co-Invested) refunds are generated")]
        public async Task ThenTheFollowingRefundsAreGenerated(string requiredPaymentType, Table table)
        {
            var expectedRefunds = table.Rows.Select(row => new
            {
                DeliveryPeriod = byte.Parse(row["Delivery Period"]),
                Amount = decimal.Parse(row["Amount"]),
                TransactionType = Enum.Parse<TransactionType>(row["Earning Type"])
            }).ToList();

            await testSession.WaitForIt(() =>
            {
                refunds = GetRefunds(requiredPaymentType);

                return refunds.Count == expectedRefunds.Count
                       && expectedRefunds.All(expected => refunds.Any(actual =>
                           actual.TransactionType == expected.TransactionType
                           && actual.DeliveryPeriod == expected.DeliveryPeriod
                           && actual.AmountDue == expected.Amount
                           && actual.CollectionPeriod.AcademicYear == academicYear
                           && actual.CollectionPeriod.Period == WithdrawalCollectionPeriod));
            }, $"Failed to find the expected {requiredPaymentType} refund required payment events.");

            var unexpectedRefunds = GetRefunds(requiredPaymentType == "Levy" ? "Co-Invested" : "Levy");
            Assert.That(unexpectedRefunds, Is.Empty, $"Refunds should only be generated as {requiredPaymentType} required payments");
        }

        [Then("each refund carries the start date, planned end date and instalment details of the original payment")]
        public void ThenEachRefundCarriesTheStartDatePlannedEndDateAndInstalmentDetailsOfTheOriginalPayment()
        {
            Assert.That(refunds, Is.Not.Empty);

            foreach (var refund in refunds.Cast<CalculatedRequiredOnProgrammeAmount>())
            {
                Assert.That(refund.StartDate, Is.EqualTo(startDate), "StartDate");
                Assert.That(refund.PlannedEndDate, Is.EqualTo(plannedEndDate), "PlannedEndDate");
                Assert.That(refund.ActualEndDate, Is.EqualTo(actualEndDate), "ActualEndDate");
                Assert.That(refund.CompletionAmount, Is.EqualTo(CompletionAmount), "CompletionAmount");
                Assert.That(refund.InstalmentAmount, Is.EqualTo(InstalmentAmount), "InstalmentAmount");
                Assert.That(refund.NumberOfInstalments, Is.EqualTo(NumberOfInstalments), "NumberOfInstalments");
                Assert.That(refund.SfaContributionPercentage, Is.EqualTo(sfaContributionPercentage), "SfaContributionPercentage");
                Assert.That(refund.ApprenticeshipEmployerType, Is.EqualTo(employerType), "ApprenticeshipEmployerType");
                Assert.That(refund.LearningAim.FundingLineType, Is.EqualTo(FundingLineType), "FundingLineType");
                Assert.That(refund.CourseType, Is.EqualTo(CourseType.ShortCourse), "CourseType");
            }
        }

        private List<PeriodisedRequiredPaymentEvent> GetRefunds(string requiredPaymentType)
        {
            return requiredPaymentType == "Levy"
                ? RequiredLevyPaymentsHandler.GetEvents(testSession.Learner).Cast<PeriodisedRequiredPaymentEvent>().ToList()
                : RequiredCoInvestedPaymentsHandler.GetEvents(testSession.Learner).Cast<PeriodisedRequiredPaymentEvent>().ToList();
        }

        private PaymentModel CreatePayment(byte deliveryPeriod, TransactionType transactionType, FundingSourceType fundingSource, decimal amount)
        {
            return new PaymentModel
            {
                EventId = Guid.NewGuid(),
                EarningEventId = Guid.NewGuid(),
                FundingSourceEventId = Guid.NewGuid(),
                RequiredPaymentEventId = Guid.NewGuid(),
                EventTime = DateTimeOffset.UtcNow,
                JobId = testSession.JobId,
                Ukprn = testSession.Provider.Ukprn,
                LearnerUln = testSession.Learner.Uln,
                LearnerReferenceNumber = testSession.Learner.LearnRefNumber,
                CollectionPeriod = new CollectionPeriod { AcademicYear = academicYear, Period = PaymentHistoryCollectionPeriod },
                DeliveryPeriod = deliveryPeriod,
                ContractType = ContractType.Act1,
                TransactionType = transactionType,
                FundingSource = fundingSource,
                Amount = amount,
                SfaContributionPercentage = sfaContributionPercentage,
                LearningAimReference = CourseCode,
                LearningAimProgrammeType = 0,
                LearningAimStandardCode = 0,
                LearningAimFrameworkCode = 0,
                LearningAimPathwayCode = 0,
                LearningAimFundingLineType = FundingLineType,
                ReportingAimFundingLineType = FundingLineType,
                PriceEpisodeIdentifier = PriceEpisodeIdentifier,
                IlrSubmissionDateTime = startDate,
                LearningStartDate = startDate,
                StartDate = startDate,
                PlannedEndDate = plannedEndDate,
                ActualEndDate = actualEndDate,
                CompletionStatus = 0,
                CompletionAmount = CompletionAmount,
                InstalmentAmount = InstalmentAmount,
                NumberOfInstalments = NumberOfInstalments,
                AccountId = accountId,
                ApprenticeshipId = apprenticeshipId,
                ApprenticeshipEmployerType = employerType,
                AgeAtStartOfLearning = 21,
                FundingPlatformType = FundingPlatformType.DigitalApprenticeshipService
            };
        }
    }
}
