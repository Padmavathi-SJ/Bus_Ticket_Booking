import { Component, OnInit, OnDestroy, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatChipsModule } from '@angular/material/chips';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatStepperModule } from '@angular/material/stepper';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatTooltipModule } from '@angular/material/tooltip';
import { Subscription, finalize } from 'rxjs';
import { BookingService } from '../../../core/services/booking.service';
import { AuthService } from '../../../core/services/auth.service';

interface Seat {
  seatNumber: string;
  isBooked: boolean;
  isSelected: boolean;
  gender?: 'Male' | 'Female';
  seatType: 'Window' | 'Aisle' | 'Middle';
  reservedFor?: 'female' | 'male' | 'general'; // Add reservation type
}

interface TripDetails {
  tripId: string;
  busId: string;
  busName: string;
  busNumber: string;
  busType: string;
  operatorName: string;
  sourceAddress: string;
  destinationAddress: string;
  pickupPoint: string;
  dropPoint: string;
  tripDate: string;
  departureTime: string;
  arrivalTime: string;
  duration: number;
  basePrice: number;
  totalSeats: number;
  femaleSeats: number;
  maleSeats: number;
  amenities: string;
  bookedSeats: number;
  availableSeats: number;
}

@Component({
  selector: 'app-seat-selection',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    RouterModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatChipsModule,
    MatFormFieldModule,
    MatInputModule,
    MatStepperModule,
    MatToolbarModule,
    MatProgressSpinnerModule,
    MatSnackBarModule,
    MatTooltipModule
  ],
  templateUrl: './seat-selection.html',
  styleUrls: ['./seat-selection.scss']
})
export class SeatSelectionComponent implements OnInit, OnDestroy {
  tripId: string = '';
  tripDetails: TripDetails | null = null;
  seats: Seat[][] = [];
  selectedSeats: Seat[] = [];
  passengerForms: FormGroup[] = []; // Array of forms for multiple passengers
  paymentForm: FormGroup; // Payment form
  isLoading = false;
  isBooking = false;
  currentStep = 0; // 0: Seat Selection, 1: Passenger Details, 2: Payment
  
