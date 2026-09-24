using System;
using SFA.DAS.Payments.Messages.Common.Events;
using SFA.DAS.Payments.Model.Core.Entities;

namespace SFA.DAS.Payments.RequiredPayments.Messages.Events
{
    // ReSharper disable once IdentifierTypo
    public interface IPeriodisedRequiredPaymentEvent : IPeriodisedPaymentEvent, IRequiredPaymentEvent
    {
        Guid EarningEventId { get; }
        FundingPlatformType FundingPlatformType { get; }
    }
}