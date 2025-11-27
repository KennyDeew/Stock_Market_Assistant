import { useAuth } from '../hooks/useAuth';
import { useSnackbar } from '../hooks/useSnackbar';
import { portfolioApi } from '../services/portfolioApi';
import type { PortfolioShort } from '../types/portfolioTypes';
import {
  Container,
  Typography,
  Paper,
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableRow,
  Box,
  CircularProgress,
  Alert,
  Button,
  useTheme,
} from '@mui/material';
import { useEffect, useState, useMemo } from 'react';
import { useNavigate } from 'react-router-dom';
import Pagination from '@mui/material/Pagination';
import EditPortfolioModal from '../components/EditPortfolioModal';
import AppLayout from '../components/AppLayout';

export default function PortfolioListPage() {
  const { isAuthenticated } = useAuth();
  const navigate = useNavigate();
  const theme = useTheme();
  const [portfolios, setPortfolios] = useState<PortfolioShort[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [page, setPage] = useState(1);
  const [pageSize] = useState(10);
  const [total, setTotal] = useState(0);

  // 🔹 Состояние для модального окна редактирования
  const [editingPortfolio, setEditingPortfolio] = useState<PortfolioShort | null>(null);
  const { openSnackbar } = useSnackbar();

  // Получаем userId из localStorage
  const userId = useMemo((): string | null => {
    const storedUser = localStorage.getItem('user');
    if (!storedUser) return null;
    try {
      const { user } = JSON.parse(storedUser);
      return typeof user?.id === 'string' ? user.id : null;
    } catch (e) {
      console.error('Failed to parse user from localStorage', e);
      return null;
    }
  }, []);

  // Загрузка портфелей
  const loadPortfolios = async () => {
    if (!userId) {
      setError('Не удалось получить ID пользователя.');
      setLoading(false);
      return;
    }
    setLoading(true);
    try {
      const response = await portfolioApi.getAll(userId, page, pageSize);
      setPortfolios(Array.isArray(response.items) ? response.items : []);
      setTotal(response.totalCount || 0);
    } catch (err: any) {
      console.error('Ошибка загрузки портфелей:', err);
      setError(err.response?.data?.message || err.message || 'Не удалось загрузить портфели.');
      setPortfolios([]);
      setTotal(0);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    loadPortfolios();
  }, [userId, page]);

  const handlePageChange = (_: React.ChangeEvent<unknown>, value: number) => setPage(value);

  if (!isAuthenticated) {
    navigate('/login', { replace: true });
    return null;
  }

  // Обработчик сохранения изменений
  const handleSave = async (id: string, data: { name: string; currency: string }) => {
    try {
      await portfolioApi.update(id, data);
      setPortfolios((prev) =>
        prev.map((p) => (p.id === id ? { ...p, name: data.name, currency: data.currency } : p))
      );
      setEditingPortfolio(null);
      openSnackbar('Портфель успешно обновлён', 'success');
    } catch (err: any) {
      openSnackbar('Не удалось обновить портфель', 'error');
      throw err;
    }
  };

  return (
    <AppLayout>
      <Container>
        {/* Заголовок */}
        <Typography variant="h4" component="h1" gutterBottom fontWeight={600} color="text.primary">
          Мои портфели
        </Typography>

        {/* Ошибка */}
        {error && (
          <Alert severity="error" sx={{ mb: 3, borderRadius: 2 }}>
            {error}
          </Alert>
        )}

        {/* Загрузка */}
        {loading ? (
          <Box display="flex" justifyContent="center" my={6}>
            <CircularProgress size={28} color="primary" />
          </Box>
        ) : (
          <>
            {/* Таблица портфелей */}
            <Paper
              sx={{
                borderRadius: 3,
                overflow: 'hidden',
                boxShadow: '0 4px 12px rgba(0,0,0,0.08)',
                border: `1px solid ${theme.palette.divider}`,
              }}
            >
              <Table>
                <TableHead>
                  <TableRow sx={{ backgroundColor: 'background.paper' }}>
                    <TableCell
                      sx={{
                        fontWeight: 600,
                        color: 'text.primary',
                        borderBottom: `2px solid ${theme.palette.divider}`,
                        px: 3,
                        py: 2,
                      }}
                    >
                      Название
                    </TableCell>
                    <TableCell
                      align="left"
                      sx={{
                        fontWeight: 600,
                        color: 'text.primary',
                        borderBottom: `2px solid ${theme.palette.divider}`,
                        px: 3,
                        py: 2,
                      }}
                    >
                      Валюта
                    </TableCell>
                    <TableCell
                      align="right"
                      sx={{
                        fontWeight: 600,
                        color: 'text.primary',
                        borderBottom: `2px solid ${theme.palette.divider}`,
                        px: 3,
                        py: 2,
                      }}
                    >
                      Действия
                    </TableCell>
                  </TableRow>
                </TableHead>
                <TableBody>
                  {portfolios.length === 0 ? (
                    <TableRow>
                      <TableCell colSpan={3} align="center" sx={{ py: 4 }}>
                        <Typography color="text.secondary">У вас пока нет портфелей</Typography>
                      </TableCell>
                    </TableRow>
                  ) : (
                    portfolios.map((p) => (
                      <TableRow
                        key={p.id}
                        hover
                        sx={{
                          '&:hover': {
                            backgroundColor: 'rgba(52, 152, 219, 0.04)',
                          },
                          '&:nth-of-type(even)': {
                            backgroundColor: 'background.default',
                          },
                        }}
                      >
                        {/* Название портфеля */}
                        <TableCell sx={{ px: 3, py: 2 }}>
                          <Button
                            onClick={() => navigate(`/portfolios/${p.id}`)}
                            sx={{
                              fontWeight: 500,
                              color: 'text.primary',
                              textAlign: 'left',
                              justifyContent: 'flex-start',
                              padding: 0,
                              minWidth: 0,
                              '&:hover': {
                                backgroundColor: 'transparent',
                                textDecoration: 'underline',
                              },
                            }}
                          >
                            {p.name}
                          </Button>
                        </TableCell>

                        {/* Валюта */}
                        <TableCell sx={{ px: 3, py: 2 }} align="left">
                          <Typography variant="body2" color="text.secondary" sx={{ fontWeight: 500 }}>
                            {p.currency || '—'}
                          </Typography>
                        </TableCell>

                        {/* Действия */}
                        <TableCell align="right" sx={{ px: 3, py: 2 }}>
                          <Button
                            size="small"
                            color="primary"
                            variant="outlined"
                            onClick={() => setEditingPortfolio(p)} // 🔹 Открываем модалку
                            sx={{
                              textTransform: 'none',
                              borderRadius: 2,
                              px: 1.5,
                              minWidth: 'auto',
                            }}
                          >
                            Редактировать
                          </Button>
                        </TableCell>
                      </TableRow>
                    ))
                  )}
                </TableBody>
              </Table>
            </Paper>

            {/* Пагинация */}
            {total > pageSize && (
              <Box sx={{ mt: 4, display: 'flex', justifyContent: 'center' }}>
                <Pagination
                  count={Math.ceil(total / pageSize)}
                  page={page}
                  onChange={handlePageChange}
                  color="primary"
                  showFirstButton
                  showLastButton
                  size="large"
                  siblingCount={1}
                  boundaryCount={1}
                />
              </Box>
            )}

            {/* Кнопки */}
            <Box mt={5} textAlign="center">
              <Button
                variant="contained"
                color="primary"
                onClick={() => navigate('/portfolios/create')}
                sx={{
                  mr: 2,
                  px: 4,
                  py: 1.2,
                  fontWeight: 600,
                  borderRadius: 3,
                  textTransform: 'none',
                  fontSize: '1rem',
                }}
                size="large"
              >
                Создать портфель
              </Button>
              <Button
                variant="outlined"
                onClick={() => navigate('/')}
                sx={{
                  px: 4,
                  py: 1.2,
                  fontWeight: 500,
                  borderRadius: 3,
                  textTransform: 'none',
                  borderColor: 'primary.main',
                  color: 'primary.main',
                }}
                size="large"
              >
                На главную
              </Button>
            </Box>
          </>
        )}

        {/* 🔹 Модальное окно редактирования */}
        {editingPortfolio && (
          <EditPortfolioModal
            open={true}
            onClose={() => setEditingPortfolio(null)}
            portfolio={editingPortfolio}
            onSave={handleSave}
          />
        )}
      </Container>
    </AppLayout>
  );
}
