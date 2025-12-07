import { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { parseJwt } from '../utils/jwt';
import type { UserType } from '../types/authTypes';
import {
  login as apiLogin,
  register as apiRegister,
  checkEmail as apiCheckEmail,
  logout as apiLogout,
  refreshTokens,
} from '../services/authApi';
import { handleApiError } from '../services/errorHandler';
import { AuthContext } from '../hooks/useAuth';

export const AuthProvider = ({ children }: { children: React.ReactNode }) => {
  const navigate = useNavigate();
  const [isAuthenticated, setIsAuthenticated] = useState(false);
  const [user, setUser] = useState<UserType | undefined>(undefined);

  const restoreUser = () => {
    const stored = localStorage.getItem('user');
    if (!stored) {
      setIsAuthenticated(false);
      setUser(undefined);
      return;
    }

    try {
      const parsed = JSON.parse(stored);
      const payload = parseJwt(parsed.accessToken);
      if (!payload || !payload.Id || !payload.Email) {
        throw new Error('Invalid token payload');
      }

      if (payload.exp && Date.now() >= payload.exp * 1000) {
        localStorage.removeItem('user');
        setIsAuthenticated(false);
        setUser(undefined);
        return;
      }

      const userData: UserType = {
        id: payload.Id,
        email: payload.Email,
        userName: payload.UserName || '',
      };

      setUser(userData);
      setIsAuthenticated(true);
    } catch (error) {
      console.error('Ошибка восстановления пользователя:', error);
      localStorage.removeItem('user');
      setIsAuthenticated(false);
      setUser(undefined);
    }
  };

  useEffect(() => {
    restoreUser();
  }, []);

  useEffect(() => {
    const handleStorageChange = () => restoreUser();
    window.addEventListener('storage', handleStorageChange);
    return () => window.removeEventListener('storage', handleStorageChange);
  }, []);

  const login = async (email: string, password: string) => {
    try {
      const { accessToken, refreshToken } = await apiLogin({ email, password });
      const payload = parseJwt(accessToken);

      if (!payload?.Id || !payload?.Email) {
        throw new Error('Недопустимый токен');
      }

      const userData: UserType = {
        id: payload.Id,
        email: payload.Email,
        userName: payload.UserName || '',
      };

      localStorage.setItem('user', JSON.stringify({ accessToken, refreshToken, user: userData }));
      setUser(userData);
      setIsAuthenticated(true);
      navigate('/portfolios', { replace: true });
    } catch (error) {
      const message = handleApiError(error);
      console.error('Login failed:', message);
      throw new Error(message);
    }
  };

  const logout = async () => {
    const storedUser = localStorage.getItem('user');
    if (storedUser) {
      const { refreshToken } = JSON.parse(storedUser);
      try {
        await apiLogout(refreshToken);
      } catch (error) {
        console.warn('Logout failed:', error);
      }
    }
    localStorage.removeItem('user');
    setUser(undefined);
    setIsAuthenticated(false);
    navigate('/login');
  };

  const register = async (email: string, password: string, fullName: string) => {
    try {
      const { accessToken, refreshToken } = await apiRegister({ email, password, fullName });
      const payload = parseJwt(accessToken);

      if (!payload?.Id || !payload?.Email) {
        throw new Error('Недопустимый токен');
      }

      const userData: UserType = {
        id: payload.Id,
        email: payload.Email,
        userName: payload.UserName || fullName,
      };

      localStorage.setItem('user', JSON.stringify({ accessToken, refreshToken, user: userData }));
      setUser(userData);
      setIsAuthenticated(true);
      navigate('/portfolios', { replace: true });
    } catch (error) {
      const message = handleApiError(error);
      console.error('Registration failed:', message);
      throw new Error(message);
    }
  };

  const checkEmail = async (email: string): Promise<boolean> => {
    try {
      return await apiCheckEmail(email);
    } catch (error) {
      const message = handleApiError(error);
      console.warn('Email check failed:', message);
      throw new Error(message);
    }
  };

  useEffect(() => {
    const refreshInterval = setInterval(async () => {
      const storedUser = localStorage.getItem('user');
      if (!storedUser) return;

      try {
        const { accessToken, refreshToken } = JSON.parse(storedUser);
        const payload = parseJwt(accessToken);
        if (!payload?.exp) return;

        const timeToExpire = payload.exp * 1000 - Date.now();
        const refreshThreshold = 5 * 60 * 1000;

        if (timeToExpire < refreshThreshold) {
          const newTokens = await refreshTokens(refreshToken);
          const updatedUser = {
            ...JSON.parse(storedUser),
            accessToken: newTokens.accessToken,
            refreshToken: newTokens.refreshToken,
          };
          localStorage.setItem('user', JSON.stringify(updatedUser));
        }
      } catch (error) {
        console.error('Не удалось обновить токен:', error);
        logout();
      }
    }, 60_000);
    return () => clearInterval(refreshInterval);
  }, [logout]);

  return (
    <AuthContext.Provider value={{ isAuthenticated, user, login, logout, register, checkEmail }}>
      {children}
    </AuthContext.Provider>
  );
};
