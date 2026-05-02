import { Component, EventEmitter, Input, OnChanges, Output, SimpleChanges } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Task, TaskStatus, CreateTaskDto, UpdateTaskDto } from '../../../shared/models/task.model';
import { TaskService } from '../../../core/services/task.service';

@Component({
  selector: 'app-task-form',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './task-form.component.html',
  styleUrls: ['./task-form.component.css']
})
export class TaskFormComponent implements OnChanges {
  @Input() task: Task | null = null;
  @Input() visible = false;
  @Output() saved = new EventEmitter<void>();
  @Output() cancelled = new EventEmitter<void>();

  loading = false;
  errorMessage = '';
  statusOptions = [
    { label: 'Todo', value: TaskStatus.Todo },
    { label: 'InProgress', value: TaskStatus.InProgress },
    { label: 'Done', value: TaskStatus.Done }
  ] as const;
  readonly TaskStatus = TaskStatus;

  form = new FormGroup({
    title: new FormControl('', [Validators.required, Validators.maxLength(200)]),
    description: new FormControl(''),
    status: new FormControl<number>(TaskStatus.Todo, [Validators.required]),
    dueDate: new FormControl('', [Validators.required])
  });

  constructor(private taskService: TaskService) {}

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['task'] && this.task) {
      this.form.setValue({
        title: this.task.title,
        description: this.task.description,
        status: this.task.status,
        dueDate: this.task.dueDate.split('T')[0]
      });
      this.errorMessage = '';
    }

    if (changes['visible'] && !this.visible) {
      this.resetForm();
    }
  }

  submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const dueDateValue = this.form.value.dueDate as string;
    const dueDate = new Date(dueDateValue);
    const today = new Date();
    today.setHours(0, 0, 0, 0);

    if (dueDate <= today) {
      this.errorMessage = 'Due date must be in the future.';
      return;
    }

    this.loading = true;
    this.errorMessage = '';

    const dto: CreateTaskDto | UpdateTaskDto = {
      title: this.form.value.title ?? '',
      description: this.form.value.description ?? '',
      status: Number(this.form.value.status) as TaskStatus,
      dueDate: dueDateValue
    };

    const request = this.task
      ? this.taskService.update(this.task.id, dto as UpdateTaskDto)
      : this.taskService.create(dto as CreateTaskDto);

    request.subscribe({
      next: () => {
        this.loading = false;
        this.saved.emit();
      },
      error: () => {
        this.loading = false;
        this.errorMessage = 'Unable to save task.';
      }
    });
  }

  cancel(): void {
    this.cancelled.emit();
  }

  private resetForm(): void {
    this.form.reset({ title: '', description: '', status: TaskStatus.Todo, dueDate: '' });
    this.errorMessage = '';
    this.loading = false;
  }
}
