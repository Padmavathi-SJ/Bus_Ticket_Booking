import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router } from '@angular/router';
import { MatSidenavModule } from '@angular/material/sidenav';
import { MatListModule } from '@angular/material/list';
import { MatIconModule } from '@angular/material/icon';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatButtonModule } from '@angular/material/button';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-operator-layout',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    MatSidenavModule,
    MatListModule,
    MatIconModule,
    MatToolbarModule,
    MatButtonModule
  ],
  templateUrl: './operator-layout.html',
  styleUrls: ['./operator-layout.scss']
})
export class OperatorLayout {
  menuItems = [
    { icon: 'dashboard', label: 'Dashboard', route: '/operator/dashboard' },
    { icon: 'directions_bus', label: 'My Buses', route: '/operator/buses' },
    { icon: 'map', label: 'Available Routes', route: '/operator/routes' },
    { icon: 'book', label: 'Bookings', route: '/operator/bookings' }
  ];

  constructor(private authService: AuthService, private router: Router) {}

  onLogout() {
    this.authService.logout();
    this.router.navigate(['/operator_login']);
  }
}
