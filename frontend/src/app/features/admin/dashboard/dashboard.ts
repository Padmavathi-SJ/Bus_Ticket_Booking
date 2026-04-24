import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { environment } from '../../../../environments/environment';

interface DashboardStats {
  totalOperators: number;
  pendingOperators: number;
  totalBuses: number;
  pendingBuses: number;
  activeBuses: number;
  totalRoutes: number;
  totalBookings: number;
  todayBookings: number;
  totalRevenue: number;
  todayRevenue: number;
}

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule, MatCardModule, MatIconModule, MatProgressSpinnerModule],
  templateUrl: './dashboard.html',
  styleUrl: './dashboard.scss',
})
export class Dashboard implements OnInit {
  stats: DashboardStats | null = null;
  isLoading = true;

  statCards = [
    { key: 'totalOperators', label: 'Total Operators', icon: 'people', color: '#667eea', subKey: 'pendingOperators', subLabel: 'Pending' },
    { key: 'activeBuses', label: 'Active Buses', icon: 'directions_bus', color: '#10b981', subKey: 'totalBuses', subLabel: 'Total' },
    { key: 'totalRoutes', label: 'Active Routes', icon: 'map', color: '#f59e0b', subKey: null, subLabel: null },
    { key: 'todayBookings', label: 'Today Bookings', icon: 'confirmation_number', color: '#ef4444', subKey: 'totalBookings', subLabel: 'Total' },
    { key: 'todayRevenue', label: 'Today Revenue', icon: 'currency_rupee', color: '#8b5cf6', subKey: 'totalRevenue', subLabel: 'Total', isRevenue: true },
    { key: 'pendingBuses', label: 'Pending Buses', icon: 'pending', color: '#f97316', subKey: null, subLabel: null }
  ];

  constructor(private http: HttpClient, private cdr: ChangeDetectorRef) {}

  ngOnInit(): void {
    this.loadDashboardStats();
  }

  loadDashboardStats(): void {
    this.isLoading = true;
    this.http.get<DashboardStats>(`${environment.apiUrl}/admin/dashboard/stats`)
      .subscribe({
        next: (data) => {
          console.log('[Admin Dashboard] Raw data received:', data);
          this.stats = data;
          this.isLoading = false;
          this.cdr.detectChanges();
          console.log('[Admin Dashboard] Stats loaded:', this.stats);
          console.log('[Admin Dashboard] isLoading:', this.isLoading);
        },
        error: (err) => {
          console.error('[Admin Dashboard] Error loading stats:', err);
          this.isLoading = false;
          this.cdr.detectChanges();
        }
      });
  }

  getStatValue(key: string): string {
    if (!this.stats) return '0';
    const value = (this.stats as any)[key];
    return value?.toString() || '0';
  }

  getSubStatValue(key: string | null): string {
    if (!key || !this.stats) return '';
    const value = (this.stats as any)[key];
    return value?.toString() || '0';
  }

  formatRevenue(value: string): string {
    const num = parseFloat(value);
    if (num >= 100000) {
      return `₹${(num / 100000).toFixed(2)}L`;
    } else if (num >= 1000) {
      return `₹${(num / 1000).toFixed(2)}K`;
    }
    return `₹${num.toFixed(2)}`;
  }
}
