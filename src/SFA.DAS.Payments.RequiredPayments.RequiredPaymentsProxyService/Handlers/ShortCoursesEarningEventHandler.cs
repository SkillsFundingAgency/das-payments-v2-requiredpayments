using ESFA.DC.Logging.Interfaces;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Client;
using NServiceBus;
using SFA.DAS.Payments.Application.Infrastructure.Logging;
using SFA.DAS.Payments.EarningEvents.Messages.Events;
using SFA.DAS.Payments.Model.Core.Entities;
using SFA.DAS.Payments.RequiredPayments.Domain;
using SFA.DAS.Payments.RequiredPayments.Messages.Events;
using SFA.DAS.Payments.RequiredPayments.RequiredPaymentsService.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.Payments.RequiredPayments.RequiredPaymentsProxyService.Handlers
{
    public class ShortCoursesEarningEventHandler : IHandleMessages<GSLShortCourseEarningsEvent>
    {
        private readonly IActorProxyFactory proxyFactory;
        private readonly IPaymentLogger paymentLogger;
        private readonly IApprenticeshipKeyService apprenticeshipKeyService;
        private readonly ESFA.DC.Logging.ExecutionContext executionContext;

        public ShortCoursesEarningEventHandler(IActorProxyFactory proxyFactory, IPaymentLogger paymentLogger, IApprenticeshipKeyService apprenticeshipKeyService, IExecutionContext executionContext)
        {
            this.proxyFactory = proxyFactory;
            this.paymentLogger = paymentLogger;
            this.apprenticeshipKeyService = apprenticeshipKeyService;
            this.executionContext = (ESFA.DC.Logging.ExecutionContext)executionContext;
        }
        public async Task Handle(GSLShortCourseEarningsEvent message, IMessageHandlerContext context)
        {
            executionContext.JobId = message.JobId.ToString();

            paymentLogger.LogInfo($"Processing GSLShortCourseEarningsEvent, UKPRN: {message.Ukprn}, Period: {message.CollectionPeriod}");

            var key = apprenticeshipKeyService.GenerateApprenticeshipKey(
                message.Ukprn,
                message.Learner.ReferenceNumber,
                message.LearningAim.FrameworkCode,
                message.LearningAim.PathwayCode,
                message.LearningAim.ProgrammeType,
                message.LearningAim.StandardCode,
                message.LearningAim.Reference,
                message.CollectionPeriod.AcademicYear,
                ContractType.Act1
            );

            var actorId = new ActorId(key);
            var actor = proxyFactory.CreateActorProxy<IRequiredPaymentsService>(
                new Uri("fabric:/SFA.DAS.Payments.RequiredPayments.ServiceFabric/RequiredPaymentsServiceActorService"),
                actorId);
            IReadOnlyCollection<PeriodisedRequiredPaymentEvent> requiredPaymentEvent;
            try
            {
                requiredPaymentEvent = await actor.HandleShortCoursesEarningEvent(message, CancellationToken.None).ConfigureAwait(false);

                if (requiredPaymentEvent != null)
                    await Task.WhenAll(requiredPaymentEvent.Select(context.Publish)).ConfigureAwait(false);

                paymentLogger.LogInfo("Successfully processed RequiredPaymentsProxyService event for Actor for " +
                                      $"learnerRef:{message.Learner.ReferenceNumber}, frameworkCode:{message.LearningAim.FrameworkCode}, " +
                                      $"pathwayCode:{message.LearningAim.PathwayCode}, programmeType:{message.LearningAim.ProgrammeType}, " +
                                      $"standardCode:{message.LearningAim.StandardCode}, learningAimReference:{message.LearningAim.Reference}, " +
                                      $"academicYear:{message.CollectionPeriod.AcademicYear}, contractType:{ContractType.Act1}");
            }
            catch (Exception ex)
            {
                paymentLogger.LogError("Failed to process Payable Earnings event for Actor for " +
                                       $"learnerRef:{message.Learner.ReferenceNumber}, frameworkCode:{message.LearningAim.FrameworkCode}, " +
                                       $"pathwayCode:{message.LearningAim.PathwayCode}, programmeType:{message.LearningAim.ProgrammeType}, " +
                                       $"standardCode:{message.LearningAim.StandardCode}, learningAimReference:{message.LearningAim.Reference}, " +
                                       $"academicYear:{message.CollectionPeriod.AcademicYear}, contractType:{ContractType.Act1}, Exception: {ex.Message}");
                throw;
            }
            paymentLogger.LogInfo($"Finished GSLShortCourseEarningsEvent. UKPRN: {message.Ukprn}, Period: {message.CollectionPeriod}");
        }
    }
}
