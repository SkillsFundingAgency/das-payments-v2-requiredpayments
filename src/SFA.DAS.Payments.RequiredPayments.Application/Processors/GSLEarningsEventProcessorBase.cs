using SFA.DAS.Payments.EarningEvents.Messages.Events;
using SFA.DAS.Payments.Model.Core;
using SFA.DAS.Payments.Model.Core.Entities;
using SFA.DAS.Payments.RequiredPayments.Messages.Events;
using SFA.DAS.Payments.RequiredPayments.Model.Entities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SFA.DAS.Payments.RequiredPayments.Application.Processors
{
    public abstract class GSLEarningsEventProcessorBase
    {
        protected void GenerateRefundPayments<TEvent>(
            TEvent earningEvent,
            List<PeriodisedRequiredPaymentEvent> requiredPaymentEvents,
            List<PaymentHistoryEntity> academicYearPayments,
            List<(EarningPeriod period, int type)> currentEarnings,
            Func<TEvent, PriceEpisode, EarningPeriod, int, bool, PeriodisedRequiredPaymentEvent>
                createRequiredPaymentEvent)
            where TEvent : EarningEvent
        {
            foreach (var historicGroup in academicYearPayments.GroupBy(x => new
            {
                x.TransactionType,
                x.DeliveryPeriod
            }))
            {
                var historicPayments = historicGroup.ToList();
                var historicAmount = historicPayments.Sum(x => x.Amount);

                var currentMatch = currentEarnings.FirstOrDefault(x =>
                    x.type == historicGroup.Key.TransactionType &&
                    x.period.Period == historicGroup.Key.DeliveryPeriod);

                var requiresRefund =
                    currentMatch.period == null ||
                    currentMatch.period.Amount != historicAmount;

                if (!requiresRefund)
                {
                    continue;
                }

                var paymentToBeRefunded = historicPayments.First();

                var refundPeriod = new EarningPeriod
                {
                    Period = paymentToBeRefunded.DeliveryPeriod,
                    Amount = -historicAmount,
                    PriceEpisodeIdentifier = paymentToBeRefunded.PriceEpisodeIdentifier,
                    AccountId = paymentToBeRefunded.AccountId,
                    TransferSenderAccountId = paymentToBeRefunded.TransferSenderAccountId,
                    SfaContributionPercentage = paymentToBeRefunded.SfaContributionPercentage,
                    ApprenticeshipEmployerType = paymentToBeRefunded.ApprenticeshipEmployerType,
                    ApprenticeshipId = paymentToBeRefunded.ApprenticeshipId
                };

                var priceEpisode =
                    earningEvent.PriceEpisodes.FirstOrDefault()
                    ?? new PriceEpisode
                    {
                        FundingLineType =
                            paymentToBeRefunded.LearningAimFundingLineType
                    };

                requiredPaymentEvents.Add(
                    createRequiredPaymentEvent(
                        earningEvent,
                        priceEpisode,
                        refundPeriod,
                        paymentToBeRefunded.TransactionType,
                        IsCoInvested(historicPayments)));
            }
        }

        protected void GenerateNewRequiredPayments<TEvent>(
            TEvent earningEvent,
            List<PeriodisedRequiredPaymentEvent> requiredPaymentEvents,
            List<PaymentHistoryEntity> academicYearPayments,
            List<(EarningPeriod period, int type)> currentEarnings,
            Func<TEvent, PriceEpisode, EarningPeriod, int, bool, PeriodisedRequiredPaymentEvent>
                createRequiredPaymentEvent)
            where TEvent : EarningEvent
        {
            foreach (var (period, type) in currentEarnings)
            {
                if (period.Period > earningEvent.CollectionPeriod.Period)
                {
                    continue;
                }

                var historicPayments = academicYearPayments
                    .Where(x =>
                        x.DeliveryPeriod == period.Period &&
                        x.TransactionType == type)
                    .ToList();

                if (historicPayments.Any() &&
                    historicPayments.Sum(x => x.Amount) == period.Amount)
                {
                    continue;
                }

                var priceEpisode =
                    earningEvent.PriceEpisodes.FirstOrDefault(x =>
                        x.Identifier == period.PriceEpisodeIdentifier)
                    ?? new PriceEpisode();

                requiredPaymentEvents.Add(
                    createRequiredPaymentEvent(
                        earningEvent,
                        priceEpisode,
                        period,
                        type,
                        false));
            }
        }

        protected static bool IsCoInvested(
            IEnumerable<PaymentHistoryEntity> payments)
        {
            return payments.Any(x =>
                x.FundingSource == FundingSourceType.CoInvestedSfa ||
                x.FundingSource == FundingSourceType.CoInvestedEmployer);
        }
    }
}