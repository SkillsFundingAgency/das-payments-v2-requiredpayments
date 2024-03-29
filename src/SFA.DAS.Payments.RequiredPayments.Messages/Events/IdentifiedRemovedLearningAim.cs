﻿using System;
using SFA.DAS.Payments.Messages.Common;
using SFA.DAS.Payments.Model.Core;
using SFA.DAS.Payments.Model.Core.Entities;

namespace SFA.DAS.Payments.RequiredPayments.Messages.Events
{
    public class IdentifiedRemovedLearningAim : IRequiredPaymentEvent, IMonitoredMessage
    {
        public DateTimeOffset EventTime { get; set; }
        public Guid EventId { get; set; }
        public long Ukprn { get; set; }
        public Learner Learner { get; set; }
        public LearningAim LearningAim { get; set; }
        public DateTime IlrSubmissionDateTime { get; set; }
        public CollectionPeriod CollectionPeriod { get; set; }
        public ContractType ContractType { get; set; }
        public long JobId { get; set; }
    }
}