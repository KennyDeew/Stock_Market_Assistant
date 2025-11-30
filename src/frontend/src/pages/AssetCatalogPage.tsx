import { useState, useEffect } from 'react';
import {
  Container,
  Typography,
  TextField,
  Button,
  Paper,
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableRow,
  Box,
  Select,
  MenuItem,
  FormControl,
  InputLabel,
  CircularProgress,
  Alert,
  Link,
} from '@mui/material';
import SearchIcon from '@mui/icons-material/Search';
import { portfolioApi, portfolioAssetApi } from '../services/portfolioApi';
import AddToPortfolioModal from '../components/AddToPortfolioModal';
import { useNavigate } from 'react-router-dom';
import { useAuth } from '../hooks/useAuth';
import { useSnackbar } from '../hooks/useSnackbar';
import { useAssetSearch } from '../hooks/useAssetSearch';
import type { PortfolioShort } from '../types/portfolioTypes';
import type { AssetShort } from '../types/assetTypes';
import { PortfolioAssetTypeValue } from '../types/portfolioAssetTypes';
import AppLayout from '../components/AppLayout';

export default function AssetCatalogPage() {
  const { isAuthenticated } = useAuth();
  const navigate = useNavigate();
  const { openSnackbar } = useSnackbar();
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [selectedAsset, setSelectedAsset] = useState<AssetShort | null>(null);
  const [portfolios, setPortfolios] = useState<PortfolioShort[]>([]);
  const [loadingPortfolios, setLoadingPortfolios] = useState(true);

  const { assets, loading, error: searchError, searchAssets, loadAssetsImmediately } = useAssetSearch();
  const [type, setType] = useState<string>('');
  const [search, setSearch] = useState('');

  // Загрузка портфелей
  useEffect(() => {
    const loadPortfolios = async () => {
      const stored = localStorage.getItem('user');
      if (!isAuthenticated || !stored) {
        setPortfolios([]);
        setLoadingPortfolios(false);
        return;
      }

      try {
        const { user } = JSON.parse(stored);
        if (!user?.id) return;

        setLoadingPortfolios(true);
        const response = await portfolioApi.getAll(user.id, 1, 100);
        setPortfolios(response.items);
      } catch (err) {
        console.error('Не удалось загрузить портфели:', err);
        openSnackbar('Не удалось загрузить портфели', 'error');
        setPortfolios([]);
      } finally {
        setLoadingPortfolios(false);
      }
    };

    loadPortfolios();
  }, [isAuthenticated, openSnackbar]);


  const handleSearch = () => {
    searchAssets(search, type);
  };

  const handleKeyDown = (e: React.KeyboardEvent) => {
    if (e.key === 'Enter') {
      handleSearch();
    }
  };

  // Загружаем ВСЕ активы при открытии страницы (без дебаунса!)
  useEffect(() => {
    // Используем прямую загрузку, чтобы не ждать debounce
    loadAssetsImmediately('', '');
  }, [loadAssetsImmediately]);

  return (
    <AppLayout>
      <Container>
        <Typography variant="h4" component="h1" gutterBottom fontWeight={600}>
          Каталог активов
        </Typography>

        {!isAuthenticated && (
          <Alert severity="info" sx={{ mb: 3 }}>
            Войдите, чтобы добавлять активы в портфели
          </Alert>
        )}

        <Paper sx={{ p: 3, mb: 3 }}>
          <Box display="flex" gap={2} flexWrap="wrap" alignItems="center">
            <TextField
              label="Поиск (тикер или название)"
              value={search}
              onChange={(e) => setSearch(e.target.value)}
              onKeyDown={handleKeyDown}
              placeholder="Например: SBER"
              fullWidth
              sx={{ flexBasis: { xs: '100%', sm: '400px' } }}
            />
            <FormControl sx={{ flexBasis: '200px' }}>
              <InputLabel>Тип</InputLabel>
              <Select value={type} onChange={(e) => setType(e.target.value)} label="Тип">
                <MenuItem value="">Все</MenuItem>
                <MenuItem value="Stock">Акции</MenuItem>
                <MenuItem value="Bond">Облигации</MenuItem>
                <MenuItem value="Crypto">Криптовалюта</MenuItem>
              </Select>
            </FormControl>
            <Button
              variant="contained"
              startIcon={<SearchIcon />}
              onClick={handleSearch}
              sx={{ flexShrink: 0 }}
            >
              Найти
            </Button>
          </Box>
          <Typography variant="body2" color="textSecondary" sx={{ mt: 1 }}>
            Нажмите «Найти», чтобы применить фильтры
          </Typography>
        </Paper>

        {searchError && <Alert severity="error" sx={{ mb: 3 }}>{searchError}</Alert>}

        {loading ? (
          <Box display="flex" justifyContent="center" alignItems="center" my={4}>
            <CircularProgress size={24} />
            <Typography variant="body1" color="textSecondary" sx={{ ml: 2 }}>
              Загрузка активов...
            </Typography>
          </Box>
        ) : (
          <Paper>
            <Table>
              <TableHead>
                <TableRow>
                  <TableCell><strong>Тикер</strong></TableCell>
                  <TableCell><strong>Название</strong></TableCell>
                  <TableCell><strong>Тип</strong></TableCell>
                  <TableCell><strong>Цена</strong></TableCell>
                  <TableCell><strong>Изменение</strong></TableCell>
                  <TableCell align="right"><strong>Действия</strong></TableCell>
                </TableRow>
              </TableHead>
              <TableBody>
                {assets.length === 0 ? (
                  <TableRow>
                    <TableCell colSpan={6} align="center" sx={{ py: 3 }}>
                      <Typography color="textSecondary">
                        {search || type ? 'Активы не найдены' : 'Нет доступных активов'}
                      </Typography>
                    </TableCell>
                  </TableRow>
                ) : (
                  assets.map((asset) => (
                    <TableRow key={asset.ticker}>
                      <TableCell sx={{ fontWeight: 500, fontSize: '1.1rem' }}>
                        <Link
                          component="button"
                          variant="body1"
                          onClick={() => navigate(`/asset/${asset.ticker}`)}
                          sx={{
                            color: 'primary.main',
                            textDecoration: 'none',
                            fontWeight: 500,
                            '&:hover': { textDecoration: 'underline' }
                          }}
                        >
                          {asset.ticker}
                        </Link>
                      </TableCell>
                      <TableCell>{asset.shortName}</TableCell>
                      <TableCell>
                        <Box
                          component="span"
                          sx={{
                            fontSize: '0.8rem',
                            px: 1,
                            py: 0.5,
                            borderRadius: 1,
                            bgcolor:
                              asset.type === 'stock'
                                ? 'primary.light'
                                : asset.type === 'bond'
                                ? 'success.light'
                                : 'warning.light',
                            color: 'white',
                          }}
                        >
                          {asset.type === 'stock' ? 'Акция' : asset.type === 'bond' ? 'Облигация' : 'Крипта'}
                        </Box>
                      </TableCell>
                      <TableCell>
                        {typeof asset.currentPrice === 'number'
                          ? asset.currentPrice.toFixed(2)
                          : '—'} {asset.currency}
                      </TableCell>
                      <TableCell>
                        <Box
                          component="span"
                          sx={{
                            color:
                              asset.changePercent != null && asset.changePercent >= 0
                                ? 'success.main'
                                : 'error.main',
                            fontWeight: 500,
                          }}
                        >
                          {asset.changePercent != null && asset.changePercent >= 0 ? '+' : ''}
                          {asset.changePercent != null ? asset.changePercent.toFixed(2) : '0.00'}%
                        </Box>
                      </TableCell>
                      <TableCell align="right">
                        <Button
                          size="small"
                          variant="contained"
                          color="primary"
                          onClick={() => {
                            setSelectedAsset(asset);
                            setIsModalOpen(true);
                          }}
                          disabled={!isAuthenticated}
                        >
                          Добавить
                        </Button>
                      </TableCell>
                    </TableRow>
                  ))
                )}
              </TableBody>
            </Table>
          </Paper>
        )}

        {/* Модалка добавления в портфель */}
        {selectedAsset && (
          <AddToPortfolioModal
            open={isModalOpen}
            onClose={() => {
              setIsModalOpen(false);
              setSelectedAsset(null);
            }}
            portfolios={portfolios}
            onAdd={async (asset, portfolioId, quantity, purchasePrice) => {
              const typeMap: Record<AssetShort['type'], PortfolioAssetTypeValue> = {
                stock: PortfolioAssetTypeValue.Share,
                bond: PortfolioAssetTypeValue.Bond,
                crypto: PortfolioAssetTypeValue.Crypto,
              };

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
          />
        )}

        <Box mt={3} textAlign="center">
          <Link
            component="button"
            variant="body2"
            onClick={() => navigate('/')}
            sx={{ textDecoration: 'none', color: 'primary.main', fontWeight: 500 }}
          >
            ← Вернуться на главную
          </Link>
        </Box>
      </Container>
    </AppLayout>
  );
}
