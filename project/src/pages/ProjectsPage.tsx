import { useEffect, useState } from 'react';
import { apiService } from '../services/api';
import type { ProjectDTO, ProjectStatusDTO } from '../types/api';
import { Plus, FolderOpen, Calendar, Edit2, Trash2 } from 'lucide-react';

export function ProjectsPage() {
  const [projects, setProjects] = useState<ProjectDTO[]>([]);
  const [statuses, setStatuses] = useState<ProjectStatusDTO[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [showCreateModal, setShowCreateModal] = useState(false);
  const [editingProject, setEditingProject] = useState<ProjectDTO | null>(null);

  useEffect(() => {
    loadData();
  }, []);

  const loadData = async () => {
    try {
      const [projectsData, statusesData] = await Promise.all([
        apiService.getProjects(),
        apiService.getProjectStatuses(),
      ]);
      setProjects(projectsData);
      setStatuses(statusesData);
    } catch (error) {
      console.error('Failed to load data:', error);
    } finally {
      setIsLoading(false);
    }
  };

  const handleCreateProject = async (e: React.FormEvent<HTMLFormElement>) => {
    e.preventDefault();
    const formData = new FormData(e.currentTarget);

    try {
      await apiService.createProject({
        projectStatusId: Number(formData.get('projectStatusId')),
        name: formData.get('name') as string,
        description: formData.get('description') as string,
      });
      await loadData();
      setShowCreateModal(false);
    } catch (error) {
      console.error('Failed to create project:', error);
      alert('Failed to create project');
    }
  };

  const handleUpdateProject = async (e: React.FormEvent<HTMLFormElement>) => {
    e.preventDefault();
    if (!editingProject) return;

    const formData = new FormData(e.currentTarget);

    try {
      await apiService.updateProject(editingProject.projectId, {
        projectStatusId: Number(formData.get('projectStatusId')),
        name: formData.get('name') as string,
        description: formData.get('description') as string,
      });
      await loadData();
      setEditingProject(null);
    } catch (error) {
      console.error('Failed to update project:', error);
      alert('Failed to update project');
    }
  };

  const handleDeleteProject = async (id: number) => {
    if (!confirm('Are you sure you want to delete this project?')) return;

    try {
      await apiService.deleteProject(id);
      await loadData();
    } catch (error) {
      console.error('Failed to delete project:', error);
      alert('Failed to delete project');
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
          <h1 className="text-3xl font-bold text-gray-900">Projects</h1>
          <p className="text-gray-600 mt-1">Manage all your projects</p>
        </div>
        <button
          onClick={() => setShowCreateModal(true)}
          className="flex items-center gap-2 bg-blue-600 text-white px-6 py-3 rounded-lg hover:bg-blue-700 transition-colors font-medium"
        >
          <Plus className="w-5 h-5" />
          New Project
        </button>
      </div>

      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
        {projects.map((project) => (
          <div
            key={project.projectId}
            className="bg-white rounded-xl shadow-sm border border-gray-200 p-6 hover:shadow-md transition-shadow"
          >
            <div className="flex items-start justify-between mb-4">
              <div className="flex-1">
                <div className="flex items-center gap-3 mb-2">
                  <FolderOpen className="w-6 h-6 text-blue-600" />
                  <h3 className="text-lg font-semibold text-gray-900">{project.name}</h3>
                </div>
                <span className="inline-block px-3 py-1 bg-blue-50 text-blue-700 text-xs font-medium rounded-full">
                  {project.projectStatusName}
                </span>
              </div>
              <div className="flex gap-2">
                <button
                  onClick={() => setEditingProject(project)}
                  className="p-2 text-gray-600 hover:text-blue-600 hover:bg-blue-50 rounded-lg transition-colors"
                >
                  <Edit2 className="w-4 h-4" />
                </button>
                <button
                  onClick={() => handleDeleteProject(project.projectId)}
                  className="p-2 text-gray-600 hover:text-red-600 hover:bg-red-50 rounded-lg transition-colors"
                >
                  <Trash2 className="w-4 h-4" />
                </button>
              </div>
            </div>

            <p className="text-gray-600 text-sm mb-4 line-clamp-3">{project.description}</p>

            <div className="pt-4 border-t border-gray-100 space-y-2">
              <div className="flex items-center gap-2 text-xs text-gray-500">
                <Calendar className="w-4 h-4" />
                <span>Created: {new Date(project.createdDate).toLocaleDateString()}</span>
              </div>
              <div className="flex items-center gap-2 text-xs text-gray-500">
                <Calendar className="w-4 h-4" />
                <span>Updated: {new Date(project.updatedDate).toLocaleDateString()}</span>
              </div>
            </div>
          </div>
        ))}
      </div>

      {projects.length === 0 && (
        <div className="bg-white rounded-xl shadow-sm border border-gray-200 p-12 text-center">
          <FolderOpen className="w-12 h-12 text-gray-400 mx-auto mb-4" />
          <h3 className="text-lg font-semibold text-gray-900 mb-2">No projects yet</h3>
          <p className="text-gray-600 mb-4">Get started by creating your first project</p>
          <button
            onClick={() => setShowCreateModal(true)}
            className="inline-flex items-center gap-2 bg-blue-600 text-white px-6 py-3 rounded-lg hover:bg-blue-700 transition-colors font-medium"
          >
            <Plus className="w-5 h-5" />
            Create Project
          </button>
        </div>
      )}

      {showCreateModal && (
        <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center p-4 z-50">
          <div className="bg-white rounded-2xl shadow-2xl max-w-2xl w-full">
            <div className="p-6 border-b border-gray-200">
              <h2 className="text-2xl font-bold text-gray-900">Create New Project</h2>
            </div>
            <form onSubmit={handleCreateProject} className="p-6 space-y-4">
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-2">
                  Project Name
                </label>
                <input
                  type="text"
                  name="name"
                  required
                  className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                />
              </div>
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-2">
                  Description
                </label>
                <textarea
                  name="description"
                  required
                  rows={4}
                  className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                />
              </div>
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-2">Status</label>
                <select
                  name="projectStatusId"
                  required
                  className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                >
                  {statuses.map((status) => (
                    <option key={status.projectStatusId} value={status.projectStatusId}>
                      {status.projectStatusName}
                    </option>
                  ))}
                </select>
              </div>
              <div className="flex gap-3 pt-4">
                <button
                  type="submit"
                  className="flex-1 bg-blue-600 text-white py-3 rounded-lg font-medium hover:bg-blue-700 transition-colors"
                >
                  Create Project
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

      {editingProject && (
        <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center p-4 z-50">
          <div className="bg-white rounded-2xl shadow-2xl max-w-2xl w-full">
            <div className="p-6 border-b border-gray-200">
              <h2 className="text-2xl font-bold text-gray-900">Edit Project</h2>
            </div>
            <form onSubmit={handleUpdateProject} className="p-6 space-y-4">
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-2">
                  Project Name
                </label>
                <input
                  type="text"
                  name="name"
                  required
                  defaultValue={editingProject.name}
                  className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                />
              </div>
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-2">
                  Description
                </label>
                <textarea
                  name="description"
                  required
                  rows={4}
                  defaultValue={editingProject.description}
                  className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                />
              </div>
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-2">Status</label>
                <select
                  name="projectStatusId"
                  required
                  defaultValue={editingProject.projectStatusId}
                  className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                >
                  {statuses.map((status) => (
                    <option key={status.projectStatusId} value={status.projectStatusId}>
                      {status.projectStatusName}
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
                  onClick={() => setEditingProject(null)}
                  className="flex-1 bg-gray-100 text-gray-700 py-3 rounded-lg font-medium hover:bg-gray-200 transition-colors"
                >
                  Cancel
                </button>
              </div>
            </form>
          </div>
        </div>
      )}
    </div>
  );
}
