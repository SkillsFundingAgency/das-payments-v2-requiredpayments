using NServiceBus;
using SFA.DAS.Payments.RequiredPayments.Messages.Events;
using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SFA.DAS.Payments.RequiredPayments.Tests.Specs.Handlers
{
    public class RequiredLevyPaymentsHandler : IHandleMessages<CalculatedRequiredLevyAmount>
    {
        public static ConcurrentBag<CalculatedRequiredLevyAmount> ReceivedEvents { get; } = new ConcurrentBag<CalculatedRequiredLevyAmount>();       
        public Task Handle(CalculatedRequiredLevyAmount message, IMessageHandlerContext context)
        {
            Console.WriteLine($"Received required payment: {message.Ukprn}, {message.Learner.Uln}, {message.CollectionPeriod.AcademicYear}-{message.CollectionPeriod.Period}, {message.AmountDue}, {message.GetType().FullName}");
            ReceivedEvents.Add(message);
            return Task.CompletedTask;
        }

        public static IEnumerable<CalculatedRequiredLevyAmount> GetEvents(Learner learner) => ReceivedEvents.Where(receivedEvent =>
            receivedEvent.Learner.Uln == learner.Uln
            && receivedEvent.Ukprn == learner.Ukprn
            && receivedEvent.Learner.ReferenceNumber == learner.LearnRefNumber);
    }
}
