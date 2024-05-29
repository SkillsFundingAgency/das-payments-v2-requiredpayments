
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NServiceBus;
using SFA.DAS.Payments.EarningEvents.Messages.Events;
using SFA.DAS.Payments.Messages.Common;
using SFA.DAS.Payments.RequiredPayments.Messages.Events;
using TechTalk.SpecFlow;
using BindingsBase = SFA.DAS.Payments.RequiredPayments.AcceptanceTests.RemoveAfterTesting.BindingsBase;
using TestsConfiguration = SFA.DAS.Payments.RequiredPayments.AcceptanceTests.RemoveAfterTesting.TestsConfiguration;

namespace SFA.DAS.Payments.RequiredPayments.AcceptanceTests.Steps
{
    [Binding]
    public class BindingBootstrapper: BindingsBase
    {

        [BeforeTestRun(Order = 51)]
        public static void AddRoutingConfig()
        {
            var endpointConfiguration = (EndpointConfiguration)Provider.GetService(typeof(EndpointConfiguration));
            endpointConfiguration?.Conventions().DefiningEventsAs(type => type.IsEvent<IPeriodisedRequiredPaymentEvent>());
            var transportConfig = (AzureServiceBusTransport)Provider.GetService(typeof(AzureServiceBusTransport));
            var routing = endpointConfiguration?.UseTransport(transportConfig);
            routing?.RouteToEndpoint(typeof(ApprenticeshipContractType2EarningEvent), Config.GetAppSetting("RequiredPaymentsServiceEndpointName"));
        }
    }
}