using NServiceBus;
using SFA.DAS.Payments.RequiredPayments.Messages.Events;
using System.Collections.Concurrent;

namespace SFA.DAS.Payments.RequiredPayments.Tests.Specs.Handlers
{
    public class CompletionPaymentHeldBackEventHandler : IHandleMessages<CompletionPaymentHeldBackEvent>
    {
        public static ConcurrentBag<CompletionPaymentHeldBackEvent> ReceivedEvents { get; } = new ConcurrentBag<CompletionPaymentHeldBackEvent>();       
        public Task Handle(CompletionPaymentHeldBackEvent message, IMessageHandlerContext context)
        {
            Console.WriteLine($"Received required payment: {message.Ukprn}, {message.Learner.Uln}, {message.CollectionPeriod.AcademicYear}-{message.CollectionPeriod.Period}, {message.AmountDue}, {message.GetType().FullName}");
            ReceivedEvents.Add(message);
            return Task.CompletedTask;
        }

        public static IEnumerable<CompletionPaymentHeldBackEvent> GetEvents(Learner learner) => ReceivedEvents.Where(receivedEvent =>
            receivedEvent.Learner.Uln == learner.Uln
            && receivedEvent.Ukprn == learner.Ukprn
            && receivedEvent.Learner.ReferenceNumber == learner.LearnRefNumber);
    }
}
