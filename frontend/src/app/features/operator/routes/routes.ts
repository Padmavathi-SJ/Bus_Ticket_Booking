import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatTableModule } from '@angular/material/table';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatCardModule } from '@angular/material/card';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatChipsModule } from '@angular/material/chips';
import { MatExpansionModule } from '@angular/material/expansion';
import { MatTooltipModule } from '@angular/material/tooltip';
import { OperatorService, Route } from '../../../core/services/operator.service';

@Component({
  selector: 'app-operator-routes',
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
    MatChipsModule,
    MatExpansionModule,
    MatTooltipModule
  ],
  templateUrl: './routes.html',
  styleUrl: './routes.scss'
})
export class OperatorRouteManagement implements OnInit {
  routes: Route[] = [];
  busForm: FormGroup;
  isLoading = false;
  seatPreview: any[] = [];

  constructor(
    private operatorService: OperatorService,
    private fb: FormBuilder,
    private cdr: ChangeDetectorRef
  ) {
    this.busForm = this.fb.group({
      busName: ['', Validators.required],
      busNumber: ['', Validators.required],
      busType: ['AC Sleeper', Validators.required],
      totalSeats: [36, [Validators.required, Validators.min(1)]],
      basePrice: [500, [Validators.required, Validators.min(0)]],
      amenities: ['Wifi, Water, Charging Point'],
      femaleSeats: [6, [Validators.required, Validators.min(0)]],
      maleSeats: [0, [Validators.required, Validators.min(0)]]
    });

    // Watch for changes to update preview
    this.busForm.valueChanges.subscribe(() => this.generateLayoutPreview());
  }

  ngOnInit(): void {
    setTimeout(() => {
      this.loadRoutes();
      this.generateLayoutPreview();
    });
  }

  generateLayoutPreview() {
    const { totalSeats, femaleSeats, maleSeats } = this.busForm.value;
    const preview = [];
    let seatNum = 1;
    
    // Standard bus has 4 seats per row (2+2) + 1 aisle in the middle = 5 columns
    const cols = 5; 
    const rows = Math.ceil(totalSeats / 4);

    for (let r = 0; r < rows; r++) {
      const row = [];
      for (let c = 0; c < cols; c++) {
        if (c === 2) { // Middle Column is the Aisle
          row.push({ type: 'aisle' });
          continue;
        }

        if (seatNum <= totalSeats) {
          let type = 'general';
          if (seatNum <= femaleSeats) type = 'female';
          else if (seatNum <= (femaleSeats + maleSeats)) type = 'male';

          row.push({ label: seatNum.toString(), type: type });
          seatNum++;
        } else {
          row.push(null);
        }
      }
      preview.push(row);
    }
    this.seatPreview = preview;
    this.cdr.detectChanges();
  }

  loadRoutes() {
    this.isLoading = true;
    this.operatorService.getRoutes().subscribe({
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

  onAddBus(routeId: string) {
    if (this.busForm.valid) {
      const busData = {
        ...this.busForm.value,
        routeId: routeId
      };
      
      this.operatorService.addBus(busData).subscribe({
        next: () => {
          this.busForm.reset({ busType: 'AC Sleeper', totalSeats: 36, basePrice: 500 });
          alert('Bus registration request sent to Admin for approval!');
        },
        error: (err) => alert('Error: ' + (err.error?.message || 'Failed to send request'))
      });
    }
  }
}
