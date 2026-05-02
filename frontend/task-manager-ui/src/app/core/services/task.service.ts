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
    return this.http.get<Task[]>(`${environment.apiUrl}/task`);
  }

  create(dto: CreateTaskDto): Observable<Task> {
    return this.http.post<Task>(`${environment.apiUrl}/task`, dto);
  }

  update(id: string, dto: UpdateTaskDto): Observable<Task> {
    return this.http.put<Task>(`${environment.apiUrl}/task/${id}`, dto);
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${environment.apiUrl}/task/${id}`);
  }
}
