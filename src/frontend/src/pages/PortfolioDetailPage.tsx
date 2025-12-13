import AddIcon from '@mui/icons-material/Add';
import EditIcon from '@mui/icons-material/Edit';
import ArrowBackIcon from '@mui/icons-material/ArrowBack';
import KeyboardArrowDownIcon from '@mui/icons-material/KeyboardArrowDown';
import KeyboardArrowUpIcon from '@mui/icons-material/KeyboardArrowUp';
import AddCircleIcon from '@mui/icons-material/AddCircle';
import DeleteForeverOutlined from '@mui/icons-material/DeleteForeverOutlined';
import InfoIcon from '@mui/icons-material/Info';
import {
  Container,
  Typography,
  Paper,
  Box,
  Button,
  CircularProgress,
  Alert,
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableRow,
  Chip,
  Collapse,
  IconButton,
  ButtonGroup,
  Tooltip,
} from '@mui/material';
import { useState, useEffect, useCallback } from 'react';
import { useParams, Link } from 'react-router-dom';
import { useSnackbar } from '../hooks/useSnackbar';
import { portfolioApi, portfolioAssetApi } from '../services/portfolioApi';
import type { PortfolioResponse, PortfolioShort, PortfolioAssetProfitLossItem, PortfolioProfitLoss } from '../types/portfolioTypes';
import {
  PortfolioAssetTypeValue,
  type PortfolioAssetShort,
  type PortfolioAssetTransaction,
} from '../types/portfolioAssetTypes';
import EditPortfolioModal from '../components/EditPortfolioModal';
import AddToPortfolioModal from '../components/AddToPortfolioModal';
import TransactionModal from '../components/TransactionModal';
import React from 'react';
import type { AssetShort } from '../types/assetTypes';
import { getAssetTypeName, getAssetTypeColor } from '../utils/assetTypeUtils';
import { getTransactionColor, getTransactionLabel } from '../utils/transactionUtils';
import { toTransactionTypeValue } from '../utils/transactionUtils';
import AppLayout from '../components/AppLayout';

// Компонент операций актива
function AssetTransactions({
  transactions,
  onTransactionDelete,
  onAddTransaction,
  loading,
}: {
  transactions: PortfolioAssetTransaction[];
  onTransactionDelete: (transactionId: string) => void;
  onAddTransaction: () => void;
  loading: boolean;
}) {
  const handleDelete = async (transactionId: string) => {
    const transaction = transactions.find(tx => tx.id === transactionId);
    if (!transaction) {
      console.error('Транзакция не найдена');
      return;
    }
    onTransactionDelete(transactionId);
  };

  if (loading) {
    return (
      <Box sx={{ m: 2, display: 'flex', justifyContent: 'center' }}>
        <CircularProgress size={20} />
      </Box>
    );
  }

  return (
    <Box sx={{ m: 2 }}>
      <Box display="flex" justifyContent="space-between" alignItems="center" mb={1}>
        <Typography variant="h6">Операции</Typography>
        <Button
          size="small"
          startIcon={<AddCircleIcon />}
          onClick={onAddTransaction}
          variant="outlined"
          color="primary"
        >
          Добавить
        </Button>
      </Box>

      {transactions.length === 0 ? (
        <Typography color="text.secondary" variant="body2">
          Нет операций
        </Typography>
      ) : (
        <Table size="small">
          <TableHead>
            <TableRow>
              <TableCell>Тип</TableCell>
              <TableCell>Кол-во</TableCell>
              <TableCell>Цена</TableCell>
              <TableCell>Дата</TableCell>
              <TableCell align="right">Действия</TableCell>
            </TableRow>
          </TableHead>
          <TableBody>
            {transactions.map((tx) => (
              <TableRow key={tx.id}>
                <TableCell>
                  <Chip
                    label={getTransactionLabel(tx.transactionType)}
                    color={getTransactionColor(tx.transactionType)}
                    size="small"
                  />
                </TableCell>
                <TableCell>{tx.quantity}</TableCell>
                <TableCell>{tx.pricePerUnit.toFixed(2)}</TableCell>
                <TableCell>{new Date(tx.transactionDate).toLocaleString()}</TableCell>
                <TableCell align="right">
                  <Button
                    size="small"
                    color="error"
                    startIcon={<DeleteForeverOutlined />}
                    onClick={() => handleDelete(tx.id)}
                  >
                    Удалить
                  </Button>
                </TableCell>
              </TableRow>
            ))}
          </TableBody>
        </Table>
      )}
    </Box>
  );
}

