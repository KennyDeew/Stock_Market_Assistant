import { useAuth } from '../hooks/useAuth';
import {
  Container,
  Typography,
  Paper,
  Button,
  Card,
  Box,
  IconButton,
  Tooltip,
  Chip,
} from '@mui/material';
import { Link } from 'react-router-dom';
import AccountBalanceIcon from '@mui/icons-material/AccountBalance';
import ShowChartIcon from '@mui/icons-material/ShowChart';
import ReceiptLongIcon from '@mui/icons-material/ReceiptLong';
import NotificationsIcon from '@mui/icons-material/Notifications';
import AddCircleIcon from '@mui/icons-material/AddCircle';
import type { AssetShort } from '../types/assetTypes';
import { assetApi } from '../services/assetApi';
import { useEffect, useState } from 'react';
import AlertModal from '../components/AlertModal';
import { alertsApi } from '../services/alertsApi';
import Table from '@mui/material/Table';
import TableBody from '@mui/material/TableBody';
import TableCell from '@mui/material/TableCell';
import TableHead from '@mui/material/TableHead';
import TableRow from '@mui/material/TableRow';
import TableContainer from '@mui/material/TableContainer';
import FormControl from '@mui/material/FormControl';
import InputLabel from '@mui/material/InputLabel';
import Select from '@mui/material/Select';
import MenuItem from '@mui/material/MenuItem';
import CircularProgress from '@mui/material/CircularProgress';
import Alert from '@mui/material/Alert';
import Pagination from '@mui/material/Pagination';
import TextField from '@mui/material/TextField';

import ProtectedLayout from '../layouts/ProtectedLayout.tsx';
import PublicLayout from '../layouts/PublicLayout.tsx';
import AddToPortfolioModal from '../components/AddToPortfolioModal';
import { portfolioApi, portfolioAssetApi } from '../services/portfolioApi';
import { useSnackbar } from '../hooks/useSnackbar';
import type { PortfolioShort } from '../types/portfolioTypes.ts';
import { getAssetTypeName, getAssetTypeColor } from '../utils/assetTypeUtils';
import { PortfolioAssetTypeValue } from '../types/portfolioAssetTypes.ts';


