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
import AppLayout from '../components/AppLayout';

// Типы
interface MoexSecurity {
  secid: string;
  boardid: string;
  shortname: string;
}

interface MoexMarketData {
  boardid: string;
  last: number | null;
  lastchange: number | null;
  lastchangeprcnt: number | null;
  time: string;
  volume: number;
  numtrades: number;
}

interface MoexResponse {
  securities: {
    data: [string, string, string][];
    columns: string[];
  };
  marketdata: {
    data: [string, number | null, number | null, number | null, string, number, number][];
    columns: string[];
  };
}

export default function AssetDetailPage() {
  const { ticker } = useParams<{ ticker: string }>();
  const navigate = useNavigate();

  const [security, setSecurity] = useState<MoexSecurity | null>(null);
  const [data, setData] = useState<MoexMarketData | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [lastUpdated, setLastUpdated] = useState<Date | null>(null);
  const [priceHistory, setPriceHistory] = useState<Array<{ time: string; price: number; change: number }>>([]);

  const fetchData = async () => {
    if (!ticker) return;

    setLoading(true);
    setError(null);

    try {
      // Шаг 1: Найти бумагу по тикеру
      const secResponse = await fetch(
        `https://iss.moex.com/iss/securities.json?q=${ticker}&limit=1&engine=stock&market=shares`
      );
      const secData: { securities: { data: any[]; columns: string[] } } = await secResponse.json();

      if (!secData.securities.data.length) {
        setError('Актив не найден на МосБирже');
        setLoading(false);
        return;
      }

      const [secRow] = secData.securities.data;
      const columns = secData.securities.columns;
      const sec = columns.reduce((obj: any, col, i) => {
        obj[col.toLowerCase()] = secRow[i];
        return obj;
      }, {}) as MoexSecurity;

      setSecurity(sec);

      // Шаг 2: Получить данные с marketdata
      const dataResponse = await fetch(
        `https://iss.moex.com/iss/engines/stock/markets/shares/securities/${sec.secid}/marketdata.json`
      );
      const marketData: MoexResponse = await dataResponse.json();

      const marketRows = marketData.marketdata.data;
      const marketColumns = marketData.marketdata.columns.map((c) => c.toLowerCase());

      const filtered = marketRows.find((row) => {
        const boardid = row[marketColumns.indexOf('boardid')];
        return boardid === 'TQBR'; // Основной рынок
      });

      if (!filtered) {
        setError('Нет данных по котировкам');
        setLoading(false);
        return;
      }

      const marketObj: any = {};
      filtered.forEach((value, i) => {
        marketObj[marketColumns[i]] = value;
      });

      const newData = marketObj as MoexMarketData;
      setData(newData);
      setLastUpdated(new Date());

      // Добавляем в историю цен
      if (newData.last !== null) {
        setPriceHistory((prev) => {
          const next = [
            ...prev,
            {
              time: newData.time || format(new Date(), 'HH:mm:ss'),
              price: newData.last!,
              change: newData.lastchangeprcnt || 0,
            },
          ].slice(-60); // ← оставляем 60 последних значений (~5 минут)
          return next;
        });
      }
    } catch (err) {
      setError('Не удалось загрузить данные с МосБиржи');
      console.error(err);
    } finally {
      setLoading(false);
    }
  };

  const handleRefresh = () => {
    fetchData();
  };

  useEffect(() => {
    fetchData();
    const interval = setInterval(fetchData, 5000);
    return () => clearInterval(interval);
  }, [ticker]);

  if (!ticker) {
    return (
      <Container maxWidth="sm">
        <Alert severity="error">Тикер не указан</Alert>
      </Container>
    );
  }

  // Подготовка данных для графика
  const chartData = useMemo(() => {
    return priceHistory.map((point) => ({
      name: point.time.slice(-8), // HH:MM:SS
      price: point.price,
      fill: point.change >= 0 ? '#4caf50' : '#f44336',
    }));
  }, [priceHistory]);

  return (
    <AppLayout>
      <Container>
        <Box display="flex" alignItems="center" mb={3}>
          <Typography variant="h5" component="h1">
            {security?.shortname || ticker}
          </Typography>
          <Tooltip title="Обновить">
            <IconButton onClick={handleRefresh} disabled={loading} size="small" sx={{ ml: 1 }}>
              <RefreshIcon />
            </IconButton>
          </Tooltip>
        </Box>

        {error && <Alert severity="error" sx={{ mb: 3 }}>{error}</Alert>}

        {loading && !data ? (
          <Box display="flex" justifyContent="center" my={4}>
            <CircularProgress />
          </Box>
        ) : (
          <Paper sx={{ p: 3 }}>
            {/* Текущая цена */}
            <Box mb={3}>
              <Typography variant="h4" fontWeight="bold" color="text.primary">
                {data?.last != null ? data.last.toFixed(2) : '—'} ₽
              </Typography>
              <Typography
                variant="body1"
                color={data?.lastchangeprcnt != null && data.lastchangeprcnt >= 0 ? 'success.main' : 'error.main'}
                fontWeight="bold"
              >
                {data?.lastchange != null && data.lastchange >= 0 ? '+' : ''}
                {data?.lastchange != null ? data.lastchange.toFixed(2) : '0.00'} ₽
                {' / '}
                {data?.lastchangeprcnt != null && data.lastchangeprcnt >= 0 ? '+' : ''}
                {data?.lastchangeprcnt != null ? data.lastchangeprcnt.toFixed(2) : '0.00'}%
              </Typography>
            </Box>

            {/* График */}
            {priceHistory.length > 1 && (
              <Box mb={3} sx={{ height: 200 }}>
                <Typography variant="subtitle2" gutterBottom>
                  Динамика цены (обновляется каждые 5 сек)
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
                <strong>Объём:</strong> {(data?.volume || 0).toLocaleString()} ₽
              </Typography>
              <Typography variant="body2">
                <strong>Сделок:</strong> {data?.numtrades?.toLocaleString() || '—'}
              </Typography>
              <Typography variant="body2">
                <strong>Время:</strong> {data?.time || '—'}
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
