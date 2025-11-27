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
import AppLayout from '../components/AppLayout';

export default function LoginPage() {
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [error, setError] = useState('');
  const { login } = useAuth();
  const [loading, setLoading] = useState(false);
  const navigate = useNavigate();

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError('');

    if (!email) return setError('Введите email');
    if (!/\S+@\S+\.\S+/.test(email)) return setError('Некорректный email');
    if (!password) return setError('Введите пароль');
    if (password.length < 6) return setError('Пароль должен быть не менее 6 символов');

    setLoading(true);
    try {
      await login(email, password);
      navigate('/portfolios', { replace: true });
    } catch (err: any) {
      setError(err.message || 'Неверный email или пароль');
    } finally {
      setLoading(false);
    }
  };

  return (
    <AppLayout maxWidth={"sm"}>
      <Container>
        <Paper sx={{ p: 4, borderRadius: 2, boxShadow: 2 }}>
          <Typography variant="h4" component="h1" gutterBottom align="center" fontWeight={600}>
            Вход
          </Typography>

          {error && <Alert severity="error" sx={{ mb: 2 }}>{error}</Alert>}

          <form onSubmit={handleSubmit}>
            <TextField
              label="Email"
              type="email"
              fullWidth
              margin="normal"
              value={email}
              onChange={(e) => setEmail(e.target.value)}
              required
              autoFocus
              sx={{ borderRadius: 1 }}
            />
            <TextField
              label="Пароль"
              type="password"
              fullWidth
              margin="normal"
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              required
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
              {loading ? <CircularProgress size={24} /> : 'Войти'}
            </Button>
          </form>

          <Box mt={2} textAlign="center">
            <Link href="/register" variant="body2">
              Нет аккаунта? Зарегистрироваться
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
