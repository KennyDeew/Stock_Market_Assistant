import { Box, CssBaseline, Drawer, List, ListItemIcon, ListItemText, Toolbar, ListItemButton, useTheme } from '@mui/material';
import { Outlet, useLocation, matchPath } from 'react-router-dom';
import { useAuth } from '../hooks/useAuth';
import { Link as RouterLink } from 'react-router-dom';
import AccountBalanceIcon from '@mui/icons-material/AccountBalance';
import ShowChartIcon from '@mui/icons-material/ShowChart';
import ReceiptLongIcon from '@mui/icons-material/ReceiptLong';
import PersonIcon from '@mui/icons-material/Person';
import LogoutIcon from '@mui/icons-material/Logout';
import DashboardIcon from '@mui/icons-material/Dashboard';
import React from 'react';

interface ProtectedLayoutProps {
  children?: React.ReactNode;
}

const ProtectedLayout = ({ children }: ProtectedLayoutProps) => {
  const { logout } = useAuth();
  const theme = useTheme();
  const drawerWidth = 240;
  const location = useLocation();

  // Базовый стиль для пункта меню
  const menuItemSx = {
    display: 'flex',
    alignItems: 'center',
    justifyContent: 'flex-start',
    gap: 1.5,
    px: 3,
    py: 1.2,
    color: 'primary.contrastText',
    borderRadius: 1,
    mx: 1,
    mb: 0.5,
    // 🔹 Полоска слева (невидима по умолчанию)
    borderLeft: '3px solid transparent',
    '&:hover': {
      backgroundColor: 'rgba(255, 255, 255, 0.1)',
    },
    '& .MuiListItemIcon-root': {
      color: 'primary.contrastText',
      minWidth: 'auto',
    },
    '& .MuiListItemText-primary': {
      color: 'primary.contrastText',
      fontWeight: 500,
      fontSize: '0.95rem',
    },
  };

  return (
    <Box sx={{ display: 'flex' }}>
      <CssBaseline />

      <Drawer
        variant="permanent"
        sx={{
          width: drawerWidth,
          flexShrink: 0,
          '& .MuiDrawer-paper': {
            width: drawerWidth,
            boxSizing: 'border-box',
            backgroundColor: 'primary.main',
            color: 'primary.contrastText',
            borderRight: `1px solid ${theme.palette.divider}`,
            display: 'flex',
            flexDirection: 'column',
            boxShadow: '2px 0 8px rgba(0, 0, 0, 0.1)',
          },
        }}
      >
        <Toolbar />

        {/* Основное меню */}
        <Box sx={{ flexGrow: 1, px: 1, py: 1 }}>
          <List sx={{ pt: 0, pb: 1 }}>
            {/* 🔹 Главная — теперь с иконкой Dashboard */}
            <ListItemButton
              component={RouterLink}
              to="/"
              sx={{
                ...menuItemSx,
                ...(location.pathname === '/' && {
                  backgroundColor: 'rgba(255, 255, 255, 0.15)',
                  fontWeight: 600,
                  borderLeftColor: '#fff',
                }),
              }}
            >
              <ListItemIcon>
                <DashboardIcon fontSize="small" />
              </ListItemIcon>
              <ListItemText primary="Главная" />
            </ListItemButton>

            {/* Портфели */}
            <ListItemButton
              component={RouterLink}
              to="/portfolios"
              sx={{
                ...menuItemSx,
                ...(matchPath('/portfolios/*', location.pathname) && {
                  backgroundColor: 'rgba(255, 255, 255, 0.15)',
                  fontWeight: 600,
                  borderLeftColor: '#fff',
                }),
              }}
            >
              <ListItemIcon>
                <AccountBalanceIcon fontSize="small" />
              </ListItemIcon>
              <ListItemText primary="Портфели" />
            </ListItemButton>

            {/* Активы */}
            <ListItemButton
              component={RouterLink}
              to="/assets"
              sx={{
                ...menuItemSx,
                ...(matchPath('/assets', location.pathname) && {
                  backgroundColor: 'rgba(255, 255, 255, 0.15)',
                  fontWeight: 600,
                  borderLeftColor: '#fff',
                }),
              }}
            >
              <ListItemIcon>
                <ShowChartIcon fontSize="small" />
              </ListItemIcon>
              <ListItemText primary="Активы" />
            </ListItemButton>

            {/* Уведомления */}
            <ListItemButton
              component={RouterLink}
              to="/alerts"
              sx={{
                ...menuItemSx,
                ...(matchPath('/alerts', location.pathname) && {
                  backgroundColor: 'rgba(255, 255, 255, 0.15)',
                  fontWeight: 600,
                  borderLeftColor: '#fff',
                }),
              }}
            >
              <ListItemIcon>
                <ReceiptLongIcon fontSize="small" />
              </ListItemIcon>
              <ListItemText primary="Уведомления" />
            </ListItemButton>

            {/* Профиль */}
            <ListItemButton
              component={RouterLink}
              to="/profile"
              sx={{
                ...menuItemSx,
                ...(matchPath('/profile', location.pathname) && {
                  backgroundColor: 'rgba(255, 255, 255, 0.15)',
                  fontWeight: 600,
                  borderLeftColor: '#fff',
                }),
              }}
            >
              <ListItemIcon>
                <PersonIcon fontSize="small" />
              </ListItemIcon>
              <ListItemText primary="Профиль" />
            </ListItemButton>
          </List>
        </Box>

        {/* Кнопка "Выйти" */}
        <Box sx={{ px: 1, pb: 1 }}>
          <ListItemButton
            onClick={logout}
            sx={{
              display: 'flex',
              alignItems: 'center',
              justifyContent: 'flex-start',
              gap: 1.5,
              px: 3,
              py: 1.2,
              color: 'primary.contrastText',
              borderRadius: 1,
              mx: 1,
              mb: 0,
              backgroundColor: 'rgba(255, 255, 255, 0.08)',
              '&:hover': {
                backgroundColor: 'rgba(255, 255, 255, 0.12)',
              },
              '& .MuiListItemIcon-root': {
                color: 'primary.contrastText',
                minWidth: 'auto',
              },
              '& .MuiListItemText-primary': {
                color: 'primary.contrastText',
                fontWeight: 500,
                fontSize: '0.95rem',
              },
            }}
          >
            <ListItemIcon>
              <LogoutIcon fontSize="small" />
            </ListItemIcon>
            <ListItemText primary="Выйти" />
          </ListItemButton>
        </Box>
      </Drawer>

      <Box component="main" sx={{ flexGrow: 1, p: 3 }}>
        {children}
        <Outlet />
      </Box>
    </Box>
  );
};

export default ProtectedLayout;