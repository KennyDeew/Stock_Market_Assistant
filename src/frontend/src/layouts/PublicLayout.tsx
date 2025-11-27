import { Box, CssBaseline, AppBar, Toolbar, Typography, Container, Button } from '@mui/material';
import { useAuth } from '../hooks/useAuth';
import { Outlet, useNavigate } from 'react-router-dom';
import React from 'react';

interface PublicLayoutProps {
  children?: React.ReactNode;
}

const PublicLayout = ({ children }: PublicLayoutProps) => {
  const { isAuthenticated, logout } = useAuth();
  const navigate = useNavigate();

  return (
    <Box sx={{ display: 'flex', flexDirection: 'column', minHeight: '100vh' }}>
      <CssBaseline />

      <AppBar position="static" color="primary">
        <Toolbar>
          <Typography
            variant="h6"
            component="div"
            sx={{
              flexGrow: 1,
              fontWeight: 600,
              color: 'primary.contrastText',
            }}
          >
            Stock Market Assistant
          </Typography>

          {isAuthenticated ? (
            <Button
              color="inherit"
              onClick={logout}
              sx={{ color: 'primary.contrastText' }}
              aria-label="Выйти"
            >
              Выйти
            </Button>
          ) : (
            <Button
              color="inherit"
              onClick={() => navigate('/login')}
              sx={{ color: 'primary.contrastText' }}
              aria-label="Войти"
            >
              Войти
            </Button>
          )}
        </Toolbar>
      </AppBar>

      <Container component="main" sx={{ mt: 4, mb: 4, flexGrow: 1 }}>
        {children}
        <Outlet />
      </Container>

      <Box component="footer" sx={{ py: 3, px: 2, mt: 'auto', bgcolor: 'grey.200' }}>
        <Typography variant="body2" color="text.secondary" align="center">
          © {new Date().getFullYear()} Stock Market Assistant
        </Typography>
      </Box>
    </Box>
  );
};

export default PublicLayout;
