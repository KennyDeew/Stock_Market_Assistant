namespace StockMarketAssistant.AnalyticsService.Domain.Common
{
    /// <summary>
    /// Базовый класс для Value Objects
    /// Value Objects неизменяемы и равенство определяется по значениям свойств, а не по идентичности
    /// </summary>
    public abstract class ValueObject
    {
        /// <summary>
        /// Получить компоненты для сравнения равенства
        /// </summary>
        /// <returns>Перечисление объектов для сравнения</returns>
        protected abstract IEnumerable<object> GetEqualityComponents();

        /// <summary>
        /// Сравнение на равенство по значениям
        /// </summary>
        public override bool Equals(object? obj)
        {
            if (obj == null || obj.GetType() != GetType())
            {
                return false;
            }

            var other = (ValueObject)obj;
            return GetEqualityComponents().SequenceEqual(other.GetEqualityComponents());
        }

        /// <summary>
        /// Получить хеш-код на основе компонентов равенства
        /// </summary>
        public override int GetHashCode()
        {
            return GetEqualityComponents()
                .Select(x => x?.GetHashCode() ?? 0)
                .Aggregate((x, y) => x ^ y);
        }

        /// <summary>
        /// Оператор равенства
        /// </summary>
        public static bool operator ==(ValueObject? left, ValueObject? right)
        {
            if (ReferenceEquals(left, right))
            {
                return true;
            }

            if (left is null || right is null)
            {
                return false;
            }

            return left.Equals(right);
        }

        /// <summary>
        /// Оператор неравенства
        /// </summary>
        public static bool operator !=(ValueObject? left, ValueObject? right)
        {
            return !(left == right);
        }
    }
}

