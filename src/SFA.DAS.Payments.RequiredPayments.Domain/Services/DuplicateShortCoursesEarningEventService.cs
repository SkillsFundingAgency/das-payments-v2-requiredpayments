using Microsoft.Extensions.Logging;
using SFA.DAS.Payments.Application.Repositories;
using SFA.DAS.Payments.EarningEvents.Messages.Events;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.Payments.RequiredPayments.Domain.Services
{
    public class DuplicateShortCoursesEarningEventService : IDuplicateShortCoursesEarningEventService
    {
        private readonly ILogger<DuplicateShortCoursesEarningEventService> logger;
        private readonly IActorDataCache<ShortCoursesEarningEventKey> cache;

        public DuplicateShortCoursesEarningEventService(ILogger<DuplicateShortCoursesEarningEventService> logger, IActorDataCache<ShortCoursesEarningEventKey> cache)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.cache = cache ?? throw new ArgumentNullException(nameof(cache));
        }
        public async Task<bool> IsDuplicate(GSLShortCourseEarningsEvent shortCoursesEarningEvent, CancellationToken cancellationToken)
        {
            logger.LogDebug($"Checking if short courses earning event of type {shortCoursesEarningEvent.GetType().Name} with guid: {shortCoursesEarningEvent.EventId} has already been received.");
            var shortCoursesEarningEventKey = new ShortCoursesEarningEventKey(shortCoursesEarningEvent);

            logger.LogDebug($"Short Courses Earning event key: {shortCoursesEarningEventKey.LogSafeKey}");
            if (await cache.Contains(shortCoursesEarningEventKey.Key, cancellationToken).ConfigureAwait(false))
            {
                logger.LogWarning($"Key: {shortCoursesEarningEventKey.LogSafeKey} found in the cache and is probably a duplicate.");
                return true;
            }
            logger.LogDebug($"New short courses earning event. Event key: {shortCoursesEarningEventKey.LogSafeKey}, event id: {shortCoursesEarningEvent.EventId}");
            await cache.Add(shortCoursesEarningEventKey.Key, shortCoursesEarningEventKey, cancellationToken);
            logger.LogInformation($"Added new short courses earning event to cache. Key: {shortCoursesEarningEventKey.LogSafeKey}");
            return false;
        }
    }
}