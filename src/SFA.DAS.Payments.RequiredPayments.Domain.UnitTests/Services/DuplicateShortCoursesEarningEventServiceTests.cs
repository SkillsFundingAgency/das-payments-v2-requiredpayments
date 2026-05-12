using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.Payments.Application.Infrastructure.Logging;
using SFA.DAS.Payments.Application.Repositories;
using SFA.DAS.Payments.EarningEvents.Messages;
using SFA.DAS.Payments.EarningEvents.Messages.Events;
using SFA.DAS.Payments.Model.Core;
using SFA.DAS.Payments.RequiredPayments.Domain.Services;

namespace SFA.DAS.Payments.RequiredPayments.Domain.UnitTests.Services;

[TestFixture]
public class DuplicateShortCoursesEarningEventServiceTests
{
    [SetUp]
    public void Setup()
    {
        mockLogger = new Mock<IPaymentLogger>();
        mockCache = new Mock<IActorDataCache<ShortCoursesEarningEventKey>>();
        sut = new DuplicateShortCoursesEarningEventService(mockLogger.Object, mockCache.Object);
    }

    private Mock<IPaymentLogger> mockLogger;
    private Mock<IActorDataCache<ShortCoursesEarningEventKey>> mockCache;
    private IDuplicateShortCoursesEarningEventService sut;

    [Test]
    public void DuplicateShortCoursesEarningEventService_Should_Throw_When_Null_Logger()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new DuplicateShortCoursesEarningEventService(null, null));
    }

    [Test]
    public void DuplicateShortCoursesEarningEventService_Should_Throw_When_Null_Cache()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            new DuplicateShortCoursesEarningEventService(mockLogger.Object, null));
    }

    [Test]
    public async Task DuplicateShortCoursesEarningEventService_IsDuplicate_Should_Return_True_If_Duplicate()
    {
        // Arrange
        mockCache.Setup(x => x.Contains(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);
        sut = new DuplicateShortCoursesEarningEventService(mockLogger.Object, mockCache.Object);
        var earningEvent = CreateDefaultEarningEvent();
        var logSafeKey = new ShortCoursesEarningEventKey(earningEvent).LogSafeKey;
        // Act
        var result = await sut.IsDuplicate(earningEvent, CancellationToken.None);
        result.Should().BeTrue();
        CheckLogState(LogLevel.Debug,
            $@"Checking if short courses earning event of type GSLShortCourseEarningsEvent with guid: {earningEvent.EventId} has already been received.");
        CheckLogState(LogLevel.Debug, $@"Short Courses Earning event key: {logSafeKey}");
        CheckLogState(LogLevel.Warning, $@"Key: {logSafeKey} found in the cache and is probably a duplicate.");
        CheckLogState(LogLevel.Debug,
            $@"New short courses earning event. Event key: {logSafeKey}, event id: {earningEvent.EventId}", 0);
        CheckLogState(LogLevel.Information, $"Added new short courses earning event to cache. Key: {logSafeKey}", 0);

        mockCache.Verify(x => x.Contains(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
        mockCache.Verify(
            x => x.Add(It.IsAny<string>(), It.IsAny<ShortCoursesEarningEventKey>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Test]
    public async Task DuplicateShortCoursesEarningEventService_IsDuplicate_Should_Return_False_If_New_Key()
    {
        // Arrange
        mockCache.Setup(x => x.Contains(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);
        mockCache.Setup(x =>
                x.Add(It.IsAny<string>(), It.IsAny<ShortCoursesEarningEventKey>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        sut = new DuplicateShortCoursesEarningEventService(mockLogger.Object, mockCache.Object);
        var earningEvent = CreateDefaultEarningEvent();
        var logSafeKey = new ShortCoursesEarningEventKey(earningEvent).LogSafeKey;
        // Act
        var result = await sut.IsDuplicate(earningEvent, CancellationToken.None);
        result.Should().BeFalse();
        CheckLogState(LogLevel.Debug,
            $@"Checking if short courses earning event of type GSLShortCourseEarningsEvent with guid: {earningEvent.EventId} has already been received.");
        CheckLogState(LogLevel.Debug, $@"Short Courses Earning event key: {logSafeKey}");
        CheckLogState(LogLevel.Warning, $@"Key: {logSafeKey} found in the cache and is probably a duplicate.", 0);
        CheckLogState(LogLevel.Debug,
            $@"New short courses earning event. Event key: {logSafeKey}, event id: {earningEvent.EventId}");
        CheckLogState(LogLevel.Information, $"Added new short courses earning event to cache. Key: {logSafeKey}");

        mockCache.Verify(x => x.Contains(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
        mockCache.Verify(
            x => x.Add(It.IsAny<string>(), It.IsAny<ShortCoursesEarningEventKey>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    private void CheckLogState(LogLevel level, string logMessage, int times = 1)
    {
        switch (level)
        {
            case LogLevel.Debug:
                mockLogger.Verify(
                    x => x.LogDebug(It.Is<string>(s => s.Contains(logMessage)), It.IsAny<object[]>(), It.IsAny<long>(), It.IsAny<string>(), It.IsAny<string>(),
                        It.IsAny<int>()), Times.Exactly(times));
                break;
            case LogLevel.Information:
                mockLogger.Verify(
                    x => x.LogInfo(It.Is<string>(s => s.Contains(logMessage)), It.IsAny<object[]>(), It.IsAny<long>(), It.IsAny<string>(), It.IsAny<string>(),
                        It.IsAny<int>()), Times.Exactly(times));
                break;
            case LogLevel.Warning:
                mockLogger.Verify(
                    x => x.LogWarning(It.Is<string>(s => s.Contains(logMessage)), It.IsAny<object[]>(), It.IsAny<long>(), It.IsAny<string>(), It.IsAny<string>(),
                        It.IsAny<int>()), Times.Exactly(times));
                break;
            case LogLevel.Error:
                mockLogger.Verify(
                    x => x.LogError(It.Is<string>(s => s.Contains(logMessage)), It.IsAny<Exception>(), It.IsAny<object[]>(), It.IsAny<long>(), null,
                        It.IsAny<string>(), It.IsAny<int>()), Times.Exactly(times));
                break;
        }
    }
    private GSLShortCourseEarningsEvent CreateDefaultEarningEvent()
    {
        return new GSLShortCourseEarningsEvent
        {
            EventId = Guid.NewGuid(),
            AgeAtStartOfLearning = 19,
            Earnings = new List<ShortCourseEarning>
            {
                new()
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
                StandardCode = 5
            }
        };
    }
}