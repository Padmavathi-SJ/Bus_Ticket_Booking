import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface Route {
  id: string;
  name: string;
  source: string;
  destination: string;
  distanceKm: number;
  isActive: boolean;
}

export interface OperatorBooking {
  id: string;
  bookingReference: string;
  busId: string;
  busName: string;
  busNumber: string;
  busType: string;
  source: string;
  destination: string;
  seatNumbers: string[];
  passengerName: string;
  passengerEmail: string;
  passengerPhone: string;
  passengerAge: number;
  passengerGender: string;
  totalAmount: number;
  bookingDate: string;
  journeyDate: string;
  status: string;
}

export interface TripSchedule {
  busId: string;
  routeId: string;
  sourceAddress: string;
  destinationAddress: string;
  pickupPoint: string;
  dropPoint: string;
  departureDateTime: string;
  arrivalDateTime: string;
  basePrice: number;
}

export interface Trip {
  id: string;
  tripDate: Date;
  sourceAddress: string;
  destinationAddress: string;
  pickupPoint: string;
  dropPoint: string;
  departureTime: string;
  arrivalTime: string;
  basePrice: number;
  status: string;
  bookedSeats: number;
  availableSeats: number;
}

@Injectable({
  providedIn: 'root'
})
export class OperatorService {
  private apiUrl = `${environment.apiUrl}/operator`;

  constructor(private http: HttpClient) {}

  getRoutes(): Observable<Route[]> {
    return this.http.get<Route[]>(`${this.apiUrl}/routes`);
  }

  addBus(bus: any): Observable<any> {
    return this.http.post(`${this.apiUrl}/add-bus`, bus);
  }

  getMyBuses(): Observable<any[]> {
    return this.http.get<any[]>(`${this.apiUrl}/my-buses`);
  }

  getMyBookings(): Observable<OperatorBooking[]> {
    return this.http.get<OperatorBooking[]>(`${this.apiUrl}/bookings`);
  }

  scheduleTrip(tripData: TripSchedule): Observable<any> {
    console.log('[OperatorService] Scheduling trip:', tripData);
    return this.http.post(`${this.apiUrl}/schedule-trip`, tripData);
  }

  getBusTrips(busId: string): Observable<Trip[]> {
    console.log('[OperatorService] Fetching trips for bus:', busId);
    return this.http.get<Trip[]>(`${this.apiUrl}/buses/${busId}/trips`);
  }
}
