﻿using System;
using System.Collections.Generic;
using System.Linq;
using NServiceBus;
using SFA.DAS.Payments.AcceptanceTests.Core;
using SFA.DAS.Payments.EarningEvents.Messages.Events;
using SFA.DAS.Payments.Model.Core;
using SFA.DAS.Payments.Model.Core.OnProgramme;
using SFA.DAS.Payments.RequiredPayments.AcceptanceTests.Data;
using TechTalk.SpecFlow;

namespace SFA.DAS.Payments.RequiredPayments.AcceptanceTests.Steps
{
    [Binding]
    public class RequiredPaymentsInputSteps: StepsBase
    {
        private readonly ScenarioContext context;

        public RequiredPaymentsInputSteps(ScenarioContext context)
        {
            this.context = context;
        }
       [When(@"an earning event is received")]
        public async void WhenAnEarningEventIsReceived()
        {
            // Get all the input data
            var processingPeriod = (short)context["ProcessingPeriod"];

            var learner = context.Get<Payments.AcceptanceTests.Core.Data.Learner>();

            IEnumerable<Course> courses = null;

            if (context.ContainsKey("Courses"))
            {
                courses = context["Courses"] as IEnumerable<Course>;
            }

            var course = courses?.FirstOrDefault();

            var learningAim = course.AsLearningAim();

            var learningEarning = CreateEarning("ContractType2OnProgrammeEarningsLearning", OnProgrammeEarningType.Learning, e => e.Learning_1);
            var completionEarning = CreateEarning("ContractType2OnProgrammeEarningsCompletion", OnProgrammeEarningType.Completion, e => e.Completion_2);

            var onProgrammeEarnings = new List<OnProgrammeEarning>();

            if (learningEarning != null)
            {
                onProgrammeEarnings.Add(learningEarning);
            }

            if (completionEarning != null)
            {
                onProgrammeEarnings.Add(completionEarning);
            }

            // now create an event from all this stuff and sent it off
            var earning = new ApprenticeshipContractType2EarningEvent
            {
                JobId = "job-1234",
                Ukprn = learner.Ukprn,
                EventTime = DateTimeOffset.UtcNow,
                Learner = new Learner
                {
                    ReferenceNumber = learner.GeneratedLearnRefNumber,
                    Uln = learner.Uln
                },
                LearningAim = learningAim,
                PriceEpisodes = new List<PriceEpisode>
                {
                    new PriceEpisode
                    {
                        StartDate = course.LearningStartDate,
                        EndDate = course.LearningPlannedEndDate.Value,
                        AgreedPrice = GetAgreedPrice("ContractType2OnProgrammeEarningsLearning"),
                        Identifier = GetPriceEpisodeIdentifier("ContractType2OnProgrammeEarningsLearning")
                    }
                }
                .AsReadOnly(),
                EarningYear = (short)DateTime.Today.Year,
                SfaContributionPercentage = 0.9M,
                
                OnProgrammeEarnings = onProgrammeEarnings.AsReadOnly()
            };

            var options = new SendOptions();
            options.RequireImmediateDispatch();

            await MessageSession.Send(earning, options).ConfigureAwait(false);
        }

        private decimal GetAgreedPrice(string storageName)
        {
            if (context.ContainsKey(storageName))
            {
                if (context[storageName] is ContractTypeEarning earning)
                {
                    return earning.TotalNegotiatedPrice;
                }
            }

            return decimal.Zero;
        }

        private string GetPriceEpisodeIdentifier(string storageName)
        {
            if (context.ContainsKey(storageName))
            {
                if (context[storageName] is ContractTypeEarning earning)
                {
                    return earning.PriceEpisodeIdentifier;
                }
            }

            return string.Empty;
        }

        private OnProgrammeEarning CreateEarning(string storageName, OnProgrammeEarningType earningType, Func<ContractTypeEarning, decimal?> amount)
        {
            OnProgrammeEarning result = null;
            if (context.ContainsKey(storageName))
            {
                if (context[storageName] is ContractTypeEarning earning && amount(earning).HasValue)
                {
                    var learningAmount = amount(earning).Value;
                    var earningPeriods = new List<EarningPeriod>();

                    for (var period = earning.FromPeriod; period <= earning.ToPeriod; period++)
                    {
                        earningPeriods.Add(new EarningPeriod { Period = period, Amount = learningAmount });
                    }

                    var learningEarning = new OnProgrammeEarning
                        { Type = earningType, Periods = earningPeriods.AsReadOnly() };

                    result = learningEarning;
                }
            }

            return result;
        }
    }
}