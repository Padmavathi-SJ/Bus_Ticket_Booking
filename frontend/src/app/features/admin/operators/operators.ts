import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatTableModule } from '@angular/material/table';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatCardModule } from '@angular/material/card';
import { MatChipsModule } from '@angular/material/chips';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatTabsModule } from '@angular/material/tabs';
import { MatMenuModule } from '@angular/material/menu';
import { AdminService, OperatorRequest } from '../../../core/services/admin.service';

@Component({
  selector: 'app-operators',
  standalone: true,
  imports: [CommonModule, MatTableModule, MatButtonModule, MatIconModule, MatCardModule, MatChipsModule, MatTooltipModule, MatTabsModule, MatMenuModule],
  templateUrl: './operators.html',
  styleUrl: './operators.scss'
})
export class OperatorManagement implements OnInit {
  operators: OperatorRequest[] = [];
  displayedColumns: string[] = ['fullName', 'companyName', 'licenseNumber', 'createdAt', 'actions'];
  isLoading = false;

  constructor(
    private adminService: AdminService,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    setTimeout(() => {
      this.loadOperators(1); // Default to Pending (Requests)
    });
  }

  onTabChange(index: number) {
    if (index === 0) {
      this.displayedColumns = ['fullName', 'companyName', 'licenseNumber', 'createdAt', 'actions'];
      this.loadOperators(1); // Requests (Pending)
    } else if (index === 1) {
      this.displayedColumns = ['fullName', 'companyName', 'licenseNumber', 'phone', 'status', 'actions'];
      this.loadOperators(2); // Available (Approved)
    } else {
      this.displayedColumns = ['fullName', 'companyName', 'status', 'createdAt', 'actions'];
      this.loadOperators(); // All
    }
  }

  loadOperators(status?: number) {
    this.isLoading = true;
    this.adminService.getOperators(status).subscribe({
      next: (data) => {
        this.operators = data;
        this.isLoading = false;
        this.cdr.detectChanges();
      },
      error: (err) => {
        console.error('Error fetching operators', err);
        this.isLoading = false;
      }
    });
  }

  onApprove(id: string) {
    if (confirm('Are you sure you want to approve this operator?')) {
      this.adminService.approveOperator(id).subscribe({
        next: () => {
          this.loadOperators(1);
          alert('Operator approved successfully!');
        },
        error: (err) => alert('Approval failed: ' + err.error.message)
      });
    }
  }

  onReject(id: string) {
    const reason = prompt('Please enter the reason for rejection:');
    if (reason) {
      this.adminService.rejectOperator(id, reason).subscribe({
        next: () => {
          this.loadOperators(1);
          alert('Operator rejected.');
        },
        error: (err) => alert('Rejection failed: ' + err.error.message)
      });
    }
  }

  onEnable(id: string) {
    if (confirm('Are you sure you want to enable this operator? All their buses will become available for booking.')) {
      this.adminService.enableOperator(id).subscribe({
        next: () => {
          this.loadOperators();
          alert('Operator enabled successfully! All buses are now available.');
        },
        error: (err) => alert('Enable failed: ' + err.error?.message || err.message)
      });
    }
  }

  onDisable(id: string) {
    if (confirm('Are you sure you want to disable this operator? All their buses will become unavailable and active bookings will be cancelled with refunds.')) {
      this.adminService.disableOperator(id).subscribe({
        next: () => {
          this.loadOperators();
          alert('Operator disabled successfully! All buses are now unavailable and customers have been notified.');
        },
        error: (err) => alert('Disable failed: ' + err.error?.message || err.message)
      });
    }
  }

  getStatusLabel(status: number): string {
    switch(status) {
      case 1: return 'Pending';
      case 2: return 'Approved';
      case 3: return 'Disabled';
      case 4: return 'Rejected';
      default: return 'Unknown';
    }
  }
}
