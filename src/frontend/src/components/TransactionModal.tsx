import { useState } from 'react';
import {
  Modal,
  Box,
  Typography,
  TextField,
  Button,
  Select,
  MenuItem,
  FormControl,
  InputLabel,
  Alert,
  Stack,
} from '@mui/material';
import AddCircleIcon from '@mui/icons-material/AddCircle';

import type { PortfolioAssetShort } from '../types/portfolioAssetTypes';

type TransactionType = 'Buy' | 'Sell';

interface TransactionModalProps {
  open: boolean;
  onClose: () => void;
  onSubmit: (data: {
    transactionType: TransactionType;
    quantity: number;
    pricePerUnit: number;
    transactionDate: string;
  }) => void;
  assetName: string;
  initialType?: TransactionType;
  isLoading?: boolean;
  asset?: PortfolioAssetShort;
}

const style = {
  position: 'absolute' as const,
  top: '50%',
  left: '50%',
  transform: 'translate(-50%, -50%)',
  width: 400,
  bgcolor: 'background.paper',
  borderRadius: 2,
  boxShadow: 24,
  p: 4,
};

export default function TransactionModal({
  open,
  onClose,
  onSubmit,
  assetName,
  initialType = 'Buy',
  isLoading = false,
  asset, // 🔥 Получаем актив
}: TransactionModalProps) {
  const [type, setType] = useState<TransactionType>(initialType);
  const [quantity, setQuantity] = useState('');
  const [price, setPrice] = useState('');
  const getDefaultDateTimeLocal = (): string => {
    const now = new Date();
    const year = now.getFullYear();
    const month = String(now.getMonth() + 1).padStart(2, '0');
    const day = String(now.getDate()).padStart(2, '0');
    const hours = String(now.getHours()).padStart(2, '0');
    const minutes = String(now.getMinutes()).padStart(2, '0');
    return `${year}-${month}-${day}T${hours}:${minutes}`;
  };

  const [executedAt, setExecutedAt] = useState(getDefaultDateTimeLocal());
  const [error, setError] = useState('');

  const handleSubmit = () => {
    const q = parseFloat(quantity);
    const p = parseFloat(price);

    if (!quantity || isNaN(q) || q <= 0) {
      setError('Количество должно быть числом больше 0');
      return;
    }
    if (!price || isNaN(p) || p <= 0) {
      setError('Цена должна быть числом больше 0');
      return;
    }

    onSubmit({
      transactionType: type,
      quantity: q,
      pricePerUnit: p,
      transactionDate: new Date(executedAt).toISOString(),
    });
  };

  // Проверка: будет ли актив удалён после продажи?
  const willBeZero = asset && type === 'Sell' && asset.totalQuantity <= parseFloat(quantity || '0');

  return (
    <Modal open={open} onClose={onClose}>
      <Box sx={style}>
        <Typography variant="h6" component="h2" gutterBottom>
          Новая операция: {assetName}
        </Typography>

        {error && <Alert severity="error" sx={{ mb: 2 }}>{error}</Alert>}

        <Stack spacing={2}>
          <FormControl fullWidth required>
            <InputLabel>Тип операции</InputLabel>
            <Select
              value={type}
              label="Тип операции"
              onChange={(e) => setType(e.target.value as TransactionType)}
            >
              <MenuItem value="Buy">Покупка</MenuItem>
              <MenuItem value="Sell">Продажа</MenuItem>
            </Select>
          </FormControl>

          <TextField
            label="Количество"
            type="number"
            value={quantity}
            onChange={(e) => {
              const val = e.target.value;
              if (val === '' || /^\d*\.?\d*$/.test(val)) setQuantity(val);
            }}
            fullWidth
            required
            slotProps={{
              input: {
                inputProps: { min: 0.001, step: 0.001 }
              }
            }}
            helperText="Минимум 0.001"
          />

          <TextField
            label="Цена за единицу"
            type="number"
            value={price}
            onChange={(e) => {
              const val = e.target.value;
              if (val === '' || /^\d*\.?\d*$/.test(val)) setPrice(val);
            }}
            fullWidth
            required
            slotProps={{
              input: {
                inputProps: { min: 0.01, step: 0.01 }
              }
            }}
            helperText="Минимум 0.01"
          />

          <TextField
            label="Дата и время"
            type="datetime-local"
            value={executedAt}
            onChange={(e) => setExecutedAt(e.target.value)}
            fullWidth
            required
            slotProps={{
              inputLabel: { shrink: true },
              input: {
                inputProps: { step: 60 }
              }
            }}
          />

          {/* Предупреждение: актив будет удалён */}
          {willBeZero && (
            <Alert severity="warning" sx={{ mt: 2 }}>
              После этой продажи актив будет полностью удалён из портфеля.
            </Alert>
          )}

          <Box mt={2} display="flex" gap={2}>
            <Button
              variant="contained"
              startIcon={<AddCircleIcon />}
              onClick={handleSubmit}
              disabled={isLoading}
              fullWidth
            >
              {isLoading ? 'Сохранение...' : 'Добавить операцию'}
            </Button>
            <Button
              variant="outlined"
              onClick={onClose}
              disabled={isLoading}
              fullWidth
            >
              Отмена
            </Button>
          </Box>
        </Stack>
      </Box>
    </Modal>
  );
}