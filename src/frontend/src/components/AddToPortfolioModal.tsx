import { useEffect, useState } from 'react';
import {
  Modal,
  Box,
  Typography,
  Button,
  TextField,
  Alert,
  Stack,
  InputAdornment,
  MenuItem,
} from '@mui/material';
import type { PortfolioShort } from '../types/portfolioTypes';
import type { AssetShort } from '../types/assetTypes';
import { useSnackbar } from '../hooks/useSnackbar';
import AssetSelector from './AssetSelector';

const modalStyle = {
  position: 'absolute' as const,
  top: '50%',
  left: '50%',
  transform: 'translate(-50%, -50%)',
  width: { xs: '90%', sm: 600 },
  bgcolor: 'background.paper',
  borderRadius: 2,
  boxShadow: '0 4px 12px rgba(0,0,0,0.15)',
  p: 4,
};

interface AddToPortfolioModalProps {
  open: boolean;
  onClose: () => void;
  portfolios: PortfolioShort[];
  onAdd: (
    asset: AssetShort,
    portfolioId: string,
    quantity: number,
    purchasePrice: number
  ) => Promise<void>;
  loadingPortfolios: boolean;
  selectedAsset?: AssetShort | null;
  initialPurchasePrice?: number;
  fixedPortfolioId?: string;
}

