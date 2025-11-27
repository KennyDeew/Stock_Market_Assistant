import type { ApiAssetType } from "./portfolioAssetTypes";

/**
 * Условие срабатывания уведомления
 */
export type AlertCondition = 'Above' | 'Below';

/**
 * Запрос на создание уведомления
 */
export interface CreateAlertRequest {
  stockCardId: string;
  assetType: ApiAssetType;
  assetTicker: string;
  assetName: string;
  targetPrice: number;
  assetCurrency: string;
  condition: AlertCondition;
}

/**
 * Ответ API — уведомление с дополнительными метаданными
 */
export interface AlertResponse {
  id: string;
  stockCardId: string;
  ticker: string;
  assetName: string;
  assetType: 'stock' | 'bond' | 'crypto'; // UI-формат
  targetPrice: number;
  assetCurrency: string; 
  condition: AlertCondition;
  isActive: boolean;
  createdAt: string;
  updatedAt?: string;
  triggeredAt?: string;
  userId: string;
  lastChecked?: string;
  isTriggered?: boolean;
}

// Алиас
export type Alert = Omit<AlertResponse, 'userId' | 'lastChecked' | 'isTriggered'>;
