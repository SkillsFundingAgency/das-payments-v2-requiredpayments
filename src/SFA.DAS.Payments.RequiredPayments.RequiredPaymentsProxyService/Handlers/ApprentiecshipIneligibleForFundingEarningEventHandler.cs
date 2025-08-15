using ESFA.DC.Logging.Interfaces;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Client;
using NServiceBus;
using SFA.DAS.Payments.Application.Infrastructure.Logging;
using SFA.DAS.Payments.EarningEvents.Messages.Events;
using SFA.DAS.Payments.RequiredPayments.Domain;
using SFA.DAS.Payments.RequiredPayments.Messages.Events;
using SFA.DAS.Payments.RequiredPayments.RemovedLearnerService.Interfaces;
using SFA.DAS.Payments.RequiredPayments.RequiredPaymentsService.Interfaces;
using System;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.Payments.RequiredPayments.RequiredPaymentsProxyService.Handlers
{
    public class ApprentiecshipIneligibleForFundingEarningEventHandler : IHandleMessages<ApprenticeshipIneligibleForFundingEarningEvent>
    {
        private readonly IActorProxyFactory proxyFactory;
        private readonly IPaymentLogger paymentLogger;

        public ApprentiecshipIneligibleForFundingEarningEventHandler(IActorProxyFactory proxyFactory, IPaymentLogger paymentLogger)
        {
            this.proxyFactory = proxyFactory;
            this.paymentLogger = paymentLogger;
        }

        public async Task Handle(ApprenticeshipIneligibleForFundingEarningEvent message, IMessageHandlerContext context)
        {
            paymentLogger.LogInfo($"Processing ApprentiecshipIneligibleForFundingEarningEvent, UKPRN: {message.Ukprn}, JobId: {message.JobId}, Period: {message.CollectionPeriod}, ILR: {message.IlrSubmissionDateTime}");

            var actorId = new ActorId(message.Ukprn.ToString());
            var actor = proxyFactory.CreateActorProxy<IRemovedLearnerService>(new Uri("fabric:/SFA.DAS.Payments.RequiredPayments.ServiceFabric/RemovedLearnerServiceActorService"), actorId);
            var removedAims = await actor.HandleApprentiecshipIneligibleForFundingEarningEvent(message.CollectionPeriod.AcademicYear, message.CollectionPeriod.Period, message.IlrSubmissionDateTime, CancellationToken.None).ConfigureAwait(false);

            foreach (var removedAim in removedAims)
            {
                removedAim.JobId = message.JobId;
                await context.Publish(removedAim).ConfigureAwait(false);
            }

            paymentLogger.LogInfo($"Finished ApprentiecshipIneligibleForFundingEarningEvent, published {removedAims.Count} aims. UKPRN: {message.Ukprn}, JobId: {message.JobId}, Period: {message.CollectionPeriod}, ILR: {message.IlrSubmissionDateTime}");
        }

    }
}