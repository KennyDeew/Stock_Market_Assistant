import { useState, useCallback } from 'react';
import { useDebounce } from './useDebounce';
import { assetApi } from '../services/assetApi';
import type { AssetShort } from '../types/assetTypes';

// 🔹 Определяем тип возвращаемого значения
interface UseAssetSearchResult {
  assets: AssetShort[];
  loading: boolean;
  error: string | null;
  searchAssets: (query: string, type?: string) => void; // дебаунс-поиск
  loadAssetsImmediately: (query: string, type?: string) => void; // прямой вызов
}

export const useAssetSearch = (): UseAssetSearchResult => {
  const [assets, setAssets] = useState<AssetShort[]>([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const loadAssets = useCallback(async (query: string, type?: string) => {
    setLoading(true);
    setError(null);

    try {
      const response = await assetApi.getAll({
        search: query,
        type: type,
        page: 0,
        pageSize: 20,
      });

      setAssets(response.data);
    } catch (err) {
      console.error('Ошибка загрузки активов', err);
      setError('Не удалось загрузить активы');
      setAssets([]);
    } finally {
      setLoading(false);
    }
  }, []);

  const debouncedSearch = useDebounce(loadAssets, 300);

  return {
    assets,
    loading,
    error,
    searchAssets: debouncedSearch,
    loadAssetsImmediately: loadAssets, // ✅ Передаём оригинальную функцию без дебаунса
  };
};
