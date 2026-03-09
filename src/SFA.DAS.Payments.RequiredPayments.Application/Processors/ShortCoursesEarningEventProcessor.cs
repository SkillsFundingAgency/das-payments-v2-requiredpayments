using AutoMapper;
using SFA.DAS.Payments.Application.Repositories;
using SFA.DAS.Payments.EarningEvents.Messages.Events;
using SFA.DAS.Payments.RequiredPayments.Application.Infrastructure;
using SFA.DAS.Payments.RequiredPayments.Domain;
using SFA.DAS.Payments.RequiredPayments.Domain.Entities;
using SFA.DAS.Payments.RequiredPayments.Messages.Events;
using SFA.DAS.Payments.RequiredPayments.Model.Entities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SFA.DAS.Payments.Model.Core;
using SFA.DAS.Payments.Model.Core.Entities;
using SFA.DAS.Payments.Model.Core.OnProgramme;

namespace SFA.DAS.Payments.RequiredPayments.Application.Processors
{
    public class ShortCoursesEarningEventProcessor : IShortCoursesEarningEventProcessor
    {
        public readonly INegativeEarningService negativeEarningService;
        private readonly IMapper mapper;
        public ShortCoursesEarningEventProcessor(INegativeEarningService negativeEarningService, IMapper mapper)
        {
            this.negativeEarningService = negativeEarningService;
            this.mapper = mapper;
        }
        public async Task<ReadOnlyCollection<PeriodisedRequiredPaymentEvent>> HandleEarningEvent(
            GSLShortCourseEarningsEvent earningEvent, IDataCache<PaymentHistoryEntity[]> paymentHistoryCache,
            CancellationToken cancellationToken)
        {
            var requiredPaymentEvents = new List<PeriodisedRequiredPaymentEvent>();

            //TODO: This is hardcoded placeholder to test happy path
            //REMOVE ONCE DONE
            //----------------------------------------------
            var calculatedRequiredLevyAmount = new CalculatedRequiredLevyAmount
            {
                EventId = Guid.NewGuid(),
                Ukprn = 12345678,
                Learner = new Learner
                {
                    ReferenceNumber = "L123456789",
                    Uln = 9876543210
                },
                LearningAim = new LearningAim
                {
                    Reference = "ZPROG001",
                    ProgrammeType = 25,
                    StandardCode = 123,
                    FrameworkCode = 456,
                    PathwayCode = 1,
                    LearningType = TrainingType.ApprenticeshipUnit,
                    CourseCode = "CourseCode",
                    FundingLineType = "FundingLineTypeTest"
                },
                CollectionPeriod = new CollectionPeriod
                {
                    AcademicYear = 2324,
                    Period = 1,
                },
                PriceEpisodeIdentifier = "PE-001",
                SfaContributionPercentage = 0.9m,
                AccountId = 12345,
                TransferSenderAccountId = 54321,
                ApprenticeshipId = 1111,
                ApprenticeshipPriceEpisodeId = 2222,
                ApprenticeshipEmployerType = ApprenticeshipEmployerType.Levy,
                LearningStartDate = new DateTime(2024, 9, 1),
                CourseCode = "CourseCode1234",
                LearningType = LearningType.Apprenticeship,
                ContractType = ContractType.Act1,
                OnProgrammeEarningType = OnProgrammeEarningType.Learning
                
            };
            // ----------------------------------------------
            requiredPaymentEvents.Add(calculatedRequiredLevyAmount);
            return new ReadOnlyCollection<PeriodisedRequiredPaymentEvent>(requiredPaymentEvents);
        }
    }
}
