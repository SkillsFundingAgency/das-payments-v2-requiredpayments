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
    public class GSLFunctionalSkillEarningsEventHandler : IHandleMessages<GSLFunctionalSkillEarningsEvent>
    {
        private readonly IActorProxyFactory proxyFactory;
        private readonly IPaymentLogger paymentLogger;
        private readonly IApprenticeshipKeyService apprenticeshipKeyService;
        private readonly ESFA.DC.Logging.ExecutionContext executionContext;

        public GSLFunctionalSkillEarningsEventHandler(IActorProxyFactory proxyFactory, 
            IPaymentLogger paymentLogger, 
            IApprenticeshipKeyService apprenticeshipKeyService, 
            IExecutionContext executionContext)
        {
            this.proxyFactory = proxyFactory;
            this.paymentLogger = paymentLogger;
            this.apprenticeshipKeyService = apprenticeshipKeyService;
            this.executionContext = (ESFA.DC.Logging.ExecutionContext)executionContext;
        }

        public async Task Handle(GSLFunctionalSkillEarningsEvent message, IMessageHandlerContext context)
        {
            executionContext.JobId = message.JobId.ToString();

            paymentLogger.LogInfo($"Processing GSLFunctionalSkillEarningsEvent, UKPRN: {message.Ukprn}, Period: {message.CollectionPeriod}");

            var key = apprenticeshipKeyService.GenerateApprenticeshipKey(
                message.Ukprn,
                message.Learner.ReferenceNumber,
                message.LearningAim.FrameworkCode,
                message.LearningAim.PathwayCode,
                message.LearningAim.ProgrammeType,
                message.LearningAim.StandardCode,
                message.LearningAim.Reference,
                message.CollectionPeriod.AcademicYear,
                message.ContractType,
                message.LearningAim.CourseCode,
                CourseType.FunctionalSkill
            );

            var actorId = new ActorId(key);
            var actor = proxyFactory.CreateActorProxy<IRequiredPaymentsService>(
                new Uri("fabric:/SFA.DAS.Payments.RequiredPayments.ServiceFabric/RequiredPaymentsServiceActorService"),
                actorId);

            IReadOnlyCollection<PeriodisedRequiredPaymentEvent> requiredPaymentEvents;

            try
            {
                // Ensure actor is initialised inside actor - actor will load historic payment data
                requiredPaymentEvents = await actor.HandleGSLFunctionalSkillEarningsEvent(message, CancellationToken.None).ConfigureAwait(false);

                if (requiredPaymentEvents != null && requiredPaymentEvents.Any())
                {
                    // Publish each outgoing required payment event for downstream consumers
                    await Task.WhenAll(requiredPaymentEvents.Select(context.Publish)).ConfigureAwait(false);
                }

                paymentLogger.LogInfo("Successfully processed RequiredPaymentsProxyService GSLFunctionalSkillEarningsEvent for Actor for " +
                                      $"learnerRef:{message.Learner.ReferenceNumber}, frameworkCode:{message.LearningAim.FrameworkCode}, " +
                                      $"pathwayCode:{message.LearningAim.PathwayCode}, programmeType:{message.LearningAim.ProgrammeType}, " +
                                      $"standardCode:{message.LearningAim.StandardCode}, learningAimReference:{message.LearningAim.Reference}, " +
                                      $"academicYear:{message.CollectionPeriod.AcademicYear}, contractType:{message.ContractType}");
            }
            catch (Exception ex)
            {
                paymentLogger.LogError("Failed to process GSLFunctionalSkillEarningsEvent for Actor for " +
                                       $"learnerRef:{message.Learner.ReferenceNumber}, frameworkCode:{message.LearningAim.FrameworkCode}, " +
                                       $"pathwayCode:{message.LearningAim.PathwayCode}, programmeType:{message.LearningAim.ProgrammeType}, " +
                                       $"standardCode:{message.LearningAim.StandardCode}, learningAimReference:{message.LearningAim.Reference}, " +
                                       $"academicYear:{message.CollectionPeriod.AcademicYear}, contractType:{message.ContractType}, Exception: {ex.Message}");
                throw;
            }

            paymentLogger.LogInfo($"Finished GSLFunctionalSkillEarningsEvent. UKPRN: {message.Ukprn}, Period: {message.CollectionPeriod}");
        }
    }
}