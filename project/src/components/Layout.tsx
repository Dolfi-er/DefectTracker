import { ReactNode, useState } from 'react';
import { useAuth } from '../contexts/AuthContext';
import {
  LayoutDashboard,
  AlertCircle,
  FolderOpen,
  Users,
  LogOut,
  Menu,
  X,
} from 'lucide-react';

interface LayoutProps {
  children: ReactNode;
  currentPage: 'dashboard' | 'defects' | 'projects' | 'users';
  onNavigate: (page: 'dashboard' | 'defects' | 'projects' | 'users') => void;
}

const mockUser = {
  fio: 'Demo User',
  roleName: 'Viewer'
};


export function Layout({ children, currentPage, onNavigate }: LayoutProps) {
  const { user, logout } = useAuth();
  const [isMobileMenuOpen, setIsMobileMenuOpen] = useState(false);

  const navigationItems = [
    { id: 'dashboard' as const, label: 'Dashboard', icon: LayoutDashboard },
    { id: 'defects' as const, label: 'Defects', icon: AlertCircle },
    { id: 'projects' as const, label: 'Projects', icon: FolderOpen },
    { id: 'users' as const, label: 'Users', icon: Users },
  ];

  const handleNavigate = (page: 'dashboard' | 'defects' | 'projects' | 'users') => {
    onNavigate(page);
    setIsMobileMenuOpen(false);
  };

  return (
    <div className="min-h-screen bg-gray-50">
      <nav className="bg-white border-b border-gray-200 fixed w-full top-0 z-40">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
          <div className="flex items-center justify-between h-16">
            <div className="flex items-center gap-3">
              <div className="w-10 h-10 bg-gradient-to-br from-blue-600 to-blue-700 rounded-xl flex items-center justify-center">
                <AlertCircle className="w-6 h-6 text-white" />
              </div>
              <div className="hidden md:block">
                <h1 className="text-xl font-bold text-gray-900">Defect Tracker</h1>
              </div>
            </div>

            <div className="hidden md:flex items-center gap-2">
              {navigationItems.map((item) => {
                const Icon = item.icon;
                const isActive = currentPage === item.id;
                return (
                  <button
                    key={item.id}
                    onClick={() => handleNavigate(item.id)}
                    className={`flex items-center gap-2 px-4 py-2 rounded-lg font-medium transition-colors ${
                      isActive
                        ? 'bg-blue-50 text-blue-600'
                        : 'text-gray-700 hover:bg-gray-50'
                    }`}
                  >
                    <Icon className="w-5 h-5" />
                    {item.label}
                  </button>
                );
              })}
            </div>

            <div className="flex items-center gap-4">
              <div className="hidden md:flex items-center gap-3">
                <div className="text-right">
                  <p className="text-sm font-medium text-gray-900">{user?.fio}</p>
                  <p className="text-xs text-gray-500">{user?.roleName}</p>
                </div>
                <button
                  onClick={logout}
                  className="p-2 text-gray-600 hover:text-red-600 hover:bg-red-50 rounded-lg transition-colors"
                  title="Logout"
                >
                  <LogOut className="w-5 h-5" />
                </button>
              </div>

              <button
                onClick={() => setIsMobileMenuOpen(!isMobileMenuOpen)}
                className="md:hidden p-2 text-gray-600 hover:bg-gray-100 rounded-lg transition-colors"
              >
                {isMobileMenuOpen ? (
                  <X className="w-6 h-6" />
                ) : (
                  <Menu className="w-6 h-6" />
                )}
              </button>
            </div>
          </div>
        </div>

        {isMobileMenuOpen && (
          <div className="md:hidden border-t border-gray-200 bg-white">
            <div className="px-4 py-4 space-y-2">
              {navigationItems.map((item) => {
                const Icon = item.icon;
                const isActive = currentPage === item.id;
                return (
                  <button
                    key={item.id}
                    onClick={() => handleNavigate(item.id)}
                    className={`w-full flex items-center gap-3 px-4 py-3 rounded-lg font-medium transition-colors ${
                      isActive
                        ? 'bg-blue-50 text-blue-600'
                        : 'text-gray-700 hover:bg-gray-50'
                    }`}
                  >
                    <Icon className="w-5 h-5" />
                    {item.label}
                  </button>
                );
              })}
              <div className="pt-4 border-t border-gray-200">
                <div className="flex items-center justify-between px-4 py-2">
                  <div>
                    <p className="text-sm font-medium text-gray-900">{user?.fio || mockUser.fio}</p>
                    <p className="text-xs text-gray-500">{user?.roleName ||mockUser.roleName}</p>
                  </div>
                  <button
                    onClick={logout}
                    className="p-2 text-gray-600 hover:text-red-600 hover:bg-red-50 rounded-lg transition-colors"
                  >
                    <LogOut className="w-5 h-5" />
                  </button>
                </div>
              </div>
            </div>
          </div>
        )}
      </nav>

      <main className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 pt-24 pb-12">
        {children}
      </main>
    </div>
  );
}
