using System;
using NUnit.Framework;
using SlotRogue.UI.Notifications;

namespace SlotRogue.UI.Tests.Notifications
{
    public sealed class WeeklyRankingScheduleTests
    {
        [Test]
        public void NextReset_BeforeWednesdayMidnight_UsesUpcomingWednesday()
        {
            var utcNow = new DateTime(
                2026,
                6,
                16,
                14,
                0,
                0,
                DateTimeKind.Utc);

            DateTime result = WeeklyRankingSchedule.GetNextResetUtc(utcNow);

            Assert.That(
                result,
                Is.EqualTo(new DateTime(
                    2026,
                    6,
                    16,
                    15,
                    0,
                    0,
                    DateTimeKind.Utc)));
        }

        [Test]
        public void NextReset_AtWednesdayMidnight_UsesNextWeek()
        {
            var utcNow = new DateTime(
                2026,
                6,
                16,
                15,
                0,
                0,
                DateTimeKind.Utc);

            DateTime result = WeeklyRankingSchedule.GetNextResetUtc(utcNow);

            Assert.That(
                result,
                Is.EqualTo(new DateTime(
                    2026,
                    6,
                    23,
                    15,
                    0,
                    0,
                    DateTimeKind.Utc)));
        }

        [Test]
        public void NextDeadline_BeforeTuesdayNinePm_UsesCurrentWeek()
        {
            var utcNow = new DateTime(
                2026,
                6,
                16,
                11,
                0,
                0,
                DateTimeKind.Utc);

            DateTime result = WeeklyRankingSchedule.GetNextDeadlineUtc(
                utcNow,
                TimeSpan.FromHours(3));

            Assert.That(
                result,
                Is.EqualTo(new DateTime(
                    2026,
                    6,
                    16,
                    12,
                    0,
                    0,
                    DateTimeKind.Utc)));
        }

        [Test]
        public void NextDeadline_AfterTuesdayNinePm_UsesNextWeek()
        {
            var utcNow = new DateTime(
                2026,
                6,
                16,
                13,
                0,
                0,
                DateTimeKind.Utc);

            DateTime result = WeeklyRankingSchedule.GetNextDeadlineUtc(
                utcNow,
                TimeSpan.FromHours(3));

            Assert.That(
                result,
                Is.EqualTo(new DateTime(
                    2026,
                    6,
                    23,
                    12,
                    0,
                    0,
                    DateTimeKind.Utc)));
        }

        [Test]
        public void NextDeadlineNotification_AfterDeadlineBeforeReset_UsesImmediateFallback()
        {
            var utcNow = new DateTime(
                2026,
                6,
                16,
                13,
                0,
                0,
                DateTimeKind.Utc);

            DateTime result = WeeklyRankingSchedule.GetNextDeadlineNotificationUtc(
                utcNow,
                TimeSpan.FromHours(3),
                TimeSpan.FromSeconds(5));

            Assert.That(
                result,
                Is.EqualTo(new DateTime(
                    2026,
                    6,
                    16,
                    13,
                    0,
                    5,
                    DateTimeKind.Utc)));
        }

        [Test]
        public void NextDeadlineNotification_TooCloseToReset_UsesNextWeek()
        {
            var utcNow = new DateTime(
                2026,
                6,
                16,
                14,
                59,
                58,
                DateTimeKind.Utc);

            DateTime result = WeeklyRankingSchedule.GetNextDeadlineNotificationUtc(
                utcNow,
                TimeSpan.FromHours(3),
                TimeSpan.FromSeconds(5));

            Assert.That(
                result,
                Is.EqualTo(new DateTime(
                    2026,
                    6,
                    23,
                    12,
                    0,
                    0,
                    DateTimeKind.Utc)));
        }
    }
}
