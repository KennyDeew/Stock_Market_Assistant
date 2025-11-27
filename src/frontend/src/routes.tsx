import { Routes as RouterRoutes, Route, Navigate } from 'react-router-dom';
import LoginPage from './pages/LoginPage.tsx';
import RegisterPage from './pages/RegisterPage.tsx';
import DashboardPage from './pages/DashboardPage.tsx';
import PortfolioListPage from './pages/PortfolioListPage.tsx';
import PortfolioCreatePage from './pages/PortfolioCreatePage.tsx';
import PortfolioDetailPage from './pages/PortfolioDetailPage.tsx';
import AssetCatalogPage from './pages/AssetCatalogPage.tsx';
import AssetDetailPage from './pages/AssetDetailPage.tsx';
import ProfilePage from './pages/ProfilePage.tsx';
import AlertsListPage from './pages/AlertsListPage.tsx';
import { ProtectedRoute } from './components/ProtectedRoute.tsx';
import ProtectedLayout from './layouts/ProtectedLayout.tsx';
import PublicLayout from './layouts/PublicLayout.tsx';

const Routes = () => (
  <RouterRoutes>
    {/* Публичные страницы */}
    <Route element={<PublicLayout />}>
      <Route path="/login" element={<LoginPage />} />
      <Route path="/register" element={<RegisterPage />} />
    </Route>

    {/* Защищённые страницы */}
    <Route element={<ProtectedRoute />}>
      <Route element={<ProtectedLayout />}>
        <Route path="/assets" element={<AssetCatalogPage />} />
        <Route path="/portfolios" element={<PortfolioListPage />} />
        <Route path="/portfolios/create" element={<PortfolioCreatePage />} />
        <Route path="/portfolios/:id" element={<PortfolioDetailPage />} />
        <Route path="/profile" element={<ProfilePage />} />
        <Route path="/alerts" element={<AlertsListPage />} />
      </Route>
    </Route>

    {/* Главная — без лэйаута, лэйаут решается внутри */}
    <Route path="/" element={<DashboardPage />} />

    {/* Детальная страница актива */}
    <Route path="/asset/:ticker" element={<AssetDetailPage />} />
    
    {/* Редирект */}
    <Route path="*" element={<Navigate to="/" />} />
  </RouterRoutes>
);

export default Routes;