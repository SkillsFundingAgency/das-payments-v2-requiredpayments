using SFA.DAS.Payments.Application.Repositories;
using SFA.DAS.Payments.EarningEvents.Messages.Events;
using SFA.DAS.Payments.Model.Core;
using SFA.DAS.Payments.RequiredPayments.Application.Infrastructure;
using SFA.DAS.Payments.RequiredPayments.Domain;
using SFA.DAS.Payments.RequiredPayments.Messages.Events;
using SFA.DAS.Payments.RequiredPayments.Model.Entities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.Payments.RequiredPayments.Application.Processors
{
    public class GSLFunctionalSkillEarningsEventProcessor : IGSLFunctionalSkillEarningsEventProcessor
    {
        private readonly IDuplicateGSLFunctionalSkillEarningEventService duplicateGSLFunctionalSkillEarningEventService;

        public GSLFunctionalSkillEarningsEventProcessor(IDuplicateGSLFunctionalSkillEarningEventService duplicateGSLFunctionalSkillEarningEventService)
        {
            this.duplicateGSLFunctionalSkillEarningEventService = duplicateGSLFunctionalSkillEarningEventService;
        }

        public async Task<ReadOnlyCollection<PeriodisedRequiredPaymentEvent>> HandleEarningEvent(GSLFunctionalSkillEarningsEvent earningEvent,
            IDataCache<PaymentHistoryEntity[]> paymentHistoryCache,
            CancellationToken cancellationToken)
        {
            try
            {
                var requiredPaymentEvents = new List<PeriodisedRequiredPaymentEvent>();

                if (await duplicateGSLFunctionalSkillEarningEventService.IsDuplicate(earningEvent, cancellationToken)
                        .ConfigureAwait(false))
                {
                    return requiredPaymentEvents.AsReadOnly();
                }

                //Gets all historical payments for this learner and academic year.This give milestone payments, completion payments, previous collection period payments.
                var academicYearPayments =
                    await GetAcademicYearPayments(
                        earningEvent,
                        paymentHistoryCache,
                        cancellationToken);

                //Extracting the latest earnings from the incoming submission.
                var currentEarnings = GetPeriods(earningEvent).ToList();

                // TO DO

                return new ReadOnlyCollection<PeriodisedRequiredPaymentEvent>(requiredPaymentEvents);
            }
            catch (Exception ex)
            {
                throw new Exception(
                    $"Error processing GSLFunctionalSkillEarningsEvent for LearningAimReference: {earningEvent.LearningAim.Reference}, " +
                    $"CollectionPeriod: Year: {earningEvent.CollectionPeriod.AcademicYear} Period: {earningEvent.CollectionPeriod.Period}. Exception: {ex.Message}",
                    ex);
            }
        }

        private static async Task<List<PaymentHistoryEntity>> GetAcademicYearPayments(GSLFunctionalSkillEarningsEvent earningEvent,
            IDataCache<PaymentHistoryEntity[]> paymentHistoryCache,
            CancellationToken cancellationToken)
        {
            var cachedPayments = await paymentHistoryCache.TryGet(CacheKeys.PaymentHistoryKey, cancellationToken);

            return cachedPayments.HasValue
                ? cachedPayments.Value.Where(x =>
                        x.LearnAimReference.Equals(earningEvent.LearningAim.Reference, StringComparison.OrdinalIgnoreCase)
                        && x.CollectionPeriod.AcademicYear == earningEvent.CollectionPeriod.AcademicYear)
                    .ToList()
                : new List<PaymentHistoryEntity>();
        }

        private IReadOnlyCollection<(EarningPeriod period, int type)> GetPeriods(
            GSLFunctionalSkillEarningsEvent earningEvent)
        {
            var result = new List<(EarningPeriod period, int type)>();

            foreach (var earning in earningEvent.Earnings)
            {
                foreach (var period in earning.Periods)
                {
                    result.Add((period, (int)earning.Type));
                }
            }

            return result;
        }
    }
}