using System.Collections.Concurrent;
using NServiceBus;
using SFA.DAS.Payments.RequiredPayments.Messages.Events;

namespace SFA.DAS.Payments.RequiredPayments.Tests.Specs.Handlers
{
    public class GSLFunctionalSkillsPaymentsHandler : IHandleMessages<PeriodisedRequiredPaymentEvent>
    {
        public static ConcurrentBag<PeriodisedRequiredPaymentEvent> ReceivedEvents { get; } = new();

        public Task Handle(PeriodisedRequiredPaymentEvent message, IMessageHandlerContext context)
        {
            Console.WriteLine($"Received required payment: {message.Ukprn}, {message.Learner.Uln}, " +
                $"{message.CollectionPeriod.AcademicYear}-{message.CollectionPeriod.Period}, {message.GetType().FullName}");
            ReceivedEvents.Add(message);
            return Task.CompletedTask;
        }

        public static IEnumerable<PeriodisedRequiredPaymentEvent> GetEvents(Learner learner) =>
            ReceivedEvents.Where(receivedEvent =>
                receivedEvent.Learner.Uln == learner.Uln &&
                receivedEvent.Ukprn == learner.Ukprn &&
                receivedEvent.Learner.ReferenceNumber == learner.LearnRefNumber);
    }
}
