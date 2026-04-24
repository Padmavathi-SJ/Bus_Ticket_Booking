import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatTableModule } from '@angular/material/table';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatCardModule } from '@angular/material/card';
import { MatInputModule } from '@angular/material/input';
import { AdminService, Station } from '../../../core/services/admin.service';

@Component({
  selector: 'app-admin-stations',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, MatTableModule, MatButtonModule, MatIconModule, MatCardModule, MatInputModule],
  templateUrl: './stations.html',
  styleUrl: './stations.scss'
})
export class StationManagement implements OnInit {
  stations: Station[] = [];
  stationForm: FormGroup;
  displayedColumns: string[] = ['name', 'city', 'state', 'code', 'status'];
  isLoading = false;
  showAddForm = false;

  constructor(
    private adminService: AdminService,
    private fb: FormBuilder
  ) {
    this.stationForm = this.fb.group({
      name: ['', Validators.required],
      city: ['', Validators.required],
      state: ['', Validators.required],
      code: ['', Validators.required]
    });
  }

  ngOnInit(): void {
    this.loadStations();
  }

  loadStations() {
    this.isLoading = true;
    this.adminService.getStations().subscribe({
      next: (data) => {
        this.stations = data;
        this.isLoading = false;
      },
      error: () => this.isLoading = false
    });
  }

  onSubmit() {
    if (this.stationForm.valid) {
      this.adminService.createStation(this.stationForm.value).subscribe({
        next: () => {
          this.loadStations();
          this.showAddForm = false;
          this.stationForm.reset();
          alert('Station added successfully!');
        },
        error: (err) => alert('Error adding station: ' + err.error.message)
      });
    }
  }
}
