using System;
using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.Payments.EarningEvents.Messages;
using SFA.DAS.Payments.EarningEvents.Messages.Events;
using SFA.DAS.Payments.Model.Core;
using SFA.DAS.Payments.RequiredPayments.Domain.Services;
using SFA.DAS.Payments.ServiceFabric.Core.Messaging;

namespace SFA.DAS.Payments.RequiredPayments.Domain.UnitTests.Services
{
    [TestFixture]
    public class ShortCoursesEarningEventKeyTests
    {
        private long jobId;
        private long ukprn;
        private Learner learner;
        private LearningAim learningAim;
        private CollectionPeriod collectionPeriod;
        private string eventType;
        private string mockBaseKey;
        private string mockBaseLogSafeKey;

        [SetUp]
        public void SetUp()
        {
            jobId = 123456;
            ukprn = 1234;
            learner = new Learner
            {
                Uln = 12345678,
                ReferenceNumber = "learn-ref"
            };
            learningAim = new LearningAim
            {
                StartDate = DateTime.Now.AddYears(-1),
                FrameworkCode = 1,
                FundingLineType = "funding-line",
                PathwayCode = 2,
                ProgrammeType = 3,
                Reference = "aim-ref",
                SequenceNumber = 4,
                StandardCode = 5,
            };
            collectionPeriod = new CollectionPeriod { AcademicYear = 2021, Period = 1 };
            eventType = nameof(GSLShortCourseEarningsEvent);
            mockBaseKey = $@"{jobId}-{ukprn}-{collectionPeriod.AcademicYear}-{collectionPeriod.Period}-{learner.Uln}-{learner.ReferenceNumber}-{learningAim.Reference}-{learningAim.ProgrammeType}-{learningAim.StandardCode}-{learningAim.FrameworkCode}-{learningAim.PathwayCode}-{learningAim.FundingLineType}-{learningAim.SequenceNumber}-{learningAim.StartDate:G}-{eventType}";
            mockBaseLogSafeKey = $@"{jobId}-{collectionPeriod.AcademicYear}-{collectionPeriod.Period}-{learner.ReferenceNumber}-{learningAim.Reference}-{learningAim.ProgrammeType}-{learningAim.StandardCode}-{learningAim.FrameworkCode}-{learningAim.PathwayCode}-{learningAim.FundingLineType}-{learningAim.SequenceNumber}-{learningAim.StartDate:G}-{eventType}";
        }
        [Test]
        public void ShortCoursesEarningEventKey_Should_Throw_If_Event_Passed_Is_Null()
        {
            // Arrange
            GSLShortCourseEarningsEvent earningEvent = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => new ShortCoursesEarningEventKey(earningEvent));

        }

        [Test]
        public void ShortCoursesEarningEventKey_Should_Inherit_From_EarningEventKey()
        {
            // Arrange & Act
            var key = new ShortCoursesEarningEventKey(CreateDefaultEarningEvent());

            // Assert
            Assert.That(key, Is.InstanceOf<EarningEventKey>());
        }

        [Test]
        public void ShortCoursesEarningEventKey_CreateKey_Should_Return_Base_String_And_GSL_Properties()
        {
            // Arrange
            var earningEvent = CreateDefaultEarningEvent();
            // Act
            var key = new ShortCoursesEarningEventKey(earningEvent).Key;

            // Assert
            key.Should().Be($"{mockBaseKey}-{earningEvent.ExternalEarningsId}-{earningEvent.EventId}-{earningEvent.LearningAim.CourseCode}");

        }

        [Test]
        public void ShortCoursesEarningEventKey_CreateLogSafeKey_Should_Return_Base_String_And_GSL_Properties()
        {
            // Arrange
            var earningEvent = CreateDefaultEarningEvent();
            // Act
            var key = new ShortCoursesEarningEventKey(earningEvent).LogSafeKey;

            // Assert
            key.Should().Be($"{mockBaseLogSafeKey}-{earningEvent.ExternalEarningsId}-{earningEvent.EventId}-{earningEvent.LearningAim.CourseCode}");

        }
        private GSLShortCourseEarningsEvent CreateDefaultEarningEvent()
        {
            return new GSLShortCourseEarningsEvent
            {
                EventId = Guid.NewGuid(),
                AgeAtStartOfLearning = 19,
                Earnings = new List<ShortCourseEarning>
                {
                    new ShortCourseEarning
                    {
                        Periods = new List<EarningPeriod>(),
                        Type = ShortCourseEarningType.Milestone1
                    }
                },
                JobId = 123456,
                CollectionPeriod = new CollectionPeriod { AcademicYear = 2021, Period = 1 },
                Ukprn = 1234,
                EventTime = DateTimeOffset.UtcNow,
                IlrSubmissionDateTime = DateTime.Now,
                Learner = new Learner
                {
                    Uln = 12345678,
                    ReferenceNumber = "learn-ref"
                },
                LearningAim = new LearningAim
                {
                    StartDate = DateTime.Now.AddYears(-1),
                    FrameworkCode = 1,
                    FundingLineType = "funding-line",
                    PathwayCode = 2,
                    ProgrammeType = 3,
                    Reference = "aim-ref",
                    SequenceNumber = 4,
                    StandardCode = 5,
                }
            };
        }
    }
}
