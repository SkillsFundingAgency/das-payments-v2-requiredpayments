using NServiceBus;
using SFA.DAS.Payments.DataLocks.Messages.Events;
using SFA.DAS.Payments.EarningEvents.Messages.Events;

namespace SFA.DAS.Payments.RequiredPayments.Tests.Specs.StepDefinitions
{
    public class MessagingContext
    {
        private IEndpointInstance endpointInstance;

        public MessagingContext()
        {
            endpointInstance = TestRunBindings.endpoint;            
        }

        public async Task Send<T>(string messageJson)
        {
            var message = System.Text.Json.JsonSerializer.Deserialize<T>(messageJson);
            await endpointInstance.Send("sfa-das-payments-requiredpayments", message);
        }

        public async Task Send(ApprenticeshipContractType2EarningEvent earningEvent)
        {
            await endpointInstance.Send("sfa-das-payments-requiredpayments", earningEvent);
        }

        public async Task Send(PayableEarningEvent earningEvent)
        {
            await endpointInstance.Send("sfa-das-payments-requiredpayments", earningEvent);
        }

        public async Task Send(GSLShortCourseEarningsEvent earningEvent)
        {
            await endpointInstance.Send("sfa-das-payments-requiredpayments", earningEvent);
        }
    }
}
