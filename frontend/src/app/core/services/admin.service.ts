import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface OperatorRequest {
  id: string;
  fullName: string;
  email: string;
  phone: string;
  companyName: string;
  licenseNumber: string;
  address: string;
  status: number;
  createdAt: string;
}

export interface Station {
  id: string;
  name: string;
  city: string;
  state: string;
  code: string;
  isActive: boolean;
}

export interface Route {
  id: string;
  name: string;
  source: string;
  destination: string;
  distanceKm: number;
  isActive: boolean;
}

export interface Bus {
  id: string;
  operatorId: string;
  operatorName: string;
  routeId: string;
  routeName: string;
  busNumber: string;
  busName: string;
  busType: string;
  totalSeats: number;
  femaleSeats: number;
  maleSeats: number;
  status: number;
}

@Injectable({
  providedIn: 'root'
})
export class AdminService {
  private apiUrl = `${environment.apiUrl}/admin`;

  constructor(private http: HttpClient) {}

  // Operators
  getOperators(status?: number): Observable<OperatorRequest[]> {
    let params = new HttpParams();
    if (status !== undefined && status !== null) {
      params = params.set('status', status.toString());
    }
    return this.http.get<OperatorRequest[]>(`${this.apiUrl}/operators`, { params });
  }

  getPendingOperators(): Observable<OperatorRequest[]> {
    return this.http.get<OperatorRequest[]>(`${this.apiUrl}/operator-requests`);
  }

  approveOperator(id: string): Observable<any> {
    return this.http.post(`${this.apiUrl}/approve-operator/${id}`, {});
  }

  rejectOperator(id: string, reason: string): Observable<any> {
    return this.http.post(`${this.apiUrl}/reject-operator/${id}`, JSON.stringify(reason), {
      headers: { 'Content-Type': 'application/json' }
    });
  }

  enableOperator(id: string): Observable<any> {
    return this.http.post(`${this.apiUrl}/enable-operator/${id}`, {});
  }

  disableOperator(id: string): Observable<any> {
    return this.http.post(`${this.apiUrl}/disable-operator/${id}`, {});
  }

  // Stations
  getStations(): Observable<Station[]> {
    return this.http.get<Station[]>(`${this.apiUrl}/stations`);
  }

  createStation(station: any): Observable<any> {
    return this.http.post(`${this.apiUrl}/stations`, station);
  }

  // Routes
  getRoutes(): Observable<Route[]> {
    return this.http.get<Route[]>(`${this.apiUrl}/routes`);
  }

  createRoute(route: any): Observable<any> {
    return this.http.post(`${this.apiUrl}/routes`, route);
  }

  // Buses
  getPendingBuses(): Observable<Bus[]> {
    return this.http.get<Bus[]>(`${this.apiUrl}/pending-buses`);
  }

  getAllBuses(source?: string, destination?: string): Observable<Bus[]> {
    let params = new HttpParams();
    if (source) params = params.set('source', source);
    if (destination) params = params.set('destination', destination);
    return this.http.get<Bus[]>(`${this.apiUrl}/all-buses`, { params });
  }

  getBusRequests(): Observable<Bus[]> {
    return this.http.get<Bus[]>(`${this.apiUrl}/fleet/requests`);
  }

  getActiveBuses(): Observable<Bus[]> {
    return this.http.get<Bus[]>(`${this.apiUrl}/fleet/active`);
  }

  getAllHistoryBuses(): Observable<Bus[]> {
    return this.http.get<Bus[]>(`${this.apiUrl}/fleet/history`);
  }

  approveBus(id: string): Observable<any> {
    return this.http.post(`${this.apiUrl}/approve-bus/${id}`, {});
  }

  rejectBus(id: string, reason: string): Observable<any> {
    return this.http.post(`${this.apiUrl}/reject-bus/${id}`, JSON.stringify(reason), {
      headers: { 'Content-Type': 'application/json' }
    });
  }

  addBus(bus: any): Observable<any> {
    return this.http.post(`${this.apiUrl}/add-bus`, bus);
  }
}
