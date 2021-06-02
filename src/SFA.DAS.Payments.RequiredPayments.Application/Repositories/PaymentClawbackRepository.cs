﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SFA.DAS.Payments.Application.Repositories;
using SFA.DAS.Payments.Model.Core.Entities;

namespace SFA.DAS.Payments.RequiredPayments.Application.Repositories
{
    public interface IPaymentClawbackRepository : IDisposable
    {
        Task<List<PaymentModel>> GetLearnerPaymentHistory(
            long ukprn,
            ContractType contractType,
            string learnerReferenceNumber,
            string learningAimReference,
            int frameworkCode,
            int pathwayCode,
            int programmeType,
            int standardCode,
            short academicYear,
            byte collectionPeriod,
            CancellationToken cancellationToken = default(CancellationToken));

        Task SaveClawbackPayments(IEnumerable<PaymentModel> clawbackPayments, CancellationToken cancellationToken = default(CancellationToken));
    }

    public class PaymentClawbackRepository : IPaymentClawbackRepository
    {
        private readonly IPaymentsDataContext dataContext;

        public PaymentClawbackRepository(IPaymentsDataContext dataContext)
        {
            this.dataContext = dataContext ?? throw new ArgumentNullException(nameof(dataContext));
        }

        public async Task<List<PaymentModel>> GetLearnerPaymentHistory(
            long ukprn, 
            ContractType contractType, 
            string learnerReferenceNumber, 
            string learningAimReference, 
            int frameworkCode, 
            int pathwayCode, 
            int programmeType, 
            int standardCode, 
            short academicYear, 
            byte collectionPeriod, 
            CancellationToken cancellationToken = default(CancellationToken))
        {
            return await dataContext.Payment
                .Where(payment =>
                            payment.Ukprn == ukprn &&
                            payment.ContractType == contractType &&
                            payment.LearnerReferenceNumber == learnerReferenceNumber &&
                            payment.LearningAimReference == learningAimReference &&
                            payment.LearningAimFrameworkCode == frameworkCode &&
                            payment.LearningAimPathwayCode == pathwayCode &&
                            payment.LearningAimProgrammeType == (int)programmeType &&
                            payment.LearningAimStandardCode == standardCode &&
                            payment.CollectionPeriod.AcademicYear == academicYear &&
                            payment.CollectionPeriod.Period < collectionPeriod)
            .ToListAsync(cancellationToken);
        }

        public async Task SaveClawbackPayments(IEnumerable<PaymentModel> clawbackPayments, CancellationToken cancellationToken = default(CancellationToken))
        {
            await dataContext.Payment.AddRangeAsync(clawbackPayments, cancellationToken);
            await dataContext.SaveChangesAsync(cancellationToken);
        }

        public void Dispose()
        {
            (dataContext as PaymentsDataContext)?.Dispose();
        }
    }
}