import axios from 'axios';
import { handleApiError } from './errorHandler';

const accountApi = axios.create({
  baseURL: '/api/v1/account',
  headers: {
    'Content-Type': 'application/json',
  },
});

// Перехватчик запроса — подставляет Bearer-токен
accountApi.interceptors.request.use(
  (config) => {
    const storedUser = localStorage.getItem('user');
    if (storedUser) {
      const { accessToken } = JSON.parse(storedUser);
      if (accessToken) {
        config.headers.Authorization = `Bearer ${accessToken}`;
      }
    }
    return config;
  },
  (error) => {
    return Promise.reject(error);
  }
);

// Перехват ошибок
accountApi.interceptors.response.use(
  (response) => response,
  (error) => {
    handleApiError(error);
    return Promise.reject(error);
  }
);

/**
 * Удаление аккаунта (по access token)
 */
export const deleteAccount = async (): Promise<void> => {
  try {
    await accountApi.delete('/account');
  } catch (error: any) {
    if (error.response?.status === 401) {
      throw new Error('Не авторизован');
    }
    if (error.response?.data?.message) {
      throw new Error(error.response.data.message);
    }
    throw new Error('Не удалось удалить аккаунт');
  }
};
