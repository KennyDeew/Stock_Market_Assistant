import { useState, useEffect, createContext, useContext } from 'react';
import { useNavigate } from 'react-router-dom';
import { parseJwt } from '../utils/jwt';
import type { UserType, AuthContextType } from '../types/authTypes';
import { login as apiLogin, register as apiRegister, checkEmail as apiCheckEmail, logout as apiLogout } from '../services/authApi';

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
  const [isAuthenticated, setIsAuthenticated] = useState(!!localStorage.getItem('user'));
  const navigate = useNavigate();

  // Извлечение пользователя из localStorage
  const getUserFromStorage = (): UserType | undefined  => {
    const storedUser = localStorage.getItem('user');
    if (!storedUser) return undefined ;
    try {
      const { accessToken } = JSON.parse(storedUser);
      const payload = parseJwt(accessToken);
      if (!payload?.Id || !payload?.Email) return undefined ;
      return {
        id: payload.Id,
        email: payload.Email,
        fullName: payload.FullName || '',
      };
    } catch {
      return undefined;
    }
  };

  const user = getUserFromStorage();

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
        fullName: payload.FullName || '',
      };

      localStorage.setItem(
        'user',
        JSON.stringify({ accessToken, refreshToken, user: userData })
      );

      setIsAuthenticated(true);
      navigate('/dashboard');
    } catch (error) {
      console.error('Login failed:', error);
      throw error;
    }
  };

  const register = async (email: string, password: string, fullName: string) => {
    try {
      const { accessToken, refreshToken } = await apiRegister({ email, password, confirmPassword: password, fullName });

      const payload = parseJwt(accessToken);
      if (!payload?.Id || !payload?.Email) {
        throw new Error('Invalid token payload');
      }

      const userData: UserType = {
        id: payload.Id,
        email: payload.Email,
        fullName: payload.FullName || fullName,
      };

      localStorage.setItem(
        'user',
        JSON.stringify({ accessToken, refreshToken, user: userData })
      );

      setIsAuthenticated(true);
      navigate('/dashboard');
    } catch (error) {
      console.error('Registration failed:', error);
      throw error;
    }
  };

  const logout = () => {
    const storedUser = localStorage.getItem('user');
    if (storedUser) {
      const { refreshToken } = JSON.parse(storedUser);
      if (refreshToken) {
        apiLogout(refreshToken).catch(console.warn); // не блокируем logout
      }
    }
    localStorage.removeItem('user');
    setIsAuthenticated(false);
    navigate('/login');
  };

  const checkEmail = async (email: string): Promise<boolean> => {
    return await apiCheckEmail(email);
  };

  // Прослушка localStorage (например, при выходе в другой вкладке)
  useEffect(() => {
    const handleStorageChange = () => {
      setIsAuthenticated(!!localStorage.getItem('user'));
    };

    window.addEventListener('storage', handleStorageChange);
    return () => window.removeEventListener('storage', handleStorageChange);
  }, []);

  // Авто-обновление токена (опционально)
  // Можно добавить проверку срока жизни токена

  return (
    <AuthContext.Provider value={{
      isAuthenticated,
      user, // можно передать, если нужно использовать в компонентах
      login,
      logout,
      register,
      checkEmail,
    }}>
      {children}
    </AuthContext.Provider>
  );
};
