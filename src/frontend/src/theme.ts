import { createTheme } from '@mui/material/styles';

// Основные цвета
const primaryColor = '#2C3E50';
const secondaryColor = '#E74C3C';
const accentColor = '#3498DB';
const successColor = '#27AE60';
const warningColor = '#F39C12';
const errorColor = '#E74C3C';

const lightBackground = '#F8F9FA';
const cardBackground = '#FFFFFF';
const textPrimary = '#2C3E50';
const textSecondary = '#7F8C8D';
const dividerColor = '#ECF0F1';

declare module '@mui/material/styles' {
  interface Palette {
    accent: Palette['primary'];
  }
  interface PaletteOptions {
    accent?: PaletteOptions['primary'];
  }
}

const theme = createTheme({
  palette: {
    primary: {
      main: primaryColor,
      light: '#34495E',
      dark: '#1A252C',
      contrastText: '#FFFFFF',
    },
    secondary: {
      main: secondaryColor,
      light: '#EB5E55',
      dark: '#BE3A2B',
      contrastText: '#FFFFFF',
    },
    accent: {
      main: accentColor,
      light: '#5DADE2',
      dark: '#2874A6',
      contrastText: '#FFFFFF',
    },
    success: {
      main: successColor,
      light: '#2ECC71',
      dark: '#239B56',
      contrastText: '#FFFFFF',
    },
    warning: {
      main: warningColor,
      light: '#F5B041',
      dark: '#D35400',
      contrastText: '#FFFFFF',
    },
    error: {
      main: errorColor,
      light: '#EC7063',
      dark: '#CB4335',
      contrastText: '#FFFFFF',
    },
    background: {
      default: lightBackground,
      paper: cardBackground,
    },
    text: {
      primary: textPrimary,
      secondary: textSecondary,
    },
    divider: dividerColor,
  },
  typography: {
    fontFamily: '"Segoe UI", "Roboto", "Oxygen", "Ubuntu", "Cantarell", "Fira Sans", "Droid Sans", "Helvetica Neue", sans-serif',
    h1: { fontSize: '2.5rem', fontWeight: 700, color: primaryColor },
    h2: { fontSize: '2rem', fontWeight: 600, color: primaryColor },
    h3: { fontSize: '1.5rem', fontWeight: 600, color: primaryColor },
    h4: { fontSize: '1.25rem', fontWeight: 600, color: primaryColor },
    h5: { fontSize: '1rem', fontWeight: 600, color: primaryColor },
    h6: { fontSize: '0.875rem', fontWeight: 600, color: primaryColor },
    body1: { fontSize: '1rem', fontWeight: 400, color: textPrimary },
    body2: { fontSize: '0.875rem', fontWeight: 400, color: textSecondary },
    button: { textTransform: 'none', fontWeight: 600 },
  },
  components: {
    MuiAppBar: {
      styleOverrides: {
        root: {
          backgroundColor: primaryColor,
          boxShadow: '0 2px 4px rgba(0,0,0,0.1)',
        },
      },
    },
    MuiToolbar: {
      styleOverrides: {
        root: {
          minHeight: '64px',
        },
      },
    },
    MuiButton: {
      styleOverrides: {
        root: {
          borderRadius: '8px',
          padding: '8px 16px',
          textTransform: 'none',
          fontWeight: 600,
          '&:hover': {
            boxShadow: '0 4px 8px rgba(0,0,0,0.1)',
          },
        },
        containedPrimary: {
          backgroundColor: primaryColor,
          '&:hover': {
            backgroundColor: '#1A252C',
          },
        },
        containedSecondary: {
          backgroundColor: secondaryColor,
          '&:hover': {
            backgroundColor: '#BE3A2B',
          },
        },
        outlined: {
          borderColor: primaryColor,
          color: primaryColor,
          '&:hover': {
            backgroundColor: 'rgba(44, 62, 80, 0.04)',
          },
        },
        text: {
          color: primaryColor,
          '&:hover': {
            backgroundColor: 'rgba(44, 62, 80, 0.04)',
          },
        },
      },
    },
    MuiCard: {
      styleOverrides: {
        root: {
          borderRadius: '12px',
          boxShadow: '0 4px 12px rgba(0,0,0,0.05)',
          border: `1px solid ${dividerColor}`,
        },
      },
    },
    MuiCardHeader: {
      styleOverrides: {
        root: { paddingBottom: '8px' },
        title: { fontSize: '1.25rem', fontWeight: 600, color: primaryColor },
      },
    },
    MuiCardContent: {
      styleOverrides: {
        root: { paddingTop: '16px', paddingBottom: '16px' },
      },
    },
    MuiTextField: {
      styleOverrides: {
        root: {
          '& .MuiOutlinedInput-root': {
            borderRadius: '8px',
            '&:hover .MuiOutlinedInput-notchedOutline': {
              borderColor: primaryColor,
            },
            '&.Mui-focused .MuiOutlinedInput-notchedOutline': {
              borderColor: accentColor,
              borderWidth: 2,
            },
          },
        },
      },
    },
    MuiSelect: {
      styleOverrides: {
        root: {
          '& .MuiOutlinedInput-root': {
            borderRadius: '8px',
            '&:hover .MuiOutlinedInput-notchedOutline': {
              borderColor: primaryColor,
            },
            '&.Mui-focused .MuiOutlinedInput-notchedOutline': {
              borderColor: accentColor,
              borderWidth: 2,
            },
          },
        },
      },
    },
    MuiTable: {
      styleOverrides: {
        root: {
          borderCollapse: 'separate',
          borderSpacing: '0 8px',
        },
      },
    },
    MuiTableCell: {
      styleOverrides: {
        head: {
          backgroundColor: '#F8F9FA',
          fontWeight: 600,
          color: textPrimary,
          borderBottom: `2px solid ${dividerColor}`,
          paddingTop: '12px',
          paddingBottom: '12px',
        },
        body: {
          borderBottom: `1px solid ${dividerColor}`,
          paddingTop: '12px',
          paddingBottom: '12px',
          '&:first-of-type': { paddingLeft: '24px' },
          '&:last-of-type': { paddingRight: '24px' },
        },
      },
    },
    MuiTableRow: {
      styleOverrides: {
        root: {
          '&:nth-of-type(even)': { backgroundColor: '#F8F9FA' },
          '&:hover': { backgroundColor: 'rgba(52, 152, 219, 0.05)' },
        },
      },
    },
    MuiChip: {
      styleOverrides: {
        root: {
          borderRadius: '16px',
          fontSize: '0.75rem',
          fontWeight: 600,
          padding: '4px 8px',
        },
        sizeSmall: {
          fontSize: '0.65rem',
          padding: '2px 6px',
        },
        colorSuccess: {
          backgroundColor: 'rgba(39, 174, 96, 0.1)',
          color: successColor,
          '&:hover': {
            backgroundColor: 'rgba(39, 174, 96, 0.2)',
          },
        },
        colorError: {
          backgroundColor: 'rgba(231, 76, 60, 0.1)',
          color: errorColor,
          '&:hover': {
            backgroundColor: 'rgba(231, 76, 60, 0.2)',
          },
        },
        colorWarning: {
          backgroundColor: 'rgba(243, 156, 18, 0.1)',
          color: warningColor,
          '&:hover': {
            backgroundColor: 'rgba(243, 156, 18, 0.2)',
          },
        },
      },
    },
    MuiPagination: {
      styleOverrides: {
        root: {
          marginTop: '24px',
        },
        ul: {
          justifyContent: 'center',
        },
      },
    },
    // Стили для кнопок пагинации
    MuiPaginationItem: {
      styleOverrides: {
        root: {
          borderRadius: '50%',
          margin: '0 4px',
          '&.Mui-selected': {
            backgroundColor: accentColor,
            color: '#FFFFFF',
            '&:hover': {
              backgroundColor: '#2874A6',
            },
          },
          '&:hover:not(.Mui-selected)': {
            backgroundColor: 'rgba(52, 152, 219, 0.1)',
          },
        },
      },
    },
    MuiLink: {
      styleOverrides: {
        root: {
          color: accentColor,
          textDecoration: 'none',
          '&:hover': {
            textDecoration: 'underline',
          },
        },
      },
    },
  },
});

export default theme;
