using System;
using Wemogy.CQRS.Commands.Abstractions;

namespace Wemogy.CQRS.Commands.ValueObjects
{
    public class ThrottleOptions<TCommand>
        where TCommand : ICommandBase
    {
        /// <summary>
        /// A function that returns the key to use for throttling the command.
        /// All commands with the same key will be throttled together.
        /// </summary>
        public Func<TCommand, string> ThrottleKeyResolver { get; }

        /// <summary>
        /// You can specify the period for throttling here.
        /// When you set it to 5 seconds, the command will be throttled for 5 seconds.
        /// </summary>
        public TimeSpan ThrottlePeriod { get; }

        public Func<TCommand, string>? SessionIdResolver { get; }

        public ThrottleOptions(
            Func<TCommand, string> throttleKeyResolver,
            TimeSpan throttlePeriod,
            Func<TCommand, string>? sessionIdResolver = null)
        {
            ThrottleKeyResolver = throttleKeyResolver;
            ThrottlePeriod = throttlePeriod;
            SessionIdResolver = sessionIdResolver;
        }

        public string GetThrottlingKey(TCommand command, DateTime dateTime)
        {
            var baseKey = ThrottleKeyResolver(command);

            return $"{baseKey}_{GetSlotId(dateTime)}";
        }

        public DateTime GetThrottlePeriodEnd(DateTime dateTime)
        {
            var secondsOfTheDay = GetSecondsOfTheDay(dateTime); // 2
            var slotId = GetSlotId(secondsOfTheDay); // 1
            var timespan = TimeSpan.FromTicks(ThrottlePeriod.Ticks * slotId); // 5
            var timestampFromUpcomingSlot = dateTime.Add(ThrottlePeriod);
            var upcomingSlotId = GetSlotId(timestampFromUpcomingSlot);

            if (upcomingSlotId == 1)
            {
                // next day
                return new DateTime(
                    timestampFromUpcomingSlot.Year,
                    timestampFromUpcomingSlot.Month,
                    timestampFromUpcomingSlot.Day,
                    ThrottlePeriod.Hours,
                    ThrottlePeriod.Minutes,
                    ThrottlePeriod.Seconds,
                    dateTime.Kind);
            }

            return new DateTime(
                dateTime.Year,
                dateTime.Month,
                dateTime.Day,
                timespan.Hours,
                timespan.Minutes,
                timespan.Seconds,
                dateTime.Kind);
        }

        private int GetSlotId(DateTime dateTime)
        {
            var secondsOfTheDay = GetSecondsOfTheDay(dateTime);
            var slotId = GetSlotId(secondsOfTheDay);
            return slotId;
        }

        private int GetSecondsOfTheDay(DateTime dateTime)
        {
            var secondsOfTheDay = (int)(dateTime - dateTime.Date).TotalSeconds;
            return secondsOfTheDay;
        }

        private int GetSlotId(int secondsOfTheDay)
        {
            var slotId = (secondsOfTheDay / (int)ThrottlePeriod.TotalSeconds) + 1;
            return slotId;
        }
    }
}
