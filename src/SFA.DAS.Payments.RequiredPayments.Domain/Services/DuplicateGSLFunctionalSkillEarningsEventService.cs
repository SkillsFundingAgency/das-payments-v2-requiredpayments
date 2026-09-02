using SFA.DAS.Payments.Application.Infrastructure.Logging;
using SFA.DAS.Payments.Application.Repositories;
using SFA.DAS.Payments.EarningEvents.Messages.Events;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.Payments.RequiredPayments.Domain.Services
{
    public class DuplicateGSLFunctionalSkillEarningsEventService : IDuplicateGSLFunctionalSkillEarningEventService
    {
        private readonly IPaymentLogger logger;
        private readonly IActorDataCache<GSLFunctionalSkillEarningsEventKey> cache;

        public DuplicateGSLFunctionalSkillEarningsEventService(IPaymentLogger logger, IActorDataCache<GSLFunctionalSkillEarningsEventKey> cache)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.cache = cache ?? throw new ArgumentNullException(nameof(cache));
        }

        public async Task<bool> IsDuplicate(GSLFunctionalSkillEarningsEvent glsFunctionalSkillEarningsEvent, CancellationToken cancellationToken)
        {
            logger.LogDebug($"Checking if GSL functional skill earning event of type {glsFunctionalSkillEarningsEvent.GetType().Name} with guid: {glsFunctionalSkillEarningsEvent.EventId} has already been received.");
            var glsFunctionalSkillEarningsEventKey = new GSLFunctionalSkillEarningsEventKey(glsFunctionalSkillEarningsEvent);

            logger.LogDebug($"GSL functional skill Earning event key: {glsFunctionalSkillEarningsEventKey.LogSafeKey}");
            if (await cache.Contains(glsFunctionalSkillEarningsEventKey.Key, cancellationToken).ConfigureAwait(false))
            {
                logger.LogWarning($"Key: {glsFunctionalSkillEarningsEventKey.LogSafeKey} found in the cache and is probably a duplicate.");
                return true;
            }
            logger.LogDebug($"New GSL functional skill earning event. Event key: {glsFunctionalSkillEarningsEventKey.LogSafeKey}, event id: {glsFunctionalSkillEarningsEvent.EventId}");
            
            await cache.Add(glsFunctionalSkillEarningsEventKey.Key, glsFunctionalSkillEarningsEventKey, cancellationToken);
            logger.LogInfo($"Added new GSL functional skill earning event to cache. Key: {glsFunctionalSkillEarningsEventKey.LogSafeKey}");

            return false;
        }
    }
}