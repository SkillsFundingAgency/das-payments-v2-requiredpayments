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

        public ShortCoursesEarningEventHandler(IActorProxyFactory proxyFactory, IPaymentLogger paymentLogger, IApprenticeshipKeyService apprenticeshipKeyService)
        {
            this.proxyFactory = proxyFactory;
            this.paymentLogger = paymentLogger;
            this.apprenticeshipKeyService = apprenticeshipKeyService;
        }
        public async Task Handle(GSLShortCourseEarningsEvent message, IMessageHandlerContext context)
        {
            paymentLogger.LogInfo($"Processing GSLShortCourseEarningsEvent, UKPRN: {message.Ukprn}, JobId: {message.JobId}, Period: {message.CollectionPeriod}, ILR: {message.IlrSubmissionDateTime}");

            var contractType = ContractType.Act1;
            var key = apprenticeshipKeyService.GenerateApprenticeshipKey(
                message.Ukprn,
                message.Learner.ReferenceNumber,
                message.LearningAim.FrameworkCode,
                message.LearningAim.PathwayCode,
                message.LearningAim.ProgrammeType,
                message.LearningAim.StandardCode,
                message.LearningAim.Reference,
                message.CollectionPeriod.AcademicYear,
                contractType
            );

            var actorId = new ActorId(key);
            var actor = proxyFactory.CreateActorProxy<IRequiredPaymentsService>(new Uri("fabric:/SFA.DAS.Payments.RequiredPayments.ServiceFabric/RequiredPaymentsService"), actorId);
            var requiredPaymentEvent = await actor.HandleShortCoursesEarningEvent(message, CancellationToken.None).ConfigureAwait(false);

            //Send RequiredPaymentEvents to ServiceBus for FundingSource to process
            try
            {
                if (requiredPaymentEvent != null)
                    await Task.WhenAll(requiredPaymentEvent.Select(context.Publish)).ConfigureAwait(false);

                paymentLogger.LogInfo("Successfully processed RequiredPaymentsProxyService event for Actor for " +
                                      $"jobId:{message.JobId}, learnerRef:{message.Learner.ReferenceNumber}, frameworkCode:{message.LearningAim.FrameworkCode}, " +
                                      $"pathwayCode:{message.LearningAim.PathwayCode}, programmeType:{message.LearningAim.ProgrammeType}, " +
                                      $"standardCode:{message.LearningAim.StandardCode}, learningAimReference:{message.LearningAim.Reference}, " +
                                      $"academicYear:{message.CollectionPeriod.AcademicYear}, contractType:{contractType}");
            }
            catch (Exception ex)
            {
                paymentLogger.LogError("Failed to process Payable Earnings event for Actor for " +
                                       $"jobId:{message.JobId}, learnerRef:{message.Learner.ReferenceNumber}, frameworkCode:{message.LearningAim.FrameworkCode}, " +
                                       $"pathwayCode:{message.LearningAim.PathwayCode}, programmeType:{message.LearningAim.ProgrammeType}, " +
                                       $"standardCode:{message.LearningAim.StandardCode}, learningAimReference:{message.LearningAim.Reference}, " +
                                       $"academicYear:{message.CollectionPeriod.AcademicYear}, contractType:{contractType}");
                throw;
            }
            paymentLogger.LogInfo($"Finished GSLShortCourseEarningsEvent. UKPRN: {message.Ukprn}, JobId: {message.JobId}, Period: {message.CollectionPeriod}, ILR: {message.IlrSubmissionDateTime}");
        }
    }
}
