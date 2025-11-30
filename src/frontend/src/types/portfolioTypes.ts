import type { PortfolioAssetShort } from "./portfolioAssetTypes";

export type CreatePortfolioRequest = {
  name: string;
  userId: string;
  currency?: string;
  isPrivate?: boolean;
};

export type UpdatePortfolioRequest = {
  name: string;
  currency?: string;
  isPrivate?: boolean;
};

export type PortfolioShort = {
  id: string;
  userId: string;
  name: string;
  currency: string;
  isPrivate: boolean;
};

export type PortfolioResponse = {
  id: string;
  userId: string;
  name: string;
  currency: string;
  isPrivate: boolean;
  assets: PortfolioAssetShort[];
};

