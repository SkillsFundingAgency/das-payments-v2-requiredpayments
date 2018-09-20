﻿using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SFA.DAS.Payments.RequiredPayments.Domain.Entities;

namespace SFA.DAS.Payments.RequiredPayments.Application.Repositories
{
    public interface IPaymentHistoryRepository
    {
        Task<IEnumerable<Payment>> GetPaymentHistory(string apprenticeshipKey, CancellationToken cancellationToken = default(CancellationToken));
    }
}
