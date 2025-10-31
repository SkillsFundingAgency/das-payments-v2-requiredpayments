using Microsoft.Extensions.Logging;
using SFA.DAS.Payments.Model.Core;
using SFA.DAS.Payments.RequiredPayments.Application.Repositories;
using SFA.DAS.Payments.RequiredPayments.Messages.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.Payments.RequiredPayments.Application.Processors
{
    public class RemovedLearnerAimIdentificationService : IRemovedLearnerAimIdentificationService
    {
        private readonly IPaymentHistoryRepository paymentHistoryRepository;

        public RemovedLearnerAimIdentificationService(IPaymentHistoryRepository paymentHistoryRepository)
        {
            this.paymentHistoryRepository = paymentHistoryRepository;
        }

        public async Task<List<IdentifiedRemovedLearningAim>> IdentifyRemovedLearnerAims(short academicYear, byte collectionPeriod, long ukprn, DateTime ilrSubmissionDateTime, CancellationToken cancellationToken)
        {
            return await paymentHistoryRepository.IdentifyRemovedLearnerAims(academicYear, collectionPeriod, ukprn, ilrSubmissionDateTime, cancellationToken).ConfigureAwait(false);
        }

        public async Task<List<IdentifiedRemovedLearningAim>> IdentifyRemovedLearnerAims(short academicYear, byte collectionPeriod, long ukprn, Learner learner, LearningAim learningAim, long jobId, DateTime ilrSubmissionDateTime, CancellationToken cancellationToken)
        {
            var contractTypes = await paymentHistoryRepository.FindRemovedAimContractTypes(academicYear, ukprn, learner, learningAim, cancellationToken).ConfigureAwait(false);

            return contractTypes.Select(contractType => new IdentifiedRemovedLearningAim
            {
                CollectionPeriod = new CollectionPeriod { AcademicYear = academicYear, Period = collectionPeriod },
                ContractType = contractType,
                EventId = Guid.NewGuid(),
                EventTime = DateTime.UtcNow,
                IlrSubmissionDateTime = ilrSubmissionDateTime,
                JobId = jobId,
                Learner = learner,
                LearningAim = learningAim,
                Ukprn = ukprn
            })
                .ToList();
        }
    }
}