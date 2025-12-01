import AppLayout from '../components/AppLayout';
import { useAuth } from '../hooks/useAuth';
import { Container, Typography, Paper, Button, Box, Alert } from '@mui/material';

export default function ProfilePage() {
  const { user, logout, deleteAccount } = useAuth();
  return (
    <AppLayout>
      <Container>
        <Paper sx={{ p: 4, mt: 4 }}>
          <Typography variant="h5" gutterBottom>
            Профиль
          </Typography>
          <Typography><strong>Email:</strong> {user?.email}</Typography>
          <Typography><strong>Имя:</strong> {user?.userName}</Typography>
          <Box mt={3}>
            <Button
              variant="outlined"
              color="secondary"
              onClick={logout}
              sx={{ mr: 2 }}
            >
              Выйти
            </Button>
            <Button
              variant="contained"
              onClick={() => alert('Функция смены пароля')}
            >
              Сменить пароль
            </Button>
            <Button
              variant="outlined"
              color="error"
              onClick={deleteAccount}
              sx={{ ml: 2 }}
            >
              Удалить аккаунт
            </Button>            
          </Box>
          {/* Подсказка */}
          <Box mt={3}>
            <Alert severity="warning">
              Удаление аккаунта приведёт к безвозвратному удалению всех данных.
            </Alert>
          </Box>       
        </Paper>
      </Container>
    </AppLayout>
  );
}
