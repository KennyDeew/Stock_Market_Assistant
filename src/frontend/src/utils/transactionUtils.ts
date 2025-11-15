import { PortfolioAssetTransactionTypeLabel } from '../types/portfolioAssetTypes';
import { PortfolioAssetTransactionTypeValue } from '../types/portfolioAssetTypes';

export const getTransactionLabel = (type: PortfolioAssetTransactionTypeLabel): string => {
  return type === 'Buy' ? 'Покупка' : 'Продажа';
};

export const getTransactionColor = (
  type: PortfolioAssetTransactionTypeLabel
): 'primary' | 'success' => {
  return type === 'Buy' ? 'primary' : 'success';
};

export const toTransactionTypeValue = (type: 'Buy' | 'Sell'): PortfolioAssetTransactionTypeValue => {
  return type === 'Buy' ? PortfolioAssetTransactionTypeValue.Buy : PortfolioAssetTransactionTypeValue.Sell;
};