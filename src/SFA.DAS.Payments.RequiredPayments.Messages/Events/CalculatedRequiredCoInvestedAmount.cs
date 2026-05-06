using SFA.DAS.Payments.Model.Core.Entities;
using SFA.DAS.Payments.RequiredPayments.Model.Entities;

namespace SFA.DAS.Payments.RequiredPayments.Messages.Events
{
    public class CalculatedRequiredCoInvestedAmount : CalculatedRequiredOnProgrammeAmount
    {
        public FundingPlatformType FundingPlatformType { get; set; } = FundingPlatformType.SubmitLearnerData;
    }
}