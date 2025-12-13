import { useEffect, useRef } from 'react';

function useDebounce<F extends (query: string, type?: string) => void | Promise<void>>(
  callback: F,
  delay: number
): F;

function useDebounce<F extends () => void | Promise<void>>(callback: F, delay: number): F;

function useDebounce(callback: (...args: unknown[]) => void | Promise<void>, delay: number) {
  const timerRef = useRef<ReturnType<typeof setTimeout> | null>(null);

  useEffect(() => {
    return () => {
      if (timerRef.current) clearTimeout(timerRef.current);
    };
  }, []);

  const debounced = (...args: unknown[]): void => {
    if (timerRef.current) clearTimeout(timerRef.current);
    timerRef.current = setTimeout(() => {
      callback(...args);
    }, delay);
  };

  return debounced;
}

export { useDebounce };
