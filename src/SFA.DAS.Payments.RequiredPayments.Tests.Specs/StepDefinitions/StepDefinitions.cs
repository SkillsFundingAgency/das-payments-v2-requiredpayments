using Reqnroll;
using SFA.DAS.Payments.AcceptanceTests.Core.Data;

namespace SFA.DAS.Payments.RequiredPayments.Tests.Specs.StepDefinitions
{
    [Binding]
    public class StepDefinitions
    {
        private readonly ScenarioContext scenarioContext;
        private readonly MessagingContext messagingContext;
        private TestSessionDataContext dataContext;
        private int currentProvider;

        public StepDefinitions(ScenarioContext scenarioContext, MessagingContext messagingContext)
        {
            this.scenarioContext = scenarioContext;
            this.messagingContext = messagingContext;            
        }

        [BeforeScenario]
        public void BeforeScenario() 
        {
            var cnn = TestRunBindings.Config["ConnectionStrings:PaymentsConnectionString"];
            dataContext = new TestSessionDataContext(cnn);
            currentProvider = new ProviderService(dataContext, TestRunBindings.Config["AppGuid"]).GenerateUkprn();
        }

        [AfterScenario]
        public void AfterScenario() { 
        }

        [Given("the Co-invested payments for the apprenticeship were recorded prior to the requirement to record the Reporting Funding Line Type")]
        public void GivenTheCo_InvestedPaymentsForTheApprenticeshipWereRecordedPriorToTheRequirementToRecordTheReportingFundingLineType()
        {
            Console.WriteLine($"UKPRN : {currentProvider}");
        }

        [When("the learner completes the apprenticeship")]
        public void WhenTheLearnerCompletesTheApprenticeship()
        {
            throw new PendingStepException();
        }

        [Then("the service should allow payment of the completion payment")]
        public void ThenTheServiceShouldAllowPaymentOfTheCompletionPayment()
        {
            throw new PendingStepException();
        }

    }
}
