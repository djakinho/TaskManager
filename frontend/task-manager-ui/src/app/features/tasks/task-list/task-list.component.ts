import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TaskFormComponent } from '../task-form/task-form.component';
import { AuthService } from '../../../core/services/auth.service';
import { TaskService } from '../../../core/services/task.service';
import { Task, TaskStatus } from '../../../shared/models/task.model';

@Component({
  selector: 'app-task-list',
  standalone: true,
  imports: [CommonModule, TaskFormComponent],
  templateUrl: './task-list.component.html',
  styleUrls: ['./task-list.component.css']
})
export class TaskListComponent implements OnInit {
  tasks: Task[] = [];
  loading = false;
  errorMessage = '';
  selectedTask: Task | null = null;
  showForm = false;
  readonly TaskStatus = TaskStatus;

  constructor(private taskService: TaskService, private authService: AuthService) {}

  ngOnInit() {
    this.loadTasks();
  }

  loadTasks(): void {
    this.loading = true;
    this.errorMessage = '';
    this.taskService.getAll().subscribe({
      next: tasks => {
        this.tasks = tasks;
        this.loading = false;
      },
      error: () => {
        this.loading = false;
        this.tasks = [];
        this.errorMessage = 'Unable to load tasks.';
      }
    });
  }

  openCreate() {
    this.selectedTask = null;
    this.showForm = true;
  }

  openEdit(task: Task) {
    this.selectedTask = task;
    this.showForm = true;
  }

  closeForm() {
    this.showForm = false;
  }

  onSaved() {
    this.closeForm();
    this.loadTasks();
  }

  deleteTask(task: Task) {
    if (!window.confirm('Delete this task?')) {
      return;
    }
    this.taskService.delete(task.id).subscribe({
      next: () => this.loadTasks(),
      error: () => {
        this.errorMessage = 'Unable to delete task.';
      }
    });
  }

  logout() {
    this.authService.logout();
  }
}
