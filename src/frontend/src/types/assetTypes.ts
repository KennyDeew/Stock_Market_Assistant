/**
 * Краткая информация об активе (для списков, автозаполнения)
 */
export type AssetShort = {
  ticker: string;
  shortName: string;
  currentPrice: number;
  currency: string;
  changePercent?: number | null;
  type: 'stock' | 'bond' | 'crypto';
  stockCardId: string;
};


/**
 * Модель акции
 */
export type ShareCard = {
  id: string;
  ticker: string;
  name: string;
  board?: string;
  description?: string;
  currency: string;
  currentPrice: number;
  changePercent?: number | null;
};

/**
 * Модель облигации
 */
export type BondCard = {
  id: string;
  ticker: string;
  name: string;
  board: string;
  description?: string;
  maturityPeriod: string;
  currency: string;
  rating: string;
  faceValue: number;
  currentPrice: number;
};

/**
 * Модель криптовалюты
 */
export type CryptoCard = {
  id: string;
  ticker: string;
  name: string;
  currentPrice: number;
  currency: string;
  priceChange24h: number;
};