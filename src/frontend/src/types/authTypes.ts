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
  email: string;
  password: string;
  fullName: string; // соответствие с бэкендом
}

/**
 * Ответ от API: токены + метаданные
 */
export interface AuthResponse {
  accessToken: string;
  refreshToken: string;
  accessExpiresAt?: string; // ISO string
  refreshExpiresAt?: string;
}

/**
 * Ответ от проверки email
 */
export interface CheckEmailResponse {
  exists: boolean; // true = email занят
}

/**
 * Пользователь (извлекается из JWT или отдельного endpoint)
 */
export interface UserType {
  id: string;
  email: string;
  userName?: string;
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
  logout: () => Promise<void>;
  register: (email: string, password: string, fullName: string) => Promise<void>;
  checkEmail: (email: string) => Promise<boolean>;
}
