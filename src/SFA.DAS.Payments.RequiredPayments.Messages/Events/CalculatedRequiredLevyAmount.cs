using SFA.DAS.Payments.Messages.Common;
using SFA.DAS.Payments.Model.Core.Entities;
using SFA.DAS.Payments.RequiredPayments.Model.Entities;
using System;

namespace SFA.DAS.Payments.RequiredPayments.Messages.Events
{
    public class CalculatedRequiredLevyAmount : CalculatedRequiredOnProgrammeAmount, ILeafLevelMessage
    {
        public int Priority { get; set; }
        public string AgreementId { get; set; }
        public DateTime? AgreedOnDate { get; set; }
        public FundingPlatformType FundingPlatformType { get; set; } = FundingPlatformType.SubmitLearnerData;
        public LearningType LearningType { get; set; } = LearningType.Apprenticeship;
        public CourseType CourseType { get; set; } = CourseType.ShortCourse;
    }
}