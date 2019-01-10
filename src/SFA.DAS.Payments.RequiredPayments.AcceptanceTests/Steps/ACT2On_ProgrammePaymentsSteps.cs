﻿using System.Linq;
using System.Threading.Tasks;
using NServiceBus;
using SFA.DAS.Payments.AcceptanceTests.Core;
using SFA.DAS.Payments.Model.Core;
using SFA.DAS.Payments.Model.Core.OnProgramme;
using SFA.DAS.Payments.PaymentsDue.Messages.Events;
using SFA.DAS.Payments.RequiredPayments.AcceptanceTests.Handlers;
using TechTalk.SpecFlow;

namespace SFA.DAS.Payments.RequiredPayments.AcceptanceTests.Steps
{
    //[Obsolete("This is a temporary binding to show how to use StepsBase and MessageSender.")]
    [Binding]
    public class ACT2On_ProgrammePaymentsSteps : StepsBase
    {
        public ACT2On_ProgrammePaymentsSteps(ScenarioContext scenarioContext) : base(scenarioContext)
        {
        }

        protected ApprenticeshipContractType2PaymentDueEvent PaymentDueEvent
        {
            get => Get<ApprenticeshipContractType2PaymentDueEvent>();
            set => Set(value);
        }
        

        [Given(@"the learner has some on-programme earnings")]
        public void GivenTheLearnerHasSomeOn_ProgrammeEarnings()
        {
            PaymentDueEvent = new ApprenticeshipContractType2PaymentDueEvent
            {
                JobId = TestSession.JobId,
                Ukprn = TestSession.Ukprn,
                Learner = new Learner { ReferenceNumber = "12345", Uln = 12345 },
                LearningAim = new LearningAim
                    {
                        FrameworkCode = 1234,
                        PathwayCode = 1234,
                        ProgrammeType = 1,
                        Reference = "Ref-1234",
                        StandardCode = 1,
                        FundingLineType = "Funding-LineType"
                },
                DeliveryPeriod = DeliveryPeriod.CreateFromAcademicYearAndPeriod("1718", 10),
                CollectionPeriod = Payments.Model.Core.CollectionPeriod.CreateFromAcademicYearAndPeriod("1718", 10),
                AmountDue = 1000,
                Type = OnProgrammeEarningType.Learning,
                PriceEpisodeIdentifier = "p1-1",
                SfaContributionPercentage = 0.90m
            };
            
        }
        
        [When(@"the earnings are sent to the required payments service")]
        public async Task WhenTheEarningsAreSentToTheRequiredPaymentsService()
        {
            
            await MessageSession.Send(PaymentDueEvent).ConfigureAwait(false);
        }
        
        [Then(@"the service should generate the required payments")]
        public async Task ThenTheServiceShouldGenerateTheRequiredPayments()
        {
            await WaitForIt(() =>
            {
                return ApprenticeshipContractType2Handler.ReceivedEvents
                    .Any(receivedEvent => receivedEvent.AmountDue == PaymentDueEvent.AmountDue &&
                                          receivedEvent.DeliveryPeriod?.Period == PaymentDueEvent.DeliveryPeriod.Period &&
                                          receivedEvent.Ukprn == TestSession.Ukprn &&
                                          receivedEvent.JobId == TestSession.JobId);
            },"Failed to find all the required payment events");
        }
    }
}
