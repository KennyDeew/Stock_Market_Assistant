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
    const isImmediate = trimmedQuery === '' && query === ''; // Разрешаем пустой запрос для `loadAssetsImmediately`

    // Только `searchAssets` должен блокировать пустые запросы
    // `loadAssetsImmediately` — может загружать всё
    if (!trimmedQuery && !isImmediate) {
      setAssets([]);
      setError(null);
      setLoading(false);
      return;
    }

    console.log('🔍 Поиск активов:', { query: trimmedQuery || '(все)', type });
    latestQueryRef.current = query;
    setLoading(true);
    setError(null);

    try {
      const response = await assetApi.getAll({
        search: trimmedQuery,
        type,
        page: 0,
        pageSize: 1000,
      });

      if (latestQueryRef.current !== query) return;

      setAssets(response.data);
    } catch (err: unknown) {
      const message = (err as Error).message || 'Не удалось загрузить активы';
      setError(message);
      if (latestQueryRef.current === query) {
        setAssets([]);
      }
    } finally {
      if (latestQueryRef.current === query) {
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
        const trimmed = query.trim();
        if (trimmed) {
          debouncedSearch(trimmed, type);
        } else {
          setAssets([]);
          setError(null);
        }
      },
      [debouncedSearch]
    ),
    loadAssetsImmediately: loadAssets,
  };
};