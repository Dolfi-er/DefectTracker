import { useEffect, useState } from 'react';
import { apiService } from '../services/api';
import type { DefectDTO, ProjectDTO, DefectStatusDTO, UserDTO, DefectUpdateDTO } from '../types/api';
import { Plus, Search, Filter, AlertCircle, Calendar, User, Edit2, Trash2 } from 'lucide-react';
import { useAuth } from '../contexts/AuthContext';

export function DefectsPage() {
  const { user } = useAuth();
  const [defects, setDefects] = useState<DefectDTO[]>([]);
  const [projects, setProjects] = useState<ProjectDTO[]>([]);
  const [statuses, setStatuses] = useState<DefectStatusDTO[]>([]);
  const [users, setUsers] = useState<UserDTO[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [searchTerm, setSearchTerm] = useState('');
  const [filterProject, setFilterProject] = useState<number | null>(null);
  const [filterStatus, setFilterStatus] = useState<number | null>(null);
  const [selectedDefect, setSelectedDefect] = useState<DefectDTO | null>(null);
  const [showCreateModal, setShowCreateModal] = useState(false);
  const [editingDefect, setEditingDefect] = useState<DefectDTO | null>(null);

  useEffect(() => {
    loadData();
  }, []);

  const loadData = async () => {
    try {
      const [defectsData, projectsData, statusesData, usersData] = await Promise.all([
        apiService.getDefects(),
        apiService.getProjects(),
        apiService.getDefectStatuses(),
        apiService.getUsers(),
      ]);
      setDefects(defectsData);
      setProjects(projectsData);
      setStatuses(statusesData);
      setUsers(usersData);
    } catch (error) {
      console.error('Failed to load data:', error);
    } finally {
      setIsLoading(false);
    }
  };

  const filteredDefects = defects.filter((defect) => {
    const matchesSearch =
      searchTerm === '' ||
      defect.info.defectName.toLowerCase().includes(searchTerm.toLowerCase()) ||
      defect.info.defectDescription.toLowerCase().includes(searchTerm.toLowerCase());
    const matchesProject = filterProject === null || defect.projectId === filterProject;
    const matchesStatus = filterStatus === null || defect.statusId === filterStatus;
    return matchesSearch && matchesProject && matchesStatus;
  });

  const getPriorityLabel = (priority: number) => {
    if (priority >= 3) return 'High';
    if (priority >= 2) return 'Medium';
    return 'Low';
  };

  const getPriorityColor = (priority: number) => {
    if (priority >= 3) return 'bg-red-100 text-red-700 border-red-200';
    if (priority >= 2) return 'bg-yellow-100 text-yellow-700 border-yellow-200';
    return 'bg-green-100 text-green-700 border-green-200';
  };

  const handleCreateDefect = async (e: React.FormEvent<HTMLFormElement>) => {
    e.preventDefault();
    const formData = new FormData(e.currentTarget);

    try {
      await apiService.createDefect({
        projectId: Number(formData.get('projectId')),
        statusId: Number(formData.get('statusId')),
        responsibleId: formData.get('responsibleId') ? Number(formData.get('responsibleId')) : null,
        createdById: user!.userId,
        info: {
          defectName: formData.get('defectName') as string,
          defectDescription: formData.get('defectDescription') as string,
          priority: Number(formData.get('priority')),
          dueDate: new Date(formData.get('dueDate') as string).toISOString(), // Преобразуем в ISO строку
        },
      });
      await loadData();
      setShowCreateModal(false);
    } catch (error) {
      console.error('Failed to create defect:', error);
      alert('Failed to create defect');
    }
  };

  const handleUpdateDefect = async (e: React.FormEvent<HTMLFormElement>) => {
    e.preventDefault();
    if (!editingDefect) return;

    const formData = new FormData(e.currentTarget);

    try {
      const updateData: DefectUpdateDTO = {
        projectId: Number(formData.get('projectId')),
        statusId: Number(formData.get('statusId')),
        responsibleId: formData.get('responsibleId') ? Number(formData.get('responsibleId')) : null,
        info: {
          defectName: formData.get('defectName') as string,
          defectDescription: formData.get('defectDescription') as string,
          priority: Number(formData.get('priority')),
          dueDate: new Date(formData.get('dueDate') as string).toISOString(), // Преобразуем в ISO строку
        },
      };

      await apiService.updateDefect(editingDefect.defectId, updateData);
      await loadData();
      setEditingDefect(null);
    } catch (error) {
      console.error('Failed to update defect:', error);
      alert('Failed to update defect');
    }
  };

  const handleDeleteDefect = async (id: number) => {
    if (!confirm('Are you sure you want to delete this defect?')) return;

    try {
      await apiService.deleteDefect(id);
      await loadData();
    } catch (error) {
      console.error('Failed to delete defect:', error);
      alert('Failed to delete defect');
    }
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
          <h1 className="text-3xl font-bold text-gray-900">Defects</h1>
          <p className="text-gray-600 mt-1">Manage and track all defects</p>
        </div>
        <button
          onClick={() => setShowCreateModal(true)}
          className="flex items-center gap-2 bg-blue-600 text-white px-6 py-3 rounded-lg hover:bg-blue-700 transition-colors font-medium"
        >
          <Plus className="w-5 h-5" />
          New Defect
        </button>
      </div>

      <div className="bg-white rounded-xl shadow-sm border border-gray-200 p-4">
        <div className="grid grid-cols-1 md:grid-cols-4 gap-4">
          <div className="relative">
            <Search className="absolute left-3 top-1/2 -translate-y-1/2 w-5 h-5 text-gray-400" />
            <input
              type="text"
              placeholder="Search defects..."
              value={searchTerm}
              onChange={(e) => setSearchTerm(e.target.value)}
              className="w-full pl-10 pr-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
            />
          </div>
          <select
            value={filterProject || ''}
            onChange={(e) => setFilterProject(e.target.value ? Number(e.target.value) : null)}
            className="px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
          >
            <option value="">All Projects</option>
            {projects.map((project) => (
              <option key={project.projectId} value={project.projectId}>
                {project.name}
              </option>
            ))}
          </select>
          <select
            value={filterStatus || ''}
            onChange={(e) => setFilterStatus(e.target.value ? Number(e.target.value) : null)}
            className="px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
          >
            <option value="">All Statuses</option>
            {statuses.map((status) => (
              <option key={status.id} value={status.id}>
                {status.statusName}
              </option>
            ))}
          </select>
          <button
            onClick={() => {
              setSearchTerm('');
              setFilterProject(null);
              setFilterStatus(null);
            }}
            className="flex items-center justify-center gap-2 px-4 py-2 border border-gray-300 rounded-lg hover:bg-gray-50 transition-colors"
          >
            <Filter className="w-5 h-5" />
            Clear Filters
          </button>
        </div>
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-2 xl:grid-cols-3 gap-6">
        {filteredDefects.map((defect) => (
          <div
            key={defect.defectId}
            className="bg-white rounded-xl shadow-sm border border-gray-200 p-6 hover:shadow-md transition-shadow"
          >
            <div className="flex items-start justify-between mb-4">
              <h3 className="text-lg font-semibold text-gray-900 line-clamp-2">
                {defect.info.defectName}
              </h3>
              <span
                className={`px-3 py-1 rounded-full text-xs font-medium border ${getPriorityColor(
                  defect.info.priority
                )}`}
              >
                {getPriorityLabel(defect.info.priority)}
              </span>
            </div>

            <p className="text-gray-600 text-sm mb-4 line-clamp-2">
              {defect.info.defectDescription}
            </p>

            <div className="space-y-2">
              <div className="flex items-center gap-2 text-sm text-gray-600">
                <AlertCircle className="w-4 h-4" />
                <span>{defect.projectName}</span>
              </div>
              <div className="flex items-center gap-2 text-sm text-gray-600">
                <Calendar className="w-4 h-4" />
                <span>{new Date(defect.info.dueDate).toLocaleDateString()}</span>
              </div>
              {defect.responsibleFio && (
                <div className="flex items-center gap-2 text-sm text-gray-600">
                  <User className="w-4 h-4" />
                  <span>{defect.responsibleFio}</span>
                </div>
              )}
            </div>

            <div className="mt-4 pt-4 border-t border-gray-100 flex items-center justify-between">
              <span className="text-sm font-medium text-blue-600">{defect.statusName}</span>
              <div className="flex gap-2">
                <button
                  onClick={() => setEditingDefect(defect)}
                  className="p-2 text-gray-600 hover:text-blue-600 hover:bg-blue-50 rounded-lg transition-colors"
                  title="Edit defect"
                >
                  <Edit2 className="w-4 h-4" />
                </button>
                <button
                  onClick={() => handleDeleteDefect(defect.defectId)}
                  className="p-2 text-gray-600 hover:text-red-600 hover:bg-red-50 rounded-lg transition-colors"
                  title="Delete defect"
                >
                  <Trash2 className="w-4 h-4" />
                </button>
                <button
                  onClick={() => setSelectedDefect(defect)}
                  className="text-sm text-blue-600 hover:text-blue-700 font-medium"
                >
                  View Details
                </button>
              </div>
            </div>
          </div>
        ))}
      </div>

      {filteredDefects.length === 0 && (
        <div className="bg-white rounded-xl shadow-sm border border-gray-200 p-12 text-center">
          <AlertCircle className="w-12 h-12 text-gray-400 mx-auto mb-4" />
          <h3 className="text-lg font-semibold text-gray-900 mb-2">No defects found</h3>
          <p className="text-gray-600">Try adjusting your filters or create a new defect</p>
        </div>
      )}

      {showCreateModal && (
        <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center p-4 z-50">
          <div className="bg-white rounded-2xl shadow-2xl max-w-2xl w-full max-h-[90vh] overflow-y-auto">
            <div className="p-6 border-b border-gray-200">
              <h2 className="text-2xl font-bold text-gray-900">Create New Defect</h2>
            </div>
            <form onSubmit={handleCreateDefect} className="p-6 space-y-4">
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-2">
                  Defect Name
                </label>
                <input
                  type="text"
                  name="defectName"
                  required
                  className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                />
              </div>
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-2">
                  Description
                </label>
                <textarea
                  name="defectDescription"
                  required
                  rows={4}
                  className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                />
              </div>
              <div className="grid grid-cols-2 gap-4">
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-2">Project</label>
                  <select
                    name="projectId"
                    required
                    className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                  >
                    {projects.map((project) => (
                      <option key={project.projectId} value={project.projectId}>
                        {project.name}
                      </option>
                    ))}
                  </select>
                </div>
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-2">Status</label>
                  <select
                    name="statusId"
                    required
                    className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                  >
                    {statuses.map((status) => (
                      <option key={status.id} value={status.id}>
                        {status.statusName}
                      </option>
                    ))}
                  </select>
                </div>
              </div>
              <div className="grid grid-cols-2 gap-4">
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-2">Priority</label>
                  <select
                    name="priority"
                    required
                    className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                  >
                    <option value="1">Low</option>
                    <option value="2">Medium</option>
                    <option value="3">High</option>
                  </select>
                </div>
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-2">Due Date</label>
                  <input
                    type="date"
                    name="dueDate"
                    required
                    className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                  />
                </div>
              </div>
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-2">
                  Assign To (Optional)
                </label>
                <select
                  name="responsibleId"
                  className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                >
                  <option value="">Unassigned</option>
                  {users.map((u) => (
                    <option key={u.userId} value={u.userId}>
                      {u.fio} ({u.roleName})
                    </option>
                  ))}
                </select>
              </div>
              <div className="flex gap-3 pt-4">
                <button
                  type="submit"
                  className="flex-1 bg-blue-600 text-white py-3 rounded-lg font-medium hover:bg-blue-700 transition-colors"
                >
                  Create Defect
                </button>
                <button
                  type="button"
                  onClick={() => setShowCreateModal(false)}
                  className="flex-1 bg-gray-100 text-gray-700 py-3 rounded-lg font-medium hover:bg-gray-200 transition-colors"
                >
                  Cancel
                </button>
              </div>
            </form>
          </div>
        </div>
      )}

      {editingDefect && (
        <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center p-4 z-50">
          <div className="bg-white rounded-2xl shadow-2xl max-w-2xl w-full max-h-[90vh] overflow-y-auto">
            <div className="p-6 border-b border-gray-200">
              <h2 className="text-2xl font-bold text-gray-900">Edit Defect</h2>
            </div>
            <form onSubmit={handleUpdateDefect} className="p-6 space-y-4">
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-2">
                  Defect Name
                </label>
                <input
                  type="text"
                  name="defectName"
                  required
                  defaultValue={editingDefect.info.defectName}
                  className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                />
              </div>
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-2">
                  Description
                </label>
                <textarea
                  name="defectDescription"
                  required
                  rows={4}
                  defaultValue={editingDefect.info.defectDescription}
                  className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                />
              </div>
              <div className="grid grid-cols-2 gap-4">
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-2">Project</label>
                  <select
                    name="projectId"
                    required
                    defaultValue={editingDefect.projectId}
                    className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                  >
                    {projects.map((project) => (
                      <option key={project.projectId} value={project.projectId}>
                        {project.name}
                      </option>
                    ))}
                  </select>
                </div>
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-2">Status</label>
                  <select
                    name="statusId"
                    required
                    defaultValue={editingDefect.statusId}
                    className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                  >
                    {statuses.map((status) => (
                      <option key={status.id} value={status.id}>
                        {status.statusName}
                      </option>
                    ))}
                  </select>
                </div>
              </div>
              <div className="grid grid-cols-2 gap-4">
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-2">Priority</label>
                  <select
                    name="priority"
                    required
                    defaultValue={editingDefect.info.priority}
                    className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                  >
                    <option value="1">Low</option>
                    <option value="2">Medium</option>
                    <option value="3">High</option>
                  </select>
                </div>
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-2">Due Date</label>
                  <input
                    type="date"
                    name="dueDate"
                    required
                    defaultValue={editingDefect.info.dueDate.split('T')[0]}
                    className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                  />
                </div>
              </div>
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-2">
                  Assign To (Optional)
                </label>
                <select
                  name="responsibleId"
                  defaultValue={editingDefect.responsibleId || ''}
                  className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                >
                  <option value="">Unassigned</option>
                  {users.map((u) => (
                    <option key={u.userId} value={u.userId}>
                      {u.fio} ({u.roleName})
                    </option>
                  ))}
                </select>
              </div>
              <div className="flex gap-3 pt-4">
                <button
                  type="submit"
                  className="flex-1 bg-blue-600 text-white py-3 rounded-lg font-medium hover:bg-blue-700 transition-colors"
                >
                  Save Changes
                </button>
                <button
                  type="button"
                  onClick={() => setEditingDefect(null)}
                  className="flex-1 bg-gray-100 text-gray-700 py-3 rounded-lg font-medium hover:bg-gray-200 transition-colors"
                >
                  Cancel
                </button>
              </div>
            </form>
          </div>
        </div>
      )}

      {selectedDefect && (
        <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center p-4 z-50">
          <div className="bg-white rounded-2xl shadow-2xl max-w-3xl w-full max-h-[90vh] overflow-y-auto">
            <div className="p-6 border-b border-gray-200">
              <div className="flex items-start justify-between">
                <div className="flex-1">
                  <h2 className="text-2xl font-bold text-gray-900 mb-2">
                    {selectedDefect.info.defectName}
                  </h2>
                  <div className="flex items-center gap-3">
                    <span
                      className={`px-3 py-1 rounded-full text-xs font-medium border ${getPriorityColor(
                        selectedDefect.info.priority
                      )}`}
                    >
                      {getPriorityLabel(selectedDefect.info.priority)}
                    </span>
                    <span className="text-sm text-blue-600 font-medium">
                      {selectedDefect.statusName}
                    </span>
                  </div>
                </div>
                <button
                  onClick={() => setSelectedDefect(null)}
                  className="text-gray-400 hover:text-gray-600 text-2xl leading-none"
                >
                  ×
                </button>
              </div>
            </div>
            <div className="p-6 space-y-6">
              <div>
                <h3 className="text-sm font-semibold text-gray-700 mb-2">Description</h3>
                <p className="text-gray-900">{selectedDefect.info.defectDescription}</p>
              </div>
              <div className="grid grid-cols-2 gap-6">
                <div>
                  <h3 className="text-sm font-semibold text-gray-700 mb-2">Project</h3>
                  <p className="text-gray-900">{selectedDefect.projectName}</p>
                </div>
                <div>
                  <h3 className="text-sm font-semibold text-gray-700 mb-2">Due Date</h3>
                  <p className="text-gray-900">
                    {new Date(selectedDefect.info.dueDate).toLocaleDateString()}
                  </p>
                </div>
                <div>
                  <h3 className="text-sm font-semibold text-gray-700 mb-2">Created By</h3>
                  <p className="text-gray-900">{selectedDefect.createdByFio}</p>
                </div>
                <div>
                  <h3 className="text-sm font-semibold text-gray-700 mb-2">Assigned To</h3>
                  <p className="text-gray-900">{selectedDefect.responsibleFio || 'Unassigned'}</p>
                </div>
                <div>
                  <h3 className="text-sm font-semibold text-gray-700 mb-2">Created</h3>
                  <p className="text-gray-900">
                    {new Date(selectedDefect.createdDate).toLocaleString()}
                  </p>
                </div>
                <div>
                  <h3 className="text-sm font-semibold text-gray-700 mb-2">Last Updated</h3>
                  <p className="text-gray-900">
                    {new Date(selectedDefect.updatedDate).toLocaleString()}
                  </p>
                </div>
              </div>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}