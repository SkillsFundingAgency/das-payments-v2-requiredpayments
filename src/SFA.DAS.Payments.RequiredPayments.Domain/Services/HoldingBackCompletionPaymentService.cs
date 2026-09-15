using System;
using SFA.DAS.Payments.Model.Core;

namespace SFA.DAS.Payments.RequiredPayments.Domain.Services
{
    public class HoldingBackCompletionPaymentService : IHoldingBackCompletionPaymentService
    {
        private static readonly DateTime PolicyChangeEffectiveDate = new DateTime(2026, 8, 1);

        public bool ShouldHoldBackCompletionPayment(decimal expectedContribution, PriceEpisode priceEpisode)
        {
            if (priceEpisode?.ActualEndDate != null && priceEpisode.ActualEndDate >= PolicyChangeEffectiveDate)
            {
                return false;
            }

            var reportedContribution = priceEpisode?.EmployerContribution ?? 0;
            var completionHoldBackExemptionCode = priceEpisode?.CompletionHoldBackExemptionCode ?? 0;

            if (completionHoldBackExemptionCode > 0)
                return false;

            return reportedContribution < expectedContribution;
        }
    }
}