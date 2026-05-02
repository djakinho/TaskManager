import { Component, EventEmitter, Input, OnChanges, Output, SimpleChanges } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Task, TaskStatus, UpdateTaskDto } from '../../../shared/models/task.model';
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
  statusOptions: TaskStatus[] = ['Todo', 'InProgress', 'Done'];

  form = new FormGroup({
    title: new FormControl('', [Validators.required, Validators.maxLength(200)]),
    description: new FormControl(''),
    status: new FormControl<TaskStatus>('Todo', [Validators.required]),
    dueDate: new FormControl('', [Validators.required])
  });

  constructor(private taskService: TaskService) {}

  ngOnChanges(changes: SimpleChanges) {
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

  submit() {
    if (this.form.invalid) {
      return;
    }

    const dueDateValue = this.form.value.dueDate as string;
    const selectedDate = new Date(dueDateValue);
    const now = new Date();

    if (selectedDate <= now) {
      this.errorMessage = 'Due date must be in the future.';
      return;
    }

    this.loading = true;
    this.errorMessage = '';

    const dto: UpdateTaskDto = {
      title: this.form.value.title ?? '',
      description: this.form.value.description ?? '',
      status: this.form.value.status ?? 'Todo',
      dueDate: dueDateValue
    };

    const request = this.task ? this.taskService.update(this.task.id, dto) : this.taskService.create(dto);

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

  cancel() {
    this.cancelled.emit();
  }

  private resetForm() {
    this.form.reset({ title: '', description: '', status: 'Todo', dueDate: '' });
    this.errorMessage = '';
    this.loading = false;
  }
}
