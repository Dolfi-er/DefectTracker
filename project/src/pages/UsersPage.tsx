import { useEffect, useState } from 'react';
import { apiService } from '../services/api';
import type { UserDTO, RoleDTO } from '../types/api';
import { Users, Shield, Mail } from 'lucide-react';

export function UsersPage() {
  const [users, setUsers] = useState<UserDTO[]>([]);
  const [roles, setRoles] = useState<RoleDTO[]>([]);
  const [isLoading, setIsLoading] = useState(true);

  useEffect(() => {
    loadData();
  }, []);

  const loadData = async () => {
    try {
      const [usersData, rolesData] = await Promise.all([
        apiService.getUsers(),
        apiService.getRoles(),
      ]);
      setUsers(usersData);
      setRoles(rolesData);
    } catch (error) {
      console.error('Failed to load data:', error);
    } finally {
      setIsLoading(false);
    }
  };

  const getRoleColor = (roleName: string) => {
    const name = roleName.toLowerCase();
    if (name.includes('admin')) return 'bg-red-100 text-red-700 border-red-200';
    if (name.includes('manager')) return 'bg-blue-100 text-blue-700 border-blue-200';
    if (name.includes('developer')) return 'bg-green-100 text-green-700 border-green-200';
    return 'bg-gray-100 text-gray-700 border-gray-200';
  };

  if (isLoading) {
    return (
      <div className="flex items-center justify-center h-64">
        <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600"></div>
      </div>
    );
  }

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-3xl font-bold text-gray-900">Users</h1>
          <p className="text-gray-600 mt-1">Manage team members and their roles</p>
        </div>
      </div>

      <div className="bg-white rounded-xl shadow-sm border border-gray-200 overflow-hidden">
        <div className="overflow-x-auto">
          <table className="w-full">
            <thead className="bg-gray-50 border-b border-gray-200">
              <tr>
                <th className="text-left py-4 px-6 font-semibold text-gray-700">User</th>
                <th className="text-left py-4 px-6 font-semibold text-gray-700">Login</th>
                <th className="text-left py-4 px-6 font-semibold text-gray-700">Role</th>
                <th className="text-center py-4 px-6 font-semibold text-gray-700">ID</th>
              </tr>
            </thead>
            <tbody>
              {users.map((user, index) => (
                <tr
                  key={user.userId}
                  className={`border-b border-gray-100 hover:bg-gray-50 transition-colors ${
                    index === users.length - 1 ? 'border-b-0' : ''
                  }`}
                >
                  <td className="py-4 px-6">
                    <div className="flex items-center gap-3">
                      <div className="w-10 h-10 bg-gradient-to-br from-blue-500 to-blue-600 rounded-full flex items-center justify-center text-white font-semibold">
                        {user.fio
                          .split(' ')
                          .map((n) => n[0])
                          .join('')
                          .toUpperCase()
                          .slice(0, 2)}
                      </div>
                      <div>
                        <p className="font-medium text-gray-900">{user.fio}</p>
                      </div>
                    </div>
                  </td>
                  <td className="py-4 px-6">
                    <div className="flex items-center gap-2 text-gray-600">
                      <Mail className="w-4 h-4" />
                      <span>{user.login}</span>
                    </div>
                  </td>
                  <td className="py-4 px-6">
                    <span
                      className={`inline-flex items-center gap-2 px-3 py-1 rounded-full text-xs font-medium border ${getRoleColor(
                        user.roleName
                      )}`}
                    >
                      <Shield className="w-3 h-3" />
                      {user.roleName}
                    </span>
                  </td>
                  <td className="py-4 px-6 text-center">
                    <span className="text-sm text-gray-500">#{user.userId}</span>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      </div>

      {users.length === 0 && (
        <div className="bg-white rounded-xl shadow-sm border border-gray-200 p-12 text-center">
          <Users className="w-12 h-12 text-gray-400 mx-auto mb-4" />
          <h3 className="text-lg font-semibold text-gray-900 mb-2">No users found</h3>
          <p className="text-gray-600">Users will appear here once registered</p>
        </div>
      )}

      <div className="bg-white rounded-xl shadow-sm border border-gray-200 p-6">
        <h2 className="text-xl font-bold text-gray-900 mb-4">Available Roles</h2>
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
          {roles.map((role) => (
            <div
              key={role.roleId}
              className="p-4 border border-gray-200 rounded-lg hover:border-blue-300 transition-colors"
            >
              <div className="flex items-center gap-3">
                <div className="w-10 h-10 bg-blue-100 rounded-lg flex items-center justify-center">
                  <Shield className="w-5 h-5 text-blue-600" />
                </div>
                <div>
                  <p className="font-medium text-gray-900">{role.roleName}</p>
                  <p className="text-xs text-gray-500">Role ID: {role.roleId}</p>
                </div>
              </div>
            </div>
          ))}
        </div>
      </div>
    </div>
  );
}
