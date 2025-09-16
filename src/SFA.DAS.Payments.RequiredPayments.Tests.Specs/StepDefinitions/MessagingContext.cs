using NServiceBus;
using SFA.DAS.Payments.EarningEvents.Messages.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    }
}
