/**
 * Условие срабатывания уведомления
 */
export type AlertCondition = 'above' | 'below';

/**
 * Запрос на создание уведомления
 */
export interface CreateAlertRequest {
  assetId: string;
  targetPrice: number;
  condition: AlertCondition;
}

/**
 * Модель уведомления
 */
export interface Alert {
  id: string;
  assetId: string;
  assetName: string;
  targetPrice: number;
  condition: AlertCondition;
  isActive: boolean;
  createdAt: string;
}
