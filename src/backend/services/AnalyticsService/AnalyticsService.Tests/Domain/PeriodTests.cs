using StockMarketAssistant.AnalyticsService.Domain.ValueObjects;

namespace StockMarketAssistant.AnalyticsService.Tests.Domain
{
    public class PeriodTests
    {
        [Fact]
        public void Constructor_ValidDates_CreatesPeriod()
        {
            // Arrange
            var start = DateTime.UtcNow.AddDays(-7);
            var end = DateTime.UtcNow;

            // Act
            var period = new Period(start, end);

            // Assert
            period.Should().NotBeNull();
            period.Start.Should().BeCloseTo(start, TimeSpan.FromSeconds(1));
            period.End.Should().BeCloseTo(end, TimeSpan.FromSeconds(1));
            period.Duration.Should().BeCloseTo(TimeSpan.FromDays(7), TimeSpan.FromSeconds(1));
        }

        [Fact]
        public void Constructor_StartAfterEnd_ThrowsArgumentException()
        {
            // Arrange
            var start = DateTime.UtcNow;
            var end = DateTime.UtcNow.AddDays(-7);

            // Act & Assert
            var act = () => new Period(start, end);

            act.Should().Throw<ArgumentException>()
                .WithMessage("*начало периода*");
        }

        [Fact]
        public void Constructor_StartEqualsEnd_ThrowsArgumentException()
        {
            // Arrange
            var date = DateTime.UtcNow;

            // Act & Assert
            var act = () => new Period(date, date);

            act.Should().Throw<ArgumentException>()
                .WithMessage("*начало периода*");
        }

        [Fact]
        public void Constructor_PeriodExceedsOneYear_ThrowsArgumentException()
        {
            // Arrange
            var start = DateTime.UtcNow.AddDays(-400);
            var end = DateTime.UtcNow;

            // Act & Assert
            var act = () => new Period(start, end);

            act.Should().Throw<ArgumentException>()
                .WithMessage("*365 дней*");
        }

        [Fact]
        public void LastHour_CreatesOneHourPeriod()
        {
            // Arrange
            var referenceTime = DateTime.UtcNow;

            // Act
            var period = Period.LastHour(referenceTime);

            // Assert
            period.Duration.Should().BeCloseTo(TimeSpan.FromHours(1), TimeSpan.FromSeconds(1));
            period.End.Should().BeCloseTo(referenceTime, TimeSpan.FromSeconds(1));
        }

        [Fact]
        public void LastDay_CreatesOneDayPeriod()
        {
            // Arrange
            var referenceTime = DateTime.UtcNow;

            // Act
            var period = Period.LastDay(referenceTime);

            // Assert
            period.Duration.Should().BeCloseTo(TimeSpan.FromDays(1), TimeSpan.FromSeconds(1));
            period.End.Should().BeCloseTo(referenceTime, TimeSpan.FromSeconds(1));
        }

        [Fact]
        public void LastWeek_CreatesSevenDaysPeriod()
        {
            // Arrange
            var referenceTime = DateTime.UtcNow;

            // Act
            var period = Period.LastWeek(referenceTime);

            // Assert
            period.Duration.Should().BeCloseTo(TimeSpan.FromDays(7), TimeSpan.FromSeconds(1));
            period.End.Should().BeCloseTo(referenceTime, TimeSpan.FromSeconds(1));
        }

        [Fact]
        public void LastMonth_CreatesOneMonthPeriod()
        {
            // Arrange
            var referenceTime = DateTime.UtcNow;

            // Act
            var period = Period.LastMonth(referenceTime);

            // Assert
            period.Duration.Should().BeCloseTo(TimeSpan.FromDays(30), TimeSpan.FromDays(2)); // Примерно месяц
            period.End.Should().BeCloseTo(referenceTime, TimeSpan.FromSeconds(1));
        }

        [Fact]
        public void Custom_ValidDates_CreatesPeriod()
        {
            // Arrange
            var start = DateTime.UtcNow.AddDays(-5);
            var end = DateTime.UtcNow;

            // Act
            var period = Period.Custom(start, end);

            // Assert
            period.Should().NotBeNull();
            period.Start.Should().BeCloseTo(start, TimeSpan.FromSeconds(1));
            period.End.Should().BeCloseTo(end, TimeSpan.FromSeconds(1));
        }

        [Fact]
        public void Equals_SameDates_ReturnsTrue()
        {
            // Arrange
            var start = DateTime.UtcNow.AddDays(-7);
            var end = DateTime.UtcNow;
            var period1 = new Period(start, end);
            var period2 = new Period(start, end);

            // Act & Assert
            period1.Should().Be(period2);
            period1.GetHashCode().Should().Be(period2.GetHashCode());
        }

        [Fact]
        public void Equals_DifferentDates_ReturnsFalse()
        {
            // Arrange
            var start1 = DateTime.UtcNow.AddDays(-7);
            var end1 = DateTime.UtcNow;
            var start2 = DateTime.UtcNow.AddDays(-5);
            var end2 = DateTime.UtcNow;
            var period1 = new Period(start1, end1);
            var period2 = new Period(start2, end2);

            // Act & Assert
            period1.Should().NotBe(period2);
        }

        [Fact]
        public void ToString_ReturnsFormattedString()
        {
            // Arrange
            var start = DateTime.UtcNow.AddDays(-7);
            var end = DateTime.UtcNow;
            var period = new Period(start, end);

            // Act
            var result = period.ToString();

            // Assert
            result.Should().Contain("UTC");
            result.Should().Contain(start.ToString("yyyy-MM-dd"));
            result.Should().Contain(end.ToString("yyyy-MM-dd"));
        }

        [Fact]
        public void Constructor_UnspecifiedDateTimeKind_ConvertsToUtc()
        {
            // Arrange
            var start = DateTime.SpecifyKind(DateTime.Now.AddDays(-7), DateTimeKind.Unspecified);
            var end = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Unspecified);

            // Act
            var period = new Period(start, end);

            // Assert
            period.Start.Kind.Should().Be(DateTimeKind.Utc);
            period.End.Kind.Should().Be(DateTimeKind.Utc);
        }
    }
}

