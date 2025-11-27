/**
 * Типы активов (соответствует C# enum PortfolioAssetType) - для отправки на сервер
 * Share = 1, Bond = 2, Crypto = 3
 */
export const PortfolioAssetTypeValue = {
  Share: 1,
  Bond: 2,
  Crypto: 3,
} as const;

export type PortfolioAssetTypeValue = (typeof PortfolioAssetTypeValue)[keyof typeof PortfolioAssetTypeValue];

/**
 * Типы транзакции (соответствует C# enum PortfolioAssetTransactionType) - для отправки на сервер
 * Buy = 1, Sell = 2
 */
export const PortfolioAssetTransactionTypeValue = {
  Buy: 1,
  Sell: 2,
} as const;

export type PortfolioAssetTransactionTypeValue =
  (typeof PortfolioAssetTransactionTypeValue)[keyof typeof PortfolioAssetTransactionTypeValue];

/**
 * Выходные значения (от сервера) — строки из enum
 */
export const PortfolioAssetTypeLabel = {
  Share: 'Share',
  Bond: 'Bond',
  Crypto: 'Crypto',
} as const;

export type PortfolioAssetTypeLabel = (typeof PortfolioAssetTypeLabel)[keyof typeof PortfolioAssetTypeLabel];

export const PortfolioAssetTransactionTypeLabel = {
  Buy: 'Buy',
  Sell: 'Sell',
} as const;

export type PortfolioAssetTransactionTypeLabel =
  (typeof PortfolioAssetTransactionTypeLabel)[keyof typeof PortfolioAssetTransactionTypeLabel];

/**
 * Тип актива, принимаемый API (как строка из enum PortfolioAssetType)
 * Используется в alertApi, portfolioApi и других
 */
export type ApiAssetType = PortfolioAssetTypeLabel;

/**
 * Краткая информация об активе в портфеле
 */
export type PortfolioAssetShort = {
  id: string;
  portfolioId: string;
  stockCardId: string;
  ticker: string;
  name: string;
  assetType: PortfolioAssetTypeLabel;
  totalQuantity: number;
  averagePurchasePrice: number;
  currency: string;
  transactions?: PortfolioAssetTransaction[];
};

/**
 * Полная информация об активе в портфеле
 */
export type PortfolioAsset = PortfolioAssetShort & {
  description: string;
  lastUpdated: string;
  transactions: PortfolioAssetTransaction[];
};

/**
 * Транзакция по активу
 */
export type PortfolioAssetTransaction = {
  id: string;
  portfolioAssetId: string;
  quantity: number;
  pricePerUnit: number;
  currency: string;
  transactionDate: string;
  transactionType: PortfolioAssetTransactionTypeLabel;
};

/**
 * Запрос на создание актива в портфеле
 */
export type CreatePortfolioAssetRequest = {
  portfolioId: string;
  stockCardId: string;
  assetType: PortfolioAssetTypeValue;
  purchasePricePerUnit: number;
  quantity: number;
};

/**
 * Сводка по доходности портфеля
 */
export type PortfolioProfitLoss = {
  portfolioId: string;
  portfolioName: string;
  totalAbsoluteReturn: number;
  totalPercentageReturn: number;
  totalInvestment: number;
  totalCurrentValue: number;
  baseCurrency: string;
  calculatedAt: string;
  assetBreakdown: Array<{
    assetId: string;
    ticker: string;
    assetName: string;
    absoluteReturn: number;
    percentageReturn: number;
    investmentAmount: number;
    currentValue: number;
    currency: string;
    quantity: number;
    averagePurchasePrice: number;
    currentPrice: number;
    weightInPortfolio: number;
  }>;
};

/**
 * Запрос на создание транзакции
 */
export interface CreateTransactionRequest {
  transactionType: PortfolioAssetTransactionTypeValue;
  quantity: number;
  pricePerUnit: number;
  transactionDate: string;
}

/**
 * Запрос на обновление транзакции
 */
export type UpdateTransactionRequest = {
  transactionType: PortfolioAssetTransactionTypeValue;
  quantity: number;
  pricePerUnit: number;
  transactionDate: string;
  currency: string;
};

/**
 * Доходность одного актива
 */
export type PortfolioAssetProfitLoss = {
  assetId: string;
  portfolioId: string;
  ticker: string;
  assetName: string;
  absoluteReturn: number;
  percentageReturn: number;
  investmentAmount: number;
  currentValue: number;
  currency: string;
  quantity: number;
  averagePurchasePrice: number;
  currentPrice: number;
  calculationType: 'Current' | 'Realized';
};
