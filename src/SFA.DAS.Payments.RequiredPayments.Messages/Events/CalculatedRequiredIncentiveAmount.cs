using SFA.DAS.Payments.Messages.Common;
using SFA.DAS.Payments.Model.Core.Entities;
using SFA.DAS.Payments.Model.Core.Incentives;
using SFA.DAS.Payments.RequiredPayments.Model.Entities;

namespace SFA.DAS.Payments.RequiredPayments.Messages.Events
{
    public class CalculatedRequiredIncentiveAmount: PeriodisedRequiredPaymentEvent, IMonitoredMessage
    {
        public IncentivePaymentType Type { get; set; }
        public override TransactionType TransactionType => (TransactionType) Type;
    }
}