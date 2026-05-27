using SFA.DAS.Payments.ServiceFabric.Core.Messaging;
using System;
using SFA.DAS.Payments.EarningEvents.Messages.Events;

namespace SFA.DAS.Payments.RequiredPayments.Domain.Services
{
    public class ShortCoursesEarningEventKey : EarningEventKey
    {
        public Guid ExternalEarningsId { get; }
        public Guid EventId { get; }
        public string CourseCode { get; }
        protected ShortCoursesEarningEventKey() { }
        public ShortCoursesEarningEventKey(GSLShortCourseEarningsEvent earningEvent) : base(earningEvent)
        {
            if (earningEvent == null) throw new ArgumentNullException(nameof(earningEvent));
            ExternalEarningsId = earningEvent.ExternalEarningsId;
            EventId = earningEvent.EventId;
            CourseCode = earningEvent.LearningAim.CourseCode;
        }
        protected override string CreateKey()
        {
            return $"{base.CreateKey()}-{ExternalEarningsId}-{EventId}-{CourseCode}";
        }
        protected override string CreateLogSafeKey()
        {
            return $"{base.CreateLogSafeKey()}-{ExternalEarningsId}-{EventId}-{CourseCode}";
        }
    }

}