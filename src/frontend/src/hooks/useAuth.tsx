import { useState, useEffect, createContext, useContext } from 'react';
import { useNavigate } from 'react-router-dom';
import { parseJwt } from '../utils/jwt';
import type { UserType, AuthContextType } from '../types/authTypes';
import { login as apiLogin, register as apiRegister, checkEmail as apiCheckEmail, logout as apiLogout, refreshTokens } from '../services/authApi';
import { deleteAccount as apiDeleteAccount} from '../services/accountApi';
import { useSnackbar } from './useSnackbar';

// Создаём контекст
const AuthContext = createContext<AuthContextType | undefined>(undefined);

// Хук для использования аутентификации
export const useAuth = () => {
  const context = useContext(AuthContext);
  if (!context) {
    throw new Error('useAuth must be used within an AuthProvider');
  }
  return context;
};

// Провайдер аутентификации
export const AuthProvider = ({ children }: { children: React.ReactNode }) => {
  const navigate = useNavigate();
  const { openSnackbar } = useSnackbar();
  const [isAuthenticated, setIsAuthenticated] = useState(false);
  const [user, setUser] = useState<UserType | undefined>(undefined);
  
  // Функция восстановления из localStorage
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
        throw new Error('Invalid token');
      }

      // Проверка срока действия
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

  // Инициализация при монтировании
  useEffect(() => {
    restoreUser();
  }, []);

  // Прослушка localStorage (вход/выход в другой вкладке, или после login)
  useEffect(() => {
    const handleStorageChange = () => {
      restoreUser();
    };
    window.addEventListener('storage', handleStorageChange);
    return () => window.removeEventListener('storage', handleStorageChange);
  }, []);

  // Функция входа
  const login = async (email: string, password: string) => {
    try {
      const { accessToken, refreshToken } = await apiLogin({ email, password });
      const payload = parseJwt(accessToken);

      if (!payload?.Id || !payload?.Email) {
        throw new Error('Invalid token payload');
      }

      const userData: UserType = {
        id: payload.Id,
        email: payload.Email,
        userName: payload.UserName || '',
      };

      const userPayload = { accessToken, refreshToken, user: userData };
      localStorage.setItem('user', JSON.stringify(userPayload));

      setUser(userData);
      setIsAuthenticated(true);
      navigate('/portfolios', { replace: true });
    } catch (error) {
      console.error('Login failed:', error);
      throw error;
    }
  };

    // Функция выхода
  const logout = () => {
    const storedUser = localStorage.getItem('user');
    if (storedUser) {
      const { refreshToken } = JSON.parse(storedUser);
      apiLogout(refreshToken).catch(console.warn);
    }
    localStorage.removeItem('user');
    setUser(undefined);
    setIsAuthenticated(false);
    navigate('/login');
  };

  // Функция регистрации
  const register = async (email: string, password: string, fullName: string) => {
    try {
      const { accessToken, refreshToken } = await apiRegister({ email, password, confirmPassword: password, fullName: fullName });
      const payload = parseJwt(accessToken);

      if (!payload?.Id || !payload?.Email) {
        throw new Error('Invalid token payload');
      }

      const userData: UserType = {
        id: payload.Id,
        email: payload.Email,
        userName: payload.UserName || fullName,
      };

      localStorage.setItem(
        'user',
        JSON.stringify({ accessToken, refreshToken, user: userData })
      );

      setIsAuthenticated(true);
      navigate('/portfolios', { replace: true });
    } catch (error) {
      console.error('Registration failed:', error);
      throw error;
    }
  };

  const deleteAccount = async () => {
    if (!window.confirm('Вы уверены, что хотите удалить аккаунт? Все данные будут безвозвратно удалены.')) {
      return;
    }

    try {
      await apiDeleteAccount();
      localStorage.removeItem('user');
      setIsAuthenticated(false);
      navigate('/login', { replace: true });
      openSnackbar('Аккаунт удалён', 'success');
    } catch (err: any) {
      console.error('Delete account failed:', err);
      openSnackbar(err.message || 'Не удалось удалить аккаунт', 'error');
    }
  };

  // Проверка email
  const checkEmail = async (email: string): Promise<boolean> => {
    return await apiCheckEmail(email);
  };

  // Прослушка localStorage (например, выход в другой вкладке)
  useEffect(() => {
    const handleStorageChange = () => {
      const hasUser = !!localStorage.getItem('user');
      if (hasUser) {
        const { accessToken } = JSON.parse(localStorage.getItem('user')!);
        const payload = parseJwt(accessToken);
        const isValid = payload && payload.Id && payload.Email && (!payload.exp || Date.now() < payload.exp * 1000);

        if (!isValid) {
          localStorage.removeItem('user');
          setIsAuthenticated(false);
        } else {
          setIsAuthenticated(true);
        }
      } else {
        setIsAuthenticated(false);
      }
    };
    window.addEventListener('storage', handleStorageChange);
    return () => window.removeEventListener('storage', handleStorageChange);
  }, []);

  // Автоматическое обновление токена (если истекает < 5 минут)
  useEffect(() => {
    const refreshInterval = setInterval(async () => {
      const storedUser = localStorage.getItem('user');
      if (!storedUser) return;

      try {
        const { accessToken, refreshToken } = JSON.parse(storedUser);
        const payload = parseJwt(accessToken);
        if (!payload?.exp) return;

        const timeToExpire = payload.exp * 1000 - Date.now();
        const refreshThreshold = 5 * 60 * 1000; // 5 минут

        if (timeToExpire < refreshThreshold) {
          console.log('Токен скоро истечёт — обновляем...');
          const newTokens = await refreshTokens(refreshToken);

          const updatedUser = {
            ...JSON.parse(storedUser),
            accessToken: newTokens.accessToken,
            refreshToken: newTokens.refreshToken,
          };

          localStorage.setItem('user', JSON.stringify(updatedUser));
          console.log('Токен успешно обновлён');
        }
      } catch (error) {
        console.error('Не удалось обновить токен:', error);
        logout();
      }
    }, 60_000); // Проверяем раз в минуту

    return () => clearInterval(refreshInterval);
  }, [logout]);

  return (
    <AuthContext.Provider value={{
      isAuthenticated,
      user,
      login,
      logout,
      register,
      checkEmail,
      deleteAccount
    }}>
      {children}
    </AuthContext.Provider>
  );
};
