using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Client;
using NServiceBus;
using SFA.DAS.Payments.Application.Infrastructure.Logging;
using SFA.DAS.Payments.EarningEvents.Messages.Events;
using SFA.DAS.Payments.Model.Core.Entities;
using SFA.DAS.Payments.RequiredPayments.Domain;
using SFA.DAS.Payments.RequiredPayments.RequiredPaymentsService.Interfaces;
using System;
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
            var actor = proxyFactory.CreateActorProxy<IRequiredPaymentsService>(new Uri("fabric:/SFA.DAS.Payments.RequiredPayments.ServiceFabric/RequiredPaymentsService"), actorId);
            var result = await actor.HandleShortCoursesEarningEvent(message, CancellationToken.None).ConfigureAwait(false);
            paymentLogger.LogInfo($"Finished GSLShortCourseEarningsEvent. UKPRN: {message.Ukprn}, JobId: {message.JobId}, Period: {message.CollectionPeriod}, ILR: {message.IlrSubmissionDateTime}");
        }
    }
}
