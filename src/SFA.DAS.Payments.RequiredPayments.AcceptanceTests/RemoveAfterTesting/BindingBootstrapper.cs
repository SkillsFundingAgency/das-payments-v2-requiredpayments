using System;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using Azure.Messaging.ServiceBus.Administration;
using ESFA.DC.ILR.TestDataGenerator.Api;
using ESFA.DC.ILR.TestDataGenerator.Api.StorageService;
using ESFA.DC.ILR.TestDataGenerator.Interfaces;
using ESFA.DC.IO.AzureStorage;
using ESFA.DC.IO.AzureStorage.Config.Interfaces;
using ESFA.DC.IO.Interfaces;
using ESFA.DC.Serialization.Interfaces;
using ESFA.DC.Serialization.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NServiceBus;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using Polly;
using Polly.Registry;
using SFA.DAS.Payments.AcceptanceTests.Core;
using SFA.DAS.Payments.AcceptanceTests.Core.Automation;
using SFA.DAS.Payments.AcceptanceTests.Core.Data;
using SFA.DAS.Payments.AcceptanceTests.Core.Infrastructure;
using SFA.DAS.Payments.AcceptanceTests.Core.Services;
using SFA.DAS.Payments.AcceptanceTests.Services.BespokeHttpClient;
using SFA.DAS.Payments.AcceptanceTests.Services.Configuration;
using SFA.DAS.Payments.AcceptanceTests.Services.Intefaces;
using SFA.DAS.Payments.AcceptanceTests.Services.Services;
using SFA.DAS.Payments.Messages.Common;
using SFA.DAS.Payments.Monitoring.Jobs.Client;
using TechTalk.SpecFlow;
using MessageReceiver = Microsoft.Azure.ServiceBus.Core.MessageReceiver;

namespace SFA.DAS.Payments.RequiredPayments.AcceptanceTests.RemoveAfterTesting
{
    [Binding]
    public class BindingBootstrapper : BindingsBase
    {
        public static EndpointConfiguration EndpointConfiguration { get; private set; }

        [BeforeTestRun(Order = -1)]
        public static void TestRunSetUp()
        {
            Collection = new ServiceCollection();

            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile("appsettings.Development.json", optional: false, reloadOnChange: true).Build();

            var config = new TestsConfiguration(configuration);
            Collection.AddScoped(_ => configuration);


            Collection.AddScoped<IConfiguration>(_ => configuration);
            Collection.AddScoped<ITestsConfiguration, TestsConfiguration>();
            Collection.AddSingleton(config);

            Collection.AddScoped<IEarningsJobClient, EarningsJobClient>();

            Collection.AddScoped<AzureStorageServiceConfig>();
            Collection.AddScoped<IAzureStorageKeyValuePersistenceServiceConfig, AzureStorageServiceConfig>();

            Collection.AddScoped<IStorageService, StorageService>();

            Collection.AddScoped<ITdgService, TdgService>();

            Collection.AddScoped<IPaymentsHelper, PaymentsHelper>();

            if (config.ValidateDcAndDasServices)
            {
                //Builder.RegisterType<UkprnService>().As<IUkprnService>().InstancePerLifetimeScope();
                Collection.AddScoped<IUlnService, UlnService>();
                Collection.AddScoped<IDcHelper, DcNullHelper>();
            }
            else
            {
                Collection.AddScoped<IIlrService, IlrNullService>();
                Collection.AddScoped<IUkprnService, RandomUkprnService>();
                Collection.AddScoped<IDcHelper, DcHelper>();
                Collection.AddScoped<IUlnService, RandomUlnService>();
            }


            Collection.AddDbContext<TestPaymentsDataContext>(options =>
                options.UseSqlServer(config.PaymentsConnectionString));

            Collection.AddDbContext<TestPaymentsDataContext>(options =>
                               options.UseSqlServer(config.PaymentsConnectionString));
            Collection.AddDbContext<SubmissionDataContext>(options =>

            Collection.AddScoped(c => new TestSession(c.GetService<IUkprnService>(), c.GetService<IUlnService>())));

            Collection.AddScoped<IReadOnlyPolicyRegistry<string>>(_ =>
            {
                var registry = new PolicyRegistry();
                registry.Add(
                    "HttpRetryPolicy",
                    Policy.Handle<HttpRequestException>()
                        .WaitAndRetryAsync(
                            3, // number of retries
                            retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)), // exponential backoff
                            (exception, timeSpan, retryCount, executionContext) =>
                            {
                                // add logging
                            }));
                return registry;
            });

            Collection.AddScoped<IJobService, JobService>();

            Collection.AddScoped<IBespokeHttpClient, BespokeHttpClient>();

            Collection.AddScoped<IJsonSerializationService, JsonSerializationService>();

