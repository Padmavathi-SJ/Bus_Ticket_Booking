import { Component } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';
import { MatCardModule } from '@angular/material/card';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatCardModule,
    MatInputModule,
    MatButtonModule,
    MatIconModule
  ],
  templateUrl: './login.html',
  styleUrls: ['./login.scss']
})
export class Login {
  loginForm: FormGroup;
  hidePassword = true;
  isLoading = false;
  errorMessage = '';

  constructor(
    private fb: FormBuilder,
    private authService: AuthService,
    private router: Router
  ) {
    this.loginForm = this.fb.group({
      email: ['', [Validators.required, Validators.email]],
      password: ['', [Validators.required, Validators.minLength(6)]]
    });
  }

  onSubmit() {
    if (this.loginForm.invalid) return;

    this.isLoading = true;
    this.errorMessage = '';

    // Simulate backend call for now since C# Auth API is not ready yet
    setTimeout(() => {
      this.isLoading = false;
      console.log('Login Form Data:', this.loginForm.value);
      // We will uncomment this when C# backend is ready:
      /*
      this.authService.login(this.loginForm.value).subscribe({
        next: (res) => {
          if (res.user.role === 1) this.router.navigate(['/admin']);
          else if (res.user.role === 2) this.router.navigate(['/operator']);
          else this.router.navigate(['/']);
        },
        error: (err) => {
          this.errorMessage = err.error.message || 'Login failed';
          this.isLoading = false;
        }
      });
      */
    }, 1000);
  }
}
