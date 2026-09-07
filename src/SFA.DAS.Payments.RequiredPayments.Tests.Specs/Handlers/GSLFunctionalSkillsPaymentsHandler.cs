using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NServiceBus;
using SFA.DAS.Payments.EarningEvents.Messages.Events;
using SFA.DAS.Payments.RequiredPayments.Messages.Events;

namespace SFA.DAS.Payments.RequiredPayments.Tests.Specs.Handlers
{
    public class GSLFunctionalSkillsPaymentsHandler : IHandleMessages<GSLFunctionalSkillEarningsEvent>
    {
            public static ConcurrentBag<GSLFunctionalSkillEarningsEvent> ReceivedEvents { get; } = new ConcurrentBag<GSLFunctionalSkillEarningsEvent>();
            public Task Handle(GSLFunctionalSkillEarningsEvent message, IMessageHandlerContext context)
            {
                //message.amountdue was removed
                Console.WriteLine($"Received required payment: {message.Ukprn}, {message.Learner.Uln}, {message.CollectionPeriod.AcademicYear}-{message.CollectionPeriod.Period}, {message.GetType().FullName}");
                ReceivedEvents.Add(message);
                return Task.CompletedTask;
            }

            public static IEnumerable<GSLFunctionalSkillEarningsEvent> GetEvents(Learner learner) => ReceivedEvents.Where(receivedEvent =>
                receivedEvent.Learner.Uln == learner.Uln
                && receivedEvent.Ukprn == learner.Ukprn
                && receivedEvent.Learner.ReferenceNumber == learner.LearnRefNumber);
        }
    }

