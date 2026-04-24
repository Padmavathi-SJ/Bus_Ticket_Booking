import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatTableModule } from '@angular/material/table';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatCardModule } from '@angular/material/card';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatInputModule } from '@angular/material/input';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatSelectModule } from '@angular/material/select';
import { MatChipsModule } from '@angular/material/chips';
import { MatTabsModule } from '@angular/material/tabs';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatExpansionModule } from '@angular/material/expansion';
import { AdminService, Bus } from '../../../core/services/admin.service';

@Component({
  selector: 'app-admin-buses',
  standalone: true,
  imports: [
    CommonModule, 
    FormsModule,
    MatTableModule, 
    MatButtonModule, 
    MatIconModule, 
    MatCardModule, 
    MatTooltipModule,
    MatInputModule,
    MatFormFieldModule,
    MatSelectModule,
    MatChipsModule,
    MatTabsModule,
    MatSnackBarModule,
    MatExpansionModule
  ],
  templateUrl: './buses.html',
  styleUrl: './buses.scss'
})
export class BusManagement implements OnInit {
  // Category Lists
  busRequests: Bus[] = [];
  activeBuses: Bus[] = [];
  allBuses: Bus[] = [];

  displayedColumns: string[] = ['busName', 'busNumber', 'route', 'type', 'seats', 'status', 'actions'];
  
  isLoading = false;
  expandedBusId: string | null = null;

  constructor(
    private adminService: AdminService,
    private cdr: ChangeDetectorRef,
    private snackBar: MatSnackBar
  ) {}

  ngOnInit(): void {
    setTimeout(() => {
      this.loadAllCategories();
    });
  }

  loadAllCategories() {
    setTimeout(() => {
      this.isLoading = true;
    });
    
    // Load Bus Requests
    this.adminService.getBusRequests().subscribe({
      next: (data) => {
        this.busRequests = data;
        this.cdr.detectChanges();
      }
    });

    // Load Active Buses
    this.adminService.getActiveBuses().subscribe({
      next: (data) => {
        this.activeBuses = data;
        this.cdr.detectChanges();
      }
    });

    // Load All History
    this.adminService.getAllHistoryBuses().subscribe({
      next: (data) => {
        this.allBuses = data;
        this.isLoading = false;
        this.cdr.detectChanges();
      },
      error: () => {
        this.isLoading = false;
        this.cdr.detectChanges();
      }
    });
  }

  approveBus(id: string) {
    this.adminService.approveBus(id).subscribe({
      next: () => {
        this.snackBar.open('Bus approved successfully!', 'Close', { duration: 3000 });
        this.loadAllCategories();
      },
      error: () => {
        this.snackBar.open('Error approving bus', 'Close', { duration: 3000 });
      }
    });
  }

  rejectBus(id: string) {
    const reason = prompt('Enter rejection reason:');
    if (reason) {
      this.adminService.rejectBus(id, reason).subscribe({
        next: () => {
          this.snackBar.open('Bus rejected', 'Close', { duration: 3000 });
          this.loadAllCategories();
        },
        error: () => {
          this.snackBar.open('Error rejecting bus', 'Close', { duration: 3000 });
        }
      });
    }
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
}
