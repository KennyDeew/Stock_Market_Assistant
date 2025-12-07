import axios from 'axios';
import { handleApiError } from './errorHandler';

/**
 * Создаём инстанс для account API
 */
const accountApi = axios.create({
  baseURL: import.meta.env.VITE_AUTH_API_URL + '/api/v1/account',
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

// ========== Интерфейсы ==========
export interface DeleteProfileRequest {
  password: string;
}

export interface ChangePasswordRequest {
  oldPassword: string;
  newPassword: string;
}

export interface UpdateProfileRequest {
  fullName: string;
  description?: string;
}

export interface ChangeEmailRequest {
  newEmail: string;
  password: string;
}

// ========== API-методы ==========

/**
 * Удаление аккаунта
 * @param password Пароль пользователя для подтверждения
 */
export const deleteAccount = async (password: string): Promise<void> => {
  try {
    await accountApi.delete('/profile', {
      data: { password }, // ← важно: тело в `delete` передаётся через `data`
    });
  } catch (error) {
    handleApiError(error); // ← единая обработка ошибок
    throw error; // ← пробрасываем дальше
  }
};

/**
 * Смена пароля
 */
export const changePassword = async (oldPassword: string, newPassword: string): Promise<void> => {
  try {
    await accountApi.put('/password', { oldPassword, newPassword });
  } catch (error) {
    handleApiError(error);
    throw error;
  }
};

/**
 * Обновление профиля (ФИО, описание)
 */
export const updateProfile = async (fullName: string, description?: string): Promise<void> => {
  try {
    await accountApi.patch('/profile', { fullName, description });
  } catch (error) {
    handleApiError(error);
    throw error;
  }
};

/**
 * Смена email
 */
export const changeEmail = async (newEmail: string, password: string): Promise<void> => {
  try {
    await accountApi.put('/email', { newEmail, password });
  } catch (error) {
    handleApiError(error);
    throw error;
  }
};
