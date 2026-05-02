import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, Observable, tap } from 'rxjs';
import { environment } from '../../../environments/environment';
import { CreateTaskDto, Task, UpdateTaskDto } from '../../shared/models/task.model';

@Injectable({ providedIn: 'root' })
export class TaskService {
  private tasksSubject = new BehaviorSubject<Task[]>([]);
  tasks$ = this.tasksSubject.asObservable();

  constructor(private http: HttpClient) {}

  getAll(): Observable<Task[]> {
    return this.http.get<Task[]>(`${environment.apiUrl}/task`).pipe(
      tap(tasks => this.tasksSubject.next(tasks))
    );
  }

  create(dto: CreateTaskDto): Observable<Task> {
    return this.http.post<Task>(`${environment.apiUrl}/task`, dto).pipe(
      tap(() => this.refreshTasks())
    );
  }

  update(id: string, dto: UpdateTaskDto): Observable<Task> {
    return this.http.put<Task>(`${environment.apiUrl}/task/${id}`, dto).pipe(
      tap(() => this.refreshTasks())
    );
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${environment.apiUrl}/task/${id}`).pipe(
      tap(() => this.refreshTasks())
    );
  }

  private refreshTasks(): void {
    this.getAll().subscribe({ error: () => {} });
  }
}
