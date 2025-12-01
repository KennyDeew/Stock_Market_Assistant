import type { PortfolioAssetShort } from "./portfolioAssetTypes";

/**
 * Запрос на создание портфеля
 */
export type CreatePortfolioRequest = {
  name: string;
  userId: string;
  currency?: string;
  isPrivate?: boolean;
};

/**
 * Запрос на обновление портфеля
 */
export type UpdatePortfolioRequest = {
  name: string;
  currency?: string;
  isPrivate?: boolean;
};

/**
 * Краткая информация о портфеле (в списке)
 */
export type PortfolioShort = {
  id: string;
  userId: string;
  name: string;
  currency: string;
  isPrivate: boolean;
};

/**
 * Полная информация о портфеле
 */
export type PortfolioResponse = {
  id: string;
  userId: string;
  name: string;
  currency: string;
  isPrivate: boolean;
  assets: PortfolioAssetShort[];
};

/**
 * Ответ с расчётом доходности портфеля
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
  assetBreakdown: PortfolioAssetProfitLossItem[];
};

/**
 * Элемент детализации доходности по активам
 */
export type PortfolioAssetProfitLossItem = {
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
};
