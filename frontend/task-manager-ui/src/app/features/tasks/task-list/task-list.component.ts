import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import {
  catchError,
  finalize,
  map,
  Observable,
  of,
  startWith,
  Subject,
  switchMap,
  tap,
} from 'rxjs';
import { TaskFormComponent } from '../task-form/task-form.component';
import { AuthService } from '../../../core/services/auth.service';
import { TaskService } from '../../../core/services/task.service';
import { Task, TaskStatus } from '../../../shared/models/task.model';

interface TaskListView {
  tasks: Task[];
  loading: boolean;
  errorMessage: string;
}

@Component({
  selector: 'app-task-list',
  standalone: true,
  imports: [CommonModule, TaskFormComponent],
  templateUrl: './task-list.component.html',
  styleUrls: ['./task-list.component.css'],
})
export class TaskListComponent implements OnInit {
  viewState$!: Observable<TaskListView>;
  selectedTask: Task | null = null;
  showForm = false;
  readonly TaskStatus = TaskStatus;

  private reloadTasks$ = new Subject<void>();

  constructor(
    private taskService: TaskService,
    private authService: AuthService,
  ) {}

  ngOnInit() {
    this.viewState$ = this.reloadTasks$.pipe(
      startWith(void 0),
      switchMap(() =>
        this.taskService.getAll().pipe(
          map((tasks) => ({
            tasks,
            loading: false,
            errorMessage: '',
          })),
          catchError(() => {
            return of({
              tasks: [],
              loading: false,
              errorMessage: 'Unable to load tasks.',
            });
          }),
          startWith({
            tasks: [],
            loading: true,
            errorMessage: '',
          }),
        ),
      ),
    );
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
    Promise.resolve().then(() => {
      this.showForm = false;
    });
  }

  onSaved() {
    Promise.resolve().then(() => {
      this.showForm = false;
      this.reloadTasks$.next();
    });
  }

  deleteTask(task: Task) {
    if (!window.confirm('Delete this task?')) {
      return;
    }

    this.taskService.delete(task.id).subscribe({
      next: () => this.reloadTasks$.next(),
      error: () => {
        // optional: to emit an error state in the UI
      },
    });
  }

  logout() {
    this.authService.logout();
  }
}
