using System.Collections.Concurrent;
using NServiceBus;
using SFA.DAS.Payments.RequiredPayments.Messages.Events;

namespace SFA.DAS.Payments.RequiredPayments.Tests.Specs.Handlers
{
    public class RequiredIncentivePaymentHandler : IHandleMessages<CalculatedRequiredIncentiveAmount>
    {
        public static ConcurrentBag<CalculatedRequiredIncentiveAmount> ReceivedEvents { get; } = new();

        public Task Handle(CalculatedRequiredIncentiveAmount message, IMessageHandlerContext context)
        {
            Console.WriteLine($"Received required payment: {message.Ukprn}, {message.Learner.Uln}, {message.CollectionPeriod.AcademicYear}-{message.CollectionPeriod.Period}, {message.AmountDue}, {message.GetType().FullName}");
            ReceivedEvents.Add(message);
            return Task.CompletedTask;
        }

        public static IEnumerable<CalculatedRequiredIncentiveAmount> GetEvents(Learner learner) => 
            ReceivedEvents.Where(receivedEvent =>
                receivedEvent.Learner.Uln == learner.Uln &&
                receivedEvent.Ukprn == learner.Ukprn &&
                receivedEvent.Learner.ReferenceNumber == learner.LearnRefNumber);
    }
}
