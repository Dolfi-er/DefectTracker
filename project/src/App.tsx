import { useState } from 'react';
import { AuthProvider, useAuth } from './contexts/AuthContext';
import { ProtectedRoute } from './components/ProtectedRoute';
import { Layout } from './components/Layout';
import { LoginPage } from './pages/LoginPage';
import { DashboardPage } from './pages/DashboardPage';
import { DefectsPage } from './pages/DefectsPage';
import { ProjectsPage } from './pages/ProjectsPage';
import { UsersPage } from './pages/UsersPage';
import { apiService } from './services/api';


apiService.setBaseURL('http://localhost:5229');

function AppContent() {
  const { user } = useAuth();
  const [currentPage, setCurrentPage] = useState<'dashboard' | 'defects' | 'projects' | 'users'>(
    'dashboard'
  );

  if (!user) {
    return <LoginPage />;
  }

  return (
    <ProtectedRoute>
      <Layout currentPage={currentPage} onNavigate={setCurrentPage}>
        {currentPage === 'dashboard' && <DashboardPage />}
        {currentPage === 'defects' && <DefectsPage />}
        {currentPage === 'projects' && <ProjectsPage />}
        {currentPage === 'users' && <UsersPage />}
      </Layout>
    </ProtectedRoute>
  );
}

function App() {
  return (
    <AuthProvider>
      <AppContent />
    </AuthProvider>
  );
}

export default App;