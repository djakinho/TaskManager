import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';
import { LoginDto, RegisterDto } from '../../../shared/models/user.model';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.css']
})
export class LoginComponent {
  loginForm = new FormGroup({
    email: new FormControl('', [Validators.required, Validators.email]),
    password: new FormControl('', [Validators.required, Validators.minLength(8)])
  });

  registerForm = new FormGroup({
    name: new FormControl('', [Validators.required]),
    email: new FormControl('', [Validators.required, Validators.email]),
    password: new FormControl('', [Validators.required, Validators.minLength(8)])
  });

  isRegistering = false;
  loading = false;
  errorMessage = '';

  constructor(private authService: AuthService, private router: Router) {}

  submitLogin() {
    if (this.loginForm.invalid) {
      return;
    }

    this.loading = true;
    this.errorMessage = '';

    this.authService.login(this.loginForm.value as LoginDto).subscribe({
      next: () => {
        this.loading = false;
        this.router.navigate(['/tasks']);
      },
      error: () => {
        this.loading = false;
        this.errorMessage = 'Invalid email or password.';
      }
    });
  }

  submitRegister() {
    if (this.registerForm.invalid) {
      return;
    }

    this.loading = true;
    this.errorMessage = '';

    this.authService.register(this.registerForm.value as RegisterDto).subscribe({
      next: () => {
        this.loading = false;
        this.isRegistering = false;
      },
      error: () => {
        this.loading = false;
        this.errorMessage = 'Registration failed. Please try again.';
      }
    });
  }

  toggleRegister() {
    this.isRegistering = !this.isRegistering;
    this.errorMessage = '';
  }
}
