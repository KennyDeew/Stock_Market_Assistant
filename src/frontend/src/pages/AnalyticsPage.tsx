import {
  Box,
  Typography,
  Paper,
  Tabs,
  Tab,
  CircularProgress,
  Alert,
  Container,
} from '@mui/material';
import { useState, useEffect } from 'react';
import { useAuth } from '../hooks/useAuth';
import { useSnackbar } from '../hooks/useSnackbar';
import AppLayout from '../components/AppLayout';
import ProtectedLayout from '../layouts/ProtectedLayout';
import { analyticsApiService } from '../services/analyticsApi';
import type { AssetRatingDto, TransactionResponseDto, PortfolioComparisonItem } from '../types/analyticsTypes';
import {
  LineChart,
  Line,
  BarChart,
  Bar,
  XAxis,
  YAxis,
  CartesianGrid,
  Tooltip as RechartsTooltip,
  Legend,
  ResponsiveContainer,
  PieChart,
  Pie,
  Cell,
} from 'recharts';
import { format, parseISO } from 'date-fns';

type TabValue = 'top-assets' | 'transactions' | 'comparison';

export default function AnalyticsPage() {
  const { isAuthenticated } = useAuth();
  const { openSnackbar } = useSnackbar();
  const [tab, setTab] = useState<TabValue>('top-assets');
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  // Данные
  const [topBought, setTopBought] = useState<AssetRatingDto[]>([]);
  const [topSold, setTopSold] = useState<AssetRatingDto[]>([]);
  const [transactions, setTransactions] = useState<TransactionResponseDto[]>([]);
  const [portfolioComparison, setPortfolioComparison] = useState<PortfolioComparisonItem[]>([]);

  useEffect(() => {
    if (!isAuthenticated) return;

    const loadData = async () => {
      setLoading(true);
      setError(null);
      const now = new Date();
      const weekAgo = new Date(now);
      weekAgo.setDate(now.getDate() - 7);

      const formatISO = (d: Date) => d.toISOString();

      try {
        const [boughtRes, soldRes, transRes] = await Promise.all([
          analyticsApiService.getTopBought(10, formatISO(weekAgo), formatISO(now), 'Global'),
          analyticsApiService.getTopSold(10, formatISO(weekAgo), formatISO(now), 'Global'),
          analyticsApiService.getAllTransactions('Week'),
        ]);

        setTopBought(boughtRes.assets);
        setTopSold(soldRes.assets);
        setTransactions(transRes.transactions);

        // Мок сравнения портфелей (в будущем — из API)
        setPortfolioComparison([
          { name: 'Портфель 1', value: 150000 },
          { name: 'Портфель 2', value: 210000 },
          { name: 'Портфель 3', value: 95000 },
        ]);
      } catch (err) {
        console.error('Ошибка загрузки аналитики:', err);
        setError('Не удалось загрузить данные аналитики');
        openSnackbar('Ошибка аналитики', 'error');
      } finally {
        setLoading(false);
      }
    };

    loadData();
  }, [isAuthenticated, openSnackbar]);

  const handleChangeTab = (_: React.SyntheticEvent, newValue: TabValue) => {
    setTab(newValue);
  };

  // Группировка транзакций по дате
  const transactionsByDate = transactions.reduce<Record<string, number>>((acc, tx) => {
    const date = format(parseISO(tx.transactionTime), 'yyyy-MM-dd');
    acc[date] = (acc[date] || 0) + tx.totalAmount;
    return acc;
  }, {});

  const dailyTransactions = Object.entries(transactionsByDate)
    .map(([date, amount]) => ({ date, amount }))
    .sort((a, b) => a.date.localeCompare(b.date));

  // Цвета для чарта
  const COLORS = ['#0088FE', '#00C49F', '#FFBB28', '#FF8042', '#8833FF'];

  const content = (
    <AppLayout>
      <Container maxWidth="lg">
        <Typography variant="h4" component="h1" gutterBottom fontWeight={700} sx={{ mt: 3 }}>
          📊 Аналитика инвестиций
        </Typography>

        <Paper sx={{ mb: 4 }}>
          <Tabs value={tab} onChange={handleChangeTab} variant="fullWidth">
            <Tab label="Топ активов" value="top-assets" />
            <Tab label="Транзакции" value="transactions" />
            <Tab label="Сравнение портфелей" value="comparison" />
          </Tabs>
        </Paper>

        {loading && (
          <Box display="flex" justifyContent="center" my={4}>
            <CircularProgress size={30} />
            <Typography variant="body1" color="textSecondary" ml={2}>
              Загрузка данных...
            </Typography>
          </Box>
        )}

        {error && <Alert severity="error" sx={{ mb: 3 }}>{error}</Alert>}

        {!loading && !error && (
          <>
            {tab === 'top-assets' && (
              <Box>
                <Typography variant="h5" gutterBottom fontWeight={600}>
                  🔝 Топ активов по покупкам и продажам
                </Typography>

                <Box sx={{ display: 'grid', gridTemplateColumns: { xs: '1fr', md: '1fr 1fr' }, gap: 4, mb: 4 }}>
                  {/* Покупки */}
                  <Paper sx={{ p: 2 }}>
                    <Typography variant="h6" gutterBottom color="success.main">
                      🛒 Покупки
                    </Typography>
                    <ResponsiveContainer width="100%" height={300}>
                      <BarChart data={topBought.slice(0, 5)} margin={{ top: 10, right: 30, left: 0, bottom: 0 }}>
                        <CartesianGrid strokeDasharray="3 3" />
                        <XAxis dataKey="ticker" />
                        <YAxis />
                        <RechartsTooltip formatter={(value: number) => [`${value} ₽`, 'Объём']} />
                        <Legend />
                        <Bar dataKey="totalBuyAmount" fill="#00C49F" name="Объём покупок" />
                      </BarChart>
                    </ResponsiveContainer>
                  </Paper>

                  {/* Продажи */}
                  <Paper sx={{ p: 2 }}>
                    <Typography variant="h6" gutterBottom color="error.main">
                      📉 Продажи
                    </Typography>
                    <ResponsiveContainer width="100%" height={300}>
                      <BarChart data={topSold.slice(0, 5)} margin={{ top: 10, right: 30, left: 0, bottom: 0 }}>
                        <CartesianGrid strokeDasharray="3 3" />
                        <XAxis dataKey="ticker" />
                        <YAxis />
                        <RechartsTooltip formatter={(value: number) => [`${value} ₽`, 'Объём']} />
                        <Legend />
                        <Bar dataKey="totalSellAmount" fill="#FF5252" name="Объём продаж" />
                      </BarChart>
                    </ResponsiveContainer>
                  </Paper>
                </Box>

                <Paper sx={{ p: 3 }}>
                  <Typography variant="h6" gutterBottom>Разделение по активам</Typography>
                  <Box height={400}>
                    <ResponsiveContainer width="100%" height="100%">
                      <PieChart>
                        <Pie
                          data={[
                            { name: 'Акции', value: topBought.filter(a => a.assetType === 0).length },
                            { name: 'Облигации', value: topBought.filter(a => a.assetType === 1).length },
                            { name: 'Крипта', value: topBought.filter(a => a.assetType === 2).length },
                          ]}
                          cx="50%"
                          cy="50%"
                          labelLine={false}
                          label={({ name, percent }) =>
                            percent ? `${name} ${(percent * 100).toFixed(0)}%` : name
                          }
                          outerRadius={80}
                          fill="#8884d8"
                          dataKey="value"
                        >
                          {Array.from({ length: 3 }).map((_, index) => (
                            <Cell key={`cell-${index}`} fill={COLORS[index % COLORS.length]} />
                          ))}
                        </Pie>
                        <Legend />
                        <RechartsTooltip />
                      </PieChart>
                    </ResponsiveContainer>
                  </Box>
                </Paper>
              </Box>
            )}

            {tab === 'transactions' && (
              <Box>
                <Typography variant="h5" gutterBottom fontWeight={600}>
                  💸 История транзакций
                </Typography>

                {dailyTransactions.length === 0 ? (
                  <Alert severity="info" sx={{ mb: 3 }}>
                    Нет данных о транзакциях за выбранный период
                  </Alert>
                ) : (
                  <Paper sx={{ p: 3 }}>
                    <ResponsiveContainer width="100%" height={400}>
                      <LineChart data={dailyTransactions}>
                        <CartesianGrid strokeDasharray="3 3" />
                        <XAxis dataKey="date" />
                        <YAxis />
                        <RechartsTooltip formatter={(value: number) => [`${value.toFixed(2)} ₽`, 'Сумма']} />
                        <Legend />
                        <Line type="monotone" dataKey="amount" stroke="#8884d8" name="Объём сделок" dot={false} />
                      </LineChart>
                    </ResponsiveContainer>
                  </Paper>
                )}
              </Box>
            )}

            {tab === 'comparison' && (
              <Box>
                <Typography variant="h5" gutterBottom fontWeight={600}>
                  📈 Сравнение портфелей
                </Typography>

                <Box sx={{ display: 'grid', gridTemplateColumns: { xs: '1fr', md: '1fr 1fr' }, gap: 4 }}>
                  <Paper sx={{ p: 2 }}>
                    <ResponsiveContainer width="100%" height={400}>
                      <BarChart data={portfolioComparison} layout="vertical" margin={{ left: 120 }}>
                        <CartesianGrid strokeDasharray="3 3" />
                        <XAxis type="number" />
                        <YAxis dataKey="name" type="category" />
                        <RechartsTooltip formatter={(value: number) => [`${value.toLocaleString()} ₽`, 'Стоимость']} />
                        <Bar dataKey="value" fill="#8884d8" name="Стоимость" />
                      </BarChart>
                    </ResponsiveContainer>
                  </Paper>

                  <Paper sx={{ p: 2 }}>
                    <ResponsiveContainer width="100%" height={400}>
                      <PieChart>
                        <Pie
                          data={portfolioComparison}
                          cx="50%"
                          cy="50%"
                          labelLine={false}
                          label={({ name, percent }) =>
                            percent ? `${name} ${(percent * 100).toFixed(0)}%` : name
                          }
                          outerRadius={100}
                          fill="#8884d8"
                          dataKey="value"
                        >
                          {portfolioComparison.map((_entry, index) => (
                            <Cell key={`cell-${index}`} fill={COLORS[index % COLORS.length]} />
                          ))}
                        </Pie>
                        <Legend />
                        <RechartsTooltip formatter={(value: number) => [`${value.toLocaleString()} ₽`, 'Стоимость']} />
                      </PieChart>
                    </ResponsiveContainer>
                  </Paper>
                </Box>
              </Box>
            )}
          </>
        )}
      </Container>
    </AppLayout>
  );

  return isAuthenticated ? <ProtectedLayout>{content}</ProtectedLayout> : null;
}