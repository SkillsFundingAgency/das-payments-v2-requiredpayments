using System;
using Microsoft.Extensions.DependencyInjection;
using NServiceBus;

namespace SFA.DAS.Payments.RequiredPayments.AcceptanceTests.RemoveAfterTesting
{
    public abstract class BindingsBase
    {
        public static IServiceCollection ServiceCollection { get; protected set; } // -1
        public static IServiceProvider ServiceProvider { get; protected set; } // 50
        public static ITestsConfiguration Config { get; protected set; }
        public static IMessageSession MessageSession { get; protected set; }
    }
}