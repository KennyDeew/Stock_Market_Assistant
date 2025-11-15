import type { PortfolioAssetShort } from "./portfolioAssetTypes";

export type CreatePortfolioRequest = {
  name: string;
  userId: string;
  currency?: string;
};

export type UpdatePortfolioRequest = {
  name: string;
  currency?: string;
};

export type PortfolioShort = {
  id: string;
  userId: string;
  name: string;
  currency: string;  
};

export type PortfolioResponse = {
  id: string;
  userId: string;
  name: string;
  currency: string;
  assets: PortfolioAssetShort[];
};

