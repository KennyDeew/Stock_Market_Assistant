import AppLayout from '../components/AppLayout';
import { useAuth } from '../hooks/useAuth';
import { Container, Typography, Paper, Button, Box, Alert, Dialog, DialogTitle, DialogContent, DialogActions, TextField } from '@mui/material';
import { useState } from 'react';
import { changePassword as apiChangePassword, deleteAccount as apiDeleteAccount } from '../services/accountApi';
import { useSnackbar } from '../hooks/useSnackbar';
import { handleApiError } from '../services/errorHandler';

export default function ProfilePage() {
  const { user, logout } = useAuth();
  const { openSnackbar } = useSnackbar();

  // Состояния для диалогов
  const [openChangePassword, setOpenChangePassword] = useState(false);
  const [openDeleteAccount, setOpenDeleteAccount] = useState(false);

  // Данные форм
  const [oldPassword, setOldPassword] = useState('');
  const [newPassword, setNewPassword] = useState('');
  const [confirmNewPassword, setConfirmNewPassword] = useState('');
  const [deletePassword, setDeletePassword] = useState('');

  const [loading, setLoading] = useState(false);

  // Проверка паролей
  const passwordsMatch = newPassword === confirmNewPassword;
  const passwordsValid = passwordsMatch && newPassword.length >= 6;

  // --- Смена пароля ---
  const handleChangePassword = async () => {
    if (!oldPassword || !newPassword) {
      openSnackbar('Заполните все поля', 'warning');
      return;
    }

    if (!passwordsValid) {
      openSnackbar('Новый пароль не соответствует требованиям', 'warning');
      return;
    }

    setLoading(true);
    try {
      await apiChangePassword(oldPassword, newPassword);
      openSnackbar('Пароль успешно изменён', 'success');
      setOpenChangePassword(false);
      resetChangePasswordForm();
    } catch (error) {
      const message = handleApiError(error);
      openSnackbar(message, 'error');
    } finally {
      setLoading(false);
    }
  };

  const resetChangePasswordForm = () => {
    setOldPassword('');
    setNewPassword('');
    setConfirmNewPassword('');
  };

  // --- Удаление аккаунта ---
  const handleDeleteAccount = async () => {
    if (!deletePassword) {
      openSnackbar('Введите пароль', 'warning');
      return;
    }

    setLoading(true);
    try {
      await apiDeleteAccount(deletePassword);
      openSnackbar('Аккаунт успешно удалён', 'success');
      logout(); // автоматически выходит и перенаправляет
    } catch (error) {
      const message = handleApiError(error);
      openSnackbar(message, 'error');
    } finally {
      setLoading(false);
    }
  };

  // --- Рендер ---
  return (
    <AppLayout>
      <Container>
        <Paper sx={{ p: 4, mt: 4, maxWidth: 400, mx: 'auto' }}>
          <Typography variant="h5" gutterBottom align="center">
            Профиль
          </Typography>

          <Typography><strong>Email:</strong> {user?.email}</Typography>
          <Typography><strong>Имя:</strong> {user?.userName}</Typography>

          <Box mt={3} display="flex" flexDirection="column" gap={2} alignItems="center">
            <Button
              variant="contained"
              onClick={() => setOpenChangePassword(true)}
              sx={{ width: '100%', maxWidth: 300 }}
            >
              Сменить пароль
            </Button>

            <Button
              variant="outlined"
              color="secondary"
              onClick={logout}
              sx={{ width: '100%', maxWidth: 300 }}
            >
              Выйти
            </Button>

            <Button
              variant="outlined"
              color="error"
              onClick={() => setOpenDeleteAccount(true)}
              sx={{ width: '100%', maxWidth: 300 }}
            >
              Удалить аккаунт
            </Button>
          </Box>

          {/* Подсказка */}
          <Box mt={3} textAlign="center">
            <Alert severity="warning" sx={{ display: 'inline-block' }}>
              Удаление аккаунта приведёт к безвозвратному удалению всех данных.
            </Alert>
          </Box>
        </Paper>
      </Container>

      {/* Диалог смены пароля */}
      <Dialog open={openChangePassword} onClose={() => !loading && setOpenChangePassword(false)}>
        <DialogTitle>Смена пароля</DialogTitle>
        <DialogContent>
          <TextField
            label="Текущий пароль"
            type="password"
            fullWidth
            margin="normal"
            value={oldPassword}
            onChange={(e) => setOldPassword(e.target.value)}
            disabled={loading}
            autoFocus
          />
          <TextField
            label="Новый пароль"
            type="password"
            fullWidth
            margin="normal"
            value={newPassword}
            onChange={(e) => setNewPassword(e.target.value)}
            disabled={loading}
            helperText="Минимум 6 символов, цифра, заглавная, строчная, спецсимвол"
          />
          <TextField
            label="Подтвердите новый пароль"
            type="password"
            fullWidth
            margin="normal"
            value={confirmNewPassword}
            onChange={(e) => setConfirmNewPassword(e.target.value)}
            disabled={loading}
            error={!passwordsMatch}
            helperText={!passwordsMatch ? 'Пароли не совпадают' : ''}
          />
        </DialogContent>
        <DialogActions>
          <Button onClick={() => {
            setOpenChangePassword(false);
            resetChangePasswordForm();
          }} disabled={loading}>
            Отмена
          </Button>
          <Button
            onClick={handleChangePassword}
            color="primary"
            disabled={loading || !passwordsValid}
          >
            {loading ? 'Сохранение...' : 'Сохранить'}
          </Button>
        </DialogActions>
      </Dialog>

      {/* Диалог удаления аккаунта */}
      <Dialog open={openDeleteAccount} onClose={() => !loading && setOpenDeleteAccount(false)}>
        <DialogTitle color="error">Удаление аккаунта</DialogTitle>
        <DialogContent>
          <Alert severity="error" sx={{ mb: 2 }}>
            Это действие нельзя отменить.
          </Alert>
          <Typography mb={2}>
            Введите пароль, чтобы подтвердить удаление:
          </Typography>
          <TextField
            label="Пароль"
            type="password"
            fullWidth
            value={deletePassword}
            onChange={(e) => setDeletePassword(e.target.value)}
            disabled={loading}
            autoFocus
          />
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setOpenDeleteAccount(false)} disabled={loading}>
            Отмена
          </Button>
          <Button
            onClick={handleDeleteAccount}
            color="error"
            disabled={loading}
          >
            {loading ? 'Удаление...' : 'Удалить'}
          </Button>
        </DialogActions>
      </Dialog>
    </AppLayout>
  );
}