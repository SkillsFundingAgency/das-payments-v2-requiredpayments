using System;
using Microsoft.Extensions.Configuration;

namespace SFA.DAS.Payments.RequiredPayments.AcceptanceTests.RemoveAfterTesting
{
    public interface ITestsConfiguration
    {
        string AcceptanceTestsEndpointName { get; }
        string StorageConnectionString { get; }
        string ServiceBusConnectionString { get; }
        string DasServiceBusConnectionString { get; }
        string PaymentsConnectionString { get; }
        string Environment { get; }
        bool ValidateDcAndDasServices { get; }
        TimeSpan TimeToWait { get; }
        TimeSpan TimeToWaitForUnexpected { get; }
        TimeSpan TimeToPause { get; }
        TimeSpan DefaultMessageTimeToLive { get; }
        string GetAppSetting(string keyName);
        string GetConnectionString(string name);

    }
    public class TestsConfiguration : ITestsConfiguration
    {
        private static IConfiguration _config;

        public static readonly string EndpointNameAppSetting = "EndpointName";
        public static readonly string StorageConnectionStringAppSetting = "StorageConnectionString";
        public static readonly string ServiceBusConnectionStringAppSetting = "ServiceBusConnectionString";
        public static readonly string DasServiceBusConnectionStringAppSetting = "DASServiceBusConnectionString";
        public static readonly string EnvironmentAppSetting = "Environment";
        public static readonly string PaymentsConnectionStringAppSetting = "PaymentsConnectionString";
        public static readonly string ValidateDcAndDasServicesAppSetting = "ValidateDcAndDasServices";
        public static readonly string TimeToWaitAppSetting = "TimeToWait";
        public static readonly string TimeToWaitForUnexpectedAppSetting = "TimeToWaitForUnexpected";
        public static readonly string TimeToPauseAppSetting = "TimeToPause";
        public static readonly string DefaultMessageTimeToLiveAppSetting = "DefaultMessageTimeToLive";

        public TestsConfiguration(IConfiguration config)
        {
            _config = config;
        }

        public string AcceptanceTestsEndpointName => GetAppSetting(EndpointNameAppSetting) ??
                                                     throw new InvalidOperationException($"{EndpointNameAppSetting} not found in app settings.");
        public string StorageConnectionString => GetConnectionString(StorageConnectionStringAppSetting);
        public string ServiceBusConnectionString => GetConnectionString(ServiceBusConnectionStringAppSetting);
        public string DasServiceBusConnectionString => GetConnectionString(DasServiceBusConnectionStringAppSetting);
        public string PaymentsConnectionString => GetConnectionString(PaymentsConnectionStringAppSetting);
        public string Environment => GetAppSetting(EnvironmentAppSetting) ??
                                     throw new InvalidOperationException($"{EnvironmentAppSetting} not found in app settings.");

        public bool ValidateDcAndDasServices =>
            bool.Parse(GetAppSetting(ValidateDcAndDasServicesAppSetting) ?? "false");

        public TimeSpan TimeToWait =>
            TimeSpan.Parse(GetAppSetting(TimeToWaitAppSetting) ?? "00:00:30");

        public TimeSpan TimeToWaitForUnexpected =>
            TimeSpan.Parse(GetAppSetting(TimeToWaitForUnexpectedAppSetting) ?? "00:00:30");

        public TimeSpan TimeToPause =>
            TimeSpan.Parse(GetAppSetting(TimeToPauseAppSetting) ?? "00:00:05");

        public TimeSpan DefaultMessageTimeToLive =>
            TimeSpan.Parse(GetAppSetting(DefaultMessageTimeToLiveAppSetting) ?? "00:20:00");

        private static string AppSettingFormatter(string appSettingName)
        {
            return $"AppSettings:{appSettingName}";
        }

        public string GetAppSetting(string keyName)
        {
            return _config.GetSection(AppSettingFormatter(keyName)).Value;
        }

        public string GetConnectionString(string name)
        {
            return _config.GetConnectionString(name) ??
                   throw new InvalidOperationException($"{name} not found in connection strings.");
        }
    }
}