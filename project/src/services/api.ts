import type {
  LoginDTO,
  RegisterDTO,
  AuthResponseDTO,
  UserDTO,
  DefectDTO,
  DefectCreateDTO,
  DefectUpdateDTO,
  ProjectDTO,
  ProjectCreateDTO,
  ProjectUpdateDTO,
  DefectStatusDTO,
  ProjectStatusDTO,
  RoleDTO,
  CommentDTO,
  CommentCreateDTO,
  DefectHistoryDTO,
  OverviewStatsDTO,
  StatusStatsDTO,
  ProjectStatsDTO,
  UserStatsDTO,
  PriorityMetricsDTO,
  TimelineStatsDTO,
} from '../types/api';

class ApiService {
  private baseURL: string = 'http://localhost:5229';
  private token: string | null = null;

  setBaseURL(url: string) {
    this.baseURL = url.replace(/\/$/, '');
  }

  setToken(token: string | null) {
    this.token = token;
    if (token) {
      localStorage.setItem('auth_token', token);
    } else {
      localStorage.removeItem('auth_token');
    }
  }

  getToken(): string | null {
    if (!this.token) {
      this.token = localStorage.getItem('auth_token');
    }
    return this.token;
  }

  private async fetch<T>(endpoint: string, options: RequestInit = {}): Promise<T> {
    const token = this.getToken();
    const headers: HeadersInit = {
      'Content-Type': 'application/json',
      ...(token && { Authorization: `Bearer ${token}` }),
      ...options.headers,
    };

    const response = await fetch(`${this.baseURL}${endpoint}`, {
      ...options,
      headers,
    });

    if (!response.ok) {
      const error = await response.text();
      throw new Error(error || `HTTP ${response.status}`);
    }

    if (response.status === 204) {
      return undefined as T;
    }

    return response.json();
  }

  async login(data: LoginDTO): Promise<AuthResponseDTO> {
    const result = await this.fetch<AuthResponseDTO>('/api/Auth/login', {
      method: 'POST',
      body: JSON.stringify(data),
    });
    this.setToken(result.token);
    return result;
  }

  async register(data: RegisterDTO): Promise<UserDTO> {
    return this.fetch<UserDTO>('/api/Auth/register', {
      method: 'POST',
      body: JSON.stringify(data),
    });
  }

  async logout(): Promise<void> {
    await this.fetch<void>('/api/Auth/logout', { method: 'POST' });
    this.setToken(null);
  }

  async getCurrentUser(): Promise<UserDTO> {
    return this.fetch<UserDTO>('/api/Auth/me');
  }

  async getDefects(): Promise<DefectDTO[]> {
    return this.fetch<DefectDTO[]>('/api/Defects');
  }

  async getDefect(id: number): Promise<DefectDTO> {
    return this.fetch<DefectDTO>(`/api/Defects/${id}`);
  }

  async createDefect(data: DefectCreateDTO): Promise<DefectDTO> {
    return this.fetch<DefectDTO>('/api/Defects', {
      method: 'POST',
      body: JSON.stringify(data),
    });
  }

  async updateDefect(id: number, data: DefectUpdateDTO): Promise<void> {
    return this.fetch<void>(`/api/Defects/${id}`, {
      method: 'PUT',
      body: JSON.stringify(data),
    });
  }

  async deleteDefect(id: number): Promise<void> {
    return this.fetch<void>(`/api/Defects/${id}`, { method: 'DELETE' });
  }

  async getProjects(): Promise<ProjectDTO[]> {
    return this.fetch<ProjectDTO[]>('/api/Projects');
  }

  async getProject(id: number): Promise<ProjectDTO> {
    return this.fetch<ProjectDTO>(`/api/Projects/${id}`);
  }

  async createProject(data: ProjectCreateDTO): Promise<ProjectDTO> {
    return this.fetch<ProjectDTO>('/api/Projects', {
      method: 'POST',
      body: JSON.stringify(data),
    });
  }

  async updateProject(id: number, data: ProjectUpdateDTO): Promise<void> {
    return this.fetch<void>(`/api/Projects/${id}`, {
      method: 'PUT',
      body: JSON.stringify(data),
    });
  }

  async deleteProject(id: number): Promise<void> {
    return this.fetch<void>(`/api/Projects/${id}`, { method: 'DELETE' });
  }

  async getDefectStatuses(): Promise<DefectStatusDTO[]> {
    return this.fetch<DefectStatusDTO[]>('/api/DefectStatuses');
  }

  async getProjectStatuses(): Promise<ProjectStatusDTO[]> {
    return this.fetch<ProjectStatusDTO[]>('/api/ProjectStatuses');
  }

  async getRoles(): Promise<RoleDTO[]> {
    return this.fetch<RoleDTO[]>('/api/Roles');
  }

  async getUsers(): Promise<UserDTO[]> {
    return this.fetch<UserDTO[]>('/api/Users');
  }

  async getCommentsByDefect(defectId: number): Promise<CommentDTO[]> {
    return this.fetch<CommentDTO[]>(`/api/Comments/defect/${defectId}`);
  }

  async createComment(data: CommentCreateDTO): Promise<CommentDTO> {
    return this.fetch<CommentDTO>('/api/Comments', {
      method: 'POST',
      body: JSON.stringify(data),
    });
  }

  async getDefectHistory(defectId: number): Promise<DefectHistoryDTO[]> {
    return this.fetch<DefectHistoryDTO[]>(`/api/DefectHistories/defect/${defectId}`);
  }

  async getOverviewStats(): Promise<OverviewStatsDTO> {
    return this.fetch<OverviewStatsDTO>('/api/Statistics/overview');
  }

  async getDefectsByStatus(): Promise<StatusStatsDTO[]> {
    return this.fetch<StatusStatsDTO[]>('/api/Statistics/defects-by-status');
  }

  async getDefectsByProject(): Promise<ProjectStatsDTO[]> {
    return this.fetch<ProjectStatsDTO[]>('/api/Statistics/defects-by-project');
  }

  async getDefectsByUser(): Promise<UserStatsDTO[]> {
    return this.fetch<UserStatsDTO[]>('/api/Statistics/defects-by-user');
  }

  async getPriorityMetrics(): Promise<PriorityMetricsDTO> {
    return this.fetch<PriorityMetricsDTO>('/api/Statistics/priority-metrics');
  }

  async getDefectsTimeline(days: number = 30): Promise<TimelineStatsDTO[]> {
    return this.fetch<TimelineStatsDTO[]>(`/api/Statistics/defects-timeline?days=${days}`);
  }
}

export const apiService = new ApiService();