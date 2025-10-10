import { useEffect, useState } from 'react';
import { apiService } from '../services/api';
import type { OverviewStatsDTO, ProjectStatsDTO, UserStatsDTO, DefectStatusDTO, TimelineStatsDTO, StatusStatsDTO } from '../types/api';
import { FileText, ArrowRight,BarChart3, AlertTriangle, CheckCircle, Clock, TrendingUp, PieChart as PieChartIcon, BarChart as BarChartIcon, Activity } from 'lucide-react';
import { PieChart, Pie, Cell, BarChart, Bar, XAxis, YAxis, CartesianGrid, Tooltip, Legend, ResponsiveContainer, LineChart, Line } from 'recharts';
import { useNavigate } from 'react-router-dom';

export function DashboardPage() {
  const navigate = useNavigate();
  const [overview, setOverview] = useState<OverviewStatsDTO | null>(null);
  const [projects, setProjects] = useState<ProjectStatsDTO[]>([]);
  const [users, setUsers] = useState<UserStatsDTO[]>([]);
  const [defectStatuses, setDefectStatuses] = useState<DefectStatusDTO[]>([]);
  const [statusStats, setStatusStats] = useState<StatusStatsDTO[]>([]);
  const [timelineStats, setTimelineStats] = useState<TimelineStatsDTO[]>([]);
  const [isLoading, setIsLoading] = useState(true);

  useEffect(() => {
    loadDashboardData();
  }, []);

  const loadDashboardData = async () => {
    try {
      const [overviewData, projectsData, usersData, statusesData, statusStatsData, timelineData] = await Promise.all([
        apiService.getOverviewStats(),
        apiService.getDefectsByProject(),
        apiService.getDefectsByUser(),
        apiService.getDefectStatuses(),
        apiService.getDefectsByStatus(),
        apiService.getDefectsTimeline(30),
      ]);
      setOverview(overviewData);
      setProjects(projectsData);
      setUsers(usersData);
      setDefectStatuses(statusesData);
      setStatusStats(statusStatsData);
      setTimelineStats(timelineData);
    } catch (error) {
      console.error('Failed to load dashboard:', error);
    } finally {
      setIsLoading(false);
    }
  };

  // Функция для получения количества дефектов по имени статуса
  const getDefectCountByStatusName = (statusName: string): number => {
    if (!overview?.defectsByStatus || !defectStatuses.length) return 0;

    const status = defectStatuses.find(s => s.statusName === statusName);
    if (!status) return 0;

    return overview.defectsByStatus[status.id] || 0;
  };

  // Функция для получения общего количества открытых дефектов (все статусы кроме Closed и Cancelled)
  const getOpenDefectsCount = (): number => {
    if (!overview?.defectsByStatus || !defectStatuses.length) return 0;

    let openCount = 0;
    const closedStatusNames = ['Closed', 'Cancelled'];
    
    defectStatuses.forEach(status => {
      if (!closedStatusNames.includes(status.statusName)) {
        openCount += overview.defectsByStatus[status.id] || 0;
      }
    });

    return openCount;
  };

  // Подготовка данных для круговой диаграммы по статусам
  const statusChartData = statusStats.map(status => ({
    name: status.statusName,
    value: status.count,
    percentage: status.percentage
  }));

  // Подготовка данных для столбчатой диаграммы по приоритетам
  const priorityChartData = [
    { name: 'Low', count: overview?.defectsByPriority?.['1'] || 0, color: '#10B981' },
    { name: 'Medium', count: overview?.defectsByPriority?.['2'] || 0, color: '#F59E0B' },
    { name: 'High', count: overview?.defectsByPriority?.['3'] || 0, color: '#EF4444' },
  ];

  // Цвета для круговой диаграммы
  const COLORS = ['#0088FE', '#00C49F', '#FFBB28', '#FF8042', '#8884D8'];

  // Форматирование даты для графика временной шкалы
  const formatTimelineDate = (dateString: string) => {
    const date = new Date(dateString);
    return date.toLocaleDateString('en-US', { month: 'short', day: 'numeric' });
  };

  // Кастомный тултип для круговой диаграммы
  const CustomTooltip = ({ active, payload }: any) => {
    if (active && payload && payload.length) {
      const data = payload[0].payload;
      return (
        <div className="bg-white p-3 border border-gray-200 rounded shadow-sm">
          <p className="font-medium">{`${data.name}`}</p>
          <p className="text-sm">{`Count: ${data.value}`}</p>
          <p className="text-sm">{`Percentage: ${data.percentage?.toFixed(1)}%`}</p>
        </div>
      );
    }
    return null;
  };

  if (isLoading) {
    return (
      <div className="flex items-center justify-center h-64">
        <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600"></div>
      </div>
    );
  }

  const getPriorityLabel = (priority: number) => {
    if (priority >= 3) return 'High';
    if (priority >= 2) return 'Medium';
    return 'Low';
  };

  const getPriorityColor = (priority: number) => {
    if (priority >= 3) return 'text-red-600 bg-red-50';
    if (priority >= 2) return 'text-yellow-600 bg-yellow-50';
    return 'text-green-600 bg-green-50';
  };

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-3xl font-bold text-gray-900">Dashboard</h1>
          <p className="text-gray-600 mt-1">Overview of all defects and projects</p>
        </div>
        <button
          onClick={() => navigate('/reports')}
          className="flex items-center gap-2 px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 transition-colors"
        >
          <FileText className="w-4 h-4" />
          Generate Reports
          <ArrowRight className="w-4 h-4" />
        </button>
      </div>

      {/* Статистические карточки */}
      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6">
        <div className="bg-white rounded-xl shadow-sm border border-gray-200 p-6">
          <div className="flex items-center justify-between mb-4">
            <div className="w-12 h-12 bg-blue-100 rounded-lg flex items-center justify-center">
              <BarChart3 className="w-6 h-6 text-blue-600" />
            </div>
          </div>
          <p className="text-gray-600 text-sm font-medium">Total Defects</p>
          <p className="text-3xl font-bold text-gray-900 mt-2">{overview?.totalDefects || 0}</p>
        </div>

        <div className="bg-white rounded-xl shadow-sm border border-gray-200 p-6">
          <div className="flex items-center justify-between mb-4">
            <div className="w-12 h-12 bg-yellow-100 rounded-lg flex items-center justify-center">
              <Clock className="w-6 h-6 text-yellow-600" />
            </div>
          </div>
          <p className="text-gray-600 text-sm font-medium">Open Defects</p>
          <p className="text-3xl font-bold text-gray-900 mt-2">
            {getOpenDefectsCount()}
          </p>
        </div>

        <div className="bg-white rounded-xl shadow-sm border border-gray-200 p-6">
          <div className="flex items-center justify-between mb-4">
            <div className="w-12 h-12 bg-red-100 rounded-lg flex items-center justify-center">
              <AlertTriangle className="w-6 h-6 text-red-600" />
            </div>
          </div>
          <p className="text-gray-600 text-sm font-medium">High Priority</p>
          <p className="text-3xl font-bold text-gray-900 mt-2">
            {overview?.defectsByPriority?.['3'] || 0}
          </p>
        </div>

        <div className="bg-white rounded-xl shadow-sm border border-gray-200 p-6">
          <div className="flex items-center justify-between mb-4">
            <div className="w-12 h-12 bg-green-100 rounded-lg flex items-center justify-center">
              <CheckCircle className="w-6 h-6 text-green-600" />
            </div>
          </div>
          <p className="text-gray-600 text-sm font-medium">Closed Defects</p>
          <p className="text-3xl font-bold text-gray-900 mt-2">
            {getDefectCountByStatusName('Closed')}
          </p>
        </div>
      </div>

      {/* Графики */}
      <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
        {/* Круговая диаграмма по статусам */}
        <div className="bg-white rounded-xl shadow-sm border border-gray-200 p-6">
          <h2 className="text-xl font-bold text-gray-900 mb-4 flex items-center gap-2">
            <PieChartIcon className="w-5 h-5 text-blue-600" />
            Defects by Status
          </h2>
          <div className="h-80">
            <ResponsiveContainer width="100%" height="100%">
              <PieChart>
                <Pie
                  data={statusChartData}
                  cx="50%"
                  cy="50%"
                  labelLine={false}
                  label={({ name, percentage }) => `${name}: ${(percentage as number).toFixed(1)}%`}
                  outerRadius={80}
                  fill="#8884d8"
                  dataKey="value"
                >
                  {statusChartData.map((entry, index) => (
                    <Cell key={`cell-${index}`} fill={COLORS[index % COLORS.length]} />
                  ))}
                </Pie>
                <Tooltip content={<CustomTooltip />} />
                <Legend />
              </PieChart>
            </ResponsiveContainer>
          </div>
        </div>

        {/* Столбчатая диаграмма по приоритетам */}
        <div className="bg-white rounded-xl shadow-sm border border-gray-200 p-6">
          <h2 className="text-xl font-bold text-gray-900 mb-4 flex items-center gap-2">
            <BarChartIcon className="w-5 h-5 text-green-600" />
            Defects by Priority
          </h2>
          <div className="h-80">
            <ResponsiveContainer width="100%" height="100%">
              <BarChart data={priorityChartData}>
                <CartesianGrid strokeDasharray="3 3" />
                <XAxis dataKey="name" />
                <YAxis />
                <Tooltip formatter={(value) => [`${value} defects`, 'Count']} />
                <Bar dataKey="count" name="Defects">
                  {priorityChartData.map((entry, index) => (
                    <Cell key={`cell-${index}`} fill={entry.color} />
                  ))}
                </Bar>
              </BarChart>
            </ResponsiveContainer>
          </div>
        </div>
      </div>

      {/* График временной шкалы */}
      <div className="bg-white rounded-xl shadow-sm border border-gray-200 p-6">
        <h2 className="text-xl font-bold text-gray-900 mb-4 flex items-center gap-2">
          <Activity className="w-5 h-5 text-purple-600" />
          Defects Timeline (Last 30 Days)
        </h2>
        <div className="h-80">
          <ResponsiveContainer width="100%" height="100%">
            <LineChart data={timelineStats}>
              <CartesianGrid strokeDasharray="3 3" />
              <XAxis 
                dataKey="date" 
                tickFormatter={formatTimelineDate}
                interval="preserveStartEnd"
              />
              <YAxis />
              <Tooltip 
                labelFormatter={(label) => `Date: ${formatTimelineDate(label)}`}
                formatter={(value) => [`${value} defects`, 'Count']}
              />
              <Legend />
              <Line 
                type="monotone" 
                dataKey="createdCount" 
                stroke="#3B82F6" 
                strokeWidth={2}
                name="Created Defects"
                dot={{ fill: '#3B82F6', strokeWidth: 2, r: 4 }}
              />
              <Line 
                type="monotone" 
                dataKey="closedCount" 
                stroke="#10B981" 
                strokeWidth={2}
                name="Closed Defects"
                dot={{ fill: '#10B981', strokeWidth: 2, r: 4 }}
              />
            </LineChart>
          </ResponsiveContainer>
        </div>
      </div>

      {/* Остальная часть дашборда (проекты, команда) */}
      <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
        <div className="bg-white rounded-xl shadow-sm border border-gray-200 p-6">
          <h2 className="text-xl font-bold text-gray-900 mb-4 flex items-center gap-2">
            <TrendingUp className="w-5 h-5 text-blue-600" />
            Recent Defects
          </h2>
          <div className="space-y-3">
            {overview?.recentDefects?.slice(0, 5).map((defect) => (
              <div
                key={defect.defectId}
                className="flex items-start justify-between p-4 bg-gray-50 rounded-lg hover:bg-gray-100 transition-colors"
              >
                <div className="flex-1 min-w-0">
                  <p className="font-medium text-gray-900 truncate">{defect.defectName}</p>
                  <p className="text-sm text-gray-600 mt-1">{defect.projectName}</p>
                  <div className="flex items-center gap-3 mt-2">
                    <span className="text-xs text-gray-500">{defect.statusName}</span>
                    {defect.responsibleFio && (
                      <span className="text-xs text-gray-500">{defect.responsibleFio}</span>
                    )}
                  </div>
                </div>
                <span
                  className={`px-3 py-1 rounded-full text-xs font-medium ${getPriorityColor(
                    defect.priority
                  )}`}
                >
                  {getPriorityLabel(defect.priority)}
                </span>
              </div>
            ))}
            {(!overview?.recentDefects || overview.recentDefects.length === 0) && (
              <p className="text-gray-500 text-center py-8">No recent defects</p>
            )}
          </div>
        </div>

        <div className="bg-white rounded-xl shadow-sm border border-gray-200 p-6">
          <h2 className="text-xl font-bold text-gray-900 mb-4">Projects Overview</h2>
          <div className="space-y-3">
            {projects.slice(0, 5).map((project) => (
              <div
                key={project.projectId}
                className="p-4 bg-gray-50 rounded-lg hover:bg-gray-100 transition-colors"
              >
                <div className="flex items-start justify-between mb-2">
                  <p className="font-medium text-gray-900">{project.projectName}</p>
                  <span className="text-sm font-semibold text-blue-600">
                    {project.totalDefects}
                  </span>
                </div>
                <div className="flex items-center gap-4 text-sm text-gray-600">
                  <span className="flex items-center gap-1">
                    <Clock className="w-4 h-4" />
                    {project.openDefects} open
                  </span>
                  <span className="flex items-center gap-1">
                    <CheckCircle className="w-4 h-4" />
                    {project.closedDefects} closed
                  </span>
                  {project.highPriorityDefects > 0 && (
                    <span className="flex items-center gap-1 text-red-600">
                      <AlertTriangle className="w-4 h-4" />
                      {project.highPriorityDefects} high
                    </span>
                  )}
                </div>
              </div>
            ))}
            {projects.length === 0 && (
              <p className="text-gray-500 text-center py-8">No projects found</p>
            )}
          </div>
        </div>
      </div>

      <div className="bg-white rounded-xl shadow-sm border border-gray-200 p-6">
        <h2 className="text-xl font-bold text-gray-900 mb-4">Team Performance</h2>
        <div className="overflow-x-auto">
          <table className="w-full">
            <thead>
              <tr className="border-b border-gray-200">
                <th className="text-left py-3 px-4 font-semibold text-gray-700">User</th>
                <th className="text-left py-3 px-4 font-semibold text-gray-700">Role</th>
                <th className="text-center py-3 px-4 font-semibold text-gray-700">Total</th>
                <th className="text-center py-3 px-4 font-semibold text-gray-700">Open</th>
                <th className="text-center py-3 px-4 font-semibold text-gray-700">Closed</th>
                <th className="text-center py-3 px-4 font-semibold text-gray-700">Overdue</th>
              </tr>
            </thead>
            <tbody>
              {users.map((user) => (
                <tr key={user.userId} className="border-b border-gray-100 hover:bg-gray-50">
                  <td className="py-3 px-4 font-medium text-gray-900">{user.userFio}</td>
                  <td className="py-3 px-4 text-gray-600">{user.roleName}</td>
                  <td className="py-3 px-4 text-center text-gray-900">
                    {user.totalAssignedDefects}
                  </td>
                  <td className="py-3 px-4 text-center">
                    <span className="inline-flex items-center justify-center w-8 h-8 bg-yellow-100 text-yellow-700 rounded-full text-sm font-medium">
                      {user.openDefects}
                    </span>
                  </td>
                  <td className="py-3 px-4 text-center">
                    <span className="inline-flex items-center justify-center w-8 h-8 bg-green-100 text-green-700 rounded-full text-sm font-medium">
                      {user.closedDefects}
                    </span>
                  </td>
                  <td className="py-3 px-4 text-center">
                    {user.overdueDefects > 0 ? (
                      <span className="inline-flex items-center justify-center w-8 h-8 bg-red-100 text-red-700 rounded-full text-sm font-medium">
                        {user.overdueDefects}
                      </span>
                    ) : (
                      <span className="text-gray-400">0</span>
                    )}
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
          {users.length === 0 && (
            <p className="text-gray-500 text-center py-8">No user data available</p>
          )}
        </div>
      </div>
    </div>
  );
}