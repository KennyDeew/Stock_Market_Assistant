import AppLayout from '../components/AppLayout';
import { useAuth } from '../hooks/useAuth';
import { Container, Typography, Paper, Button, Box } from '@mui/material';

export default function ProfilePage() {
  const { user, logout } = useAuth();

  return (
    <AppLayout>
      <Container>
        <Paper sx={{ p: 4, mt: 4 }}>
          <Typography variant="h5" gutterBottom>
            Профиль
          </Typography>
          <Typography><strong>Email:</strong> {user?.email}</Typography>
          <Typography><strong>Имя:</strong> {user?.fullName}</Typography>
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
          </Box>
        </Paper>
      </Container>
    </AppLayout>
  );
}