  private subscription?: Subscription;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private fb: FormBuilder,
    private bookingService: BookingService,
    private authService: AuthService,
    private snackBar: MatSnackBar,
    private cdr: ChangeDetectorRef
  ) {
    // Initialize payment form
    this.paymentForm = this.fb.group({
      paymentMethod: ['', Validators.required],
      paymentStatus: ['', Validators.required]
    });
  }

  ngOnInit(): void {
    this.tripId = this.route.snapshot.paramMap.get('tripId') || '';
    
    console.log('SeatSelectionComponent initialized');
    console.log('Trip ID from route:', this.tripId);
    
    if (!this.tripId) {
      console.error('No trip ID provided in route');
      this.router.navigate(['/user']);
      return;
    }

    this.loadTripDetails();
  }

  createPassengerForm(index: number): FormGroup {
    const user = this.authService.currentUserValue;
    
    // Pre-fill first passenger with user details
    if (index === 0 && user) {
      return this.fb.group({
        fullName: [user.fullName || '', [Validators.required, Validators.minLength(3)]],
        email: [user.email || '', [Validators.required, Validators.email]],
        phone: ['', [Validators.required, Validators.pattern(/^[0-9]{10}$/)]],
        age: ['', [Validators.required, Validators.min(1), Validators.max(120)]],
        gender: ['', Validators.required]
      });
    }
    
    // Empty form for additional passengers
    return this.fb.group({
      fullName: ['', [Validators.required, Validators.minLength(3)]],
      email: ['', [Validators.required, Validators.email]],
      phone: ['', [Validators.required, Validators.pattern(/^[0-9]{10}$/)]],
      age: ['', [Validators.required, Validators.min(1), Validators.max(120)]],
      gender: ['', Validators.required]
    });
  }

  loadTripDetails(): void {
    this.isLoading = true;
    console.log('Loading trip details for ID:', this.tripId);
    
    this.subscription = this.bookingService.getTripDetails(this.tripId)
      .subscribe({
        next: (data: TripDetails) => {
          console.log('Trip details received:', data);
          this.tripDetails = data;
          this.isLoading = false;
          console.log('tripDetails assigned:', this.tripDetails);
          this.generateSeats(data.totalSeats);
          this.loadBookedSeats();
          this.cdr.detectChanges();
        },
        error: (err: any) => {
          console.error('Error loading trip details:', err);
          this.isLoading = false;
          this.snackBar.open('Failed to load trip details: ' + (err.message || 'Unknown error'), 'Close', { duration: 5000 });
          this.router.navigate(['/user']);
        }
      });
  }

  generateSeats(totalSeats: number): void {
    console.log('Generating seats, totalSeats:', totalSeats);
    
    // Get female and male seat counts from trip details
    const femaleSeats = this.tripDetails?.femaleSeats || 0;
    const maleSeats = this.tripDetails?.maleSeats || 0;
    
    // If no specific allocation, use default distribution
    // 30% female, 40% male, 30% general
    let femaleSeatCount = femaleSeats;
    let maleSeatCount = maleSeats;
    
    if (femaleSeats === 0 && maleSeats === 0) {
      femaleSeatCount = Math.floor(totalSeats * 0.3);
      maleSeatCount = Math.floor(totalSeats * 0.4);
    }
    
    // Generate seat layout (4 seats per row: 2-2 configuration)
    const rows = Math.ceil(totalSeats / 4);
    this.seats = [];

    let seatNumber = 1;
    let femaleCount = 0;
    let maleCount = 0;
    
    for (let row = 0; row < rows; row++) {
      const rowSeats: Seat[] = [];
      
      for (let col = 0; col < 4; col++) {
        if (seatNumber <= totalSeats) {
          const seatType = col === 0 || col === 3 ? 'Window' : col === 1 ? 'Aisle' : 'Middle';
          
          // Determine reservation type
          let reservedFor: 'female' | 'male' | 'general' = 'general';
          
          if (femaleCount < femaleSeatCount) {
            reservedFor = 'female';
            femaleCount++;
          } else if (maleCount < maleSeatCount) {
            reservedFor = 'male';
            maleCount++;
          }
          
          rowSeats.push({
            seatNumber: `${seatNumber}`,
            isBooked: false,
            isSelected: false,
            seatType: seatType,
            reservedFor: reservedFor
          });
          seatNumber++;
        }
      }
      
      this.seats.push(rowSeats);
    }
    console.log('Seats generated:', this.seats.length, 'rows');
    console.log('Female seats:', femaleSeatCount, 'Male seats:', maleSeatCount, 'General:', totalSeats - femaleSeatCount - maleSeatCount);
    console.log('First row:', this.seats[0]);
  }

  loadBookedSeats(): void {
    this.bookingService.getBookedSeatsForTrip(this.tripId).subscribe({
      next: (bookedSeats: string[]) => {
        console.log('Booked seats for trip:', bookedSeats);
        // Mark booked seats
        this.seats.forEach(row => {
          row.forEach(seat => {
            if (bookedSeats.includes(seat.seatNumber)) {
              seat.isBooked = true;
            }
          });
        });
      },
      error: (err: any) => {
        console.error('Failed to load booked seats', err);
      }
    });
  }

  toggleSeat(seat: Seat): void {
    if (seat.isBooked) {
      this.snackBar.open('This seat is already booked', 'Close', { duration: 2000 });
      return;
    }

    seat.isSelected = !seat.isSelected;

    if (seat.isSelected) {
      this.selectedSeats.push(seat);
    } else {
      this.selectedSeats = this.selectedSeats.filter(s => s.seatNumber !== seat.seatNumber);
    }
  }

  getTotalPrice(): number {
    if (!this.tripDetails) return 0;
    return this.selectedSeats.length * this.tripDetails.basePrice;
  }

  proceedToPassengerDetails(): void {
    if (this.selectedSeats.length === 0) {
      this.snackBar.open('Please select at least one seat', 'Close', { duration: 3000 });
      return;
    }
    
    // Create a form for each selected seat
    this.passengerForms = [];
    for (let i = 0; i < this.selectedSeats.length; i++) {
      this.passengerForms.push(this.createPassengerForm(i));
    }
    
    this.currentStep = 1;
  }

  backToSeatSelection(): void {
    this.currentStep = 0;
  }

  proceedToPayment(): void {
    // Validate all passenger forms
    const allFormsValid = this.passengerForms.every(form => form.valid);
    
    if (!allFormsValid) {
      this.snackBar.open('Please fill all passenger details correctly', 'Close', { duration: 3000 });
      return;
    }

    this.currentStep = 2;
  }

  backToPassengerDetails(): void {
    this.currentStep = 1;
  }

  confirmBooking(): void {
    // Validate payment form
    if (!this.paymentForm.valid) {
      this.snackBar.open('Please complete payment details', 'Close', { duration: 3000 });
      return;
    }

    // Validate all passenger forms
    const allFormsValid = this.passengerForms.every(form => form.valid);
    
    if (!allFormsValid) {
      this.snackBar.open('Please fill all passenger details correctly', 'Close', { duration: 3000 });
      return;
    }

    if (this.selectedSeats.length === 0) {
      this.snackBar.open('No seats selected', 'Close', { duration: 3000 });
      return;
    }

    const paymentData = this.paymentForm.value;

    this.isBooking = true;

    // Create bookings for each passenger
    const bookingPromises = this.selectedSeats.map((seat, index) => {
      const passengerData = this.passengerForms[index].value;
      
      const bookingData = {
        tripId: this.tripId,
        seatNumbers: [seat.seatNumber],
        passengerDetails: passengerData,
        totalAmount: this.tripDetails?.basePrice || 0,
        paymentMethod: paymentData.paymentMethod,
        paymentStatus: paymentData.paymentStatus
      };

      return this.bookingService.createBooking(bookingData).toPromise();
    });

    Promise.all(bookingPromises)
      .then(() => {
        this.isBooking = false;
        this.snackBar.open(`${this.selectedSeats.length} booking(s) confirmed successfully! Confirmation email sent.`, 'Close', { duration: 5000 });
        this.router.navigate(['/user/bookings']);
      })
      .catch((err: any) => {
        this.isBooking = false;
        this.snackBar.open(err.error?.message || 'Booking failed. Please try again.', 'Close', { duration: 3000 });
      });
  }

  cancelBooking(): void {
    this.router.navigate(['/user']);
  }

  getAmenitiesList(amenities: string): string[] {
    return amenities ? amenities.split(',').map(a => a.trim()) : [];
  }

  ngOnDestroy(): void {
    this.subscription?.unsubscribe();
  }
}
