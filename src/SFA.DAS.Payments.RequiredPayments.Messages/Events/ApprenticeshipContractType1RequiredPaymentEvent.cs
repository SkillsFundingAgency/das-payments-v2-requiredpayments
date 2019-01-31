﻿namespace SFA.DAS.Payments.RequiredPayments.Messages.Events
{
    public class ApprenticeshipContractType1RequiredPaymentEvent : ApprenticeshipContractTypeRequiredPaymentEvent
    {
        public int Priority { get; set; }
        public long CommitmentId { get; set; }
        public string AgreementId { get; set; }
        public long EmployerAccountId { get; set; }
    }
}