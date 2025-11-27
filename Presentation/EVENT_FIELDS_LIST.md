# Список полей события транзакции портфеля

**Топик Kafka:** `portfolio.transactions`
**Отправитель:** Portfolio Service
**Получатель:** Analytics Service

## Полный список полей

| № | Поле | Тип данных | Обязательное | Описание | Пример значения |
|---|------|------------|--------------|----------|-----------------|
| 1 | `transactionId` | `Guid` (string) | ✅ Да | Уникальный идентификатор транзакции из Portfolio Service | `"3fa85f64-5717-4562-b3fc-2c963f66afa6"` |
| 2 | `portfolioId` | `Guid` (string) | ✅ Да | Идентификатор портфеля, к которому относится транзакция | `"7c9e6679-7425-40de-944b-e07fc1f90ae7"` |
| 3 | `portfolioAssetId` | `Guid` (string) | ✅ Да | Идентификатор актива портфеля (PortfolioAsset.Id) | `"8d8e6679-7425-40de-944b-e07fc1f90ae8"` |
| 4 | `stockCardId` | `Guid` (string) | ✅ Да | Идентификатор карточки актива из StockCard Service (PortfolioAsset.StockCardId) | `"9f9e6679-7425-40de-944b-e07fc1f90ae9"` |
| 5 | `assetType` | `int` | ✅ Да | Тип актива: 1=Share (Акция), 2=Bond (Облигация), 3=Crypto (Криптовалюта) | `1` |
| 6 | `transactionType` | `int` | ✅ Да | Тип транзакции: 1=Buy (Покупка), 2=Sell (Продажа) | `1` |
| 7 | `quantity` | `int` | ✅ Да | Количество единиц актива в транзакции | `10` |
| 8 | `pricePerUnit` | `decimal` | ✅ Да | Цена за единицу актива | `150.50` |
| 9 | `totalAmount` | `decimal` | ✅ Да | Общая стоимость транзакции (вычисляется как `quantity * pricePerUnit`) | `1505.00` |
| 10 | `transactionTime` | `DateTime` (ISO 8601) | ✅ Да | Время совершения транзакции в формате UTC (ISO 8601) | `"2025-01-22T10:30:00Z"` |
| 11 | `currency` | `string` | ✅ Да | Валюта транзакции (RUB, USD, EUR и т.д.), макс. 10 символов | `"RUB"` |
| 12 | `metadata` | `string?` | ❌ Нет | Дополнительные метаданные транзакции (опционально) | `null` или `"Продажа части позиции"` |

## Обязательные поля (11 из 12)

Все поля обязательны, кроме `metadata`.

## Маппинг значений enum

### AssetType

| Значение | Portfolio Service | Analytics Service | Описание |
|----------|-------------------|-------------------|----------|
| `1` | `PortfolioAssetType.Share` | `AssetType.Share` | Акция |
| `2` | `PortfolioAssetType.Bond` | `AssetType.Bond` | Облигация |
| `3` | `PortfolioAssetType.Crypto` | `AssetType.Crypto` | Криптовалюта |

### TransactionType

| Значение | Portfolio Service | Analytics Service | Описание |
|----------|-------------------|-------------------|----------|
| `1` | `PortfolioAssetTransactionType.Buy` | `TransactionType.Buy` | Покупка |
| `2` | `PortfolioAssetTransactionType.Sell` | `TransactionType.Sell` | Продажа |

## Источники данных в Portfolio Service

| Поле события | Источник в Portfolio Service | Примечание |
|--------------|------------------------------|------------|
| `transactionId` | `PortfolioAssetTransaction.Id` | Прямое значение |
| `portfolioId` | `PortfolioAsset.PortfolioId` | Через связь PortfolioAsset |
| `portfolioAssetId` | `PortfolioAssetTransaction.PortfolioAssetId` | Прямое значение |
| `stockCardId` | `PortfolioAsset.StockCardId` | Через связь PortfolioAsset |
| `assetType` | `PortfolioAsset.AssetType` | Маппинг enum через связь PortfolioAsset |
| `transactionType` | `PortfolioAssetTransaction.TransactionType` | Маппинг enum |
| `quantity` | `PortfolioAssetTransaction.Quantity` | Прямое значение |
| `pricePerUnit` | `PortfolioAssetTransaction.PricePerUnit` | Прямое значение |
| `totalAmount` | Вычисляется: `quantity * pricePerUnit` | Вычисляемое поле |
| `transactionTime` | `PortfolioAssetTransaction.TransactionDate` | Прямое значение (конвертируется в UTC) |
| `currency` | `PortfolioAssetTransaction.Currency` | Прямое значение |
| `metadata` | Не хранится в Portfolio Service | Опциональное поле, можно передать `null` |

## Валидация полей

### Правила валидации

1. **transactionId**: Должен быть валидным GUID, не пустым
2. **portfolioId**: Должен быть валидным GUID, не пустым
3. **portfolioAssetId**: Должен быть валидным GUID, не пустым
4. **stockCardId**: Должен быть валидным GUID, не пустым
5. **assetType**: Должен быть 1, 2 или 3
6. **transactionType**: Должен быть 1 или 2
7. **quantity**: Должно быть > 0
8. **pricePerUnit**: Должно быть >= 0
9. **totalAmount**: Должно быть равно `quantity * pricePerUnit`
10. **transactionTime**: Должно быть в формате ISO 8601, не в будущем
11. **currency**: Не должно быть пустым, макс. 10 символов
12. **metadata**: Может быть `null` или строкой

## Пример полного JSON сообщения

```json
{
  "transactionId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "portfolioId": "7c9e6679-7425-40de-944b-e07fc1f90ae7",
  "portfolioAssetId": "8d8e6679-7425-40de-944b-e07fc1f90ae8",
  "stockCardId": "9f9e6679-7425-40de-944b-e07fc1f90ae9",
  "assetType": 1,
  "transactionType": 1,
  "quantity": 10,
  "pricePerUnit": 150.50,
  "totalAmount": 1505.00,
  "transactionTime": "2025-01-22T10:30:00Z",
  "currency": "RUB",
  "metadata": null
}
```

## Порядок полей в JSON

Рекомендуемый порядок полей (для читаемости):

1. Идентификаторы (transactionId, portfolioId, portfolioAssetId, stockCardId)
2. Типы (assetType, transactionType)
3. Финансовые данные (quantity, pricePerUnit, totalAmount)
4. Временные данные (transactionTime)
5. Дополнительные данные (currency, metadata)

