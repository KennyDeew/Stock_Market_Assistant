import { Box, Container } from '@mui/material';
import React from 'react';

interface AppLayoutProps {
  children: React.ReactNode;
  maxWidth?: number | string;
  py?: number;
  disableGutters?: boolean;
}

export default function AppLayout({ children, maxWidth = 1024, py = 4, disableGutters = false }: AppLayoutProps) {
  return (
    <Container maxWidth={false} disableGutters={disableGutters}>
      <Box sx={{ maxWidth, mx: 'auto', width: '100%', px: 2, py }}>
        {children}
      </Box>
    </Container>
  );
}