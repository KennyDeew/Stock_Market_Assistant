import { useState, useEffect, useMemo } from 'react';
import {
  Container,
  Typography,
  Paper,
  Box,
  CircularProgress,
  Alert,
  IconButton,
  Tooltip,
} from '@mui/material';
import RefreshIcon from '@mui/icons-material/Refresh';
import { useNavigate, useParams } from 'react-router-dom';
import { format } from 'date-fns';
import { ru } from 'date-fns/locale';

// 📈 Recharts
import {
  LineChart,
  Line,
  XAxis,
  YAxis,
  CartesianGrid,
  Tooltip as RechartsTooltip,
  ResponsiveContainer,
} from 'recharts';

// 🔌 SignalR
import { HubConnection, HubConnectionBuilder } from '@microsoft/signalr';
import AppLayout from '../components/AppLayout';

// Типы
interface MoexSecurity {
  secid: string;
  boardid: string;
  shortname: string;
}

interface PriceUpdate {
  ticker: string;
  price: number;
  change: number;
  changePercent: number;
  time: string;
  volume: number;
  numTrades: number;
}

export default function AssetDetailPage() {
  const { ticker } = useParams<{ ticker: string }>();
  const navigate = useNavigate();

  const [security, setSecurity] = useState<MoexSecurity | null>(null);
  const [price, setPrice] = useState<number | null>(null);
  const [change, setChange] = useState<number | null>(null);
  const [changePercent, setChangePercent] = useState<number | null>(null);
  const [volume, setVolume] = useState<number>(0);
  const [numTrades, setNumTrades] = useState<number>(0);
  const [time, setTime] = useState<string | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [lastUpdated, setLastUpdated] = useState<Date | null>(null);
  const [priceHistory, setPriceHistory] = useState<Array<{ time: string; price: number }>>([]);

  const [connection, setConnection] = useState<HubConnection | null>(null);

  // 🔹 Создание подключения
  useEffect(() => {
    if (!ticker) return;

    const newConnection = new HubConnectionBuilder()
      .withUrl(import.meta.env.VITE_STOCKCARD_API_URL + '/pricehub')
      .withAutomaticReconnect()
      .build();

    setConnection(newConnection);

    return () => {
      if (newConnection) {
        newConnection.stop();
      }
    };
  }, [ticker]);

  // 🔹 Запуск и подписка
  useEffect(() => {
    if (!connection || !ticker) return;

    const startConnection = async () => {
      try {
        await connection.start();
        console.log('SignalR: подключено к pricehub');

        // Подписываемся на тикер (как массив)
        await connection.invoke('Subscribe', [ticker.toUpperCase()]);

        // Обработка входящих обновлений
        connection.on('PriceUpdate', (update: PriceUpdate) => {
          if (update.ticker.toUpperCase() !== ticker.toUpperCase()) return;

          setPrice(Number(update.price) || 0);
          setChange(Number(update.change) || 0);
          setChangePercent(Number(update.changePercent) || 0);
          setVolume(Number(update.volume) || 0);
          setNumTrades(Number(update.numTrades) || 0);
          setTime(update.time || new Date().toISOString().split('T')[1].slice(0, 8));
          setLastUpdated(new Date());

          // Добавляем в историю
          setPriceHistory((prev) => {
            const next = [
              ...prev,
              { time: update.time, price: update.price },
            ].slice(-60); // последние 60 точек
            return next;
          });

          setLoading(false);
          setError(null);
        });

        // Обработка ошибок (если сервер отправляет)
        connection.on('Error', (message: string) => {
          setError(`Ошибка: ${message}`);
        });
      } catch (err) {
        console.error('Ошибка подключения к SignalR', err);
        setError('Не удалось подключиться к стриму цен');
        setLoading(false);
      }
    };

    startConnection();

    // Отписка при размонтировании
    return () => {
      if (connection.state === 'Connected') {
        connection.invoke('Unsubscribe', [ticker.toUpperCase()]);
      }
      connection.off('PriceUpdate');
      connection.off('Error');
    };
  }, [connection, ticker]);

  // 🔹 Получение информации о бумаге (разово)
  useEffect(() => {
    const fetchSecurityInfo = async () => {
      if (!ticker) return;

      try {
        const secResponse = await fetch(
          `https://iss.moex.com/iss/securities.json?q=${ticker}&limit=1&engine=stock&market=shares`
        );
        const secData = await secResponse.json();

        if (!secData.securities.data.length) {
          setError('Актив не найден на МосБирже');
          return;
        }

        const [secRow] = secData.securities.data;
        const columns = secData.securities.columns;
        
        type MoexValue = string | number | null;

        const sec = columns.reduce(
          (obj: Record<string, MoexValue>, col: string, index: number) => {
            obj[col.toLowerCase()] = secRow[index];
            return obj;
          },
          {} as Record<string, MoexValue>
        ) as MoexSecurity;

        setSecurity(sec);
      } catch (err) {
        console.error('Ошибка загрузки информации о бумаге', err);
        setError('Не удалось загрузить информацию об активе');
      }
    };

    fetchSecurityInfo();
  }, [ticker]);

  // 🔹 Ручное обновление (опционально)
  const handleRefresh = () => {
    if (connection?.state === 'Connected') {
      connection.invoke('ForceUpdate', ticker); // если реализовано
    } else {
      window.location.reload();
    }
  };

  // Подготовка данных для графика
  const chartData = useMemo(() => {
    return priceHistory.map((point) => {
      const date = new Date(point.time);
      return {
        name: date.toLocaleTimeString('ru-RU', { hour: '2-digit', minute: '2-digit', second: '2-digit' }),
        price: point.price,
      };
    });
  }, [priceHistory]);


  if (!ticker) {
    return (
      <Container maxWidth="sm">
        <Alert severity="error">Тикер не указан</Alert>
      </Container>
    );
  }

  return (
    <AppLayout>
      <Container>
        <Box display="flex" alignItems="center" mb={3}>
          <Typography variant="h5" component="h1">
            {security?.shortname || ticker}
          </Typography>
          <Tooltip title="Обновить">
            <span>
              <IconButton
                onClick={handleRefresh}
                disabled={loading}
                size="small"
                sx={{ ml: 1 }}
              >
                <RefreshIcon />
              </IconButton>
            </span>
          </Tooltip>
        </Box>

        {error && <Alert severity="error" sx={{ mb: 3 }}>{error}</Alert>}

        {loading && !price ? (
          <Box display="flex" justifyContent="center" my={4}>
            <CircularProgress />
          </Box>
        ) : (
          <Paper sx={{ p: 3 }}>
            {/* Текущая цена */}
            <Box mb={3}>
              <Typography variant="h4" fontWeight="bold" color="text.primary">
                {price != null ? price.toFixed(2) : '—'} ₽
              </Typography>
              <Typography
                variant="body1"
                color={changePercent != null && changePercent >= 0 ? 'success.main' : 'error.main'}
                fontWeight="bold"
              >
                {change != null && change >= 0 ? '+' : ''}
                {change != null ? change.toFixed(2) : '0.00'} ₽
                {' / '}
                {changePercent != null && changePercent >= 0 ? '+' : ''}
                {changePercent != null ? changePercent.toFixed(2) : '0.00'}%
              </Typography>
            </Box>

            {/* График */}
            {priceHistory.length > 1 && (
              <Box mb={3} sx={{ height: 200, minHeight: 200, width: '100%', position: 'relative' }}>
                <Typography variant="subtitle2" gutterBottom>
                  Динамика цены (в реальном времени)
                </Typography>
                  <ResponsiveContainer width="100%" height="100%">
                    <LineChart data={chartData} margin={{ top: 5, right: 10, left: 0, bottom: 5 }}>
                    <CartesianGrid strokeDasharray="3 3" opacity={0.2} />
                    <XAxis dataKey="name" tick={{ fontSize: 10 }} />
                    <YAxis domain={['dataMin', 'dataMax']} tick={{ fontSize: 10 }} width={60} />
                    <RechartsTooltip
                      contentStyle={{ backgroundColor: '#f5f5f5', borderColor: '#ddd' }}
                      labelStyle={{ fontWeight: 600 }}
                      formatter={(value: number) => [value.toFixed(2) + ' ₽', 'Цена']}
                    />
                    <Line
                      type="monotone"
                      dataKey="price"
                      stroke="#1976d2"
                      strokeWidth={2}
                      dot={false}
                      activeDot={{ r: 4, fill: '#1976d2' }}
                    />
                  </LineChart>
                </ResponsiveContainer>
              </Box>
            )}

            {/* Дополнительная инфа */}
            <Box>
              <Typography variant="body2" color="textSecondary" gutterBottom>
                Последнее обновление: {lastUpdated ? format(lastUpdated, 'HH:mm:ss', { locale: ru }) : '—'}
              </Typography>
              <Typography variant="body2">
                <strong>Объём:</strong> {volume.toLocaleString()} ₽
              </Typography>
              <Typography variant="body2">
                <strong>Сделок:</strong> {numTrades.toLocaleString()}
              </Typography>
              <Typography variant="body2">
                <strong>Время:</strong> {time ? time.slice(11, 19) : '—'}
              </Typography>
            </Box>
          </Paper>
        )}

        <Box mt={3} textAlign="center">
          <Typography
            component="button"
            variant="body2"
            onClick={() => navigate(-1)}
            sx={{
              background: 'none',
              border: 'none',
              color: 'primary.main',
              cursor: 'pointer',
              textDecoration: 'none',
              fontWeight: 500,
              '&:hover': { textDecoration: 'underline' }
            }}
          >
            ← Вернуться назад
          </Typography>
        </Box>
      </Container>
    </AppLayout>
  );
}