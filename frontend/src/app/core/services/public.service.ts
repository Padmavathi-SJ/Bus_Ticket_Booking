import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface BusSearchResult {
  id: string;
  busName: string;
  busNumber: string;
  busType: string;
  totalSeats: number;
  availableSeats: number;
  femaleSeats?: number;
  maleSeats?: number;
  basePrice: number;
  amenities: string;
  routeName: string;
  operatorName: string;
  source: string;
  destination: string;
}

export interface TripSearchResult {
  tripId: string;
  busId: string;
  busName: string;
  busNumber: string;
  busType: string;
  operatorName: string;
  totalSeats: number;
  femaleSeats: number;
  maleSeats: number;
  amenities: string;
  tripDate: Date;
  sourceAddress: string;
  destinationAddress: string;
  pickupPoint: string;
  dropPoint: string;
  departureTime: string;
  arrivalTime: string;
  duration: number;
  basePrice: number;
  bookedSeats: number;
  availableSeats: number;
}

@Injectable({
  providedIn: 'root'
})
export class PublicService {
  private apiUrl = `${environment.apiUrl}/buses`;

  constructor(private http: HttpClient) {}

  searchBuses(source?: string, destination?: string): Observable<any[]> {
    let params = new HttpParams();
    if (source) params = params.set('source', source);
    if (destination) params = params.set('destination', destination);
    
    return this.http.get<any[]>(`${this.apiUrl}/search`, { params });
  }

  searchTrips(source?: string, destination?: string, tripDate?: Date): Observable<TripSearchResult[]> {
    let params = new HttpParams();
    if (source) params = params.set('source', source);
    if (destination) params = params.set('destination', destination);
    if (tripDate) {
      // Format date as YYYY-MM-DD
      const dateStr = tripDate.toISOString().split('T')[0];
      params = params.set('tripDate', dateStr);
    }
    
    console.log('[PublicService] Searching trips with params:', { source, destination, tripDate: tripDate?.toISOString() });
    return this.http.get<TripSearchResult[]>(`${this.apiUrl}/search-trips`, { params });
  }
}