            EndpointConfiguration = new EndpointConfiguration(config.AcceptanceTestsEndpointName);
            Collection.AddSingleton(EndpointConfiguration);

            var conventions = EndpointConfiguration.Conventions();
            conventions.DefiningMessagesAs(type => type.IsMessage());

            Collection.AddSingleton<IUkprnService, RandomUkprnService>();

            EndpointConfiguration.UsePersistence<AzureStoragePersistence>()
                .ConnectionString(config.StorageConnectionString);

            var transportSettings = new AzureServiceBusTransport(config.ServiceBusConnectionString)
            {
                
                SubscriptionNamingConvention = (ruleName => ruleName.Split('.').LastOrDefault() ?? ruleName),
                TransportTransactionMode = TransportTransactionMode.ReceiveOnly
                
            };

            Collection.AddSingleton(transportSettings);

            var transportConfig = EndpointConfiguration.UseTransport(transportSettings);

            Collection.AddSingleton(transportConfig);


            /*//Uses built in ForwardingTopology
            transportConfig.ConnectionString(config.ServiceBusConnectionString)
                .Transactions(TransportTransactionMode.ReceiveOnly);

            //This replaces ValidateAndHashIfNeeded flag.
            transportConfig.SubscriptionNameShortener(ruleName => ruleName.Split('.').LastOrDefault() ?? ruleName);
            transportConfig.SubscriptionNameShortener(n => n.Length > maxEntityName ? HashName(n) : n);
            transportConfig.RuleNameShortener(
                ruleName => ruleName.Split('.').LastOrDefault() ?? ruleName);*/

            EndpointConfiguration.UseSerialization<NewtonsoftJsonSerializer>();

            //EndpointConfiguration.UseSerialization<NewtonsoftSerializer>();

            EndpointConfiguration.EnableInstallers();
        }


        private static string HashName(string input)
        {
            var inputBytes = Encoding.Default.GetBytes(input);
            // use MD5 hash to get a 16-byte hash of the string
            using (var provider = new MD5CryptoServiceProvider())
            {
                var hashBytes = provider.ComputeHash(inputBytes);
                return new Guid(hashBytes).ToString();
            }
        }


        [BeforeTestRun(Order = 50)]
        public static void CreateContainer()
        {
            Provider = Collection.BuildServiceProvider();
        }

        [BeforeTestRun(Order = 75)]
        public static async Task ClearQueue()
        {
            var serviceBusAdminClient = new ServiceBusAdministrationClient(Config.ServiceBusConnectionString);

            if (!await serviceBusAdminClient.QueueExistsAsync(Config.AcceptanceTestsEndpointName))
            {
                Console.WriteLine($"'{Config.AcceptanceTestsEndpointName}' not found.");
                return;
            }

            Console.WriteLine($"Now clearing queue: '{Config.AcceptanceTestsEndpointName}'");
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            var messageReceiver = new MessageReceiver(Config.ServiceBusConnectionString,
                Config.AcceptanceTestsEndpointName, Microsoft.Azure.ServiceBus.ReceiveMode.ReceiveAndDelete);
            while (true)
            {
                var messages = await messageReceiver.ReceiveAsync(500, TimeSpan.FromSeconds(1));
                if (messages == null || !messages.Any())
                {
                    break;
                }
            }

            var queueDescription = await serviceBusAdminClient.GetQueueAsync(Config.AcceptanceTestsEndpointName);
            if (queueDescription.HasValue)
            {
                if (queueDescription.Value.DefaultMessageTimeToLive != Config.DefaultMessageTimeToLive)
                {
                    queueDescription.Value.DefaultMessageTimeToLive = Config.DefaultMessageTimeToLive;
                    await serviceBusAdminClient.UpdateQueueAsync(queueDescription);
                }
            }


            Console.WriteLine(
                $"Finished purging messages from {Config.AcceptanceTestsEndpointName}. Took: {stopwatch.ElapsedMilliseconds}ms");
        }

        [BeforeTestRun(Order = 99)]
        public static void StartBus()
        {
            var startableEndpoint = EndpointWithExternallyManagedContainer.Create(EndpointConfiguration, Collection);

            MessageSession = startableEndpoint.Start(Provider).Result;
        }

        [AfterScenario]
        public static void MarkFailedTestsToFeatureContext(FeatureContext featureContext, TestContext testContext)
        {
            if (testContext.Result.Outcome != ResultState.Success)
            {
                featureContext["FailedTests"] = true;
            }
        }

        [BeforeScenario]
        public static void DontRunIfFailedPreviousScenario(FeatureContext featureContext)
        {
            if (featureContext.ContainsKey("FailedTests") &&
                (bool)featureContext["FailedTests"])
            {
                Assert.Fail("Failing as previous examples of this feature have failed");
            }
        }
    }
}