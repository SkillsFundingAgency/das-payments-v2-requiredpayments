using SFA.DAS.Payments.Model.Core;
using SFA.DAS.Payments.RequiredPayments.Messages.Events;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.Payments.RequiredPayments.Application
{
    public interface IRemovedLearnerAimIdentificationService
    {
        Task<List<IdentifiedRemovedLearningAim>> IdentifyRemovedLearnerAims(short academicYear, byte collectionPeriod, long ukprn, DateTime ilrSubmissionDateTime, CancellationToken cancellationToken);
        Task<List<IdentifiedRemovedLearningAim>> IdentifyRemovedLearnerAims(short academicYear, byte collectionPeriod, long ukprn, Learner learner, LearningAim learningAim, long jobId, DateTime ilrSubmissionDateTime, CancellationToken cancellationToken);
    }
}
