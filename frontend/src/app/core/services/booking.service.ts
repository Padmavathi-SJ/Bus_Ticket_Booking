import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface BookingRequest {
  busId?: string; // Optional for backward compatibility
  tripId?: string; // New field for trip-based bookings
  seatNumbers: string[];
  passengerDetails: {
    fullName: string;
    email: string;
    phone: string;
    age: number;
    gender: string;
  };
  totalAmount: number;
}

export interface Booking {
  id: string;
  busId: string;
  busName: string;
  busNumber: string;
  operatorName: string;
  source: string;
  destination: string;
  seatNumbers: string[];
  passengerName: string;
  passengerEmail: string;
  passengerPhone: string;
  totalAmount: number;
  bookingDate: string;
  journeyDate: string;
  status: 'Confirmed' | 'Cancelled' | 'Completed';
}

@Injectable({
  providedIn: 'root'
})
export class BookingService {
  private apiUrl = `${environment.apiUrl}`;

  constructor(private http: HttpClient) {}

  getBusDetails(busId: string): Observable<any> {
    const url = `${this.apiUrl}/buses/${busId}`;
    console.log('BookingService: Fetching bus details from:', url);
    return this.http.get<any>(url);
  }

  getTripDetails(tripId: string): Observable<any> {
    const url = `${this.apiUrl}/buses/trips/${tripId}`;
    console.log('BookingService: Fetching trip details from:', url);
    return this.http.get<any>(url);
  }

  getBookedSeats(busId: string): Observable<string[]> {
    return this.http.get<string[]>(`${this.apiUrl}/bookings/booked-seats/${busId}`);
  }

  getBookedSeatsForTrip(tripId: string): Observable<string[]> {
    const url = `${this.apiUrl}/bookings/booked-seats/trip/${tripId}`;
    console.log('BookingService: Fetching booked seats for trip from:', url);
    return this.http.get<string[]>(url);
  }

  createBooking(bookingData: BookingRequest): Observable<any> {
    return this.http.post(`${this.apiUrl}/bookings`, bookingData);
  }

  getUserBookings(): Observable<Booking[]> {
    const url = `${this.apiUrl}/bookings/my-bookings`;
    console.log('[BookingService] Fetching user bookings from:', url);
    return this.http.get<Booking[]>(url);
  }

  cancelBooking(bookingId: string): Observable<any> {
    return this.http.put(`${this.apiUrl}/bookings/${bookingId}/cancel`, {});
  }

  getBookingDetails(bookingId: string): Observable<Booking> {
    return this.http.get<Booking>(`${this.apiUrl}/bookings/${bookingId}`);
  }
}
