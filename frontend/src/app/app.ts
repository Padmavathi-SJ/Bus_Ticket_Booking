import { Component, signal, OnInit } from '@angular/core';
import { RouterOutlet, Router, NavigationEnd, RouterModule } from '@angular/router';
import { CommonModule } from '@angular/common';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatMenuModule } from '@angular/material/menu';
import { AuthService } from './core/services/auth.service';
import { filter } from 'rxjs';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [
    RouterOutlet, 
    CommonModule, 
    RouterModule,
    MatButtonModule, 
    MatIconModule, 
    MatToolbarModule,
    MatTooltipModule,
    MatMenuModule
  ],
  templateUrl: './app.html',
  styleUrl: './app.scss'
})
export class App implements OnInit {
  isLoggedIn: boolean;
  showMainHeader = true;

  constructor(public authService: AuthService, private router: Router) {
    this.isLoggedIn = this.authService.isLoggedIn();
  }

  ngOnInit() {
    this.authService.currentUser$.subscribe(() => {
      // Drive header auth UI from token presence to avoid stale user object issues.
      this.isLoggedIn = this.authService.isLoggedIn();
    });

    // Initial check for header visibility
    this.checkHeaderVisibility(this.router.url);

    // Watch for navigation changes
    this.router.events.pipe(
      filter(event => event instanceof NavigationEnd)
    ).subscribe((event: any) => {
      this.checkHeaderVisibility(event.urlAfterRedirects);
    });
  }

  private checkHeaderVisibility(url: string) {
    // Hide header on all panels - each role has its own specific interface
    // Admin panel: /admin/* (except /admin-login)
    // Operator panel: /operator/*
    // Customer panel: /customer/*
    // User panel (public): /user
    // Only show header on auth pages (login, register)
    const isAuthPage = url.includes('/login') || 
                       url.includes('/register');
    this.showMainHeader = isAuthPage;
  }

  logout() {
    this.authService.logout();
    this.router.navigate(['/']);
  }
}
