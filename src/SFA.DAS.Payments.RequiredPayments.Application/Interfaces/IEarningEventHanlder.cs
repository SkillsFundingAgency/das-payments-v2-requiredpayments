﻿using System.Threading;
using System.Threading.Tasks;
using SFA.DAS.Payments.PaymentsDue.Messages.Events;
using SFA.DAS.Payments.RequiredPayments.Messages.Events;

namespace SFA.DAS.Payments.RequiredPayments.Application
{
    public interface IPaymentDueEventHanlder
    {
        Task<RequiredPaymentEvent> HandlePaymentDue(PaymentDueEvent paymentDue, CancellationToken cancellationToken = default(CancellationToken));
    }
}