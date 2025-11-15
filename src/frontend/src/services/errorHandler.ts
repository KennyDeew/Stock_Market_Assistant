import { AxiosError } from 'axios';
import type { ApiError } from '../types/errorTypes';

/**
 * Централизованная обработка ошибок API
 */
export const handleApiError = (error: unknown) => {
  // Проверяем, является ли ошибка Axios-ошибкой
  if (error instanceof AxiosError) {
    const axiosError = error as AxiosError<{ message?: string } | ApiError | undefined>;

    if (axiosError.response) {
      const { status, data, statusText } = axiosError.response;

      // Извлекаем message
      const message =
        data && typeof data === 'object' && 'message' in data
          ? (data as { message?: string }).message
          : data?.toString() || statusText;

      switch (status) {
        case 401:
          console.warn('Сессия истекла. Выполняется выход...');
          localStorage.removeItem('user');
          window.location.href = '/login';
          break;
        case 403:
          console.error('Доступ запрещён:', message);
          break;
        case 404:
          console.error('Ресурс не найден:', axiosError.config?.url);
          break;
        default:
          console.error(`Ошибка API [${status}]:`, message);
      }
    } else if (axiosError.request) {
      console.error('Нет ответа от сервера. Проверьте подключение.');
    } else {
      console.error('Ошибка при настройке запроса:', axiosError.message);
    }
  } else {
    console.error('Неизвестная ошибка:', error);
  }
};