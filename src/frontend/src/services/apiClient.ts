import axios, { type AxiosInstance } from 'axios';
import { refreshTokens } from './authApi';
import { handleApiError } from './errorHandler';

/**
 * Создаёт инстанс с авторизацией и авто-обновлением токенов
 * Для приватных API
 */
export const createPrivateApiClient = (baseURL: string): AxiosInstance => {
  const instance = axios.create({
    baseURL,
    headers: { 'Content-Type': 'application/json' },
  });

  // Добавляем токен
  instance.interceptors.request.use(
    (config) => {
      const stored = localStorage.getItem('user');
      if (stored) {
        const { accessToken } = JSON.parse(stored);
        config.headers.Authorization = `Bearer ${accessToken}`;
      }
      return config;
    },
    (error) => Promise.reject(error)
  );

  // Обработка 401 → обновление токена
  instance.interceptors.response.use(
    (response) => response,
    async (error) => {
      const originalRequest = error.config;

      if (error.response?.status === 401 && !originalRequest?.__retry) {
        originalRequest.__retry = true;

        try {
          const stored = localStorage.getItem('user');
          if (!stored) throw new Error('No refresh token');
          const { refreshToken } = JSON.parse(stored);

          const { accessToken: newAccessToken, refreshToken: newRefreshToken } = await refreshTokens(refreshToken);

          const userData = { ...JSON.parse(stored), accessToken: newAccessToken, refreshToken: newRefreshToken };
          localStorage.setItem('user', JSON.stringify(userData));

          if (originalRequest.headers) {
            originalRequest.headers.Authorization = `Bearer ${newAccessToken}`;
          }

          return instance(originalRequest);
        } catch (refreshError) {
          localStorage.removeItem('user');
          window.location.href = '/login';
          return Promise.reject(refreshError);
        }
      }

      return handleApiError(error);
    }
  );

  return instance;
};

/**
 * Создаёт инстанс без авторизации
 * Для публичных API (например, assetApi)
 */
export const createPublicApiClient = (baseURL: string): AxiosInstance => {
  const instance = axios.create({
    baseURL,
    headers: { 'Content-Type': 'application/json' },
  });

  // Никаких заголовков авторизации
  // Просто обработка ошибок
  instance.interceptors.response.use(
    (response) => response,
    (error) => handleApiError(error)
  );

  return instance;
};
