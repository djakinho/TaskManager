import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { tap } from 'rxjs/operators';
import { environment } from '../../../environments/environment';
import { LoginDto, RegisterDto } from '../../shared/models/user.model';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly tokenKey = 'task-manager-token';

  constructor(
    private http: HttpClient,
    private router: Router,
  ) {}

  login(dto: LoginDto) {
    return this.http
      .post<{ token: string }>(`${environment.apiUrl}/auth/login`, dto)
      .pipe(tap((response) => sessionStorage.setItem(this.tokenKey, response.token)));
  }

  register(dto: RegisterDto) {
    return this.http.post(`${environment.apiUrl}/auth/register`, dto);
  }

  logout() {
    sessionStorage.removeItem(this.tokenKey);
    this.router.navigate(['/login']);
  }

  isLoggedIn(): boolean {
    return !!sessionStorage.getItem(this.tokenKey);
  }

  getToken(): string | null {
    return sessionStorage.getItem(this.tokenKey);
  }
}