export default function AddToPortfolioModal({
  open,
  onClose,
  portfolios,
  onAdd,
  loadingPortfolios,
  selectedAsset: initialAsset = null,
  initialPurchasePrice,
  fixedPortfolioId,
}: AddToPortfolioModalProps) {
  const [selectedAsset, setSelectedAsset] = useState<AssetShort | null>(initialAsset);
  const [portfolioId, setPortfolioId] = useState<string>('');
  const [quantity, setQuantity] = useState<string>('');
  const [price, setPrice] = useState<string>(
    initialPurchasePrice ? initialPurchasePrice.toFixed(2) : ''
  );
  const [error, setError] = useState<string>('');
  const [submitting, setSubmitting] = useState(false);
  const { openSnackbar } = useSnackbar();

  const isAssetFixed = !!initialAsset;
  const isPortfolioFixed = !!fixedPortfolioId;
  const isPricePrefilled = !!initialPurchasePrice;

  // Устанавливаем фиксированный портфель при открытии
  useEffect(() => {
    if (fixedPortfolioId) {
      setPortfolioId(fixedPortfolioId);
    } else {
      setPortfolioId(''); // сброс, если нет фиксации
    }
  }, [fixedPortfolioId]);

  // Подставляем цену, если есть актив, но цена пустая
  useEffect(() => {
    if (
      initialAsset &&
      !price &&
      initialPurchasePrice == null &&
      typeof initialAsset.currentPrice === 'number'
    ) {
      setPrice(initialAsset.currentPrice.toFixed(2));
    }
  }, [initialAsset, price, initialPurchasePrice]);

  // 🔥 Сброс формы только при полном закрытии модалки
  useEffect(() => {
    if (!open) {
      // Даем время анимации закрытия
      const timer = setTimeout(() => {
        setSelectedAsset(null);
        setPortfolioId('');
        setQuantity('');
        setPrice('');
        setError('');
      }, 300);
      return () => clearTimeout(timer);
    }
  }, [open]);

  const handleSubmit = async () => {
    if (!selectedAsset) return setError('Выберите актив');
    if (!portfolioId) return setError('Выберите портфель');
    const q = parseFloat(quantity);
    const p = parseFloat(price);
    if (isNaN(q) || q <= 0) return setError('Количество должно быть больше 0');
    if (isNaN(p) || p <= 0) return setError('Цена должна быть больше 0');

    setSubmitting(true);
    try {
      await onAdd(selectedAsset, portfolioId, q, p);
      openSnackbar('Актив успешно добавлен в портфель', 'success');
      onClose();
    } catch (err: any) {
      openSnackbar('Ошибка: ' + (err.message || 'не удалось добавить актив'), 'error');
    } finally {
      setSubmitting(false);
    }
  };

  // Находим текущий портфель для отображения
  const currentPortfolio = portfolios.find(p => p.id === portfolioId);
  const isCrypto = selectedAsset?.type === 'crypto';

  return (
    <Modal 
      open={open} 
      onClose={onClose} 
      // 🔥 ИСПРАВЛЕННЫЕ ПРОПСЫ:
      disableRestoreFocus={false} // Разрешить восстановление фокуса
      disableEscapeKeyDown={false} // Разрешить ESC
      keepMounted={false} // Не держать в DOM когда закрыто
      closeAfterTransition // Плавное закрытие
    >
      <Box sx={modalStyle}>
        <Typography variant="h6" component="h2" gutterBottom fontWeight="bold">
          Добавить актив в портфель
        </Typography>

        <Stack spacing={3}>
          {/* Выбор актива */}
          <AssetSelector
            selectedAsset={selectedAsset}
            onSelect={(asset) => {
              if (!isAssetFixed) {
                setSelectedAsset(asset);
                setError('');
              }
            }}
            disabled={isAssetFixed}
            label="Поиск актива"
          />

          {/* Информация о выбранном активе */}
          {selectedAsset && (
            <Alert severity="info">
              Выбран: <strong>{selectedAsset.shortName || selectedAsset.ticker}</strong>.&nbsp;
              Валюта: <strong>{selectedAsset.currency}</strong>
              {isAssetFixed && ' (нельзя изменить)'}
              {isPricePrefilled && (
                <Typography variant="body2" color="text.secondary" mt={1}>
                  Цена подставлена из текущей котировки. Вы можете изменить её.
                </Typography>
              )}
            </Alert>
          )}

          {error && <Alert severity="error">{error}</Alert>}

          {/* Выбор портфеля */}
          <TextField
            select
            label="Портфель"
            value={portfolioId}
            onChange={(e) => {
              if (!isPortfolioFixed) {
                setPortfolioId(e.target.value);
              }
            }}
            fullWidth
            required
            disabled={loadingPortfolios || isPortfolioFixed || portfolios.length === 0}
            error={!!error && !portfolioId}
            variant="outlined"
          >
            {isPortfolioFixed ? (
              <MenuItem value={portfolioId}>
                {currentPortfolio?.name || 'Текущий портфель'} ({currentPortfolio?.currency || '—'})
              </MenuItem>
            ) : (
              [
                <MenuItem key="placeholder" value="" disabled>
                  Выберите портфель
                </MenuItem>,
                portfolios.length === 0 ? (
                  <MenuItem key="no-portfolio" disabled>
                    Нет доступных портфелей
                  </MenuItem>
                ) : (
                  portfolios.map((p) => (
                    <MenuItem key={p.id} value={p.id}>
                      {p.name} ({p.currency})
                    </MenuItem>
                  ))
                ),
              ]
            )}
          </TextField>

          {/* Количество */}
          <TextField
            label="Количество"
            type="number"
            value={quantity}
            onChange={(e) => setQuantity(e.target.value)}
            fullWidth
            required
            error={!!error && (isNaN(parseFloat(quantity)) || parseFloat(quantity) <= 0)}
            variant="outlined"
            slotProps={{
              input: {
                inputProps: {
                  min: isCrypto ? 0.001 : 1,
                  step: isCrypto ? 0.001 : 1
                },
              },
            }}
          />

          {/* Цена покупки */}
          <TextField
            label="Цена покупки"
            type="number"
            value={price}
            onChange={(e) => setPrice(e.target.value)}
            fullWidth
            required
            error={!!error && (isNaN(parseFloat(price)) || parseFloat(price) <= 0)}
            variant="outlined"
            slotProps={{
              input: {
                inputProps: { min: 0.01, step: 0.01 },
                startAdornment: (
                  <InputAdornment position="start">
                    {selectedAsset?.currency || '—'}
                  </InputAdornment>
                ),
              },
            }}
          />
        </Stack>

        <Box display="flex" gap={2} justifyContent="flex-end" mt={3}>
          <Button
            variant="outlined"
            onClick={onClose}
            color="inherit"
            disabled={submitting}
          >
            Отмена
          </Button>
          <Button
            variant="contained"
            color="primary"
            onClick={handleSubmit}
            disabled={submitting || !selectedAsset || !portfolioId || !quantity || !price}
          >
            {submitting ? 'Добавление...' : 'Добавить в портфель'}
          </Button>
        </Box>
      </Box>
    </Modal>
  );
}