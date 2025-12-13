import { createPrivateApiClient } from './apiClient'; // Приватный клиент
import type {
  TopAssetsResponseDto,
  TransactionsListResponseDto,
} from '../types/analyticsTypes';

// Инстанс для /analytics
const analyticsApi = createPrivateApiClient(import.meta.env.VITE_ANALYTICS_API_URL + '/api/analytics');

analyticsApi.interceptors.request.use(
  (config) => {
    const token = localStorage.getItem('authToken'); // Или ваш способ получения токена
    console.log('Отправляем запрос с токеном:', token ? 'Токен есть' : 'Токена нет'); // <-- Добавьте эту строку
    if (token) {
      config.headers.Authorization = `Bearer ${token}`;
    }
    return config;
  },
  (error) => {
    return Promise.reject(error);
  }
);

export const analyticsApiService = {
  /**
   * Получить топ активов по покупкам
   */
  getTopBought: async (
    top: number = 5,
    startDate: string,
    endDate: string,
    context: 'Global' | 'Portfolio' = 'Global',
    portfolioId?: string
  ): Promise<TopAssetsResponseDto> => {
    try {
      const params = new URLSearchParams({
        top: top.toString(),
        startDate,
        endDate,
        context,
        ...(portfolioId && { portfolioId }),
      });

      const response = await analyticsApi.get<TopAssetsResponseDto>(`/assets/top-bought?${params}`);
      return response.data;
    } catch (error) {
      console.error('Ошибка при получении топа покупок:', error);
      throw error;
    }
  },

  /**
   * Получить топ активов по продажам
   */
  getTopSold: async (
    top: number = 5,
    startDate: string,
    endDate: string,
    context: 'Global' | 'Portfolio' = 'Global',
    portfolioId?: string
  ): Promise<TopAssetsResponseDto> => {
    try {
      const params = new URLSearchParams({
        top: top.toString(),
        startDate,
        endDate,
        context,
        ...(portfolioId && { portfolioId }),
      });

      const response = await analyticsApi.get<TopAssetsResponseDto>(`/assets/top-sold?${params}`);
      return response.data;
    } catch (error) {
      console.error('Ошибка при получении топа продаж:', error);
      throw error;
    }
  },

  /**
   * Получить все транзакции за период
   */
  getAllTransactions: async (
    periodType: 'Today' | 'Week' | 'Month' | 'Custom' = 'Week',
    transactionType?: 'Buy' | 'Sell',
    startDate?: string,
    endDate?: string
  ): Promise<TransactionsListResponseDto> => {
    try {
      const params = new URLSearchParams({
        periodType,
      });

      if (transactionType) params.append('transactionType', transactionType);
      if (startDate) params.append('startDate', startDate);
      if (endDate) params.append('endDate', endDate);

      const response = await analyticsApi.get<TransactionsListResponseDto>(`/transactions?${params}`);
      return response.data;
    } catch (error) {
      console.error('Ошибка при получении транзакций:', error);
      throw error;
    }
  },

  /**
   * Универсальный метод: получить топ активов по типу
   */
  getTopAssets: async (
    type: 'bought' | 'sold',
    options: {
      top?: number;
      startDate: string;
      endDate: string;
      context?: 'Global' | 'Portfolio';
      portfolioId?: string;
    }
  ) => {
    return type === 'bought'
      ? await analyticsApiService.getTopBought(
          options.top ?? 5,
          options.startDate,
          options.endDate,
          options.context ?? 'Global',
          options.portfolioId
        )
      : await analyticsApiService.getTopSold(
          options.top ?? 5,
          options.startDate,
          options.endDate,
          options.context ?? 'Global',
          options.portfolioId
        );
  },
};