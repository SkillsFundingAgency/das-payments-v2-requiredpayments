﻿using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ESFA.DC.Logging.Interfaces;
using NServiceBus;
using SFA.DAS.Payments.Application.Infrastructure.Logging;
using SFA.DAS.Payments.RequiredPayments.Application.Processors;
using SFA.DAS.Payments.RequiredPayments.Messages.Events;
using ExecutionContext = ESFA.DC.Logging.ExecutionContext;

namespace SFA.DAS.Payments.RequiredPayments.ClawbackRemovedLearnerService.Handlers
{
    public class IdentifiedRemovedLearningAimEventHandler : IHandleMessages<IdentifiedRemovedLearningAim>
    {
        private readonly IClawbackRemovedLearnerAimsProcessor clawbackRemovedLearnerAimsProcessor;
        private readonly IExecutionContext executionContext;
        private readonly IPaymentLogger logger;

        public IdentifiedRemovedLearningAimEventHandler(
            IClawbackRemovedLearnerAimsProcessor clawbackRemovedLearnerAimsProcessor, IPaymentLogger logger,
            IExecutionContext executionContext)
        {
            this.clawbackRemovedLearnerAimsProcessor = clawbackRemovedLearnerAimsProcessor ??
                                                       throw new ArgumentNullException(
                                                           nameof(clawbackRemovedLearnerAimsProcessor));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.executionContext = executionContext ?? throw new ArgumentNullException(nameof(executionContext));
        }

        public async Task Handle(IdentifiedRemovedLearningAim message, IMessageHandlerContext context)
        {
            logger.LogDebug("Processing 'IdentifiedRemovedLearningAim' message.");
            ((ExecutionContext)executionContext).JobId = message.JobId.ToString();

            var calculatedRequiredLevyAmount = await clawbackRemovedLearnerAimsProcessor
                .GenerateClawbackForRemovedLearnerAim(message, CancellationToken.None).ConfigureAwait(false);

            logger.LogDebug($"Got {calculatedRequiredLevyAmount?.Count ?? 0} Calculated Required Levy Amount events.");

            if (calculatedRequiredLevyAmount != null)
                await Task.WhenAll(calculatedRequiredLevyAmount.Select(context.Publish)).ConfigureAwait(false);

            logger.LogInfo("Successfully processed IdentifiedRemovedLearningAim event for " +
                           $"jobId:{message.JobId}, learnerRef:{message.Learner.ReferenceNumber}, frameworkCode:{message.LearningAim.FrameworkCode}, " +
                           $"pathwayCode:{message.LearningAim.PathwayCode}, programmeType:{message.LearningAim.ProgrammeType}, " +
                           $"standardCode:{message.LearningAim.StandardCode}, learningAimReference:{message.LearningAim.Reference}, " +
                           $"academicYear:{message.CollectionPeriod.AcademicYear}, contractType:{message.ContractType}");
        }
    }
}