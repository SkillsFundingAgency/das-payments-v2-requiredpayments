using SFA.DAS.Payments.Model.Core;
using System;

namespace SFA.DAS.Payments.RequiredPayments.Domain.Services
{
    public class HoldingBackCompletionPaymentService : IHoldingBackCompletionPaymentService
    {
        public static readonly DateTime FundingRules2026EligibilityDate = new(2026, 8, 1);
        public bool ShouldHoldBackCompletionPayment(decimal expectedContribution, PriceEpisode priceEpisode)
        {
            if (priceEpisode.ActualEndDate >= FundingRules2026EligibilityDate)
                return false;

            var reportedContribution = priceEpisode.EmployerContribution ?? 0;
            var completionHoldBackExemptionCode = priceEpisode.CompletionHoldBackExemptionCode ?? 0;

            if (completionHoldBackExemptionCode > 0)
                return false;

            return reportedContribution < expectedContribution;
        }
    }
}