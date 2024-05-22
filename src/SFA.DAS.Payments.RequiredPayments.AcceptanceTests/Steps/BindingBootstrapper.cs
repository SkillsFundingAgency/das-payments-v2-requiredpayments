﻿using Autofac;
using NServiceBus;
using SFA.DAS.Payments.AcceptanceTests.Core;
using SFA.DAS.Payments.AcceptanceTests.Core.Infrastructure;
using SFA.DAS.Payments.EarningEvents.Messages.Events;
using SFA.DAS.Payments.Messages.Common;
using SFA.DAS.Payments.RequiredPayments.Messages.Events;
using TechTalk.SpecFlow;

namespace SFA.DAS.Payments.RequiredPayments.AcceptanceTests.Steps
{
    [Binding]
    public class BindingBootstrapper: BindingsBase
    {

        [BeforeTestRun(Order = 51)]
        public static void AddRoutingConfig()
        {
            var endpointConfiguration = Container.Resolve<EndpointConfiguration>();
            
            endpointConfiguration.Conventions().DefiningEventsAs(type => type.IsEvent<IPeriodisedRequiredPaymentEvent>());
            var transportConfig = Container.Resolve<AzureServiceBusTransport>();
            var routing = endpointConfiguration.UseTransport(transportConfig);
            routing.RouteToEndpoint(typeof(ApprenticeshipContractType2EarningEvent), Config.GetAppSetting("RequiredPaymentsServiceEndpointName"));
        }
    }
}