export interface AssetRatingDto {
  id: string;
  stockCardId: string;
  assetType: number;
  ticker: string;
  name: string;
  buyTransactionCount: number;
  sellTransactionCount: number;
  totalBuyAmount: number;
  totalSellAmount: number;
  transactionCountRank: number;
  transactionAmountRank: number;
  lastUpdated: string;
}

export interface TransactionResponseDto {
  id: string;
  portfolioId: string;
  stockCardId: string;
  assetType: number;
  transactionType: 'Buy' | 'Sell';
  quantity: number;
  pricePerUnit: number;
  totalAmount: number;
  transactionTime: string;
  currency: string;
}

export interface TopAssetsResponseDto {
  assets: AssetRatingDto[];
  startDate: string;
  endDate: string;
  context: number;
  top: number;
}

export interface TransactionsListResponseDto {
  transactions: TransactionResponseDto[];
  totalCount: number;
  startDate: string;
  endDate: string;
  filteredTransactionType: 'Buy' | 'Sell' | null;
}
