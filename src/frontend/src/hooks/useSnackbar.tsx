import { createContext, useContext } from 'react';

interface SnackbarContextType {
  openSnackbar: (message: string, severity?: 'success' | 'error' | 'info' | 'warning') => void;
}

export const SnackbarContext = createContext<SnackbarContextType | undefined>(undefined);

export const useSnackbar = (): SnackbarContextType => {
  const context = useContext(SnackbarContext);
  if (!context) {
    throw new Error('useSnackbar must be used within a SnackbarProvider');
  }
  return context;
};