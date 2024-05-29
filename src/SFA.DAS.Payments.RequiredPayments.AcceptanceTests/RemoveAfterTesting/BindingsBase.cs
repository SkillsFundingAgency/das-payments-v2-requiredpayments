using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NServiceBus;

namespace SFA.DAS.Payments.RequiredPayments.AcceptanceTests.RemoveAfterTesting
{
    public abstract class BindingsBase
    {
        public static IServiceCollection Collection { get; protected set; } // -1
        public static IServiceProvider Provider { get; protected set; } // 50

        public static ITestsConfiguration Config
        {
            get
            {
                var config = Provider.GetService<IConfiguration>();
                return new TestsConfiguration(config);
            }
        }

        public static IMessageSession MessageSession { get; protected set; }
    }
}