import { Injectable, PLATFORM_ID, Inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, Observable, tap } from 'rxjs';
import { environment } from '../../../environments/environment';
import { AuthResponse, User, UserRole } from '../models/user.model';
import { isPlatformBrowser } from '@angular/common';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private apiUrl = `${environment.apiUrl}/auth`;

  // This keeps track of the currently logged-in user across the app
  private currentUserSubject = new BehaviorSubject<User | null>(null);
  public currentUser$ = this.currentUserSubject.asObservable();

  constructor(
    private http: HttpClient,
    @Inject(PLATFORM_ID) private platformId: Object
  ) {
    this.loadUserFromStorage();
  }

  // 1. Admin Login
  adminLogin(credentials: any): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${this.apiUrl}/admin-login`, credentials).pipe(
      tap(response => this.handleAuthSuccess(response))
    );
  }

  // 2. Customer Login
  userLogin(credentials: any): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${this.apiUrl}/user-login`, credentials).pipe(
      tap(response => this.handleAuthSuccess(response))
    );
  }

  // 3. Operator Login
  operatorLogin(credentials: any): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${this.apiUrl}/operator-login`, credentials).pipe(
      tap(response => this.handleAuthSuccess(response))
    );
  }

  // 4. Customer Registration
  registerCustomer(data: any): Observable<any> {
    return this.http.post(`${this.apiUrl}/user-register`, data);
  }

  // 5. Operator Registration
  registerOperator(data: any): Observable<any> {
    return this.http.post(`${this.apiUrl}/operator-register`, data);
  }

  isLoggedIn(): boolean {
    if (isPlatformBrowser(this.platformId)) {
      return !!localStorage.getItem('token');
    }
    return false;
  }

  logout() {
    if (isPlatformBrowser(this.platformId)) {
      localStorage.removeItem('token');
      localStorage.removeItem('refreshToken');
      localStorage.removeItem('user');
    }
    this.currentUserSubject.next(null);
  }

  get currentUserValue(): User | null {
    return this.currentUserSubject.value;
  }

  getToken(): string | null {
    if (isPlatformBrowser(this.platformId)) {
      return localStorage.getItem('token');
    }
    return null;
  }

  private handleAuthSuccess(response: AuthResponse) {
    if (isPlatformBrowser(this.platformId)) {
      localStorage.setItem('token', response.token);
      localStorage.setItem('refreshToken', response.refreshToken);
      localStorage.setItem('user', JSON.stringify(response.user));
    }
    this.currentUserSubject.next(response.user);
  }

  private loadUserFromStorage() {
    if (isPlatformBrowser(this.platformId)) {
      const userJson = localStorage.getItem('user');
      if (userJson) {
        this.currentUserSubject.next(JSON.parse(userJson));
      }
    }
  }
}
