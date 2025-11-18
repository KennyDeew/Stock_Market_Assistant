import axios from 'axios';
import { handleApiError } from './errorHandler';
import type { ShareCard, BondCard, CryptoCard, AssetShort } from '../types/assetTypes';

// Создаём инстансы с полными URL через переменные окружения
const stockApi = axios.create({
  baseURL: import.meta.env.VITE_STOCKCARD_API_URL + '/api/v1/ShareCard',
});

const bondApi = axios.create({
  baseURL: import.meta.env.VITE_STOCKCARD_API_URL + '/api/v1/BondCard',
});

const cryptoApi = axios.create({
  baseURL: import.meta.env.VITE_STOCKCARD_API_URL + '/api/v1/CryptoCard',
});

// Добавляем Authorization-заголовок, если есть токен
const addAuthInterceptor = (instance: any) => {
  instance.interceptors.request.use((config: any) => {
    const storedUser = localStorage.getItem('user');
    if (storedUser) {
      try {
        const { accessToken } = JSON.parse(storedUser);
        if (accessToken) {
          config.headers.Authorization = `Bearer ${accessToken}`;
        }
      } catch (e) {
        console.warn('Failed to read access token');
      }
    }
    return config;
  });
};

[stockApi, bondApi, cryptoApi].forEach(addAuthInterceptor);

export const assetApi = {
  /**
   * Получить все акции
   */
  getAllStocks: async (): Promise<ShareCard[]> => {
    try {
      // Обновляем цены перед загрузкой
      await stockApi.put('/UpdateAllPrices');      
      const response = await stockApi.get<ShareCard[]>('');
      return response.data;
    } catch (error) {
      handleApiError(error);
      throw error;
    }
  },

  /**
   * Получить акцию по тикеру
   */
  getStockByTicker: async (ticker: string): Promise<ShareCard> => {
    try {
      const response = await stockApi.get<ShareCard>(`/${ticker}`);
      return response.data;
    } catch (error) {
      handleApiError(error);
      throw error;
    }
  },

  /**
   * Получить все облигации
   */
  getAllBonds: async (): Promise<BondCard[]> => {
    try {
      // Обновляем цены перед загрузкой
      await bondApi.put('/UpdateAllPrices');      
      const response = await bondApi.get<BondCard[]>('');
      return response.data;
    } catch (error) {
      handleApiError(error);
      throw error;
    }
  },

  /**
   * Получить облигацию по тикеру
   */
  getBondByTicker: async (ticker: string): Promise<BondCard> => {
    try {
      const response = await bondApi.get<BondCard>(`/${ticker}`);
      return response.data;
    } catch (error) {
      handleApiError(error);
      throw error;
    }
  },

  /**
   * Получить все криптовалюты
   */
  getAllCrypto: async (): Promise<CryptoCard[]> => {
    try {
      const response = await cryptoApi.get<CryptoCard[]>('');
      return response.data;
    } catch (error) {
      handleApiError(error);
      throw error;
    }
  },

  /**
   * Получить криптовалюту по тикеру
   */
  getCryptoByTicker: async (ticker: string): Promise<CryptoCard> => {
    try {
      const response = await cryptoApi.get<CryptoCard>(`/${ticker}`);
      return response.data;
    } catch (error) {
      handleApiError(error);
      throw error;
    }
  },

  /**
   * Универсальный метод: поиск и фильтрация активов
   */
   getAll: async ({
    search = '',
    type = '',
    page = 0,
    pageSize = 10,
  }: {
    search?: string;
    type?: string;
    page?: number;
    pageSize?: number;
  }): Promise<{ data: AssetShort[]; total: number }> => {
    try {
      const [stocks, bonds, crypto] = await Promise.all([
        type === '' || type === 'Stock'
          ? assetApi.getAllStocks().catch(err => {
              console.error('Ошибка загрузки акций:', err);
              return [] as ShareCard[];
            })
          : Promise.resolve([]),

        type === '' || type === 'Bond'
          ? assetApi.getAllBonds().catch(err => {
              console.error('Ошибка загрузки облигаций:', err);
              return [] as BondCard[];
            })
          : Promise.resolve([]),

        type === '' || type === 'Crypto'
          ? assetApi.getAllCrypto().catch(err => {
              console.error('Ошибка загрузки крипты:', err);
              return [] as CryptoCard[];
            })
          : Promise.resolve([])
      ]);

      const allAssets: AssetShort[] = [
        // Акции
        ...stocks.map(s => ({
          ticker: s.ticker,
          shortName: s.name || s.ticker,
          currentPrice: Number(s.currentPrice) || 0,
          currency: s.currency || 'RUB',
          changePercent: s.changePercent != null
            ? Number(s.changePercent)
            : null,
          type: 'stock' as const,
          stockCardId: s.id || s.ticker
        })),

        // Облигации
        ...bonds.map(b => ({
          ticker: b.ticker,
          shortName: b.name || b.ticker,
          currentPrice: Number(b.currentPrice) || 0,
          currency: b.currency || 'USD',
          changePercent: null,
          type: 'bond' as const,
          stockCardId: b.id || b.ticker
        })),

        // Криптовалюты
        ...crypto.map(c => ({
          ticker: c.ticker,
          shortName: c.name || c.ticker,
          currentPrice: Number(c.currentPrice) || 0,
          currency: c.currency || 'USD',
          changePercent: Number(c.priceChange24h) || 0,
          type: 'crypto' as const,
          stockCardId: c.id || c.ticker
        }))
      ];

      const filtered = allAssets.filter(asset =>
        asset.ticker.toLowerCase().includes(search.toLowerCase()) ||
        asset.shortName.toLowerCase().includes(search.toLowerCase())
      );

      const start = page * pageSize;
      const data = filtered.slice(start, start + pageSize);

      return { data, total: filtered.length };
    } catch (error) {
      console.error('Критическая ошибка в getAll:', error);
      handleApiError(error);
      throw error;
    }
  },
};
