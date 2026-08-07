using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NServiceBus;
using SFA.DAS.Payments.RequiredPayments.Messages.Events;

namespace SFA.DAS.Payments.RequiredPayments.Tests.Specs.Handlers
{
    public class PeriodisedRequiredPaymentEventHandler : IHandleMessages<PeriodisedRequiredPaymentEvent>
    {
        public static ConcurrentBag<PeriodisedRequiredPaymentEvent> ReceivedEvents { get; } = new ConcurrentBag<PeriodisedRequiredPaymentEvent>();

        public Task Handle(PeriodisedRequiredPaymentEvent message, IMessageHandlerContext context)
        {
            ReceivedEvents.Add(message);
            return Task.CompletedTask;
        }

        public static IEnumerable<PeriodisedRequiredPaymentEvent> GetEvents(Learner learner) =>
            ReceivedEvents.Where(receivedEvent =>
                receivedEvent.Learner?.Uln == learner.Uln
                && receivedEvent.Ukprn == learner.Ukprn
                && receivedEvent.Learner.ReferenceNumber == learner.LearnRefNumber);
    }
}
