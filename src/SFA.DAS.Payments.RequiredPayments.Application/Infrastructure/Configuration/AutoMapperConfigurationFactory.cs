﻿using AutoMapper;
using SFA.DAS.Payments.EarningEvents.Messages.Entities;
using SFA.DAS.Payments.EarningEvents.Messages.Events;
using SFA.DAS.Payments.RequiredPayments.Domain.Entities;
using SFA.DAS.Payments.RequiredPayments.Messages.Entities;
using SFA.DAS.Payments.RequiredPayments.Messages.Events;
using SFA.DAS.Payments.RequiredPayments.Model.Entities;

namespace SFA.DAS.Payments.RequiredPayments.Application.Infrastructure.Configuration
{
    //TODO: should use automapper profiles instead
    public class AutoMapperConfigurationFactory
    {
        public static MapperConfiguration CreateMappingConfig()
        {
            return new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<PaymentEntity, Payment>();

                cfg.CreateMap<LearnerEntity, Learner>()
                    .ForMember(dst => dst.IsTemp, opt => opt.Ignore());

                cfg.CreateMap<PaymentDue, CalculatedPaymentDueEvent>()
                    .ForMember(dst => dst.PaymentDueEntity, opt => opt.Ignore())
                    .ForMember(dst => dst.EventTime, opt => opt.Ignore())
                    .AfterMap((src, dst) =>
                    {
                        dst.PaymentDueEntity = new PaymentDueEntity();
                    });

                cfg.CreateMap<PayableEarningEvent, PayableEarning>()
                    .ForMember(dst => dst.Course, opt => opt.Ignore())
                    .AfterMap((src, dst) =>
                    {
                        dst.Course = new Course
                        {
                            //TODO: map course
                        };
                    });
            });
        }
    }
}
