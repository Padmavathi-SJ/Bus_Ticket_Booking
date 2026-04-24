import { Component, OnInit, OnDestroy, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatTableModule } from '@angular/material/table';
import { MatIconModule } from '@angular/material/icon';
import { MatTabsModule } from '@angular/material/tabs';
import { MatChipsModule } from '@angular/material/chips';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatButtonModule } from '@angular/material/button';
import { MatTooltipModule } from '@angular/material/tooltip';
import { Subscription, finalize } from 'rxjs';
import { OperatorService, OperatorBooking } from '../../../core/services/operator.service';

@Component({
  selector: 'app-operator-bookings',
  standalone: true,
  imports: [
    CommonModule,
    MatCardModule,
    MatTableModule,
    MatIconModule,
    MatTabsModule,
    MatChipsModule,
    MatProgressSpinnerModule,
    MatButtonModule,
    MatTooltipModule
  ],
  templateUrl: './bookings.html',
  styleUrls: ['./bookings.scss']
})
export class OperatorBookingManagement implements OnInit, OnDestroy {
  bookings: OperatorBooking[] = [];
  filteredBookings: OperatorBooking[] = [];
  isLoading = false;
  selectedTab = 0;
  private subscription?: Subscription;

  displayedColumns: string[] = [
    'bookingId',
    'busName',
    'passengerName',
    'passengerEmail',
    'passengerPhone',
    'journeyDate',
    'route',
    'seats',
    'amount',
    'status'
  ];

  constructor(
    private operatorService: OperatorService,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    this.loadBookings();
  }

  loadBookings(): void {
    this.isLoading = true;
    this.subscription = this.operatorService.getMyBookings()
      .pipe(finalize(() => {
        this.isLoading = false;
        this.cdr.detectChanges();
      }))
      .subscribe({
        next: (data: OperatorBooking[]) => {
          console.log('[Operator Bookings] Data received:', data);
          this.bookings = data;
          this.filterBookings();
          this.cdr.detectChanges();
          console.log('[Operator Bookings] Bookings set, isLoading:', this.isLoading);
        },
        error: (err: any) => {
          console.error('[Operator Bookings] Failed to load bookings', err);
          this.bookings = [];
          this.filteredBookings = [];
          this.cdr.detectChanges();
        }
      });
  }

  onTabChange(index: number): void {
    this.selectedTab = index;
    this.filterBookings();
  }

  filterBookings(): void {
    switch (this.selectedTab) {
      case 0: // All
        this.filteredBookings = this.bookings;
        break;
      case 1: // Confirmed
        this.filteredBookings = this.bookings.filter(b => b.status === 'Confirmed');
        break;
      case 2: // Cancelled
        this.filteredBookings = this.bookings.filter(b => b.status === 'Cancelled');
        break;
      default:
        this.filteredBookings = this.bookings;
    }
    console.log('[Operator Bookings] Filtered bookings:', this.filteredBookings.length);
    this.cdr.detectChanges();
  }

  getConfirmedCount(): number {
    return this.bookings.filter(b => b.status === 'Confirmed').length;
  }

  getCancelledCount(): number {
    return this.bookings.filter(b => b.status === 'Cancelled').length;
  }

  getTotalRevenue(): number {
    return this.bookings
      .filter(b => b.status === 'Confirmed')
      .reduce((sum, b) => sum + b.totalAmount, 0);
  }

  getTotalSeats(): number {
    return this.bookings
      .filter(b => b.status === 'Confirmed')
      .reduce((sum, b) => sum + b.seatNumbers.length, 0);
  }

  getStatusColor(status: string): string {
    switch (status) {
      case 'Confirmed':
        return 'primary';
      case 'Cancelled':
        return 'warn';
      case 'Completed':
        return 'accent';
      default:
        return '';
    }
  }

  formatBookingId(id: string): string {
    return `BK${id.substring(0, 8).toUpperCase()}`;
  }

  ngOnDestroy(): void {
    this.subscription?.unsubscribe();
  }
}
