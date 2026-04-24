import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterModule, ActivatedRoute } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';
import { MatCardModule } from '@angular/material/card';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-user-login',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterModule, MatCardModule, MatInputModule, MatButtonModule, MatIconModule],
  templateUrl: './user-login.html',
  styleUrls: ['../login/login.scss']
})
export class UserLogin implements OnInit {
  loginForm: FormGroup;
  hidePassword = true;
  isLoading = false;
  errorMessage = '';
  returnUrl = '/user';

  constructor(
    private fb: FormBuilder, 
    private authService: AuthService, 
    private router: Router,
    private route: ActivatedRoute
  ) {
    this.loginForm = this.fb.group({
      email: ['', [Validators.required, Validators.email]],
      password: ['', [Validators.required, Validators.minLength(6)]]
    });
  }

  ngOnInit() {
    // Get return URL from route parameters or default to '/user'
    this.returnUrl = this.route.snapshot.queryParams['returnUrl'] || '/user';
  }

  onSubmit() {
    if (this.loginForm.invalid) return;

    this.isLoading = true;
    this.errorMessage = '';

    this.authService.userLogin(this.loginForm.value).subscribe({
      next: () => {
        // Navigate to return URL or default to /user
        this.router.navigateByUrl(this.returnUrl);
      },
      error: (err) => {
        this.errorMessage = err.error.message || 'Login failed';
        this.isLoading = false;
      }
    });
  }
}
