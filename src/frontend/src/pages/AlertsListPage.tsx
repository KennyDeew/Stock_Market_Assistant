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
  Box
} from '@mui/material';
import DeleteIcon from '@mui/icons-material/Delete';
import { alertsApi } from '../services/alertsApi';
import type { AlertResponse } from '../types/alertTypes';

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
      setAlerts(response.data);
    } catch (err: any) {
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

  const getConditionText = (condition: string) => {
    switch (condition) {
      case 'EqualTo': return 'достигнет';
      case 'GreaterThanEqual': return 'станет ≥';
      case 'LessThanEqual': return 'станет ≤';
      default: return condition;
    }
  };

  return (
    <Container maxWidth="md">
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
                    <strong>{alert.ticker}</strong> — {alert.name}
                  </TableCell>
                  <TableCell>
                    <i>Цена {getConditionText(alert.condition)}</i>
                  </TableCell>
                  <TableCell><strong>{alert.targetPrice.toFixed(2)} $</strong></TableCell>
                  <TableCell>
                    {new Date(alert.createdAt).toLocaleDateString()}
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
  );
}
