import axios, { type AxiosInstance } from 'axios';
import { handleApiError } from './errorHandler';
import type {
  PortfolioShort,
  CreatePortfolioRequest,
  UpdatePortfolioRequest,
  PortfolioResponse,
} from '../types/portfolioTypes';
import type { PaginatedResponse } from '../types/paginationTypes';
import type {
  CreatePortfolioAssetRequest,
  CreateTransactionRequest,
  PortfolioAsset,
  PortfolioAssetProfitLoss,
  PortfolioAssetShort,
  PortfolioAssetTransaction,
  PortfolioProfitLoss,
  UpdateTransactionRequest,
} from '../types/portfolioAssetTypes';

// 🔹 Axios инстанс для /portfolios
const portfolioApiInstance = axios.create({
  baseURL: import.meta.env.VITE_PORTFOLIO_API_URL + '/api/v1/portfolios',
});

// 🔹 Отдельный инстанс для /portfolio-assets
const portfolioAssetsApi = axios.create({
  baseURL: import.meta.env.VITE_PORTFOLIO_API_URL + '/api/v1/portfolio-assets',
});

// 🔹 Общая функция для настройки интерсепторов
const setupInterceptors = (instance: AxiosInstance) => {
  // Добавляем Authorization
  instance.interceptors.request.use(
    (config) => {
      const storedUser = localStorage.getItem('user');
      if (storedUser) {
        try {
          const { accessToken } = JSON.parse(storedUser);
          if (accessToken) {
            config.headers.Authorization = `Bearer ${accessToken}`;
          }
        } catch (e) {
          console.error('Failed to parse stored user');
        }
      }
      return config;
    },
    (error) => {
      return Promise.reject(error);
    }
  );

  // Обработка ошибок
  instance.interceptors.response.use(
    (response) => response,
    (error) => {
      handleApiError(error);
      return Promise.reject(error);
    }
  );
};

// Настраиваем оба инстанса
setupInterceptors(portfolioApiInstance);
setupInterceptors(portfolioAssetsApi);

// 🔹 Основные методы портфеля
export const portfolioApi = {
  /**
   * Получить все портфели пользователя
   */
  getAll: async (
    userId: string,
    page: number = 1,
    pageSize: number = 10
  ): Promise<PaginatedResponse<PortfolioShort>> => {
    const response = await portfolioApiInstance.get<PaginatedResponse<PortfolioShort>>(
      `/user/${userId}`,
      {
        params: { page, pageSize },
      }
    );
    return response.data;
  },

  /**
   * Получить портфель по ID
   */
  getById: async (id: string): Promise<PortfolioResponse> => {
    const response = await portfolioApiInstance.get<PortfolioResponse>(`/${id}`);
    return response.data;
  },

  /**
   * Создать новый портфель
   */
  create: async (data: CreatePortfolioRequest): Promise<PortfolioShort> => {
    const response = await portfolioApiInstance.post<PortfolioShort>('', data);
    return response.data;
  },

  /**
   * Обновить портфель
   */
  update: async (id: string, data: UpdatePortfolioRequest): Promise<void> => {
    await portfolioApiInstance.put(`/${id}`, data);
  },

  /**
   * Удалить портфель
   */
  delete: async (id: string): Promise<void> => {
    await portfolioApiInstance.delete(`/${id}`);
  },

  /**
   * Расчёт доходности портфеля
   */
  getPortfolioProfitLoss: async (
    id: string,
    calculationType: 'Current' | 'Realized' = 'Current'
  ): Promise<PortfolioProfitLoss> => {
    const response = await portfolioApiInstance.get<PortfolioProfitLoss>(
      `/${id}/profit-loss`,
      {
        params: { calculationType },
      }
    );
    return response.data;
  },
};

// 🔹 Методы для активов портфеля
export const portfolioAssetApi = {
  /**
   * Создать актив в портфеле
   */
  create: async (data: CreatePortfolioAssetRequest): Promise<PortfolioAssetShort> => {
    const response = await portfolioAssetsApi.post<PortfolioAssetShort>('', data);
    return response.data;
  },

  /**
   * Получить актив по ID
   */
  getById: async (id: string): Promise<PortfolioAsset> => {
    const response = await portfolioAssetsApi.get<PortfolioAsset>(`/${id}`);
    return response.data;
  },

  /**
   * Получить все активы портфеля
   */
  getAll: async (portfolioId: string): Promise<PortfolioAssetShort[]> => {
    const response = await portfolioAssetsApi.get<PortfolioAssetShort[]>('', {
      params: { portfolioId },
    });
    return response.data;
  },

  /**
   * Удалить актив
   */
  delete: async (id: string): Promise<void> => {
    await portfolioAssetsApi.delete(`/${id}`);
  },

  /**
   * Получить транзакции актива
   */
  getTransactions: async (id: string): Promise<PortfolioAssetTransaction[]> => {
    const response = await portfolioAssetsApi.get<PortfolioAssetTransaction[]>(
      `/${id}/transactions`
    );
    return response.data;
  },

  /**
   * Добавить транзакцию к активу
   */
  addTransaction: async (
    assetId: string,
    data: CreateTransactionRequest
  ): Promise<void> => {
    await portfolioAssetsApi.post(`/${assetId}/transactions`, data);
  },

  /**
   * Обновить транзакцию
   */
  updateTransaction: async (
    assetId: string,
    transactionId: string,
    data: UpdateTransactionRequest
  ): Promise<void> => {
    await portfolioAssetsApi.put(`/${assetId}/transactions/${transactionId}`, data);
  },

  /**
   * Удалить транзакцию
   */
  deleteTransaction: async (assetId: string, transactionId: string): Promise<void> => {
    await portfolioAssetsApi.delete(`/${assetId}/transactions/${transactionId}`);
  },

  /**
   * Расчёт доходности актива
   */
  getProfitLoss: async (
    assetId: string,
    calculationType: 'Current' | 'Realized' = 'Current'
  ): Promise<PortfolioAssetProfitLoss> => {
    const response = await portfolioAssetsApi.get<PortfolioAssetProfitLoss>(
      `/${assetId}/profit-loss`,
      {
        params: { calculationType },
      }
    );
    return response.data;
  },

  /**
   * Текущая доходность актива
   */
  getCurrentProfitLoss: async (assetId: string): Promise<PortfolioAssetProfitLoss> => {
    const response = await portfolioAssetsApi.get<PortfolioAssetProfitLoss>(
      `/${assetId}/current-profit-loss`
    );
    return response.data;
  },

  /**
   * Реализованная доходность актива
   */
  getRealizedProfitLoss: async (assetId: string): Promise<PortfolioAssetProfitLoss> => {
    const response = await portfolioAssetsApi.get<PortfolioAssetProfitLoss>(
      `/${assetId}/realized-profit-loss`
    );
    return response.data;
  },
};
