/**
 * Запрос на вход
 */
export interface LoginRequest {
  email: string;
  password: string;
}

/**
 * Запрос на регистрацию
 */
export interface RegisterRequest {
  fullName: string;
  email: string;
  password: string;
  confirmPassword: string;
}

/**
 * Ответ от API: только токены
 */
export interface AuthResponse {
  accessToken: string;
  refreshToken: string;
}

/**
 * Ответ от проверки email
 */
export interface CheckEmailResponse {
  exists: boolean;
}

/**
 * Пользователь (извлекается из JWT или отдельного endpoint)
 */
export interface UserType {
  id: string;
  email: string;
  // Добавь, если бэкенд возвращает:
  fullName?: string;
  // role?: string;
}

/**
 * Данные формы регистрации (фронтенд)
 */
export interface RegisterFormValues {
  email: string;
  password: string;
  confirmPassword: string;
  fullName: string;
}

/**
 * Тип контекста аутентификации
 */
export interface AuthContextType {
  isAuthenticated: boolean;
  user?: UserType; 
  login: (email: string, password: string) => Promise<void>;
  logout: () => void;
  register: (email: string, password: string, fullName: string) => Promise<void>;
  checkEmail: (email: string) => Promise<boolean>;
}
