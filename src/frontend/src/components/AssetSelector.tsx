import { Autocomplete, TextField, CircularProgress, Typography, Box, Chip } from '@mui/material';
import type { AssetShort } from '../types/assetTypes';
import { useAssetSearch } from '../hooks/useAssetSearch';
import { getAssetTypeName, getAssetTypeColor } from '../utils/assetTypeUtils';
import { useState } from 'react';

interface AssetSelectorProps {
  selectedAsset: AssetShort | null;
  onSelect: (asset: AssetShort | null) => void;
  disabled?: boolean;
  label?: string;
}

export default function AssetSelector({
  selectedAsset,
  onSelect,
  disabled = false,
  label = 'Поиск актива',
}: AssetSelectorProps) {
  const { assets, loading: isLoadingFromSearch, searchAssets, error } = useAssetSearch();
  const [inputValue, setInputValue] = useState('');
  const [open, setOpen] = useState(false);

  // Не показываем спиннер, если поле пустое
  const shouldShowLoading = inputValue.trim() !== '' && isLoadingFromSearch;

  // Упростить обработку ввода
  const handleInputChange = (value: string) => {
    setInputValue(value);

    // Не выполняем поиск, если компонент disabled
    if (!disabled && value.trim()) {
      searchAssets(value, '');
      setOpen(true);
    } else {
      setOpen(false);
    }
  };

  return (
    <Autocomplete
      open={open}
      onOpen={() => {
        if (!disabled) {
          setOpen(true);
        }
      }}
      onClose={() => {
        setOpen(false);
      }}
      openOnFocus={true}
      autoHighlight
      autoSelect={false}
      value={selectedAsset}
      onChange={(_, value) => {
        onSelect(value);
        if (value) {
          setInputValue(`${value.ticker} — ${value.shortName}`);
        }
        setOpen(false);
      }}
      inputValue={inputValue}
      onInputChange={(_, newInputValue) => {
        handleInputChange(newInputValue);
      }}
      options={assets}
      getOptionLabel={(option) => {
        const label = `${option.ticker} — ${option.shortName || 'Без названия'}`;
        return label;
      }}
      isOptionEqualToValue={(option, value) => option.ticker === value.ticker}
      loading={shouldShowLoading}
      disabled={disabled}
      noOptionsText={inputValue ? 'Нет совпадений' : 'Начните вводить для поиска...'}
      filterOptions={(x) => {
        return x;
      }}
      clearOnBlur={false}
      renderInput={(params) => (
        <TextField
          {...params}
          label={label}
          disabled={disabled}
          sx={{ zIndex: 1 }}
          error={!!error}
          helperText={error || undefined}
          slotProps={{
            input: {
              ...params.InputProps,
              endAdornment: (
                <>
                  {shouldShowLoading ? <CircularProgress color="inherit" size={20} /> : null}
                  {params.InputProps.endAdornment}
                </>
              ),
            },
          }}
          autoFocus
        />
      )}
      renderOption={(props, option) => (
        <Box component="li" {...props} key={option.ticker}>
          <Box>
            <Typography variant="subtitle2" fontWeight={500}>
              {option.ticker} — {option.shortName}
            </Typography>
            <Typography variant="body2" color="text.secondary">
              {typeof option.currentPrice === 'number' ? option.currentPrice.toFixed(2) : '—'} {option.currency}
            </Typography>
            <Box sx={{ display: 'flex', gap: 1, mt: 0.5 }}>
              <Chip
                size="small"
                label={getAssetTypeName(option.type)}
                sx={{
                  fontSize: '0.75rem',
                  '& .MuiChip-label': { fontSize: '0.75rem', fontWeight: 500 },
                  bgcolor: (theme) => {
                    const color = getAssetTypeColor(option.type);
                    return theme.palette[color].main;
                  },
                  color: 'white',
                }}
              />
              <Typography variant="caption" color="text.secondary">
                {option.changePercent != null
                  ? (option.changePercent >= 0 ? '+' : '') + option.changePercent.toFixed(2) + '%'
                  : '—'}
              </Typography>
            </Box>
          </Box>
        </Box>
      )}
      clearOnEscape
      disablePortal={false}
      sx={{ 
        zIndex: 1300,
        '& .MuiAutocomplete-popper': {
          zIndex: 1400
        }
      }}
    />
  );
}