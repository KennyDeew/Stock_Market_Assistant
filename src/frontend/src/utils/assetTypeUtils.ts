import type { AssetShort } from '../types/assetTypes';
import type { PortfolioAssetTypeLabel } from '../types/portfolioAssetTypes';

type AssetTypeValue =
  | AssetShort['type'] // 'stock' | 'bond' | 'crypto'
  | PortfolioAssetTypeLabel; // 'Share' | 'Bond' | 'Crypto'

/**
 * Утилиты для работы с типами активов
 */

// Типы интерфейса
export type UiAssetType = 'stock' | 'bond' | 'crypto';

/**
 * Маппинг типа актива из API (PascalCase) в тип для интерфейса (snake_case)
 * @param type Тип из API: 'Stock' | 'Bond' | 'Crypto'
 * @returns Тип для UI: 'stock' | 'bond' | 'crypto'
 */
export const mapApiToUiAssetType = (type: string): UiAssetType => {
  switch (type) {
    case 'Stock': return 'stock';
    case 'Bond': return 'bond';
    case 'Crypto': return 'crypto';
    default: return 'stock';
  }
};

/**
 * Маппинг типа актива из UI в API (соответствует PortfolioAssetType)
 * @param type Тип из интерфейса: 'stock' | 'bond' | 'crypto'
 * @returns Тип для API: 'Share' | 'Bond' | 'Crypto'
 */
export const mapUiToApiAssetType = (type: UiAssetType): 'Share' | 'Bond' | 'Crypto' => {
  const map: Record<UiAssetType, 'Share' | 'Bond' | 'Crypto'> = {
    stock: 'Share',
    bond: 'Bond',
    crypto: 'Crypto',
  };
  return map[type];
};


/**
 * Преобразует любой тип актива (из AssetShort, PortfolioAssetTypeLabel, API) в человекочитаемое название
 * @param type Тип актива
 * @returns Человекочитаемое название
 */
export const getAssetTypeName = (type: AssetTypeValue): string => {
  const map: Record<AssetTypeValue, string> = {
    stock: 'Акция',
    bond: 'Облигация',
    crypto: 'Криптовалюта',
    Share: 'Акция',
    Bond: 'Облигация',
    Crypto: 'Криптовалюта',
  };

  return map[type] || 'Неизвестно';
};

/**
 * Возвращает цветовую метку, совместимую с MUI theme.palette
 * @param type Тип актива
 * @returns Цвет: 'primary' | 'success' | 'warning'
 */
export const getAssetTypeColor = (
  type: AssetTypeValue
): 'primary' | 'success' | 'warning' => {
  const map: Record<AssetTypeValue, 'primary' | 'success' | 'warning'> = {
    stock: 'primary',
    bond: 'success',
    crypto: 'warning',
    Share: 'primary',
    Bond: 'success',
    Crypto: 'warning',
  };

  // Если тип неизвестен — возвращаем 'primary' как fallback
  return map[type] || 'primary';
};

/**
 * Приведение любого формата типа к UiAssetType
 * @param type Тип из API, UI или домена
 * @returns UiAssetType
 */
export const normalizeAssetType = (type: string): UiAssetType => {
  if (['stock', 'Stock'].includes(type)) return 'stock';
  if (['bond', 'Bond'].includes(type)) return 'bond';
  if (['crypto', 'Crypto'].includes(type)) return 'crypto';
  return 'stock';
};
