import { useState } from 'react';
import {
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  TextField,
  Button,
  Select,
  MenuItem,
  FormControl,
  InputLabel,
  Typography,
} from '@mui/material';

interface AlertModalProps {
  open: boolean;
  onClose: () => void;
  asset: { ticker: string; shortName: string; currency: string };
  onSubmit: (data: { targetPrice: number; condition: 'above' | 'below' }) => Promise<void>;
}

export default function AlertModal({ open, onClose, asset, onSubmit }: AlertModalProps) {
  const [targetPrice, setTargetPrice] = useState<number | ''>('');
  const [condition, setCondition] = useState<'above' | 'below'>('above');
  const [loading, setLoading] = useState(false);

  const handleSubmit = async () => {
    if (!targetPrice || typeof targetPrice !== 'number') return;

    setLoading(true);
    try {
      await onSubmit({ targetPrice, condition }); // ✅ Передаём правильно
    } finally {
      setLoading(false);
    }
  };

  return (
    <Dialog open={open} onClose={onClose} maxWidth="sm" fullWidth>
      <DialogTitle>Установить уведомление</DialogTitle>
      <DialogContent>
        <Typography gutterBottom>
          Актив: <strong>{asset.ticker}</strong> ({asset.shortName})
        </Typography>

        <TextField
          label="Целевая цена"
          type="number"
          fullWidth
          margin="normal"
          value={targetPrice}
          onChange={(e) => setTargetPrice(e.target.value === '' ? '' : Number(e.target.value))}
          InputProps={{ endAdornment: <span>{asset.currency}</span> }}
        />

        <FormControl fullWidth margin="normal">
          <InputLabel>Условие</InputLabel>
          <Select
            value={condition}
            onChange={(e) => setCondition(e.target.value as 'above' | 'below')}
            label="Условие"
          >
            <MenuItem value="above">Цена выше</MenuItem>
            <MenuItem value="below">Цена ниже</MenuItem>
          </Select>
        </FormControl>
      </DialogContent>
      <DialogActions>
        <Button onClick={onClose} disabled={loading}>
          Отмена
        </Button>
        <Button onClick={handleSubmit} variant="contained" disabled={loading || !targetPrice}>
          Сохранить
        </Button>
      </DialogActions>
    </Dialog>
  );
}
