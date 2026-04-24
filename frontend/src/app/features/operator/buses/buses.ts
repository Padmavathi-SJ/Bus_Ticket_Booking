import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatTableModule } from '@angular/material/table';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatTabsModule } from '@angular/material/tabs';
import { MatChipsModule } from '@angular/material/chips';
import { MatDialogModule, MatDialog } from '@angular/material/dialog';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatNativeDateModule } from '@angular/material/core';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatExpansionModule } from '@angular/material/expansion';
import { RouterModule } from '@angular/router';
import { OperatorService } from '../../../core/services/operator.service';

@Component({
  selector: 'app-operator-buses',
  standalone: true,
  imports: [
    CommonModule, 
    ReactiveFormsModule,
    MatCardModule, 
    MatTableModule, 
    MatIconModule, 
    MatButtonModule, 
    MatTabsModule,
    MatChipsModule,
    MatDialogModule,
    MatDatepickerModule,
    MatNativeDateModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatSnackBarModule,
    MatExpansionModule,
    RouterModule
  ],
  templateUrl: './buses.html',
  styleUrls: ['./buses.scss']
})
export class OperatorBusManagement implements OnInit {
  allBuses: any[] = [];
  activeBuses: any[] = [];
  isLoading = false;
  expandedBusId: string | null = null;
  showScheduleDialog: boolean = false;
  showTripsDialog: boolean = false;
  showBookingsDialog: boolean = false;
  selectedBus: any = null;
  tripForm!: FormGroup;
  minDate = new Date();
  
  // Trip viewing
  busTrips: any[] = [];
  selectedDate: Date = new Date();
  selectedTrip: any = null;
  isLoadingTrips = false;
  
  // Bookings viewing
  busBookings: any[] = [];
  selectedBookingDate: Date = new Date();
  bookingsForSelectedDate: any[] = [];
  isLoadingBookings = false;
  tripDatesWithBookings: Date[] = [];

  constructor(
    private operatorService: OperatorService, 
    private cdr: ChangeDetectorRef,
    private fb: FormBuilder,
    private snackBar: MatSnackBar,
    private dialog: MatDialog
  ) {
    this.initTripForm();
  }

  ngOnInit(): void {
    this.loadBuses();
  }

  initTripForm(): void {
    this.tripForm = this.fb.group({
      tripDate: ['', Validators.required],
      sourceAddress: ['', [Validators.required, Validators.minLength(3)]],
      destinationAddress: ['', [Validators.required, Validators.minLength(3)]],
      pickupPoint: ['', [Validators.required, Validators.minLength(3)]],
      dropPoint: ['', [Validators.required, Validators.minLength(3)]],
      departureTime: ['', Validators.required],
      arrivalTime: ['', Validators.required],
      basePrice: ['', [Validators.required, Validators.min(1)]]
    });
  }

  loadBuses() {
    this.isLoading = true;
    this.operatorService.getMyBuses().subscribe({
      next: (data: any[]) => {
        console.log('Operator Buses Data:', data);
        this.allBuses = data;
        // Filter active buses (Status 2 = Approved) - check both camelCase and PascalCase
        this.activeBuses = data.filter(b => (b.status === 2 || b.Status === 2));
        this.isLoading = false;
        this.cdr.detectChanges();
      },
      error: () => {
        this.isLoading = false;
        this.cdr.detectChanges();
      }
    });
  }

  getStatusLabel(status: number): string {
    switch (status) {
      case 1: return 'Pending';
      case 2: return 'Approved';
      case 3: return 'Disabled';
      case 4: return 'Rejected';
      default: return 'Unknown';
    }
  }

  toggleBusDetails(busId: string) {
    this.expandedBusId = this.expandedBusId === busId ? null : busId;
  }

