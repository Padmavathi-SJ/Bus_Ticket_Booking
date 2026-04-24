import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Subscription, finalize } from 'rxjs';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup } from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatCardModule } from '@angular/material/card';
import { MatChipsModule } from '@angular/material/chips';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatMenuModule } from '@angular/material/menu';
import { MatDividerModule } from '@angular/material/divider';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatNativeDateModule } from '@angular/material/core';
import { Router, RouterModule } from '@angular/router';
import { PublicService, TripSearchResult } from '../../../core/services/public.service';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-user-panel',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatIconModule,
    MatCardModule,
    MatChipsModule,
    MatTooltipModule,
    MatProgressSpinnerModule,
    MatToolbarModule,
    MatMenuModule,
    MatDividerModule,
    MatDatepickerModule,
    MatNativeDateModule,
    RouterModule
  ],
  templateUrl: './user-panel.html',
  styleUrls: ['./user-panel.scss']
})
export class UserPanelComponent implements OnInit, OnDestroy {
  searchForm: FormGroup;
  availableTrips: TripSearchResult[] = [];
  isSearching = false;
  isLoggedIn = false;
  userName = '';
  userEmail = '';
  minDate = new Date(); // Prevent selecting past dates
  private searchSubscription?: Subscription;

  constructor(
    private fb: FormBuilder,
    private publicService: PublicService,
    private authService: AuthService,
    private router: Router
  ) {
    this.searchForm = this.fb.group({
      source: [''],
      destination: [''],
      tripDate: [null]
    });
  }

  ngOnInit(): void {
    // Check if user is logged in and get user details
    this.isLoggedIn = this.authService.isLoggedIn();
    
    if (this.isLoggedIn) {
      const user = this.authService.currentUserValue;
      this.userName = user?.fullName || user?.email || 'User';
      this.userEmail = user?.email || '';
    }

    // Subscribe to auth changes
    this.authService.currentUser$.subscribe(user => {
      this.isLoggedIn = this.authService.isLoggedIn();
      if (user) {
        this.userName = user.fullName || user.email || 'User';
        this.userEmail = user.email || '';
      } else {
        this.userName = '';
        this.userEmail = '';
      }
    });
    
    // Load all active trips by default
    this.loadTrips();
  }

  onSearch() {
    this.loadTrips();
  }

  onClearFilters() {
    this.searchForm.reset({
      source: '',
      destination: '',
      tripDate: null
    });
    this.loadTrips();
  }

  private loadTrips() {
    this.searchSubscription?.unsubscribe();

    const { source, destination, tripDate } = this.searchForm.value;

    console.log('[User Panel] Searching trips:', { source, destination, tripDate });

    // Use setTimeout to avoid ExpressionChangedAfterItHasBeenCheckedError
    setTimeout(() => {
      this.isSearching = true;
    });

    this.searchSubscription = this.publicService
      .searchTrips(source, destination, tripDate)
      .pipe(finalize(() => {
        setTimeout(() => {
          this.isSearching = false;
        });
      }))
      .subscribe({
        next: (data) => {
          console.log('[User Panel] Trips received:', data);
          this.availableTrips = data ?? [];
        },
        error: (err) => {
          console.error('[User Panel] Error loading trips:', err);
          this.availableTrips = [];
        }
      });
  }

  ngOnDestroy(): void {
    this.searchSubscription?.unsubscribe();
  }

  bookNow(tripId: string) {
    if (this.authService.isLoggedIn()) {
      // Navigate to seat selection page with trip ID
      console.log('[User Panel] Booking trip:', tripId);
      this.router.navigate(['/customer/seat-selection', tripId]);
    } else {
      // Redirect to login with return URL to come back to user panel
      this.router.navigate(['/user_login'], { 
        queryParams: { returnUrl: `/user` } 
      });
    }
  }

  navigateToLogin() {
    this.router.navigate(['/user_login']);
  }

  navigateToRegister() {
    this.router.navigate(['/register']);
  }

  navigateToMyBookings() {
    this.router.navigate(['/user/bookings']);
  }

  navigateToProfile() {
    this.router.navigate(['/user/profile']);
  }

  logout() {
    this.authService.logout();
    this.isLoggedIn = false;
    this.userName = '';
    this.userEmail = '';
    // Reload trips to show public view
    this.loadTrips();
  }

  getAmenitiesList(amenities: string): string[] {
    return amenities ? amenities.split(',').map(a => a.trim()) : [];
  }

  getSeatLayoutPreview(totalSeats: number): string {
    const rows = Math.ceil(totalSeats / 4);
    return `${rows} rows × 4 seats`;
  }

  getUserInitials(): string {
    if (!this.userName) return 'U';
    const names = this.userName.split(' ');
    if (names.length >= 2) {
      return (names[0][0] + names[1][0]).toUpperCase();
    }
    return this.userName.substring(0, 2).toUpperCase();
  }

  formatDuration(minutes: number): string {
    const hours = Math.floor(minutes / 60);
    const mins = minutes % 60;
    if (hours > 0 && mins > 0) {
      return `${hours}h ${mins}m`;
    } else if (hours > 0) {
      return `${hours}h`;
    } else {
      return `${mins}m`;
    }
  }
}
