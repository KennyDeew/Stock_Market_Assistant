import { createContext, useContext } from 'react';
import type { AuthContextType } from '../types/authTypes';

// Создаём контекст
export const AuthContext = createContext<AuthContextType | undefined>(undefined);

// Хук для использования аутентификации
export const useAuth = () => {
  const context = useContext(AuthContext);
  if (!context) {
    throw new Error('useAuth must be used within an AuthProvider');
  }
  return context;
};
