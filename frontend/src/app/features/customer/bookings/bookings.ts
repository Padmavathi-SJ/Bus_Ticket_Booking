import { Component, OnInit, OnDestroy, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterModule } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatChipsModule } from '@angular/material/chips';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { Subscription } from 'rxjs';
import { BookingService, Booking } from '../../../core/services/booking.service';

@Component({
  selector: 'app-bookings',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatChipsModule,
    MatToolbarModule,
    MatProgressSpinnerModule,
    MatSnackBarModule,
    MatDialogModule
  ],
  templateUrl: './bookings.html',
  styleUrls: ['./bookings.scss']
})
export class BookingsComponent implements OnInit, OnDestroy {
  bookings: Booking[] = [];
  isLoading = false;
  private subscription?: Subscription;

  constructor(
    private router: Router,
    private bookingService: BookingService,
    private snackBar: MatSnackBar,
    private dialog: MatDialog,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    this.loadBookings();
  }

  loadBookings(): void {
    this.isLoading = true;
    console.log('[Customer Bookings] Loading bookings...');
    this.subscription = this.bookingService.getUserBookings()
      .subscribe({
        next: (data: Booking[]) => {
          console.log('[Customer Bookings] Bookings received:', data);
          console.log('[Customer Bookings] Number of bookings:', data.length);
          this.bookings = data;
          console.log('[Customer Bookings] this.bookings after assignment:', this.bookings);
          console.log('[Customer Bookings] this.bookings.length:', this.bookings.length);
          this.isLoading = false;
          console.log('[Customer Bookings] isLoading set to:', this.isLoading);
          
          // Force change detection
          this.cdr.detectChanges();
          console.log('[Customer Bookings] Change detection triggered');
          console.log('[Customer Bookings] Loading complete');
        },
        error: (err: any) => {
          console.error('[Customer Bookings] Error loading bookings:', err);
          console.error('[Customer Bookings] Error details:', err.error);
          this.isLoading = false;
          this.cdr.detectChanges();
          this.snackBar.open('Failed to load bookings', 'Close', { duration: 3000 });
        }
      });
  }

  goBack(): void {
    this.router.navigate(['/user']);
  }

  cancelBooking(booking: Booking): void {
    if (confirm(`Are you sure you want to cancel booking for ${booking.busName}?`)) {
      this.bookingService.cancelBooking(booking.id).subscribe({
        next: () => {
          this.snackBar.open('Booking cancelled successfully', 'Close', { duration: 3000 });
          this.loadBookings();
        },
        error: (err: any) => {
          this.snackBar.open('Failed to cancel booking', 'Close', { duration: 3000 });
        }
      });
    }
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

  ngOnDestroy(): void {
    this.subscription?.unsubscribe();
  }
}