export default function PortfolioDetailPage() {
  const { id } = useParams();
  const { openSnackbar } = useSnackbar();

  const [portfolio, setPortfolio] = useState<PortfolioResponse | null>(null);
  const [assets, setAssets] = useState<PortfolioAssetShort[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [openAssetId, setOpenAssetId] = useState<string | null>(null);
  const [isEditModalOpen, setIsEditModalOpen] = useState(false);
  const [isAddModalOpen, setIsAddModalOpen] = useState(false);
  const [isTransactionModalOpen, setIsTransactionModalOpen] = useState(false);
  const [selectedAssetForTransaction, setSelectedAssetForTransaction] = useState<PortfolioAssetShort | null>(null);

  const [portfolios, setPortfolios] = useState<PortfolioShort[]>([]);
  const [loadingPortfolios, setLoadingPortfolios] = useState(true);

  const [assetTransactions, setAssetTransactions] = useState<Record<string, PortfolioAssetTransaction[]>>({});
  const [loadingTransactions, setLoadingTransactions] = useState<string | null>(null);

  // ✅ Доходность
  const [profitLoss, setProfitLoss] = useState<
    | {
        portfolio: PortfolioProfitLoss;
        assets: PortfolioAssetProfitLossItem[];
      }
    | null
  >(null);
  const [loadingPL, setLoadingPL] = useState(false);
  const [calculationType, setCalculationType] = useState<'Current' | 'Realized'>('Current');

  const loadPortfolio = useCallback(async () => {
    setLoading(true);
    try {
      const data = await portfolioApi.getById(id!);
      setPortfolio(data);
      setAssets(data.assets);
    } catch (error: unknown) {
      const message = error instanceof Error ? error.message : 'Не удалось загрузить портфель';
      setError(message);
    } finally {
      setLoading(false);
    }
  }, [id]);

  const loadProfitLoss = useCallback(async () => {
    if (!id) return;
    setLoadingPL(true);
    try {
      const [portfolioPL, assetsPL] = await Promise.all([
        portfolioApi.getPortfolioProfitLoss(id, calculationType),
        portfolioApi.getPortfolioAssetsProfitLoss(id, calculationType),
      ]);
      setProfitLoss({ portfolio: portfolioPL, assets: assetsPL });
    } catch (error) {
      console.error('Failed to load profit/loss data', error);
      openSnackbar('Не удалось загрузить доходность', 'error');
    } finally {
      setLoadingPL(false);
    }
  }, [id, calculationType, openSnackbar]);

  const loadAvailablePortfolios = useCallback(async () => {
    try {
      const storedUser = localStorage.getItem('user');
      const userId = storedUser ? JSON.parse(storedUser)?.user?.id : null;
      if (!userId) return;

      const response = await portfolioApi.getAll(userId, 1, 100);
      setPortfolios(response.items);
    } catch (error) {
      console.error('Failed to load portfolios for AddToPortfolioModal', error);
      openSnackbar('Не удалось загрузить портфели', 'error');
    } finally {
      setLoadingPortfolios(false);
    }
  }, [openSnackbar]);  

  useEffect(() => {
    if (!id) return;
    loadPortfolio();
    loadAvailablePortfolios();
  }, [id, loadPortfolio, loadAvailablePortfolios]);

  useEffect(() => {
    if (id) loadProfitLoss();
  }, [id, calculationType, loadProfitLoss]);

  const loadAssetTransactions = async (assetId: string, force = false) => {
    if (loadingTransactions === assetId) return;

    if (!force && assetTransactions[assetId]) {
      return;
    }

    setLoadingTransactions(assetId);
    try {
      const fullAsset = await portfolioAssetApi.getById(assetId);
      setAssetTransactions((prev) => ({
        ...prev,
        [assetId]: fullAsset.transactions || [],
      }));
    } catch (err) {
      console.error(`Ошибка загрузки транзакций для актива ${assetId}`, err);
      openSnackbar('Не удалось загрузить операции', 'warning');
    } finally {
      setLoadingTransactions(null);
    }
  };

  const handleSave = async (id: string, data: { name: string; currency: string }) => {
    try {
      await portfolioApi.update(id, data);
      setPortfolio((prev) =>
        prev ? { ...prev, name: data.name, currency: data.currency } : null
      );
      setIsEditModalOpen(false);
      openSnackbar('Портфель обновлён', 'success');
    } catch {
      openSnackbar('Не удалось обновить портфель', 'error');
    }
  };

  const handleAddAsset = async (
    asset: AssetShort,
    portfolioId: string,
    quantity: number,
    purchasePrice: number
  ) => {
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
      openSnackbar('Актив добавлен', 'success');
      await loadPortfolio();
    } catch (err: unknown) {
      openSnackbar('Ошибка: ' + ((err as Error).message || 'ошибка'), 'error');
      throw err;
    }
  };

  const handleDeleteAsset = async (assetId: string) => {
    if (!window.confirm('Удалить актив и все его операции?')) return;
    try {
      await portfolioAssetApi.delete(assetId);
      setAssets((prev) => prev.filter((a) => a.id !== assetId));
      setAssetTransactions((prev) => {
        const next = { ...prev };
        delete next[assetId];
        return next;
      });
      openSnackbar('Актив удалён', 'success');
    } catch {
      openSnackbar('Ошибка при удалении актива', 'error');
    }
  };

  const handleDeleteTransaction = async (assetId: string, transactionId: string) => {
    if (!window.confirm('Удалить транзакцию?')) return;

    try {
      await portfolioAssetApi.deleteTransaction(assetId, transactionId);

      const updatedAsset = await portfolioAssetApi.getById(assetId);

      setAssets((prev) =>
        prev.map((a) =>
          a.id === updatedAsset.id
            ? {
                ...a,
                totalQuantity: updatedAsset.totalQuantity,
                averagePurchasePrice: updatedAsset.averagePurchasePrice,
              }
            : a
        )
      );

      setAssetTransactions((prev) => ({
        ...prev,
        [assetId]: (prev[assetId] || []).filter((tx) => tx.id !== transactionId),
      }));

      openSnackbar('Операция удалена', 'success');
    } catch (err) {
      console.error('Ошибка при удалении транзакции', err);
      openSnackbar('Ошибка при удалении операции', 'error');
    }
  };

  const handleAddTransaction = async (data: {
    transactionType: 'Buy' | 'Sell';
    quantity: number;
    pricePerUnit: number;
    transactionDate: string;
  }) => {
    if (!selectedAssetForTransaction) return;

    try {
      const transactionTypeValue = toTransactionTypeValue(data.transactionType);
      const isSell = data.transactionType === 'Sell';
      const willBeZero = isSell && selectedAssetForTransaction.totalQuantity <= data.quantity;

      await portfolioAssetApi.addTransaction(selectedAssetForTransaction.id, {
        ...data,
        transactionType: transactionTypeValue,
      });

      openSnackbar('Операция добавлена', 'success');

      if (willBeZero) {
        setAssets((prev) => prev.filter((a) => a.id !== selectedAssetForTransaction.id));
        setAssetTransactions((prev) => {
          const next = { ...prev };
          delete next[selectedAssetForTransaction.id];
          return next;
        });
        setOpenAssetId(null);
      } else {
        const updatedAsset = await portfolioAssetApi.getById(selectedAssetForTransaction.id);
        setAssets((prev) =>
          prev.map((a) =>
            a.id === updatedAsset.id
              ? {
                  ...a,
                  totalQuantity: updatedAsset.totalQuantity,
                  averagePurchasePrice: updatedAsset.averagePurchasePrice,
                }
              : a
          )
        );
        await loadAssetTransactions(selectedAssetForTransaction.id, true);
      }

      setIsTransactionModalOpen(false);
    } catch (err) {
      console.error('Ошибка при добавлении операции', err);
      openSnackbar('Ошибка при добавлении операции', 'error');
    }
  };

  if (loading) return <CircularProgress />;
  if (error) return <Alert severity="error">{error}</Alert>;
  if (!portfolio) return <Alert severity="info">Портфель не найден</Alert>;

  return (
    <AppLayout>
      <Container>
        {/* Заголовок */}
        <Box display="flex" justifyContent="space-between" alignItems="center" mb={3}>
          <Typography variant="h4">{portfolio.name}</Typography>
        </Box>

        {/* Кнопки действий */}
        <Box display="flex" justifyContent="flex-end" gap={1} mb={2}>
          <Button
            variant="contained"
            startIcon={<EditIcon />}
            onClick={() => setIsEditModalOpen(true)}
          >
            Редактировать портфель
          </Button>
          <Button
            variant="outlined"
            startIcon={<AddIcon />}
            onClick={() => setIsAddModalOpen(true)}
          >
            Добавить актив
          </Button>
        </Box>

        {/* Таблица активов */}
        <Paper>
          <Table>
            <TableHead>
              <TableRow>
                <TableCell />
                <TableCell>Тикер</TableCell>
                <TableCell>Название</TableCell>
                <TableCell>Тип</TableCell>
                <TableCell>Кол-во</TableCell>
                <TableCell>Ср. цена</TableCell>
                <TableCell align="right">Действия</TableCell>
              </TableRow>
            </TableHead>
            <TableBody>
              {assets.map((asset) => (
                <React.Fragment key={asset.id}>
                  <TableRow>
                    <TableCell>
                      <IconButton
                        size="small"
                        onClick={() => {
                          const isOpening = openAssetId !== asset.id;
                          setOpenAssetId(isOpening ? asset.id : null);
                          if (isOpening) {
                            loadAssetTransactions(asset.id);
                          }
                        }}
                      >
                        {openAssetId === asset.id ? <KeyboardArrowUpIcon /> : <KeyboardArrowDownIcon />}
                      </IconButton>
                    </TableCell>
                    <TableCell sx={{ fontWeight: 500 }}>{asset.ticker}</TableCell>
                    <TableCell sx={{ color: 'text.secondary' }}>{asset.name}</TableCell>
                    <TableCell>
                      <Chip
                        label={getAssetTypeName(asset.assetType)}
                        color={getAssetTypeColor(asset.assetType)}
                        size="small"
                      />
                    </TableCell>
                    <TableCell>{asset.totalQuantity}</TableCell>
                    <TableCell>{asset.averagePurchasePrice.toFixed(2)}</TableCell>
                    <TableCell align="right">
                      <Button
                        size="small"
                        color="error"
                        onClick={() => handleDeleteAsset(asset.id)}
                      >
                        Удалить
                      </Button>
                    </TableCell>
                  </TableRow>

                  <TableRow>
                    <TableCell colSpan={7} sx={{ py: 0 }}>
                      <Collapse in={openAssetId === asset.id} timeout="auto">
                        <AssetTransactions
                          transactions={assetTransactions[asset.id] || []}
                          loading={loadingTransactions === asset.id}
                          onTransactionDelete={(transactionId) =>
                            handleDeleteTransaction(asset.id, transactionId)
                          }
                          onAddTransaction={() => {
                            setSelectedAssetForTransaction(asset);
                            setIsTransactionModalOpen(true);
                          }}
                        />
                      </Collapse>
                    </TableCell>
                  </TableRow>
                </React.Fragment>
              ))}
            </TableBody>
          </Table>
        </Paper>

        {/* Доходность */}
        <Box mt={5}>
          <Typography variant="h5" gutterBottom>
            Доходность портфеля
          </Typography>

          <Box mb={2} display="flex" alignItems="center" gap={1}>
            <ButtonGroup variant="outlined" color="primary">
              <Button
                onClick={() => setCalculationType('Current')}
                disabled={calculationType === 'Current'}
              >
                Текущая
              </Button>
              <Button
                onClick={() => setCalculationType('Realized')}
                disabled={calculationType === 'Realized'}
              >
                Реализованная
              </Button>
            </ButtonGroup>
            <Tooltip
              title={
                <Box>
                  <strong>Текущая:</strong> Прибыль/убыток на основе текущей рыночной цены.<br />
                  <strong>Реализованная:</strong> Прибыль/убыток от проданных активов.
                </Box>
              }
              arrow
            >
              <InfoIcon color="action" fontSize="small" sx={{ cursor: 'pointer' }} />
            </Tooltip>
          </Box>

          {loadingPL ? (
            <Box display="flex" justifyContent="center" my={4}>
              <CircularProgress size={24} />
            </Box>
          ) : profitLoss ? (
            <>
            {/* Общая доходность */}
            <Paper sx={{ p: 3, mb: 3, backgroundColor: 'background.paper' }}>
              <Box
                sx={{
                  display: 'grid',
                  gridTemplateColumns: { xs: '1fr', md: '1fr 1fr' },
                  gap: 3,
                }}
              >
                {/* Всего инвестировано */}
                <Box>
                  <Typography color="text.secondary">Всего инвестировано</Typography>
                  <Typography variant="h6">
                    {profitLoss.portfolio.totalInvestment.toFixed(2)} {profitLoss.portfolio.baseCurrency}
                  </Typography>
                </Box>

                {/* Текущая стоимость */}
                <Box>
                  <Typography color="text.secondary">Текущая стоимость</Typography>
                  <Typography variant="h6">
                    {profitLoss.portfolio.totalCurrentValue.toFixed(2)} {profitLoss.portfolio.baseCurrency}
                  </Typography>
                </Box>

                {/* Абсолютная доходность */}
                <Box>
                  <Typography color="text.secondary">Абсолютная доходность</Typography>
                  <Typography
                    variant="h6"
                    color={profitLoss.portfolio.totalAbsoluteReturn >= 0 ? 'success.main' : 'error.main'}
                  >
                    {profitLoss.portfolio.totalAbsoluteReturn >= 0 ? '+' : ''}
                    {profitLoss.portfolio.totalAbsoluteReturn.toFixed(2)} {profitLoss.portfolio.baseCurrency}
                  </Typography>
                </Box>

                {/* Процентная доходность */}
                <Box>
                  <Typography color="text.secondary">Процентная доходность</Typography>
                  <Typography
                    variant="h6"
                    color={profitLoss.portfolio.totalPercentageReturn >= 0 ? 'success.main' : 'error.main'}
                  >
                    {profitLoss.portfolio.totalPercentageReturn >= 0 ? '+' : ''}
                    {profitLoss.portfolio.totalPercentageReturn.toFixed(2)} %
                  </Typography>
                </Box>
              </Box>
            </Paper>
              {/* Доходность по активам */}
              <Typography variant="h6" gutterBottom>
                По активам
              </Typography>
              <Paper>
                <Table size="small">
                  <TableHead>
                    <TableRow>
                      <TableCell>Тикер</TableCell>
                      <TableCell>Актив</TableCell>
                      <TableCell>Кол-во</TableCell>
                      <TableCell>Ср. цена</TableCell>
                      <TableCell>Тек. цена</TableCell>
                      <TableCell>Доход</TableCell>
                      <TableCell>Вес</TableCell>
                    </TableRow>
                  </TableHead>
                  <TableBody>
                    {profitLoss.assets.map((item) => (
                      <TableRow key={item.assetId}>
                        <TableCell sx={{ fontWeight: 500 }}>{item.ticker}</TableCell>
                        <TableCell>{item.assetName}</TableCell>
                        <TableCell>{item.quantity}</TableCell>
                        <TableCell>{item.averagePurchasePrice.toFixed(2)}</TableCell>
                        <TableCell>{item.currentPrice.toFixed(2)}</TableCell>
                        <TableCell>
                          <Typography
                            color={item.absoluteReturn >= 0 ? 'success.main' : 'error.main'}
                            fontSize="0.875rem"
                          >
                            {item.absoluteReturn >= 0 ? '+' : ''}
                            {item.absoluteReturn.toFixed(2)} {item.currency}
                          </Typography>
                          <Typography
                            color={item.percentageReturn >= 0 ? 'success.main' : 'error.main'}
                            fontSize="0.75rem"
                          >
                            {item.percentageReturn >= 0 ? '+' : ''}
                            {item.percentageReturn.toFixed(2)} %
                          </Typography>
                        </TableCell>
                        <TableCell>{(item.weightInPortfolio).toFixed(1)}%</TableCell>
                      </TableRow>
                    ))}
                  </TableBody>
                </Table>
              </Paper>
            </>
          ) : (
            <Alert severity="info">Нет данных о доходности</Alert>
          )}
        </Box>

        {/* Кнопка "Назад" */}
        <Box mt={3}>
          <Button
            startIcon={<ArrowBackIcon />}
            component={Link}
            to="/portfolios"
            variant="outlined"
          >
            Назад к списку
          </Button>
        </Box>

        {/* Модальные окна */}
        {isEditModalOpen && portfolio && (
          <EditPortfolioModal
            open={true}
            onClose={() => setIsEditModalOpen(false)}
            portfolio={portfolio}
            onSave={handleSave}
          />
        )}

        {isAddModalOpen && (
          <AddToPortfolioModal
            key={portfolio?.id + '-add-modal'}
            open={isAddModalOpen}
            onClose={() => setIsAddModalOpen(false)}
            portfolios={portfolios}
            onAdd={handleAddAsset}
            loadingPortfolios={loadingPortfolios}
            fixedPortfolioId={portfolio.id}
          />
        )}

        {selectedAssetForTransaction && (
          <TransactionModal
            open={isTransactionModalOpen}
            onClose={() => setIsTransactionModalOpen(false)}
            onSubmit={handleAddTransaction}
            assetName={selectedAssetForTransaction.name}
            initialType="Buy"
            isLoading={false}
            asset={selectedAssetForTransaction}
          />
        )}
      </Container>
    </AppLayout>
  );
}
