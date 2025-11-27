import { useState, useEffect } from 'react';
import { 
  Container, 
  Typography, 
  Paper, 
  Table, 
  TableBody, 
  TableCell, 
  TableHead, 
  TableRow, 
  IconButton, 
  Alert,
  Box,
  Chip
} from '@mui/material';
import DeleteIcon from '@mui/icons-material/Delete';
import ShowChartIcon from '@mui/icons-material/ShowChart';
import TrendingDownIcon from '@mui/icons-material/TrendingDown';
import CurrencyBitcoinIcon from '@mui/icons-material/CurrencyBitcoin';
import { alertsApi } from '../services/alertsApi';
import type { AlertCondition, AlertResponse } from '../types/alertTypes';
import { getAssetTypeName, getAssetTypeColor, mapApiToUiAssetType } from '../utils/assetTypeUtils';
import AppLayout from '../components/AppLayout';

export default function AlertsListPage() {
  const [alerts, setAlerts] = useState<AlertResponse[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');

  useEffect(() => {
    loadAlerts();
  }, []);

  const loadAlerts = async () => {
    setLoading(true);
    try {
      const response = await alertsApi.getAll();
      console.log('Ответ от alertsApi.getAll():', response); // 🔍

      if (!Array.isArray(response)) {
        console.error('Ожидался массив, но пришёл:', response);
        setError('Некорректный формат данных от сервера');
        setAlerts([]); // безопасно
      } else {
        setAlerts(response);
      }
    } catch (err: any) {
      console.error('Ошибка загрузки уведомлений:', err);
      setError(err.message || 'Не удалось загрузить уведомления');
    } finally {
      setLoading(false);
    }
  };

  const handleDelete = async (id: string) => {
    if (!window.confirm('Удалить уведомление?')) return;
    try {
      await alertsApi.delete(id);
      setAlerts(alerts.filter(a => a.id !== id));
    } catch (err: any) {
      alert('Ошибка при удалении');
    }
  };

  /**
   * Преобразует условие в текст
   * @param condition Условие: Above | Below
   * @returns Человекочитаемый текст
   */
  const getConditionText = (condition: AlertCondition) => {
    switch (condition) {
      case 'Above':
        return 'станет ≥'; // или "выше"
      case 'Below':
        return 'станет ≤'; // или "ниже"
      default:
        return condition;
    }
  };

  /**
   * Возвращает иконку по типу актива
   * @param type Тип актива
   * @returns React-компонент иконки
   */
  const getAssetTypeIcon = (type: string) => {
    const normalized = mapApiToUiAssetType(type);
    switch (normalized) {
      case 'stock': return <ShowChartIcon fontSize="small" />;
      case 'bond': return <TrendingDownIcon fontSize="small" />;
      case 'crypto': return <CurrencyBitcoinIcon fontSize="small" />;
      default: return <ShowChartIcon fontSize="small" />;
    }
  };

  return (
    <AppLayout>
      <Container>
        <Typography variant="h4" component="h1" gutterBottom>
          Мои уведомления
        </Typography>

        {error && <Alert severity="error" sx={{ mb: 3 }}>{error}</Alert>}

        {loading ? (
          <Typography>Загрузка...</Typography>
        ) : alerts.length === 0 ? (
          <Paper sx={{ p: 4, textAlign: 'center' }}>
            <Typography color="textSecondary">У вас пока нет подписок.</Typography>
          </Paper>
        ) : (
          <Paper>
            <Table>
              <TableHead>
                <TableRow>
                  <TableCell>Актив</TableCell>
                  <TableCell>Тип</TableCell>
                  <TableCell>Условие</TableCell>
                  <TableCell>Целевая цена</TableCell>
                  <TableCell>Создано</TableCell>
                  <TableCell align="right">Действия</TableCell>
                </TableRow>
              </TableHead>
              <TableBody>
                {alerts.map((alert) => (
                  <TableRow key={alert.id}>
                    <TableCell>
                      <strong>{alert.ticker}</strong> — {alert.assetName}
                    </TableCell>
                    <TableCell>
                      <Chip
                        icon={getAssetTypeIcon(alert.assetType)}
                        label={getAssetTypeName(mapApiToUiAssetType(alert.assetType))}
                        size="small"
                        sx={{
                          '&.MuiChip-root': {
                            bgcolor: (theme) => {
                              const color = getAssetTypeColor(mapApiToUiAssetType(alert.assetType));
                              return theme.palette[color].main;
                            },
                            color: 'white',
                            '& .MuiChip-icon': {
                              color: 'white !important',
                              marginLeft: '4px',
                            },
                          },
                        }}
                      />
                    </TableCell>
                    <TableCell>
                      <i>Цена {getConditionText(alert.condition)}</i>
                    </TableCell>
                    <TableCell><strong>{alert.targetPrice.toFixed(2)} {alert.assetCurrency}</strong></TableCell>
                    <TableCell>
                      {new Date(alert.createdAt).toLocaleString(undefined, {
                        day: '2-digit',
                        month: '2-digit',
                        year: 'numeric',
                        hour: '2-digit',
                        minute: '2-digit',
                      })}
                    </TableCell>
                    <TableCell align="right">
                      <IconButton 
                        size="small" 
                        onClick={() => handleDelete(alert.id)}
                        color="error"
                        aria-label="удалить"
                      >
                        <DeleteIcon fontSize="small" />
                      </IconButton>
                    </TableCell>
                  </TableRow>
                ))}
              </TableBody>
            </Table>
          </Paper>
        )}

        <Box mt={3}>
          <Typography variant="body2" color="textSecondary">
            Уведомления проверяются автоматически. При достижении условия вы получите оповещение.
          </Typography>
        </Box>
      </Container>
    </AppLayout>
  );
}
