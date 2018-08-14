﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Client;
using NServiceBus;
using SFA.DAS.Payment.ServiceFabric.Core;
using SFA.DAS.Payments.EarningEvents.Messages.Events;
using SFA.DAS.Payments.RequiredPayments.Domain.Enums;
using SFA.DAS.Payments.RequiredPayments.Domain.Interfaces;
using SFA.DAS.Payments.RequiredPayments.Messages.Events;
using SFA.DAS.Payments.RequiredPayments.RequiredPaymentsService.Interfaces;

namespace SFA.DAS.Payments.RequiredPayments.RequiredPaymentsProxyService.Handlers
{
    public class PayableEarningEventHandler : IHandleMessages<PayableEarningEvent>
    {
        private readonly IApprenticeshipKeyService _apprenticeshipKeyService;
        private readonly IEndpointCommunicationSender<IPaymentsDueEvent> _endpoint;
        private readonly IActorProxyFactory _proxyFactory;

        public PayableEarningEventHandler(IApprenticeshipKeyService apprenticeshipKeyService, IEndpointCommunicationSender<IPaymentsDueEvent> endpoint, IActorProxyFactory proxyFactory)
        {
            Debug.WriteLine("******************************************************** handler started");
            _apprenticeshipKeyService = apprenticeshipKeyService;
            _endpoint = endpoint;
            _proxyFactory = proxyFactory ?? new ActorProxyFactory();
        }

        public async Task Handle(PayableEarningEvent message, IMessageHandlerContext context)
        {
            try
            {
                var key = _apprenticeshipKeyService.GenerateKey(
                    message.Ukprn,
                    message.LearnRefNumber,
                    message.LearnAim.FrameworkCode,
                    message.LearnAim.PathwayCode,
                    (ProgrammeType) message.LearnAim.ProgrammeType,
                    message.LearnAim.StandardCode,
                    message.LearnAim.LearnAimRef
                );

                var actorId = new ActorId(key);
                var actor = _proxyFactory.CreateActorProxy<IRequiredPaymentsService>(new Uri("fabric:/SFA.DAS.Payments.RequiredPayments.ServiceFabric/RequiredPaymentsServiceActorService"), actorId);
                var paymentsDue = await actor.HandlePayableEarning(message, CancellationToken.None).ConfigureAwait(false);

                await Task.WhenAll(paymentsDue.Select(p => _endpoint.Send(p)).ToArray()).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Trace.TraceError($"Error: {ex.Message}, Ex: {ex}");
                throw;
            }
        }
    }
}
