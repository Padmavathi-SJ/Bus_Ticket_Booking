import { Routes } from '@angular/router';
import { AdminLogin } from './features/auth/admin-login/admin-login';
import { UserLogin } from './features/auth/user-login/user-login';
import { OperatorLogin } from './features/auth/operator-login/operator-login';
import { Register } from './features/auth/register/register';
import { RegisterOperator } from './features/auth/register-operator/register-operator';
import { Dashboard as AdminDashboard } from './features/admin/dashboard/dashboard';
import { Dashboard as CustomerDashboard } from './features/customer/dashboard/dashboard';
import { Dashboard as OperatorDashboard } from './features/operator/dashboard/dashboard';
import { AdminLayout } from './features/admin/admin-layout/admin-layout';
import { OperatorLayout } from './features/operator/operator-layout/operator-layout';
import { OperatorManagement } from './features/admin/operators/operators';
import { RouteManagement } from './features/admin/routes/routes';
import { StationManagement } from './features/admin/stations/stations';
import { BusManagement as AdminBusManagement } from './features/admin/buses/buses';
import { OperatorRouteManagement } from './features/operator/routes/routes';
import { OperatorBusManagement } from './features/operator/buses/buses';
import { OperatorBookingManagement } from './features/operator/bookings/bookings';
import { UserPanelComponent } from './features/customer/user-panel/user-panel';

import { SeatSelectionComponent } from './features/customer/seat-selection/seat-selection';
import { BookingsComponent } from './features/customer/bookings/bookings';

export const routes: Routes = [
  // User Panel (Landing Page)
  { path: '', redirectTo: '/user', pathMatch: 'full' },
  { path: 'user', component: UserPanelComponent },
  { path: 'home', redirectTo: '/user', pathMatch: 'full' },

  // Auth Routes
  { path: 'admin-login', component: AdminLogin },
  { path: 'user_login', component: UserLogin },
  { path: 'operator_login', component: OperatorLogin },
  { path: 'register', component: Register },
  { path: 'user_register', redirectTo: '/register', pathMatch: 'full' },
  { path: 'operator_register', component: RegisterOperator },
  
  // Admin Routes (changed from /admin-panel to /admin)
  { 
    path: 'admin', 
    component: AdminLayout,
    children: [
      { path: '', redirectTo: 'dashboard', pathMatch: 'full' },
      { path: 'dashboard', component: AdminDashboard },
      { path: 'operators', component: OperatorManagement },
      { path: 'buses', component: AdminBusManagement },
      { path: 'routes', component: RouteManagement },
      { path: 'stations', component: StationManagement }
    ]
  },

  // Operator Routes
  {
    path: 'operator',
    component: OperatorLayout,
    children: [
      { path: '', redirectTo: 'dashboard', pathMatch: 'full' },
      { path: 'dashboard', component: OperatorDashboard },
      { path: 'routes', component: OperatorRouteManagement },
      { path: 'buses', component: OperatorBusManagement },
      { path: 'bookings', component: OperatorBookingManagement }
    ]
  },
  
  // Customer Routes
  { path: 'customer/dashboard', component: CustomerDashboard },
  { path: 'customer/seat-selection/:tripId', component: SeatSelectionComponent },
  
  // User Routes (Authenticated)
  { path: 'user/bookings', component: BookingsComponent },

  // Redirects
  { path: '**', redirectTo: '/user' }
];
