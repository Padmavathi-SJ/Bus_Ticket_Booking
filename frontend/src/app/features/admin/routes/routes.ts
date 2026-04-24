import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatTableModule } from '@angular/material/table';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatCardModule } from '@angular/material/card';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatDialogModule } from '@angular/material/dialog';
import { MatChipsModule } from '@angular/material/chips';
import { MatExpansionModule } from '@angular/material/expansion';
import { AdminService, Route } from '../../../core/services/admin.service';

@Component({
  selector: 'app-admin-routes',
  standalone: true,
  imports: [
    CommonModule, 
    ReactiveFormsModule, 
    MatTableModule, 
    MatButtonModule, 
    MatIconModule, 
    MatCardModule, 
    MatInputModule, 
    MatSelectModule,
    MatDialogModule,
    MatChipsModule,
    MatExpansionModule
  ],
  templateUrl: './routes.html',
  styleUrl: './routes.scss'
})
export class RouteManagement implements OnInit {
  routes: Route[] = [];
  routeForm: FormGroup;
  busForm: FormGroup;
  displayedColumns: string[] = ['name', 'source', 'destination', 'distance', 'status'];
  busColumns: string[] = ['busName', 'busNumber', 'type', 'seats', 'price', 'status'];
  
  isLoading = false;
  showAddForm = false;
  selectedRouteForBus: string | null = null;

  constructor(
    private adminService: AdminService,
    private fb: FormBuilder,
    private cdr: ChangeDetectorRef
  ) {
    this.routeForm = this.fb.group({
      name: ['', Validators.required],
      source: ['', Validators.required],
      destination: ['', Validators.required],
      distanceKm: [0, [Validators.required, Validators.min(1)]]
    });

    this.busForm = this.fb.group({
      busName: ['', Validators.required],
      busNumber: ['', Validators.required],
      busType: ['AC Sleeper', Validators.required],
      totalSeats: [36, [Validators.required, Validators.min(1)]],
      basePrice: [500, [Validators.required, Validators.min(0)]],
      amenities: ['Wifi, Water, Charging Point']
    });
  }

  ngOnInit(): void {
    setTimeout(() => {
      this.loadData();
    });
  }

  loadData() {
    this.isLoading = true;
    this.adminService.getRoutes().subscribe({
      next: (data) => {
        this.routes = data;
        this.isLoading = false;
        this.cdr.detectChanges();
      },
      error: (err) => {
        console.error('Error fetching routes', err);
        this.isLoading = false;
        this.cdr.detectChanges();
      }
    });
  }

  onSubmit() {
    if (this.routeForm.valid) {
      this.adminService.createRoute(this.routeForm.value).subscribe({
        next: () => {
          this.loadData();
          this.showAddForm = false;
          this.routeForm.reset({ distanceKm: 0 });
          alert('Route created successfully!');
        },
        error: (err) => alert('Error: ' + err.error?.message || 'Failed to create route')
      });
    }
  }

  onAddBus(routeId: string) {
    if (this.busForm.valid) {
      const busData = {
        ...this.busForm.value,
        routeId: routeId
      };
      
      this.adminService.addBus(busData).subscribe({
        next: () => {
          this.selectedRouteForBus = null;
          this.busForm.reset({ busType: 'AC Sleeper', totalSeats: 36, basePrice: 500 });
          alert('Bus added successfully to this route!');
          // We might want to refresh the bus list for this route specifically, 
          // but for now let's keep it simple.
        },
        error: (err) => alert('Error: ' + err.error?.message || 'Failed to add bus')
      });
    }
  }
}