export default function DashboardPage() {
  const { isAuthenticated } = useAuth();
  const { openSnackbar } = useSnackbar();
  const [assets, setAssets] = useState<AssetShort[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [typeFilter, setTypeFilter] = useState<string>('');
  const [search, setSearch] = useState('');
  const [page, setPage] = useState(0);
  const [pageSize] = useState(15);
  const [total, setTotal] = useState(0);
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [isPortfolioModalOpen, setIsPortfolioModalOpen] = useState(false);
  const [selectedAsset, setSelectedAsset] = useState<AssetShort | null>(null);
  const [portfolios, setPortfolios] = useState<PortfolioShort[]>([]);
  const [loadingPortfolios, setLoadingPortfolios] = useState(true);

  useEffect(() => setPage(0), [typeFilter, search]);

  useEffect(() => {
    const loadAssets = async () => {
      setLoading(true);
      try {
        const response = await assetApi.getAll({ search, type: typeFilter, page, pageSize });
        setAssets(response.data);
        setTotal(response.total);
      } catch (err: any) {
        setError(err.message || 'Не удалось загрузить котировки');
      } finally {
        setLoading(false);
      }
    };
    loadAssets();
  }, [page, typeFilter, search]);

  useEffect(() => {
    if (!isAuthenticated) return;

    setLoadingPortfolios(true);
    try {
      const storedUser = localStorage.getItem('user');
      if (!storedUser) {
        openSnackbar('Не удалось получить данные пользователя', 'error');
        setLoadingPortfolios(false);
        return;
      }

      const userId = JSON.parse(storedUser)?.user?.id;
      if (!userId) {
        openSnackbar('Не удалось найти ID пользователя', 'error');
        setLoadingPortfolios(false);
        return;
      }

      portfolioApi.getAll(userId, 1, 100)
        .then(res => setPortfolios(res.items))
        .catch((err) => {
          console.error('Failed to load portfolios:', err);
          openSnackbar('Не удалось загрузить портфели', 'error');
        })
        .finally(() => setLoadingPortfolios(false));
    } catch (err) {
      console.error('Failed to parse user data:', err);
      openSnackbar('Ошибка данных пользователя', 'error');
      setLoadingPortfolios(false);
    }
  }, [isAuthenticated]);

  const content = (
    <Container maxWidth="md" sx={{ py: 4 }}>
      <Typography
        variant="h3"
        component="h1"
        align="center"
        fontWeight={700}
        sx={{
          mb: 4,
          mt: 2,
          fontSize: { xs: '1.8rem', sm: '2.2rem', md: '2.5rem' },
          background: 'linear-gradient(90deg, #2C3E50 40%, #3498DB 100%)',
          WebkitBackgroundClip: 'text',
          WebkitTextFillColor: 'transparent',
          backgroundClip: 'text',
          textDecoration: 'underline',
          textUnderlineOffset: '8px',
          textDecorationColor: 'primary.main',
          textDecorationThickness: '2px',
        }}
      >
        Добро пожаловать в Stock Market Assistant
      </Typography>
      <Box
        sx={{
          display: 'grid',
          gridTemplateColumns: { xs: '1fr', md: 'repeat(3, 1fr)' },
          gap: 3,
          mb: 5,
        }}
      >
        {[
          {
            to: '/assets',
            icon: <ShowChartIcon sx={{ fontSize: 36 }} color="primary" />,
            title: 'Котировки',
            subtitle: 'Акции, облигации, крипта',
            desc: 'Следите за ценами в реальном времени.',
          },
          {
            to: '/portfolios',
            icon: <AccountBalanceIcon sx={{ fontSize: 36 }} color="success" />,
            title: 'Портфели',
            subtitle: 'Управление инвестициями',
            desc: isAuthenticated
              ? 'Создавайте, отслеживайте доходность.'
              : 'Доступно после входа.',
            disabled: !isAuthenticated,
          },
          {
            to: isAuthenticated ? '/alerts' : '/login',
            icon: <ReceiptLongIcon sx={{ fontSize: 36 }} color="warning" />,
            title: 'Уведомления',
            subtitle: 'Целевые цены',
            desc: isAuthenticated
              ? 'Подписывайтесь на цели.'
              : 'Доступно после входа.',
            disabled: !isAuthenticated,
          },
        ].map((item, i) => (
          <Link key={i} to={item.to} style={{ textDecoration: 'none' }}>
            <Card
              sx={{
                p: 3,
                cursor: item.disabled ? 'not-allowed' : 'pointer',
                opacity: item.disabled ? 0.6 : 1,
                '&:hover': {
                  transform: item.disabled ? 'none' : 'translateY(-4px)',
                  boxShadow: item.disabled ? 'none' : '0 4px 12px rgba(52, 152, 219, 0.15)',
                },
                border: '1px solid #ECF0F1',
              }}
            >
              {item.icon}
              <Typography variant="h6" fontWeight={600} mt={1} mb={0.5}>
                {item.title}
              </Typography>
              <Typography variant="subtitle2" color="text.secondary" mb={1}>
                {item.subtitle}
              </Typography>
              <Typography variant="body2" color="text.secondary">
                {item.desc}
              </Typography>
            </Card>
          </Link>
        ))}
      </Box>

      {/* Таблица активов */}
      <Paper sx={{ p: 3, mb: 4 }}>
        <Box>
          <Typography variant="h6" gutterBottom fontWeight={500}>
            Актуальные котировки
          </Typography>
          <Box sx={{ display: 'flex', gap: 2, flexWrap: 'wrap', alignItems: 'center', mb: 2 }}>
            <FormControl size="small" sx={{ flexBasis: { xs: '100%', sm: 200 } }}>
              <InputLabel>Тип актива</InputLabel>
              <Select
                value={typeFilter}
                onChange={(e) => setTypeFilter(e.target.value)}
                label="Тип актива"
              >
                <MenuItem value="">Все</MenuItem>
                <MenuItem value="Stock">Акции</MenuItem>
                <MenuItem value="Bond">Облигации</MenuItem>
                <MenuItem value="Crypto">Криптовалюта</MenuItem>
              </Select>
            </FormControl>
            <TextField
              size="small"
              label="Поиск по тикеру или названию"
              value={search}
              onChange={(e) => setSearch(e.target.value)}
              placeholder="Например: SBER"
              sx={{ flex: 1, minWidth: 200 }}
            />
            <Button
              variant="outlined"
              color="secondary"
              onClick={() => setSearch('')}
              disabled={!search}
            >
              Сбросить
            </Button>
          </Box>

          {error && <Alert severity="error" sx={{ mb: 2 }}>{error}</Alert>}

          {loading ? (
            <Box display="flex" justifyContent="center" my={4}>
              <CircularProgress size={24} />
            </Box>
          ) : (
            <Box>
              <TableContainer>
                <Table>
                  <TableHead>
                    <TableRow>
                      <TableCell><strong>Тикер</strong></TableCell>
                      <TableCell><strong>Название</strong></TableCell>
                      <TableCell><strong>Тип</strong></TableCell>
                      <TableCell align="right"><strong>Цена</strong></TableCell>
                      <TableCell align="right"><strong>Изм.</strong></TableCell>
                      {isAuthenticated && (
                        <TableCell align="right"><strong>Действие</strong></TableCell>
                      )}
                    </TableRow>
                  </TableHead>
                  <TableBody>
                    {assets.length === 0 ? (
                      <TableRow>
                        <TableCell
                          colSpan={isAuthenticated ? 6 : 5}
                          align="center"
                          sx={{ py: 3 }}
                        >
                          <Typography color="textSecondary">Нет активов</Typography>
                        </TableCell>
                      </TableRow>
                    ) : (
                      assets.map((asset) => (
                        <TableRow key={asset.ticker}>
                          <TableCell sx={{ fontWeight: 500 }}>{asset.ticker}</TableCell>
                          <TableCell>{asset.shortName}</TableCell>
                          <TableCell>
                            <Chip
                              label={getAssetTypeName(asset.type)}
                              size="small"
                              sx={{
                                '&.MuiChip-root': {
                                  bgcolor: (theme) => {
                                    const color = getAssetTypeColor(asset.type);
                                    return theme.palette[color].main;
                                  },
                                  color: 'white',
                                },
                              }}
                            />
                          </TableCell>
                          <TableCell align="right">
                            {typeof asset.currentPrice === 'number'
                              ? asset.currentPrice.toFixed(2)
                              : '—'} {asset.currency}
                          </TableCell>
                          <TableCell align="right">
                            <Box
                              component="span"
                              sx={{
                                color:
                                  typeof asset.changePercent === 'number' && asset.changePercent >= 0
                                    ? 'success.main'
                                    : 'error.main',
                                fontWeight: 500,
                              }}
                            >
                              {typeof asset.changePercent === 'number'
                                ? (asset.changePercent >= 0 ? '+' : '') + asset.changePercent.toFixed(2)
                                : '—'}
                              %
                            </Box>
                          </TableCell>
                          {isAuthenticated && (
                            <TableCell align="right">
                              <Box display="flex" justifyContent="flex-end" gap={0.5}>
                                <Tooltip title="Установить уведомление о цене">
                                  <IconButton
                                    color="warning"
                                    size="small"
                                    onClick={() => {
                                      setSelectedAsset(asset);
                                      setIsModalOpen(true);
                                    }}
                                    aria-label="уведомить о цене"
                                  >
                                    <NotificationsIcon fontSize="small" />
                                  </IconButton>
                                </Tooltip>
                                <Tooltip title="Добавить в портфель">
                                  <IconButton
                                    color="primary"
                                    size="small"
                                    onClick={() => {
                                      setSelectedAsset(asset);
                                      setIsPortfolioModalOpen(true);
                                    }}
                                    aria-label="добавить в портфель"
                                  >
                                    <AddCircleIcon fontSize="small" />
                                  </IconButton>
                                </Tooltip>
                              </Box>
                            </TableCell>
                          )}
                        </TableRow>
                      ))
                    )}
                  </TableBody>
                </Table>
              </TableContainer>

              {total > pageSize && (
                <Box sx={{ mt: 3, display: 'flex', justifyContent: 'center' }}>
                  <Pagination
                    count={Math.ceil(total / pageSize)}
                    page={page + 1}
                    onChange={(_, v) => setPage(v - 1)}
                    color="primary"
                    showFirstButton
                    showLastButton
                  />
                </Box>
              )}
            </Box>
          )}
        </Box>
      </Paper>

      {/* Призыв к регистрации */}
      {!isAuthenticated && (
        <Paper sx={{ p: 4, textAlign: 'center', mb: 4 }}>
          <Typography variant="h6" gutterBottom fontWeight={500}>
            Хотите управлять портфелями?
          </Typography>
          <Box sx={{ display: 'flex', justifyContent: 'center', gap: 2, mt: 2 }}>
            <Button href="/login" variant="contained" color="primary" size="large" sx={{ px: 4 }}>
              Войти
            </Button>
            <Button href="/register" variant="outlined" size="large" sx={{ px: 4 }}>
              Зарегистрироваться
            </Button>
          </Box>
        </Paper>
      )}

      {/* Модалка уведомлений */}
      {selectedAsset && (
        <AlertModal
          open={isModalOpen}
          onClose={() => {
            setIsModalOpen(false);
            setSelectedAsset(null);
          }}
          asset={selectedAsset}
          onSubmit={async (data) => {
            try {
              await alertsApi.create({
                assetId: selectedAsset.ticker,
                targetPrice: data.targetPrice,
                condition: data.condition,
              });
              openSnackbar('Подписка установлена', 'success');
              setIsModalOpen(false);
              setSelectedAsset(null);
            } catch (err: any) {
              openSnackbar('Ошибка: ' + (err.message || 'ошибка'), 'error');
            }
          }}
        />
      )}

      {/* Модалка добавления в портфель */}
      {selectedAsset && (
        <AddToPortfolioModal
          open={isPortfolioModalOpen}
          onClose={() => {
            setIsPortfolioModalOpen(false);
            setSelectedAsset(null);
          }}
          portfolios={portfolios}
          onAdd={async (asset, portfolioId, quantity, purchasePrice) => {
            const typeMap: Record<AssetShort['type'], PortfolioAssetTypeValue> = {
              stock: PortfolioAssetTypeValue.Share,
              bond: PortfolioAssetTypeValue.Bond,
              crypto: PortfolioAssetTypeValue.Crypto,
            };

            if (!portfolioId || !asset.stockCardId) {
              openSnackbar('Ошибка: нет ID', 'error');
              throw new Error('Missing IDs');
            }

            if (quantity <= 0 || purchasePrice <= 0) {
              openSnackbar('Количество и цена должны быть больше 0', 'error');
              throw new Error('Invalid input');
            }

            try {
              await portfolioAssetApi.create({
                portfolioId,
                stockCardId: asset.stockCardId,
                assetType: typeMap[asset.type],
                quantity: Math.floor(quantity),
                purchasePricePerUnit: Number(purchasePrice.toFixed(8)),
              });
              openSnackbar('Актив успешно добавлен', 'success');
            } catch (err: any) {
              openSnackbar('Ошибка: ' + (err.message || 'ошибка'), 'error');
              throw err;
            }
          }}
          loadingPortfolios={loadingPortfolios}
          selectedAsset={selectedAsset}
          initialPurchasePrice={selectedAsset.currentPrice}
        />
      )}


    </Container>
  );

  return isAuthenticated ? (
    <ProtectedLayout>{content}</ProtectedLayout>
  ) : (
    <PublicLayout>{content}</PublicLayout>
  );
}
