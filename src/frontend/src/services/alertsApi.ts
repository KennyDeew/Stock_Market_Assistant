import { createPrivateApiClient } from './apiClient';
import type { CreateAlertRequest, AlertResponse } from '../types/alertTypes';

// Создаём приватный API-клиент
const api = createPrivateApiClient('/api/v1/alerts');

/**
 * API для работы с уведомлениями
 */
export const alertsApi = {
  /**
   * Получить все уведомления
   */
  getAll: async (): Promise<AlertResponse[]> => {
    const response = await api.get<AlertResponse[]>('');
    return response.data;
  },

  /**
   * Создать новое уведомление
   */
  create: async (data: CreateAlertRequest): Promise<AlertResponse> => {
    const response = await api.post<AlertResponse>('', data);
    return response.data;
  },

  /**
   * Удалить уведомление
   */
  delete: async (id: string): Promise<void> => {
    await api.delete(`/${id}`); // DELETE ничего не возвращает
  },
};
