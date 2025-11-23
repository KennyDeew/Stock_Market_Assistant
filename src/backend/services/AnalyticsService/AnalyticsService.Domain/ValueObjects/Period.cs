using StockMarketAssistant.AnalyticsService.Domain.Common;

namespace StockMarketAssistant.AnalyticsService.Domain.ValueObjects
{
    /// <summary>
    /// Value Object для представления временного периода для агрегации
    /// Неизменяемый объект, равенство определяется по значениям Start и End
    /// </summary>
    public class Period : ValueObject
    {
        /// <summary>
        /// Начало периода
        /// </summary>
        public DateTime Start { get; }

        /// <summary>
        /// Конец периода
        /// </summary>
        public DateTime End { get; }

        /// <summary>
        /// Длительность периода
        /// </summary>
        public TimeSpan Duration => End - Start;

        /// <summary>
        /// Конструктор для создания периода
        /// </summary>
        /// <param name="start">Начало периода</param>
        /// <param name="end">Конец периода</param>
        /// <exception cref="ArgumentException">Если параметры невалидны</exception>
        public Period(DateTime start, DateTime end)
        {
            // Нормализация дат в UTC
            var startUtc = start.Kind == DateTimeKind.Unspecified
                ? DateTime.SpecifyKind(start, DateTimeKind.Utc)
                : start.ToUniversalTime();

            var endUtc = end.Kind == DateTimeKind.Unspecified
                ? DateTime.SpecifyKind(end, DateTimeKind.Utc)
                : end.ToUniversalTime();

            ValidatePeriod(startUtc, endUtc);

            Start = startUtc;
            End = endUtc;
        }

        /// <summary>
        /// Создать период за последний час от указанного времени
        /// </summary>
        /// <param name="referenceTime">Время отсчёта (обычно текущее время)</param>
        /// <returns>Период за последний час</returns>
        public static Period LastHour(DateTime referenceTime)
        {
            var end = referenceTime.ToUniversalTime();
            var start = end.AddHours(-1);
            return new Period(start, end);
        }

        /// <summary>
        /// Создать период за последний день от указанного времени
        /// </summary>
        /// <param name="referenceTime">Время отсчёта (обычно текущее время)</param>
        /// <returns>Период за последний день</returns>
        public static Period LastDay(DateTime referenceTime)
        {
            var end = referenceTime.ToUniversalTime();
            var start = end.AddDays(-1);
            return new Period(start, end);
        }

        /// <summary>
        /// Создать период за последнюю неделю от указанного времени
        /// </summary>
        /// <param name="referenceTime">Время отсчёта (обычно текущее время)</param>
        /// <returns>Период за последнюю неделю</returns>
        public static Period LastWeek(DateTime referenceTime)
        {
            var end = referenceTime.ToUniversalTime();
            var start = end.AddDays(-7);
            return new Period(start, end);
        }

        /// <summary>
        /// Создать период за последний месяц от указанного времени
        /// </summary>
        /// <param name="referenceTime">Время отсчёта (обычно текущее время)</param>
        /// <returns>Период за последний месяц</returns>
        public static Period LastMonth(DateTime referenceTime)
        {
            var end = referenceTime.ToUniversalTime();
            var start = end.AddMonths(-1);
            return new Period(start, end);
        }

        /// <summary>
        /// Создать произвольный период
        /// </summary>
        /// <param name="start">Начало периода</param>
        /// <param name="end">Конец периода</param>
        /// <returns>Произвольный период</returns>
        /// <exception cref="ArgumentException">Если параметры невалидны</exception>
        public static Period Custom(DateTime start, DateTime end)
        {
            return new Period(start, end);
        }

        /// <summary>
        /// Валидация периода
        /// </summary>
        /// <param name="start">Начало периода (уже нормализовано в UTC)</param>
        /// <param name="end">Конец периода (уже нормализовано в UTC)</param>
        /// <exception cref="ArgumentException">Если параметры невалидны</exception>
        private static void ValidatePeriod(DateTime start, DateTime end)
        {
            // Валидация: Start должен быть < End
            if (start >= end)
            {
                throw new ArgumentException("Начало периода должно быть раньше конца периода", nameof(start));
            }

            // Валидация: максимальная длительность периода - 1 год (365 дней)
            var maxDuration = TimeSpan.FromDays(365);
            var duration = end - start;
            if (duration > maxDuration)
            {
                throw new ArgumentException(
                    $"Длительность периода не может превышать 1 год (365 дней). Текущая длительность: {duration.TotalDays:F2} дней",
                    nameof(end));
            }
        }

        /// <summary>
        /// Получить компоненты для сравнения равенства
        /// </summary>
        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Start;
            yield return End;
        }

        /// <summary>
        /// Строковое представление периода
        /// </summary>
        public override string ToString()
        {
            return $"[{Start:yyyy-MM-dd HH:mm:ss} UTC - {End:yyyy-MM-dd HH:mm:ss} UTC]";
        }
    }
}