  generateSeatLayout(totalSeats: number, femaleSeats: number, maleSeats: number): any[] {
    const seats = [];
    const rows = Math.ceil(totalSeats / 4);
    
    // If no specific allocation, use default distribution
    // 30% female, 40% male, 30% general
    if (femaleSeats === 0 && maleSeats === 0) {
      femaleSeats = Math.floor(totalSeats * 0.3);
      maleSeats = Math.floor(totalSeats * 0.4);
    }
    
    let seatNumber = 1;
    let femaleCount = 0;
    let maleCount = 0;
    
    for (let row = 0; row < rows; row++) {
      const rowSeats = [];
      
      for (let col = 0; col < 4; col++) {
        if (seatNumber <= totalSeats) {
          let seatType = 'general';
          
          // Assign female seats first (typically front rows)
          if (femaleCount < femaleSeats) {
            seatType = 'female';
            femaleCount++;
          } 
          // Then assign male seats
          else if (maleCount < maleSeats) {
            seatType = 'male';
            maleCount++;
          }
          
          rowSeats.push({
            number: seatNumber,
            type: seatType,
            position: col === 0 || col === 3 ? 'window' : 'aisle'
          });
          seatNumber++;
        }
      }
      
      seats.push(rowSeats);
    }
    
    return seats;
  }

  openScheduleDialog(bus: any): void {
    this.selectedBus = bus;
    this.showScheduleDialog = true;
    this.tripForm.reset();
  }

  closeScheduleDialog(): void {
    this.showScheduleDialog = false;
    this.selectedBus = null;
    this.tripForm.reset();
  }

  scheduleTrip(): void {
    if (this.tripForm.invalid) {
      this.snackBar.open('Please fill all required fields correctly', 'Close', { duration: 3000 });
      return;
    }

    if (!this.selectedBus) {
      this.snackBar.open('Bus information is missing. Please try again.', 'Close', { duration: 3000 });
      return;
    }

    const formValue = this.tripForm.value;
    
    // Combine date and time
    const tripDate = new Date(formValue.tripDate);
    const [depHours, depMinutes] = formValue.departureTime.split(':');
    const [arrHours, arrMinutes] = formValue.arrivalTime.split(':');
    
    const departureDateTime = new Date(tripDate);
    departureDateTime.setHours(parseInt(depHours), parseInt(depMinutes));
    
    const arrivalDateTime = new Date(tripDate);
    arrivalDateTime.setHours(parseInt(arrHours), parseInt(arrMinutes));

    const tripData = {
      busId: this.selectedBus.id,
      routeId: this.selectedBus.routeId || this.selectedBus.RouteId,
      sourceAddress: formValue.sourceAddress,
      destinationAddress: formValue.destinationAddress,
      pickupPoint: formValue.pickupPoint,
      dropPoint: formValue.dropPoint,
      departureDateTime: departureDateTime.toISOString(),
      arrivalDateTime: arrivalDateTime.toISOString(),
      basePrice: formValue.basePrice
    };

    console.log('[Schedule Trip] Selected Bus:', this.selectedBus);
    console.log('[Schedule Trip] Submitting trip data:', tripData);
    
    this.operatorService.scheduleTrip(tripData).subscribe({
      next: (response) => {
        console.log('[Schedule Trip] Success:', response);
        this.snackBar.open('Trip scheduled successfully!', 'Close', { duration: 3000 });
        this.closeScheduleDialog();
        // Reload trips if the trips dialog is open
        if (this.showTripsDialog) {
          this.loadBusTrips();
        }
      },
      error: (err) => {
        console.error('[Schedule Trip] Error:', err);
        this.snackBar.open('Failed to schedule trip: ' + (err.error?.message || err.message), 'Close', { duration: 5000 });
      }
    });
  }

  openTripsDialog(bus: any): void {
    this.selectedBus = bus;
    this.showTripsDialog = true;
    this.selectedDate = new Date();
    this.loadBusTrips();
  }

  closeTripsDialog(): void {
    this.showTripsDialog = false;
    this.selectedBus = null;
    this.busTrips = [];
    this.selectedTrip = null;
  }

