﻿using Autofac;
using Castle.Core.Internal;
using System;
using System.Runtime.CompilerServices;
using System.Threading;
using SFA.DAS.Payments.ServiceFabric.Core.Infrastructure.Ioc;

[assembly: InternalsVisibleTo(InternalsVisible.ToDynamicProxyGenAssembly2)]

namespace SFA.DAS.Payments.RequiredPayments.RequiredPaymentsProxyService
{
    internal static class Program
    {
        private static void Main()
        {
            try
            {
                var builder = ServiceFabricContainerFactory.CreateBuilderForStatelessService<RequiredPaymentsProxyService>();

                //RegisterServices(builder);

                //builder.RegisterServiceFabricSupport();
                //builder.RegisterStatelessService<RequiredPaymentsProxyService>("SFA.DAS.Payments.RequiredPayments.RequiredPaymentsProxyServiceType");

                using (builder.Build())
                {
                    Thread.Sleep(Timeout.Infinite);
                }
            }
            catch (Exception e)
            {
                ServiceEventSource.Current.ServiceHostInitializationFailed(e.ToString());
                throw;
            }
        }

        private static void RegisterServices(ContainerBuilder builder)
        {
            // TODO: use configuration
            //builder.Register((c, p) => new ServiceConfig
            //{
            //    IncomingEndpointName = "payments-required-payments",
            //    OutgoingEndpointName = "sfa-das-payments-paymentsdue-proxyservice-out",
            //    DestinationEndpointName = "sfa-das-payments-requiredpayments-proxyservice",
            //    StorageConnectionString = "UseDevelopmentStorage=true",
            //    LoggerConnectionstring = "Server=.;Database=AppLog;User Id=SFActor; Password=SFActor;"
            //}).AsImplementedInterfaces();

            //Register Logger
            //builder.Register((c, p) =>
            //{
            //    var config = c.Resolve<IServiceConfig>();
            //    return new LoggerOptions
            //    {
            //        LoggerConnectionString = config.LoggerConnectionstring
            //    };
            //}).As<LoggerOptions>().SingleInstance();
            //builder.RegisterType<VersionInfo>().As<IVersionInfo>().SingleInstance();
            //builder.RegisterModule<LoggerModule>();


            // Register Service Fabric Service components
//            builder.RegisterType<ActorProxyFactory>().As<IActorProxyFactory>();
//            builder.RegisterType<RequiredPaymentsProxyService>().AsImplementedInterfaces();


            //builder.Register((c, p) =>
            //{
            //    var config = c.Resolve<IServiceConfig>();
            //    return new EndpointCommunicationSender<IPaymentsDueEvent>(
            //        config.OutgoingEndpointName,
            //        config.StorageConnectionString,
            //        config.DestinationEndpointName,
            //        c.Resolve<ILifetimeScope>()
            //    );
            //}).As<IEndpointCommunicationSender<IPaymentsDueEvent>>();

            //builder.Register((c, p) =>
            //{
            //    var config = c.Resolve<IServiceConfig>();
            //    return new EndpointCommunicationListener<IPayableEarningEvent>(
            //        config.IncomingEndpointName,
            //        config.StorageConnectionString,
            //        c.Resolve<ILifetimeScope>()
            //    );
            //}).As<IEndpointCommunicationListener<IPayableEarningEvent>>();
        }
    }
}