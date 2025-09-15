using Microsoft.Extensions.Configuration;
using NServiceBus;

namespace SFA.DAS.Payments.RequiredPayments.Tests.Specs.StepDefinitions
{
    [Binding]
    public class TestRunBindings 
    {
        public static IEndpointInstance endpoint { get; private set; }
        public static IConfiguration Config { get; private set; }

        [BeforeTestRun]
        public static async Task SetUpMessaging()
        {
            Config = new ConfigurationBuilder()
                .AddJsonFile(Path.Combine(Directory.GetCurrentDirectory(), "appSettings.json"))
                .AddJsonFile(Path.Combine(Directory.GetCurrentDirectory(), "appSettings.local.json"), true)
                .Build();

            var endpointConfig = new EndpointConfiguration("SFA.DAS.Payments.RequiredPayments.Tests.Specs");
            endpointConfig.SendOnly();
            var storageConnectionString = Config["ConnectionStrings:StorageConnectionString"];
            endpointConfig.UsePersistence<AzureTablePersistence>().ConnectionString(storageConnectionString);
            endpointConfig.UseTransport<AzureServiceBusTransport>()
                .ConnectionString(Config["ConnectionStrings:ServiceBusConnectionString"]);

            var startable = await Endpoint.Create(endpointConfig);
            endpoint = await startable.Start();
        }
    }
}
