using System;
using FluentAssertions;
using Wemogy.CQRS.Commands.ValueObjects;
using Wemogy.CQRS.UnitTests.TestApplication.Commands.TrackUserLogin;
using Xunit;

namespace Wemogy.CQRS.UnitTests.Commands.ValueObjects;

public class ThrottleOptionsTests
{
    [Theory]
    [InlineData(0, 0, 0, "test_1")]
    [InlineData(0, 0, 1, "test_1")]
    [InlineData(0, 0, 4, "test_1")]
    [InlineData(0, 0, 5, "test_2")]
    [InlineData(0, 0, 6, "test_2")]
    [InlineData(23, 59, 59, "test_17280")]
    public void ThrottleOptions_GetThrottlingKey_ShouldWork(int hourOfDay, int minuteOfDay, int secondOfDay, string expectedThrottlingKey)
    {
        // Arrange
        var throttleOptions = new ThrottleOptions<TrackUserLoginCommand>(
            command => command.UserId,
            TimeSpan.FromSeconds(5));
        var command = new TrackUserLoginCommand("test");
        var dateTime = new DateTime(2023, 01, 01, hourOfDay, minuteOfDay, secondOfDay);

        // Act
        var throttlingKey = throttleOptions.GetThrottlingKey(
            command,
            dateTime);

        // Assert
        throttlingKey.Should().BeEquivalentTo(expectedThrottlingKey);
    }

    [Theory]
    [InlineData(0, 0, 0, 0, 0, 5)]
    [InlineData(0, 0, 1, 0, 0, 5)]
    [InlineData(0, 0, 4, 0, 0, 5)]
    [InlineData(0, 0, 5, 0, 0, 10)]
    [InlineData(0, 0, 6, 0, 0, 10)]
    [InlineData(23, 59, 59, 0, 0, 5, true)]
    public void ThrottleOptions_GetThrottlePeriodEnd_ShouldWork(
        int hourOfDay,
        int minuteOfDay,
        int secondOfDay,
        int expectedThrottlePeriodEndHour,
        int expectedThrottlePeriodEndMinute,
        int expectedThrottlePeriodEndSecond,
        bool expectNextDay = false)
    {
        // Arrange
        var throttleOptions = new ThrottleOptions<TrackUserLoginCommand>(
            command => command.UserId,
            TimeSpan.FromSeconds(5));
        var dateTime = new DateTime(2023, 01, 31, hourOfDay, minuteOfDay, secondOfDay);

        // Act
        var throttlePeriodEnd = throttleOptions.GetThrottlePeriodEnd(
            dateTime);

        // Assert
        throttlePeriodEnd.Hour.Should().Be(expectedThrottlePeriodEndHour);
        throttlePeriodEnd.Minute.Should().Be(expectedThrottlePeriodEndMinute);
        throttlePeriodEnd.Second.Should().Be(expectedThrottlePeriodEndSecond);
        if (expectNextDay)
        {
            throttlePeriodEnd.Year.Should().Be(2023);
            throttlePeriodEnd.Month.Should().Be(2);
            throttlePeriodEnd.Day.Should().Be(01);
        }
    }
}
