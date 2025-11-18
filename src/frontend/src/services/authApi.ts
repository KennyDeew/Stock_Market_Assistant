import axios from 'axios';
import { handleApiError } from './errorHandler';
import type {
  LoginRequest,
  RegisterRequest,
  AuthResponse,
  CheckEmailResponse,
} from '../types/authTypes';

/**
 * Создаём отдельный инстанс для auth API
 */
const authApi = axios.create({
  baseURL: import.meta.env.VITE_AUTH_API_URL + '/api/v1/auth',
  headers: {
    'Content-Type': 'application/json',
  },
});

// Перехват ошибок
authApi.interceptors.response.use(
  (response) => response,
  (error) => {
    handleApiError(error);
    return Promise.reject(error);
  }
);

/**
 * Проверка, занят ли email
 */
export const checkEmail = async (email: string): Promise<boolean> => {
  try {
    const response = await authApi.post<CheckEmailResponse>('/check-email', { email });
    return !response.data.exists; // true = доступен
  } catch (error: any) {
    if (error.response?.status === 409) {
      return false;
    }
    console.error('Ошибка при проверке email:', error);
    throw error;
  }
};

/**
 * Регистрация пользователя
 */
export const register = async (data: RegisterRequest): Promise<AuthResponse> => {
  try {
    const response = await authApi.post<AuthResponse>('/register', data);
    return response.data;
  } catch (error) {
    handleApiError(error);
    throw error;
  }
};

/**
 * Вход пользователя
 */
export const login = async (data: LoginRequest): Promise<AuthResponse> => {
  try {
    const response = await authApi.post<AuthResponse>('/login', data);
    return response.data;
  } catch (error) {
    handleApiError(error);
    throw error;
  }
};

/**
 * Выход: инвалидация refreshToken (опционально)
 */
export const logout = async (refreshToken: string): Promise<void> => {
  try {
    await authApi.post('/logout', { refreshToken });
  } catch (error) {
    console.warn('Logout failed (optional):', error);
  }
};

/**
 * Обновление токенов
 */
export const refreshTokens = async (refreshToken: string): Promise<AuthResponse> => {
  try {
    const response = await authApi.post<AuthResponse>('/refresh', { refreshToken });
    return response.data;
  } catch (error) {
    handleApiError(error);
    throw error;
  }
};