  loadBusTrips(): void {
    if (!this.selectedBus) return;
    
    this.isLoadingTrips = true;
    console.log('[Trips Dialog] Loading trips for bus:', this.selectedBus.busName);
    console.log('[Trips Dialog] Current selected date:', this.selectedDate.toDateString());
    
    this.operatorService.getBusTrips(this.selectedBus.id).subscribe({
      next: (trips) => {
        console.log('[Trips Dialog] Trips loaded from API:', trips);
        this.busTrips = trips;
        this.busTrips.forEach(trip => {
          console.log(`[Trips Dialog] Trip ${trip.id}: ${new Date(trip.tripDate).toDateString()} - ${trip.sourceAddress} to ${trip.destinationAddress}`);
        });
        this.filterTripsByDate();
        this.isLoadingTrips = false;
        this.cdr.detectChanges();
      },
      error: (err) => {
        console.error('[Trips Dialog] Error loading trips:', err);
        this.isLoadingTrips = false;
        this.snackBar.open('Failed to load trips: ' + (err.error?.message || err.message), 'Close', { duration: 5000 });
        this.cdr.detectChanges();
      }
    });
  }

  onDateChange(date: Date | null): void {
    console.log('[Trips Dialog] Date changed:', date);
    if (date) {
      this.selectedDate = date;
      console.log('[Trips Dialog] Selected date set to:', this.selectedDate.toDateString());
      console.log('[Trips Dialog] Total trips loaded:', this.busTrips.length);
      console.log('[Trips Dialog] Trips for selected date:', this.getTripsForDate(this.selectedDate).length);
      this.filterTripsByDate();
      this.cdr.detectChanges();
    }
  }

  filterTripsByDate(): void {
    const selectedDateStr = this.selectedDate.toDateString();
    console.log('[Trips Dialog] Filtering trips for date:', selectedDateStr);
    
    const tripsOnDate = this.busTrips.filter(trip => {
      const tripDateStr = new Date(trip.tripDate).toDateString();
      console.log(`[Trips Dialog] Comparing trip date ${tripDateStr} with selected ${selectedDateStr}`);
      return tripDateStr === selectedDateStr;
    });
    
    console.log('[Trips Dialog] Trips found for date:', tripsOnDate.length);
    
    if (tripsOnDate.length > 0) {
      this.selectedTrip = tripsOnDate[0];
      console.log('[Trips Dialog] Selected trip:', this.selectedTrip);
    } else {
      this.selectedTrip = null;
      console.log('[Trips Dialog] No trips found for this date');
    }
  }

  getTripsForDate(date: Date): any[] {
    if (!date) {
      console.log('[Trips Dialog] getTripsForDate called with null/undefined date');
      return [];
    }
    const dateStr = date.toDateString();
    const trips = this.busTrips.filter(trip => 
      new Date(trip.tripDate).toDateString() === dateStr
    );
    console.log(`[Trips Dialog] getTripsForDate(${dateStr}): Found ${trips.length} trips`);
    return trips;
  }

  hasTripsOnDate(date: Date): boolean {
    return this.getTripsForDate(date).length > 0;
  }

  selectTrip(trip: any): void {
    this.selectedTrip = trip;
  }

  getOccupancyPercentage(trip: any): number {
    const total = trip.bookedSeats + trip.availableSeats;
    return (trip.bookedSeats / total) * 100;
  }

  getOccupancyColor(percentage: number): string {
    if (percentage >= 80) return '#ef4444'; // Red
    if (percentage >= 50) return '#f59e0b'; // Orange
    return '#10b981'; // Green
  }

  dateClass = (date: Date): string => {
    const hasTrips = this.hasTripsOnDate(date);
    const isSelected = this.selectedDate && date.toDateString() === this.selectedDate.toDateString();
    
    if (isSelected) return 'selected-date';
    if (hasTrips) return 'has-trips-date';
    return '';
  }

  // Bookings Dialog Methods
  openBookingsDialog(bus: any): void {
    this.selectedBus = bus;
    this.showBookingsDialog = true;
    this.selectedBookingDate = new Date();
    this.loadBusBookings();
  }

  closeBookingsDialog(): void {
    this.showBookingsDialog = false;
    this.selectedBus = null;
    this.busBookings = [];
    this.bookingsForSelectedDate = [];
    this.tripDatesWithBookings = [];
  }

