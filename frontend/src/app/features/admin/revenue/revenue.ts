import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatTableModule } from '@angular/material/table';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatNativeDateModule } from '@angular/material/core';
import { MatButtonModule } from '@angular/material/button';
import { FormsModule } from '@angular/forms';
import { environment } from '../../../../environments/environment';

interface BusRevenue {
  busId: string;
  busName: string;
  busNumber: string;
  busType: string;
  operatorName: string | null;
  totalBookings: number;
  confirmedBookings: number;
  cancelledBookings: number;
  totalRevenue: number;
  confirmedRevenue: number;
}

interface DailyRevenue {
  date: string;
  revenue: number;
  bookings: number;
}

interface RevenueData {
  totalRevenue: number;
  busRevenues: BusRevenue[];
  dailyRevenues: DailyRevenue[];
}

@Component({
  selector: 'app-revenue',
  standalone: true,
  imports: [
    CommonModule,
    MatCardModule,
    MatIconModule,
    MatProgressSpinnerModule,
    MatTableModule,
    MatDatepickerModule,
    MatFormFieldModule,
    MatInputModule,
    MatNativeDateModule,
    MatButtonModule,
    FormsModule
  ],
  templateUrl: './revenue.html',
  styleUrl: './revenue.scss',
})
export class Revenue implements OnInit {
  revenueData: RevenueData | null = null;
  isLoading = true;
  startDate: Date | null = null;
  endDate: Date | null = null;

  displayedColumns: string[] = [
    'busName',
    'busNumber',
    'busType',
    'operatorName',
    'totalBookings',
    'confirmedBookings',
    'cancelledBookings',
    'confirmedRevenue'
  ];

  constructor(private http: HttpClient, private cdr: ChangeDetectorRef) {
    // Set default date range to last 30 days
    this.endDate = new Date();
    this.startDate = new Date();
    this.startDate.setDate(this.startDate.getDate() - 30);
  }

  ngOnInit(): void {
    this.loadRevenueData();
  }

  loadRevenueData(): void {
    this.isLoading = true;
    
    let url = `${environment.apiUrl}/admin/revenue`;
    const params: string[] = [];
    
    if (this.startDate) {
      params.push(`startDate=${this.startDate.toISOString()}`);
    }
    if (this.endDate) {
      params.push(`endDate=${this.endDate.toISOString()}`);
    }
    
    if (params.length > 0) {
      url += '?' + params.join('&');
    }

    this.http.get<RevenueData>(url).subscribe({
      next: (data) => {
        console.log('[Revenue] Data received:', data);
        this.revenueData = data;
        this.isLoading = false;
        this.cdr.detectChanges();
      },
      error: (err) => {
        console.error('[Revenue] Error loading data:', err);
        this.isLoading = false;
        this.cdr.detectChanges();
      }
    });
  }

  onDateRangeChange(): void {
    this.loadRevenueData();
  }

  formatRevenue(value: number): string {
    if (value >= 100000) {
      return `₹${(value / 100000).toFixed(2)}L`;
    } else if (value >= 1000) {
      return `₹${(value / 1000).toFixed(2)}K`;
    }
    return `₹${value.toFixed(2)}`;
  }

  formatDate(dateString: string): string {
    const date = new Date(dateString);
    return date.toLocaleDateString('en-IN', { 
      day: '2-digit', 
      month: 'short', 
      year: 'numeric' 
    });
  }

  getMaxRevenue(): number {
    if (!this.revenueData?.dailyRevenues.length) return 0;
    return Math.max(...this.revenueData.dailyRevenues.map(d => d.revenue));
  }

  getBarHeight(revenue: number): string {
    const max = this.getMaxRevenue();
    if (max === 0) return '0%';
    return `${(revenue / max) * 100}%`;
  }
}
