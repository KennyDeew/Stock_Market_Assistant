import {
  Autocomplete,
  TextField,
  CircularProgress,
  Box,
  Typography,
  Chip,
} from '@mui/material';
import { useState, useEffect } from 'react';
import { useDebounce } from '../hooks/useDebounce';
import { assetApi } from '../services/assetApi';
import type { AssetShort } from '../types/assetTypes';
import { getAssetTypeName, getAssetTypeColor } from '../utils/assetTypeUtils';

interface AssetSelectorProps {
  onSelect: (asset: AssetShort | null) => void;
  selectedAsset?: AssetShort | null;
  disabled?: boolean;
  label?: string;
}

export default function AssetSelector({
  onSelect,
  selectedAsset,
  disabled,
  label = 'Выберите актив',
}: AssetSelectorProps) {
  const [search, setSearch] = useState('');
  const [options, setOptions] = useState<AssetShort[]>([]);
  const [loading, setLoading] = useState(false);

  const debouncedSearch = useDebounce(setSearch, 500);

  useEffect(() => {
    const loadAssets = async () => {
      if (!search.trim()) {
        setOptions([]);
        return;
      }

      setLoading(true);
      try {
        const response = await assetApi.getAll({ search, page: 0, pageSize: 10 });
        setOptions(response.data);
      } catch (err) {
        setOptions([]);
      } finally {
        setLoading(false);
      }
    };

    loadAssets();
  }, [search]);

  return (
    <Autocomplete
      value={selectedAsset}
      onChange={(_, value) => onSelect(value)}
      options={options}
      getOptionLabel={(option) => `${option.ticker} — ${option.shortName}`}
      isOptionEqualToValue={(option, value) => option.ticker === value.ticker}
      loading={loading}
      disabled={disabled}
      noOptionsText="Начните вводить для поиска..."
      renderInput={(params) => (
        <TextField
          {...params}
          label={label}
          variant="outlined"
          onChange={(e) => debouncedSearch(e.target.value)}
          InputProps={{
            ...params.InputProps,
            readOnly: disabled,
            endAdornment: (
              <>
                {loading ? <CircularProgress color="inherit" size={20} /> : null}
                {params.InputProps.endAdornment}
              </>
            ),
          }}
        />
      )}
      renderOption={(props, option) => (
        <li {...props} key={option.ticker}>
          <Box sx={{ flexGrow: 1 }}>
            <Typography variant="subtitle2" fontWeight={500}>{option.ticker}</Typography>
            <Typography variant="body2" color="text.secondary">{option.shortName}</Typography>
            <Box sx={{ display: 'flex', gap: 1, mt: 0.5 }}>
              <Chip
                size="small"
                label={getAssetTypeName(option.type)}
                sx={{
                  fontSize: '0.75rem',
                  '&.MuiChip-root': {
                    bgcolor: (theme) => {
                      const color = getAssetTypeColor(option.type);
                      return theme.palette[color].main;
                    },
                    color: 'white',
                  },
                }}
              />
              <Typography variant="caption" color="text.secondary">
                {typeof option.currentPrice === 'number'
                  ? `${option.currentPrice.toFixed(2)} ${option.currency}`
                  : '—'}
              </Typography>
            </Box>
          </Box>
        </li>
      )}
    />
  );
}
