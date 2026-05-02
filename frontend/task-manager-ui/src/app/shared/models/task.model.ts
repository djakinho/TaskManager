export type TaskStatus = 'Todo' | 'InProgress' | 'Done';

export interface Task {
  id: string;
  title: string;
  description: string;
  status: TaskStatus;
  dueDate: string;
  userId: string;
  createdAt: string;
}

export interface CreateTaskDto {
  title: string;
  description: string;
  status: TaskStatus;
  dueDate: string;
}

export interface UpdateTaskDto {
  title: string;
  description: string;
  status: TaskStatus;
  dueDate: string;
}
