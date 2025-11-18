import type { AssetShort } from '../types/assetTypes';
import type { PortfolioAssetTypeLabel } from '../types/portfolioAssetTypes';

type AssetTypeValue =
  | AssetShort['type'] // 'stock' | 'bond' | 'crypto'
  | PortfolioAssetTypeLabel; // 'Share' | 'Bond' | 'Crypto'

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
