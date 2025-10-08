export interface LoginDTO {
  login: string;
  password: string;
}

export interface RegisterDTO {
  login: string;
  password: string;
  fio: string;
  roleId: number;
}

export interface UserDTO {
  userId: number;
  roleId: number;
  login: string;
  fio: string;
  roleName: string;
}

export interface AuthResponseDTO {
  token: string;
  user: UserDTO;
  expires: string;
}

export interface InfoDTO {
  infoId: number;
  defectName: string;
  defectDescription: string;
  priority: number;
  dueDate: string;
  statusChangeDate: string;
}

export interface DefectDTO {
  defectId: number;
  projectId: number;
  statusId: number;
  infoId: number;
  responsibleId: number | null;
  createdById: number;
  createdDate: string;
  updatedDate: string;
  projectName: string;
  statusName: string;
  responsibleFio: string | null;
  createdByFio: string;
  info: InfoDTO;
}

export interface DefectCreateDTO {
  projectId: number;
  statusId: number;
  responsibleId?: number | null;
  createdById: number;
  info: {
    defectName: string;
    defectDescription: string;
    priority: number;
    dueDate: string;
  };
}

export interface DefectUpdateDTO {
  projectId: number;
  statusId: number;
  responsibleId?: number | null;
  info: {
    defectName: string;
    defectDescription: string;
    priority: number;
    dueDate: string;
  };
}

export interface ProjectDTO {
  projectId: number;
  projectStatusId: number;
  name: string;
  description: string;
  createdDate: string;
  updatedDate: string;
  projectStatusName: string;
}

export interface ProjectCreateDTO {
  projectStatusId: number;
  name: string;
  description: string;
}

export interface ProjectUpdateDTO {
  projectStatusId: number;
  name: string;
  description: string;
}

export interface DefectStatusDTO {
  id: number;
  statusName: string;
  statusDescription: string;
}

export interface ProjectStatusDTO {
  projectStatusId: number;
  projectStatusName: string;
  projectStatusDescription: string;
}

export interface RoleDTO {
  roleId: number;
  roleName: string;
}

export interface CommentDTO {
  commentId: number;
  defectId: number;
  userId: number;
  commentText: string;
  createdDate: string;
  isDeleted: boolean;
  userFio: string;
}

export interface CommentCreateDTO {
  defectId: number;
  userId: number;
  commentText: string;
}

export interface DefectHistoryDTO {
  historyId: number;
  defectId: number;
  userId: number;
  fieldName: string;
  oldValue: string;
  newValue: string;
  changeDate: string;
  userFio: string;
}

export interface OverviewStatsDTO {
  totalDefects: number;
  defectsByStatus: Record<string, number>;
  defectsByPriority: Record<string, number>;
  recentDefects: DefectStatsDTO[];
}

export interface DefectStatsDTO {
  defectId: number;
  defectName: string;
  projectName: string;
  statusName: string;
  responsibleFio: string;
  createdDate: string;
  priority: number;
}

export interface StatusStatsDTO {
  statusId: number;
  statusName: string;
  count: number;
  percentage: number;
}

export interface ProjectStatsDTO {
  projectId: number;
  projectName: string;
  totalDefects: number;
  openDefects: number;
  closedDefects: number;
  highPriorityDefects: number;
}

export interface UserStatsDTO {
  userId: number;
  userFio: string;
  roleName: string;
  totalAssignedDefects: number;
  openDefects: number;
  closedDefects: number;
  overdueDefects: number;
  averageCompletionDays: number | null;
}

export interface PriorityMetricsDTO {
  totalDefects: number;
  averagePriority: number;
  highPriorityCount: number;
  mediumPriorityCount: number;
  lowPriorityCount: number;
  overdueHighPriority: number;
}

export interface TimelineStatsDTO {
  date: string;
  createdCount: number;
  closedCount: number;
}


export interface UserCreateDTO {
  roleId: number;
  login: string;
  fio: string;
  password: string; // Изменяем hash на password для удобства
}

export interface UserUpdateDTO {
  roleId: number;
  login: string;
  fio: string;
  password?: string; // Делаем пароль опциональным
}