import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { portfolioApi } from '../services/portfolioApi';
import type { CreatePortfolioRequest } from '../types/portfolioTypes';
import {
  Container,
  Typography,
  TextField,
  Button,
  Paper,
  Box,
  Alert,
  FormControl,
  InputLabel,
  Select,
  MenuItem,
} from '@mui/material';
import { Link } from 'react-router-dom';
import type { SelectChangeEvent } from '@mui/material/Select';
import AppLayout from '../components/AppLayout';

export default function PortfolioCreatePage() {
  const navigate = useNavigate();
  const [error, setError] = useState('');
  const [loading, setLoading] = useState(false);

  const [formData, setFormData] = useState<Omit<CreatePortfolioRequest, 'userId'>>({
    name: '',
    currency: 'RUB',
  });

  const [touched, setTouched] = useState({
    name: false,
  });

  // Валидация
  const errors = {
    name: !formData.name.trim() ? 'Название портфеля обязательно' : '',
  };

  const isFormValid = !errors.name;

  // Получаем userId из localStorage
  const getUserId = (): string | null => {
    const storedUser = localStorage.getItem('user');
    if (!storedUser) return null;
    try {
      const { user } = JSON.parse(storedUser);
      return typeof user?.id === 'string' ? user.id : null;
    } catch (e) {
      console.error('Failed to parse user from localStorage');
      return null;
    }
  };

  const handleNameChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    setFormData((prev) => ({
      ...prev,
      name: e.target.value,
    }));
  };

  const handleCurrencyChange = (e: SelectChangeEvent<string>) => {
    setFormData((prev) => ({
      ...prev,
      currency: e.target.value,
    }));
  };

  const handleBlur = (field: string) => () => {
    setTouched((prev) => ({ ...prev, [field]: true }));
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setTouched({ name: true });

    if (!isFormValid) return;

    setError('');
    setLoading(true);

    const userId = getUserId();
    if (!userId) {
      setError('Не удалось получить ID пользователя');
      setLoading(false);
      return;
    }

    try {
      await portfolioApi.create({
        name: formData.name.trim(),
        userId,
        currency: formData.currency,
      });
      navigate('/portfolios');
    } catch (err: any) {
      setError(err.message || 'Не удалось создать портфель');
    } finally {
      setLoading(false);
    }
  };

  return (
    <AppLayout>
      <Container>
        <Paper sx={{ p: 4, mt: 4 }}>
          <Typography variant="h5" component="h1" gutterBottom>
            Создание портфеля
          </Typography>

          {error && <Alert severity="error" sx={{ mb: 2 }}>{error}</Alert>}

          <form onSubmit={handleSubmit}>
            {/* Название */}
            <TextField
              label="Название"
              name="name"
              value={formData.name}
              onChange={handleNameChange}
              onBlur={handleBlur('name')}
              fullWidth
              margin="normal"
              required
              autoFocus
              error={touched.name && !!errors.name}
              helperText={touched.name && errors.name}
              placeholder="Например: Основной"
            />

            {/* Валюта */}
            <FormControl fullWidth margin="normal" required>
              <InputLabel>Валюта</InputLabel>
              <Select
                name="currency"
                value={formData.currency}
                onChange={handleCurrencyChange} // ✅ Теперь совместимо
                label="Валюта"
              >
                <MenuItem value="RUB">RUB ₽</MenuItem>
                <MenuItem value="USD">USD $</MenuItem>
                <MenuItem value="EUR">EUR €</MenuItem>
              </Select>
            </FormControl>

            <Box mt={3} display="flex" gap={2}>
              <Button
                type="submit"
                variant="contained"
                color="primary"
                disabled={loading || !isFormValid}
                fullWidth
              >
                {loading ? 'Создание...' : 'Создать'}
              </Button>
              <Button
                component={Link}
                to="/portfolios"
                variant="outlined"
                fullWidth
              >
                Отмена
              </Button>
            </Box>
          </form>
        </Paper>
      </Container>
    </AppLayout>
  );
}
