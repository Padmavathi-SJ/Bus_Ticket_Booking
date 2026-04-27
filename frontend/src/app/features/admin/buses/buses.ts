import { Component, OnInit, ChangeDetectorRef, Inject } from '@angular/core';
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
import { MatDialog, MatDialogModule, MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
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
    MatExpansionModule,
    MatDialogModule
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
    private snackBar: MatSnackBar,
    private dialog: MatDialog
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
        console.log('[Admin Buses] Bus Requests Data:', data);
        this.busRequests = data.map(bus => ({
          ...bus,
          femaleSeats: bus.femaleSeats || 0,
          maleSeats: bus.maleSeats || 0
        }));
        console.log('[Admin Buses] Mapped Bus Requests:', this.busRequests);
        this.busRequests.forEach(bus => {
          console.log(`[Admin Buses] ${bus.busName}: Total=${bus.totalSeats}, Female=${bus.femaleSeats}, Male=${bus.maleSeats}`);
        });
        this.cdr.detectChanges();
      }
    });

    // Load Active Buses
    this.adminService.getActiveBuses().subscribe({
      next: (data) => {
        console.log('[Admin Buses] Active Buses Data:', data);
        this.activeBuses = data.map(bus => ({
          ...bus,
          femaleSeats: bus.femaleSeats || 0,
          maleSeats: bus.maleSeats || 0
        }));
        console.log('[Admin Buses] Mapped Active Buses:', this.activeBuses);
        this.activeBuses.forEach(bus => {
          console.log(`[Admin Buses] ${bus.busName}: Total=${bus.totalSeats}, Female=${bus.femaleSeats}, Male=${bus.maleSeats}`);
        });
        this.cdr.detectChanges();
      }
    });

    // Load All History
    this.adminService.getAllHistoryBuses().subscribe({
      next: (data) => {
        console.log('[Admin Buses] All History Buses Data:', data);
        this.allBuses = data.map(bus => ({
          ...bus,
          femaleSeats: bus.femaleSeats || 0,
          maleSeats: bus.maleSeats || 0
        }));
        console.log('[Admin Buses] Mapped All History Buses:', this.allBuses);
        this.allBuses.forEach(bus => {
          console.log(`[Admin Buses] ${bus.busName}: Total=${bus.totalSeats}, Female=${bus.femaleSeats}, Male=${bus.maleSeats}`);
        });
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

  openSeatLayoutDialog(bus: Bus) {
    this.dialog.open(SeatLayoutDialogComponent, {
      width: '800px',
      maxHeight: '90vh',
      data: bus
    });
  }

  generateSeatLayout(totalSeats: number, femaleSeats: number, maleSeats: number): any[] {
    const seats = [];
    const rows = Math.ceil(totalSeats / 4);
    
    console.log(`[Admin Seat Layout] Generating layout - Total: ${totalSeats}, Female: ${femaleSeats}, Male: ${maleSeats}`);
    
    // Use the actual values from database
    const actualFemaleSeats = femaleSeats || 0;
    const actualMaleSeats = maleSeats || 0;
    const generalSeats = totalSeats - actualFemaleSeats - actualMaleSeats;
    
    console.log(`[Admin Seat Layout] Distribution - Female: ${actualFemaleSeats}, Male: ${actualMaleSeats}, General: ${generalSeats}`);
    
    let seatNumber = 1;
    let femaleCount = 0;
    let maleCount = 0;
    let generalCount = 0;
    
    for (let row = 0; row < rows; row++) {
      const rowSeats = [];
      
      for (let col = 0; col < 4; col++) {
        if (seatNumber <= totalSeats) {
          let seatType = 'general';
          
          // Assign female seats first (typically front rows)
          if (femaleCount < actualFemaleSeats) {
            seatType = 'female';
            femaleCount++;
          } 
          // Then assign male seats
          else if (maleCount < actualMaleSeats) {
            seatType = 'male';
            maleCount++;
          }
          // Remaining are general seats
          else {
            seatType = 'general';
            generalCount++;
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
    
    console.log(`[Admin Seat Layout] Final counts - Female: ${femaleCount}, Male: ${maleCount}, General: ${generalCount}`);
    
    return seats;
  }
}

// Seat Layout Dialog Component
@Component({
  selector: 'app-seat-layout-dialog',
  standalone: true,
  imports: [
    CommonModule,
    MatDialogModule,
    MatButtonModule,
    MatIconModule
  ],
  template: `
    <div class="seat-layout-dialog">
      <div class="dialog-header">
        <h2>
          <mat-icon>event_seat</mat-icon>
          Seat Layout - {{data.busName}}
        </h2>
        <button mat-icon-button (click)="close()">
          <mat-icon>close</mat-icon>
        </button>
      </div>

      <div class="dialog-content">
        <div class="bus-info-card">
          <div class="info-item">
            <mat-icon>confirmation_number</mat-icon>
            <span>{{data.busNumber}}</span>
          </div>
          <div class="info-item">
            <mat-icon>directions_bus</mat-icon>
            <span>{{data.busType}}</span>
          </div>
          <div class="info-item">
            <mat-icon>map</mat-icon>
            <span>{{data.routeName}}</span>
          </div>
        </div>

        <div class="seat-stats">
          <div class="stat-item">
            <mat-icon>event_seat</mat-icon>
            <div>
              <span class="value">{{data.totalSeats}}</span>
              <span class="label">Total Seats</span>
            </div>
          </div>
          <div class="stat-item female">
            <mat-icon>female</mat-icon>
            <div>
              <span class="value">{{data.femaleSeats}}</span>
              <span class="label">Female Reserved</span>
            </div>
          </div>
          <div class="stat-item male">
            <mat-icon>male</mat-icon>
            <div>
              <span class="value">{{data.maleSeats}}</span>
              <span class="label">Male Reserved</span>
            </div>
          </div>
          <div class="stat-item general">
            <mat-icon>people</mat-icon>
            <div>
              <span class="value">{{data.totalSeats - data.femaleSeats - data.maleSeats}}</span>
              <span class="label">General Seats</span>
            </div>
          </div>
        </div>

        <div class="seat-legend">
          <div class="legend-item">
            <div class="seat-icon female-seat"></div>
            <span>Female Reserved</span>
          </div>
          <div class="legend-item">
            <div class="seat-icon male-seat"></div>
            <span>Male Reserved</span>
          </div>
          <div class="legend-item">
            <div class="seat-icon general-seat"></div>
            <span>General</span>
          </div>
        </div>

        <div class="bus-layout">
          <div class="driver-section">
            <mat-icon>drive_eta</mat-icon>
            <span>Driver</span>
          </div>

          <div class="seats-grid">
            <div class="seat-row" *ngFor="let row of seatLayout; let rowIndex = index">
              <span class="row-number">{{rowIndex + 1}}</span>
              
              <div class="seats-container">
                <div 
                  *ngFor="let seat of row; let colIndex = index"
                  class="seat-display"
                  [class.female-seat]="seat.type === 'female'"
                  [class.male-seat]="seat.type === 'male'"
                  [class.general-seat]="seat.type === 'general'"
                  [class.aisle-gap]="colIndex === 1">
                  <mat-icon>event_seat</mat-icon>
                  <span class="seat-number">{{seat.number}}</span>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>

      <div class="dialog-actions">
        <button mat-raised-button color="primary" (click)="close()">
          <mat-icon>close</mat-icon>
          Close
        </button>
      </div>
    </div>
  `,
  styles: [`
    .seat-layout-dialog {
      display: flex;
      flex-direction: column;
      max-height: 90vh;
    }

    .dialog-header {
      display: flex;
      justify-content: space-between;
      align-items: center;
      padding: 20px 24px;
      border-bottom: 1px solid #e0e0e0;
      background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
      color: white;

      h2 {
        margin: 0;
        display: flex;
        align-items: center;
        gap: 12px;
        font-size: 20px;
        font-weight: 600;

        mat-icon {
          font-size: 28px;
          width: 28px;
          height: 28px;
        }
      }

      button {
        color: white;
      }
    }

    .dialog-content {
      padding: 24px;
      overflow-y: auto;
      flex: 1;
    }

    .bus-info-card {
      display: flex;
      gap: 20px;
      padding: 16px;
      background: #f8f9fa;
      border-radius: 8px;
      margin-bottom: 20px;
      flex-wrap: wrap;

      .info-item {
        display: flex;
        align-items: center;
        gap: 8px;
        font-weight: 500;
        color: #333;

        mat-icon {
          color: #667eea;
        }
      }
    }

    .seat-stats {
      display: grid;
      grid-template-columns: repeat(auto-fit, minmax(150px, 1fr));
      gap: 16px;
      margin-bottom: 24px;

      .stat-item {
        display: flex;
        align-items: center;
        gap: 12px;
        padding: 16px;
        background: white;
        border-radius: 12px;
        border: 2px solid #e0e0e0;
        box-shadow: 0 2px 4px rgba(0,0,0,0.05);

        mat-icon {
          font-size: 32px;
          width: 32px;
          height: 32px;
        }

        div {
          display: flex;
          flex-direction: column;

          .value {
            font-size: 24px;
            font-weight: 700;
            line-height: 1;
          }

          .label {
            font-size: 12px;
            color: #666;
            margin-top: 4px;
          }
        }

        &.female {
          border-color: #ff69b4;
          color: #ff69b4;
        }

        &.male {
          border-color: #4169e1;
          color: #4169e1;
        }

        &.general {
          border-color: #28a745;
          color: #28a745;
        }
      }
    }

    .seat-legend {
      display: flex;
      gap: 24px;
      justify-content: center;
      margin-bottom: 24px;
      padding: 16px;
      background: white;
      border-radius: 8px;
      box-shadow: 0 2px 4px rgba(0,0,0,0.05);

      .legend-item {
        display: flex;
        align-items: center;
        gap: 8px;
        font-size: 14px;
        font-weight: 500;

        .seat-icon {
          width: 36px;
          height: 36px;
          border-radius: 6px;
          border: 2px solid;

          &.female-seat {
            background: #ffe6f0;
            border-color: #ff69b4;
          }

          &.male-seat {
            background: #e6f0ff;
            border-color: #4169e1;
          }

          &.general-seat {
            background: #e6ffe6;
            border-color: #28a745;
          }
        }
      }
    }

    .bus-layout {
      background: white;
      border-radius: 12px;
      padding: 24px;
      box-shadow: 0 2px 8px rgba(0,0,0,0.1);

      .driver-section {
        display: flex;
        align-items: center;
        justify-content: center;
        gap: 8px;
        padding: 16px;
        background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
        color: white;
        border-radius: 8px;
        margin-bottom: 24px;
        font-weight: 600;
        font-size: 16px;

        mat-icon {
          font-size: 28px;
          width: 28px;
          height: 28px;
        }
      }

      .seats-grid {
        display: flex;
        flex-direction: column;
        gap: 12px;
      }

      .seat-row {
        display: flex;
        align-items: center;
        gap: 12px;

        .row-number {
          width: 40px;
          text-align: center;
          font-weight: 700;
          color: #667eea;
          font-size: 16px;
        }

        .seats-container {
          display: flex;
          gap: 10px;
          flex: 1;
          justify-content: center;
        }

        .seat-display {
          width: 56px;
          height: 56px;
          border-radius: 8px;
          display: flex;
          flex-direction: column;
          align-items: center;
          justify-content: center;
          border: 2px solid;
          cursor: default;
          transition: all 0.2s;

          mat-icon {
            font-size: 24px;
            width: 24px;
            height: 24px;
          }

          .seat-number {
            font-size: 11px;
            font-weight: 700;
            margin-top: 2px;
          }

          &.female-seat {
            background: #ffe6f0;
            border-color: #ff69b4;
            color: #ff69b4;
          }

          &.male-seat {
            background: #e6f0ff;
            border-color: #4169e1;
            color: #4169e1;
          }

          &.general-seat {
            background: #e6ffe6;
            border-color: #28a745;
            color: #28a745;
          }

          &.aisle-gap {
            margin-left: 24px;
          }

          &:hover {
            transform: scale(1.05);
            box-shadow: 0 4px 12px rgba(0,0,0,0.15);
          }
        }
      }
    }

    .dialog-actions {
      padding: 16px 24px;
      border-top: 1px solid #e0e0e0;
      display: flex;
      justify-content: flex-end;
      background: #f8f9fa;

      button {
        display: flex;
        align-items: center;
        gap: 8px;
      }
    }
  `]
})
export class SeatLayoutDialogComponent {
  seatLayout: any[] = [];

  constructor(
    public dialogRef: MatDialogRef<SeatLayoutDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: Bus
  ) {
    console.log('[Seat Layout Dialog] Bus data:', data);
    this.seatLayout = this.generateSeatLayout(data.totalSeats, data.femaleSeats, data.maleSeats);
  }

  close() {
    this.dialogRef.close();
  }

  generateSeatLayout(totalSeats: number, femaleSeats: number, maleSeats: number): any[] {
    const seats = [];
    const rows = Math.ceil(totalSeats / 4);
    
    console.log(`[Dialog Seat Layout] Generating layout - Total: ${totalSeats}, Female: ${femaleSeats}, Male: ${maleSeats}`);
    
    const actualFemaleSeats = femaleSeats || 0;
    const actualMaleSeats = maleSeats || 0;
    const generalSeats = totalSeats - actualFemaleSeats - actualMaleSeats;
    
    console.log(`[Dialog Seat Layout] Distribution - Female: ${actualFemaleSeats}, Male: ${actualMaleSeats}, General: ${generalSeats}`);
    
    let seatNumber = 1;
    let femaleCount = 0;
    let maleCount = 0;
    let generalCount = 0;
    
    for (let row = 0; row < rows; row++) {
      const rowSeats = [];
      
      for (let col = 0; col < 4; col++) {
        if (seatNumber <= totalSeats) {
          let seatType = 'general';
          
          if (femaleCount < actualFemaleSeats) {
            seatType = 'female';
            femaleCount++;
          } 
          else if (maleCount < actualMaleSeats) {
            seatType = 'male';
            maleCount++;
          }
          else {
            seatType = 'general';
            generalCount++;
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
    
    console.log(`[Dialog Seat Layout] Final counts - Female: ${femaleCount}, Male: ${maleCount}, General: ${generalCount}`);
    
    return seats;
  }
}
