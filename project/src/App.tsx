import { useState, useEffect } from 'react';
import { AuthProvider, useAuth } from './contexts/AuthContext';
import { ProtectedRoute } from './components/ProtectedRoute';
import { Layout } from './components/Layout';
import { LoginPage } from './pages/LoginPage';
import { DashboardPage } from './pages/DashboardPage';
import { DefectsPage } from './pages/DefectsPage';
import { ProjectsPage } from './pages/ProjectsPage';
import { UsersPage } from './pages/UsersPage';
import { apiService } from './services/api';

function AppContent() {
  const { user } = useAuth();
  const [currentPage, setCurrentPage] = useState<'dashboard' | 'defects' | 'projects' | 'users'>(
    'dashboard'
  );
  const [apiUrl, setApiUrl] = useState('');
  const [isConfigured, setIsConfigured] = useState(false);

  useEffect(() => {
    const savedUrl = localStorage.getItem('api_base_url');
    if (savedUrl) {
      apiService.setBaseURL(savedUrl);
      setApiUrl(savedUrl);
      setIsConfigured(true);
    }
  }, []);

  const handleConfigureApi = (e: React.FormEvent<HTMLFormElement>) => {
    e.preventDefault();
    const formData = new FormData(e.currentTarget);
    const url = formData.get('apiUrl') as string;
    apiService.setBaseURL(url);
    localStorage.setItem('api_base_url', url);
    setIsConfigured(true);
  };

  if (!isConfigured) {
    return (
      <div className="min-h-screen bg-gradient-to-br from-slate-900 via-slate-800 to-slate-900 flex items-center justify-center p-4">
        <div className="w-full max-w-md">
          <div className="bg-white rounded-2xl shadow-2xl p-8">
            <div className="text-center mb-8">
              <h1 className="text-3xl font-bold text-gray-900 mb-2">API Configuration</h1>
              <p className="text-gray-600">Enter your API base URL to get started</p>
            </div>
            <form onSubmit={handleConfigureApi} className="space-y-4">
              <div>
                <label htmlFor="apiUrl" className="block text-sm font-medium text-gray-700 mb-2">
                  API Base URL
                </label>
                <input
                  id="apiUrl"
                  name="apiUrl"
                  type="text"
                  defaultValue={apiUrl}
                  placeholder="https://api.example.com"
                  required
                  className="w-full px-4 py-3 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                />
                <p className="mt-2 text-sm text-gray-500">
                  Example: https://api.example.com (without trailing slash)
                </p>
              </div>
              <button
                type="submit"
                className="w-full bg-blue-600 text-white py-3 rounded-lg font-medium hover:bg-blue-700 transition-colors"
              >
                Save Configuration
              </button>
            </form>
          </div>
        </div>
      </div>
    );
  }

  //if (!user) {
    //return <LoginPage />;
  //}

  return (
    //<ProtectedRoute>
      <Layout currentPage={currentPage} onNavigate={setCurrentPage}>
        {currentPage === 'dashboard' && <DashboardPage />}
        {currentPage === 'defects' && <DefectsPage />}
        {currentPage === 'projects' && <ProjectsPage />}
        {currentPage === 'users' && <UsersPage />}
      </Layout>
    //</ProtectedRoute>
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
