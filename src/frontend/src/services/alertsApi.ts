import axios from 'axios';
import type { CreateAlertRequest, AlertResponse } from '../types/alertTypes';

const api = axios.create({
  baseURL: import.meta.env.VITE_API_URL + '/api/v1/alerts',
});

api.interceptors.request.use((config) => {
  const storedUser = localStorage.getItem('user');
  if (storedUser) {
    try {
      const { accessToken } = JSON.parse(storedUser);
      if (accessToken) {
        config.headers.Authorization = `Bearer ${accessToken}`;
      }
    } catch (e) {
      console.error('Failed to parse stored user');
    }
  }
  return config;
});

export const alertsApi = {
  getAll: (): Promise<{ data: AlertResponse[] }> => api.get(''),
  create: (data: CreateAlertRequest) => api.post<AlertResponse>('', data),
  delete: (id: string) => api.delete(`/${id}`),
};
