import { useState } from 'react';
import { useAuth } from '../hooks/useAuth';
import { useNavigate } from 'react-router-dom';
import {
  Container,
  Box,
  Typography,
  TextField,
  Button,
  Paper,
  Link,
  Alert,
  CircularProgress,
} from '@mui/material';
import type { RegisterFormValues } from '../types/authTypes';
import AppLayout from '../components/AppLayout';

export default function RegisterPage() {
  const { register, checkEmail } = useAuth();
  const navigate = useNavigate();

  const [formData, setFormData] = useState<RegisterFormValues>({
    email: '',
    password: '',
    confirmPassword: '',
    fullName: '',
  });

  const [errors, setErrors] = useState({
    email: '',
    password: '',
    confirmPassword: '',
    fullName: '',
    general: '',
  });

  const [loading, setLoading] = useState(false);
  const [emailChecked, setEmailChecked] = useState(false);
  const [emailAvailable, setEmailAvailable] = useState(false);
  const [checkingEmail, setCheckingEmail] = useState(false);

  const validate = () => {
    const newErrors = { email: '', password: '', confirmPassword: '', fullName: '', general: '' };
    let isValid = true;

    if (!formData.fullName) {
      newErrors.fullName = 'ФИО обязательно';
      isValid = false;
    }

    if (!formData.email) {
      newErrors.email = 'Email обязателен';
      isValid = false;
    } else if (!/\S+@\S+\.\S+/.test(formData.email)) {
      newErrors.email = 'Некорректный email';
      isValid = false;
    } else if (!emailChecked) {
      newErrors.email = 'Проверьте доступность email';
      isValid = false;
    } else if (!emailAvailable) {
      newErrors.email = 'Этот email уже занят';
      isValid = false;
    }

    if (!formData.password) {
      newErrors.password = 'Пароль обязателен';
      isValid = false;
    } else if (formData.password.length < 6) {
      newErrors.password = 'Пароль должен быть не менее 6 символов';
      isValid = false;
    } else if (!/\d/.test(formData.password)) {
      newErrors.password = 'Пароль должен содержать хотя бы одну цифру (0-9)';
      isValid = false;
    } else if (!/[A-Z]/.test(formData.password)) {
      newErrors.password = 'Пароль должен содержать хотя бы одну заглавную букву';
      isValid = false;
    } else if (!/[a-z]/.test(formData.password)) {
      newErrors.password = 'Пароль должен содержать хотя бы одну строчную букву';
      isValid = false;
    } else if (!/[^A-Za-z0-9]/.test(formData.password)) {
      newErrors.password = 'Пароль должен содержать хотя бы один спецсимвол (!, @, #, $ и т.д.)';
      isValid = false;
    }

    if (!formData.confirmPassword) {
      newErrors.confirmPassword = 'Подтвердите пароль';
      isValid = false;
    } else if (formData.password !== formData.confirmPassword) {
      newErrors.confirmPassword = 'Пароли не совпадают';
      isValid = false;
    }

    setErrors(newErrors);
    return isValid;
  };

  const handleCheckEmail = async () => {
    if (!formData.email) {
      setErrors(prev => ({ ...prev, email: 'Введите email' }));
      return;
    }

    setCheckingEmail(true);
    setErrors(prev => ({ ...prev, email: '' }));

    try {
      const available = await checkEmail(formData.email);
      setEmailAvailable(available);
      setEmailChecked(true);
      if (!available) {
        setErrors(prev => ({ ...prev, email: 'Этот email уже занят' }));
      }
    } catch (error: any) {
      const message = error.message || 'Ошибка при проверке email';
      setErrors(prev => ({ ...prev, email: message }));
      setEmailChecked(false);
    } finally {
      setCheckingEmail(false);
    }
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setErrors(prev => ({ ...prev, general: '' }));
    if (!validate()) return;

    setLoading(true);
    try {
      await register(formData.email, formData.password, formData.fullName);
      navigate('/login', { replace: true });
    } catch (error: any) {
      setErrors(prev => ({
        ...prev,
        general: error instanceof Error ? error.message : 'Не удалось зарегистрироваться. Проверьте данные.'
      }));
    } finally {
      setLoading(false);
    }
  };

  const handleChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const { name, value } = e.target;
    setFormData(prev => ({ ...prev, [name]: value }));
    if (errors[name as keyof typeof errors]) {
      setErrors(prev => ({ ...prev, [name]: '' }));
    }
    if (name === 'email') {
      setEmailChecked(false);
      setEmailAvailable(false);
    }
  };

  return (
    <AppLayout maxWidth="sm">
      <Container>
        <Paper sx={{ p: 4, borderRadius: 2, boxShadow: 2 }}>
          <Typography variant="h4" component="h1" gutterBottom align="center" fontWeight={600}>
            Регистрация
          </Typography>

          {errors.general && <Alert severity="error" sx={{ mb: 2 }}>{errors.general}</Alert>}

          <form onSubmit={handleSubmit}>
            <TextField
              label="Имя"
              type="text"
              fullWidth
              margin="normal"
              name="fullName"
              value={formData.fullName}
              onChange={handleChange}
              required
              error={!!errors.fullName}
              helperText={errors.fullName}
              sx={{ borderRadius: 1 }}
            />

            <TextField
              label="Email"
              type="email"
              fullWidth
              margin="normal"
              name="email"
              value={formData.email}
              onChange={handleChange}
              required
              error={!!errors.email}
              helperText={errors.email}
              slotProps={{
                input: {
                  endAdornment: (
                    <Button
                      size="small"
                      onClick={handleCheckEmail}
                      disabled={checkingEmail || !formData.email}
                      sx={{ minWidth: 'auto', px: 1 }}
                    >
                      {checkingEmail ? 'Проверка...' : 'Проверить'}
                    </Button>
                  ),
                },
              }}
            />

            <TextField
              label="Пароль"
              type="password"
              fullWidth
              margin="normal"
              name="password"
              value={formData.password}
              onChange={handleChange}
              required
              error={!!errors.password}
              helperText={errors.password}
              sx={{ borderRadius: 1 }}
            />

            <Typography variant="body2" color="text.secondary" sx={{ mt: 1 }}>
              Пароль должен содержать: минимум 6 символов, цифру, заглавную и строчную букву, а также спецсимвол (например, !, @, #, $).
            </Typography>

            <TextField
              label="Подтвердите пароль"
              type="password"
              fullWidth
              margin="normal"
              name="confirmPassword"
              value={formData.confirmPassword}
              onChange={handleChange}
              required
              error={!!errors.confirmPassword}
              helperText={errors.confirmPassword}
              sx={{ borderRadius: 1 }}
            />

            <Button
              type="submit"
              variant="contained"
              color="primary"
              fullWidth
              size="large"
              sx={{ mt: 3, py: 1.5 }}
              disabled={loading}
            >
              {loading ? <CircularProgress size={24} /> : 'Зарегистрироваться'}
            </Button>
          </form>

          <Box mt={2} textAlign="center">
            <Link href="/login" variant="body2">
              Уже есть аккаунт? Войти
            </Link>
          </Box>

          <Box sx={{ mt: 3, textAlign: 'center' }}>
            <Link
              component="button"
              variant="body2"
              onClick={() => navigate('/')}
              sx={{ textDecoration: 'none', color: 'primary.main', fontWeight: 500 }}
            >
              ← Вернуться на главную
            </Link>
          </Box>
        </Paper>
      </Container>
    </AppLayout>
  );
}