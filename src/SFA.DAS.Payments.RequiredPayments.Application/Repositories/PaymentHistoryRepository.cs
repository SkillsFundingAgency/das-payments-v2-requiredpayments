﻿using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using SFA.DAS.Payments.RequiredPayments.Application.Data;
using SFA.DAS.Payments.RequiredPayments.Model.Entities;

namespace SFA.DAS.Payments.RequiredPayments.Application.Repositories
{
    public class PaymentHistoryRepository : IPaymentHistoryRepository
    {
        private readonly IRequiredPaymentsDataContext _requiredPaymentsDataContext;
        private readonly IMapper _mapper;

        public PaymentHistoryRepository(RequiredPaymentsDataContext requiredPaymentsDataContext, IMapper mapper)
        {
            _requiredPaymentsDataContext = requiredPaymentsDataContext;
            _mapper = mapper;
        }

        public async Task<PaymentEntity[]> GetPaymentHistory(string apprenticeshipKey, CancellationToken cancellationToken = default(CancellationToken))
        {
            return await _requiredPaymentsDataContext.PaymentHistory
                .Where(p => p.ApprenticeshipKey == apprenticeshipKey)
                .ToArrayAsync(cancellationToken);
        }
    }
}