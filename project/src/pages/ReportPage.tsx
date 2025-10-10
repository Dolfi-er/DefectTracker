import { useState } from 'react';
import { apiService } from '../services/api';
import * as XLSX from 'xlsx';
import { saveAs } from 'file-saver';
import { Download, FileText, BarChart3, Users, AlertTriangle, Loader2 } from 'lucide-react';

export function ReportsPage() {
  const [loading, setLoading] = useState<string | null>(null);

  const exportToExcel = (data: any[], fileName: string) => {
    const worksheet = XLSX.utils.json_to_sheet(data);
    const workbook = XLSX.utils.book_new();
    XLSX.utils.book_append_sheet(workbook, worksheet, 'Sheet1');
    
    const excelBuffer = XLSX.write(workbook, { bookType: 'xlsx', type: 'array' });
    const dataBlob = new Blob([excelBuffer], { 
      type: 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet;charset=UTF-8' 
    });
    
    saveAs(dataBlob, `${fileName}_${new Date().toISOString().split('T')[0]}.xlsx`);
  };

  const handleExportDefects = async () => {
    setLoading('defects');
    try {
      const defects = await apiService.getDefects();
      const data = defects.map(defect => ({
        'Defect ID': defect.defectId,
        'Defect Name': defect.info.defectName,
        'Description': defect.info.defectDescription,
        'Project': defect.projectName,
        'Status': defect.statusName,
        'Priority': defect.info.priority,
        'Due Date': new Date(defect.info.dueDate).toLocaleDateString(),
        'Responsible': defect.responsibleFio || 'Unassigned',
        'Created By': defect.createdByFio,
        'Created Date': new Date(defect.createdDate).toLocaleDateString(),
        'Updated Date': new Date(defect.updatedDate).toLocaleDateString(),
      }));
      exportToExcel(data, 'defects_report');
    } catch (error) {
      console.error('Failed to export defects:', error);
      alert('Failed to export defects report');
    } finally {
      setLoading(null);
    }
  };

  const handleExportProjects = async () => {
    setLoading('projects');
    try {
      const projects = await apiService.getDefectsByProject();
      const data = projects.map(project => ({
        'Project ID': project.projectId,
        'Project Name': project.projectName,
        'Total Defects': project.totalDefects,
        'Open Defects': project.openDefects,
        'Closed Defects': project.closedDefects,
        'High Priority Defects': project.highPriorityDefects,
      }));
      exportToExcel(data, 'projects_report');
    } catch (error) {
      console.error('Failed to export projects:', error);
      alert('Failed to export projects report');
    } finally {
      setLoading(null);
    }
  };

  const handleExportUsers = async () => {
    setLoading('users');
    try {
      const users = await apiService.getDefectsByUser();
      const data = users.map(user => ({
        'User ID': user.userId,
        'Full Name': user.userFio,
        'Role': user.roleName,
        'Total Assigned Defects': user.totalAssignedDefects,
        'Open Defects': user.openDefects,
        'Closed Defects': user.closedDefects,
        'Overdue Defects': user.overdueDefects,
        'Average Completion Days': user.averageCompletionDays?.toFixed(2) || 'N/A',
      }));
      exportToExcel(data, 'users_report');
    } catch (error) {
      console.error('Failed to export users:', error);
      alert('Failed to export users report');
    } finally {
      setLoading(null);
    }
  };

  const handleExportOverview = async () => {
    setLoading('overview');
    try {
      const [overview, statusStats, timelineStats] = await Promise.all([
        apiService.getOverviewStats(),
        apiService.getDefectsByStatus(),
        apiService.getDefectsTimeline(30),
      ]);

      // Overview data
      const overviewData = [{
        'Total Defects': overview.totalDefects,
        'High Priority Defects': overview.defectsByPriority?.['3'] || 0,
        'Medium Priority Defects': overview.defectsByPriority?.['2'] || 0,
        'Low Priority Defects': overview.defectsByPriority?.['1'] || 0,
      }];

      // Status data
      const statusData = statusStats.map(status => ({
        'Status': status.statusName,
        'Count': status.count,
        'Percentage': `${status.percentage.toFixed(1)}%`,
      }));

      // Timeline data
      const timelineData = timelineStats.map(day => ({
        'Date': new Date(day.date).toLocaleDateString(),
        'Defects Created': day.createdCount,
        'Defects Closed': day.closedCount,
      }));

      // Create workbook with multiple sheets
      const workbook = XLSX.utils.book_new();
      
      XLSX.utils.book_append_sheet(workbook, 
        XLSX.utils.json_to_sheet(overviewData), 'Overview');
      XLSX.utils.book_append_sheet(workbook, 
        XLSX.utils.json_to_sheet(statusData), 'Defects by Status');
      XLSX.utils.book_append_sheet(workbook, 
        XLSX.utils.json_to_sheet(timelineData), 'Timeline (30 days)');
      
      const excelBuffer = XLSX.write(workbook, { bookType: 'xlsx', type: 'array' });
      const dataBlob = new Blob([excelBuffer], { 
        type: 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet;charset=UTF-8' 
      });
      
      saveAs(dataBlob, `dashboard_overview_${new Date().toISOString().split('T')[0]}.xlsx`);
    } catch (error) {
      console.error('Failed to export overview:', error);
      alert('Failed to export overview report');
    } finally {
      setLoading(null);
    }
  };

  const reportCards = [
    {
      id: 'overview',
      title: 'Dashboard Overview',
      description: 'Complete dashboard statistics including defects by status and timeline',
      icon: BarChart3,
      color: 'bg-blue-500',
      onClick: handleExportOverview,
    },
    {
      id: 'defects',
      title: 'All Defects',
      description: 'Detailed list of all defects with complete information',
      icon: AlertTriangle,
      color: 'bg-red-500',
      onClick: handleExportDefects,
    },
    {
      id: 'projects',
      title: 'Projects Report',
      description: 'Defects statistics grouped by projects',
      icon: FileText,
      color: 'bg-green-500',
      onClick: handleExportProjects,
    },
    {
      id: 'users',
      title: 'Team Performance',
      description: 'User performance metrics and defect assignments',
      icon: Users,
      color: 'bg-purple-500',
      onClick: handleExportUsers,
    },
  ];

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-3xl font-bold text-gray-900">Reports</h1>
        <p className="text-gray-600 mt-1">Export data to Excel format</p>
      </div>

      <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
        {reportCards.map((report) => {
          const Icon = report.icon;
          return (
            <div key={report.id} className="bg-white rounded-xl shadow-sm border border-gray-200 p-6">
              <div className="flex items-start justify-between mb-4">
                <div className={`w-12 h-12 ${report.color} rounded-lg flex items-center justify-center`}>
                  <Icon className="w-6 h-6 text-white" />
                </div>
              </div>
              <h3 className="text-lg font-semibold text-gray-900 mb-2">{report.title}</h3>
              <p className="text-gray-600 text-sm mb-4">{report.description}</p>
              <button
                onClick={report.onClick}
                disabled={loading === report.id}
                className="w-full flex items-center justify-center gap-2 px-4 py-2 bg-gray-900 text-white rounded-lg hover:bg-gray-800 disabled:opacity-50 disabled:cursor-not-allowed transition-colors"
              >
                {loading === report.id ? (
                  <Loader2 className="w-4 h-4 animate-spin" />
                ) : (
                  <Download className="w-4 h-4" />
                )}
                {loading === report.id ? 'Exporting...' : 'Export to Excel'}
              </button>
            </div>
          );
        })}
      </div>

      <div className="bg-blue-50 border border-blue-200 rounded-xl p-6">
        <div className="flex items-start gap-4">
          <div className="w-8 h-8 bg-blue-100 rounded-lg flex items-center justify-center flex-shrink-0">
            <FileText className="w-4 h-4 text-blue-600" />
          </div>
          <div>
            <h3 className="font-semibold text-blue-900 mb-2">About Reports</h3>
            <p className="text-blue-700 text-sm">
              All reports are generated in Excel format and include the current date in the filename. 
              The Dashboard Overview report contains multiple sheets with different statistics.
            </p>
          </div>
        </div>
      </div>
    </div>
  );
}