  loadBusBookings(): void {
    if (!this.selectedBus) return;
    
    this.isLoadingBookings = true;
    console.log('[Bookings Dialog] Loading bookings for bus:', this.selectedBus.busName);
    
    // First load all trips for this bus
    this.operatorService.getBusTrips(this.selectedBus.id).subscribe({
      next: (trips) => {
        console.log('[Bookings Dialog] Trips loaded:', trips);
        
        // Filter only future/active trips
        const today = new Date();
        today.setHours(0, 0, 0, 0);
        
        const activeTrips = trips.filter((trip: any) => {
          const tripDate = new Date(trip.tripDate);
          tripDate.setHours(0, 0, 0, 0);
          return tripDate >= today;
        });
        
        console.log('[Bookings Dialog] Active trips:', activeTrips);
        
        // Get dates with bookings (trips that have bookedSeats > 0)
        this.tripDatesWithBookings = activeTrips
          .filter((trip: any) => trip.bookedSeats > 0)
          .map((trip: any) => new Date(trip.tripDate));
        
        console.log('[Bookings Dialog] Dates with bookings:', this.tripDatesWithBookings);
        
        // Now load actual bookings from operator service
        this.operatorService.getMyBookings().subscribe({
          next: (allBookings) => {
            console.log('[Bookings Dialog] All operator bookings:', allBookings);
            
            // Filter bookings for this specific bus
            this.busBookings = allBookings.filter((booking: any) => 
              booking.busId === this.selectedBus.id
            );
            
            console.log('[Bookings Dialog] Bookings for this bus:', this.busBookings);
            
            this.filterBookingsByDate();
            this.isLoadingBookings = false;
            this.cdr.detectChanges();
          },
          error: (err) => {
            console.error('[Bookings Dialog] Error loading bookings:', err);
            this.isLoadingBookings = false;
            this.snackBar.open('Failed to load bookings', 'Close', { duration: 3000 });
            this.cdr.detectChanges();
          }
        });
      },
      error: (err) => {
        console.error('[Bookings Dialog] Error loading trips:', err);
        this.isLoadingBookings = false;
        this.snackBar.open('Failed to load trip data', 'Close', { duration: 3000 });
        this.cdr.detectChanges();
      }
    });
  }

  onBookingDateChange(date: Date | null): void {
    console.log('[Bookings Dialog] Date changed:', date);
    if (date) {
      this.selectedBookingDate = date;
      this.filterBookingsByDate();
      this.cdr.detectChanges();
    }
  }

  filterBookingsByDate(): void {
    const selectedDateStr = this.selectedBookingDate.toDateString();
    console.log('[Bookings Dialog] Filtering bookings for date:', selectedDateStr);
    
    this.bookingsForSelectedDate = this.busBookings.filter(booking => {
      const bookingDateStr = new Date(booking.journeyDate).toDateString();
      return bookingDateStr === selectedDateStr;
    });
    
    console.log('[Bookings Dialog] Bookings found for date:', this.bookingsForSelectedDate.length);
  }

  hasBookingsOnDate(date: Date): boolean {
    return this.tripDatesWithBookings.some(d => 
      d.toDateString() === date.toDateString()
    );
  }

  bookingDateClass = (date: Date): string => {
    const hasBookings = this.hasBookingsOnDate(date);
    const isSelected = this.selectedBookingDate && date.toDateString() === this.selectedBookingDate.toDateString();
    const isPast = date < new Date(new Date().setHours(0, 0, 0, 0));
    
    if (isPast) return 'past-date';
    if (isSelected) return 'selected-booking-date';
    if (hasBookings) return 'has-bookings-date';
    return '';
  }

  getTotalBookingsForDate(date: Date): number {
    const dateStr = date.toDateString();
    return this.busBookings.filter(booking => 
      new Date(booking.journeyDate).toDateString() === dateStr
    ).length;
  }

  getTotalSeatsBookedForDate(date: Date): number {
    const dateStr = date.toDateString();
    return this.busBookings
      .filter(booking => new Date(booking.journeyDate).toDateString() === dateStr)
      .reduce((total, booking) => total + (booking.seatNumbers?.length || 0), 0);
  }
}
