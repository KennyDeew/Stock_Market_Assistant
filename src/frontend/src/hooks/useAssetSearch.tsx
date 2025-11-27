import { useState, useCallback, useRef } from 'react';
import { useDebounce } from './useDebounce';
import { assetApi } from '../services/assetApi';
import type { AssetShort } from '../types/assetTypes';

interface UseAssetSearchResult {
  assets: AssetShort[];
  loading: boolean;
  error: string | null;
  searchAssets: (query: string, type?: string) => void;
  loadAssetsImmediately: (query: string, type?: string) => void;
}

export const useAssetSearch = (): UseAssetSearchResult => {
  const [assets, setAssets] = useState<AssetShort[]>([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const latestQueryRef = useRef<string>('');

  const loadAssets = useCallback(async (query: string, type?: string) => {
    const trimmedQuery = query.trim();

    if (!trimmedQuery) {
      setAssets([]);
      setError(null);
      setLoading(false);
      return;
    }

    console.log('🔍 Поиск активов:', { query: trimmedQuery, type });
    latestQueryRef.current = trimmedQuery;
    setLoading(true);
    setError(null);

    try {
      const response = await assetApi.getAll({
        search: trimmedQuery,
        type,
        page: 0,
        pageSize: 20,
      });

      // Проверяем, не устарел ли запрос
      if (latestQueryRef.current !== trimmedQuery) {
        console.log(`❌ Игнорируем устаревший ответ для "${trimmedQuery}"`);
        return;
      }

      console.log('✅ Получены активы:', response.data.length);
      setAssets(response.data); // 🔥 Убедитесь, что это выполняется
    } catch (err: any) {
      console.error('Ошибка загрузки активов', err);
      const message = err.message || 'Не удалось загрузить активы';
      setError(message);
      if (latestQueryRef.current === trimmedQuery) {
        setAssets([]);
      }
    } finally {
      if (latestQueryRef.current === trimmedQuery) {
        setLoading(false);
      }
    }
  }, []);

  const debouncedSearch = useDebounce(loadAssets, 300);

  return {
    assets,
    loading,
    error,
    // Обёртка: чтобы не передавать пустой query напрямую в debouncedSearch
    searchAssets: useCallback(
      (query: string, type?: string) => {
        if (query.trim()) {
          debouncedSearch(query, type);
        } else {
          setAssets([]);
          setError(null);
          setLoading(false);
        }
      },
      [debouncedSearch]
    ),
    loadAssetsImmediately: loadAssets,
  };
};