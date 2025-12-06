import { AxiosError } from 'axios';
import type { ApiError } from '../types/errorTypes';

// Кастомный формат бэкенда
export interface BackendError {
  result: null;
  errors: Array<{
    code: string;
    message: string;
    type: number;
    invalidField: string | null;
  }>;
  timeGenerated: string;
}

// Карта локализации кодов ошибок
const ERROR_MESSAGES: Record<string, string> = {
  'PasswordRequiresDigit': 'Пароль должен содержать хотя бы одну цифру (0-9).',
  'PasswordRequiresLower': 'Пароль должен содержать хотя бы одну строчную букву (a-z).',
  'PasswordRequiresUpper': 'Пароль должен содержать хотя бы одну заглавную букву (A-Z).',
  'PasswordRequiresNonAlphanumeric': 'Пароль должен содержать хотя бы один спецсимвол (!, @, #, $ и т.д.).',
  'PasswordTooShort': 'Пароль слишком короткий. Минимум 6 символов.',
  'EmailAlreadyExists': 'Этот email уже зарегистрирован.',
  'EmailNotFound': 'Пользователь с таким email не найден.',
  'InvalidCredentials': 'Неверный email или пароль.',
  'core.operation.execute.failure': 'Произошла ошибка при обработке запроса. Проверьте данные.',
  // Добавь другие по мере появления
};

/**
 * Извлекает человеко-читаемое сообщение из ошибки
 */
export const getErrorMessage = (error: unknown): string => {
  if (error instanceof AxiosError) {
    const response = error.response;
    const data = response?.data;

    // 1. Кастомный формат бэкенда: { result: null, errors: [...] }
    if (data && typeof data === 'object' && 'errors' in data) {
      const errors = (data as BackendError).errors;
      if (Array.isArray(errors)) {
        return errors
          .map(e => ERROR_MESSAGES[e.code] || e.message || 'Ошибка ввода')
          .join('\n');
      }
    }

    // 2. Стандартный ApiError
    if (data && typeof data === 'object' && 'message' in data) {
      const apiError = data as ApiError;
      const message = apiError.message || 'Неизвестная ошибка';
      return message;
    }

    // 3. Problem Details (ASP.NET Core)
    if (data && typeof data === 'object' && ('title' in data || 'detail' in data)) {
      const detail = (data as { detail?: string }).detail;
      const title = (data as { title?: string }).title;
      return detail || title || 'Ошибка сервера';
    }

    // 4. Статус-коды
    if (response?.status === 401) {
      return 'Сессия истекла. Войдите снова.';
    }
    if (response?.status === 409) {
      return 'Email уже занят.';
    }
    if (response?.status === 404) {
      return 'Ресурс не найден.';
    }
    if (response?.status === 500) {
      return 'Ошибка сервера. Попробуйте позже.';
    }
    if (error.code === 'ERR_NETWORK') {
      return 'Нет подключения к серверу. Проверьте интернет.';
    }
  }

  return 'Произошла ошибка. Попробуйте позже.';
};

/**
 * Централизованная обработка ошибок API (логирование + возврат сообщения)
 */
export const handleApiError = (error: unknown): string => {
  const message = getErrorMessage(error);

  if (error instanceof AxiosError) {
    const { status } = error.response || {};

    switch (status) {
      case 401:
        console.warn('Сессия истекла. Выполняется выход...', message);
        localStorage.removeItem('user');
        window.location.href = '/login';
        break;
      case 403:
        console.error('Доступ запрещён:', message);
        break;
      case 404:
        console.error('Ресурс не найден:', error.config?.url);
        break;
      case 400:
      case 409:
        console.warn('Ошибка ввода:', message);
        break;
      default:
        if (status && status >= 500) {
          console.error(`Ошибка сервера [${status}]:`, message);
        } else {
          console.error(`Ошибка API [${status}]:`, message);
        }
    }
  } else {
    console.error('Неизвестная ошибка:', error);
  }

  return message;
};