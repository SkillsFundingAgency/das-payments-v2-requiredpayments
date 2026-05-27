using SFA.DAS.Payments.EarningEvents.Messages.Events;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.Payments.RequiredPayments.Domain
{
    public interface IDuplicateShortCoursesEarningEventService
    {
        Task<bool> IsDuplicate(GSLShortCourseEarningsEvent earningEvent, CancellationToken cancellationToken);
    }
}
