using System.Threading;
using System.Threading.Tasks;
using SFA.DAS.Payments.EarningEvents.Messages.Events;

namespace SFA.DAS.Payments.RequiredPayments.Domain
{
    public interface IDuplicateGSLFunctionalSkillEarningEventService
    {
        Task<bool> IsDuplicate(GSLFunctionalSkillEarningsEvent earningEvent, CancellationToken cancellationToken);
    }
}
