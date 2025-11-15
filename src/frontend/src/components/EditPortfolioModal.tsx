import { useState } from 'react';
import {
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  Button,
  TextField,
  MenuItem,
  Typography,
} from '@mui/material';

// Пропсы для модалки
interface EditPortfolioModalProps {
  open: boolean;
  onClose: () => void;
  portfolio: {
    id: string;
    name: string;
    currency: string;
  };
  onSave: (id: string, data: { name: string; currency: string }) => Promise<void>;
}

export default function EditPortfolioModal({
  open,
  onClose,
  portfolio,
  onSave,
}: EditPortfolioModalProps) {
  const [formData, setFormData] = useState({
    name: portfolio.name,
    currency: portfolio.currency,
  });
  const [errors, setErrors] = useState({
    name: '',
  });
  const [submitting, setSubmitting] = useState(false);

  const validate = (): boolean => {
    const newErrors = { name: '' };
    let isValid = true;

    if (!formData.name.trim()) {
      newErrors.name = 'Название портфеля обязательно';
      isValid = false;
    }

    setErrors(newErrors);
    return isValid;
  };

  const handleSubmit = async () => {
    if (!validate()) return;

    setSubmitting(true);
    try {
      await onSave(portfolio.id, formData);
      onClose();
    } catch (err: any) {
      setErrors({
        name: err.message || 'Не удалось обновить портфель',
      });
    } finally {
      setSubmitting(false);
    }
  };

  const handleChange = (field: 'name' | 'currency') => (e: React.ChangeEvent<HTMLInputElement>) => {
    setFormData((prev) => ({ ...prev, [field]: e.target.value }));
    // Сбрасываем ошибку при вводе
    if (errors.name && field === 'name') {
      setErrors((prev) => ({ ...prev, name: '' }));
    }
  };

  return (
    <Dialog open={open} onClose={onClose} maxWidth="sm" fullWidth>
      <DialogTitle>Редактировать портфель</DialogTitle>
      <DialogContent>
        <Typography variant="body2" color="text.secondary" sx={{ mb: 2 }}>
          Измените название или валюту портфеля
        </Typography>

        <TextField
          autoFocus
          margin="normal"
          label="Название"
          value={formData.name}
          onChange={handleChange('name')}
          fullWidth
          required
          error={!!errors.name}
          helperText={errors.name}
        />

        <TextField
          select
          margin="normal"
          label="Валюта"
          value={formData.currency}
          onChange={handleChange('currency')}
          fullWidth
        >
          {['RUB', 'USD', 'EUR'].map((curr) => (
            <MenuItem key={curr} value={curr}>
              {curr}
            </MenuItem>
          ))}
        </TextField>
      </DialogContent>
      <DialogActions>
        <Button onClick={onClose} disabled={submitting}>
          Отмена
        </Button>
        <Button
          variant="contained"
          onClick={handleSubmit}
          disabled={submitting}
        >
          {submitting ? 'Сохранение...' : 'Сохранить'}
        </Button>
      </DialogActions>
    </Dialog>
  );
}
