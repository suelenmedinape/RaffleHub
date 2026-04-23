import { Component, inject, OnInit, signal } from '@angular/core';
import { CommonModule, CurrencyPipe, DatePipe } from '@angular/common';
import { RouterModule } from '@angular/router';
import { BookingService } from '../../../service/booking-service';
import { MyBookingsDto } from '../../models/booking-model';
import { LoadingComponent } from '../../../components/loading/loading-component';

import { LoadingService } from '../../../core/loading-service';

@Component({
  selector: 'app-my-bookings',
  standalone: true,
  imports: [CommonModule, RouterModule, CurrencyPipe, DatePipe, LoadingComponent],
  templateUrl: './my-bookings.component.html',
  styleUrl: './my-bookings.component.css'
})
export class MyBookingsComponent implements OnInit {
  private readonly bookingService = inject(BookingService);
  private readonly loadingService = inject(LoadingService);

  protected bookings = signal<MyBookingsDto[]>([]);
  protected isLoading = this.loadingService.isLoading;

  ngOnInit() {
    this.loadBookings();
  }

  loadBookings() {
    this.bookingService.getMyBookings().subscribe({
      next: (response) => {
        this.bookings.set(response.data);
      },
      error: () => {
        // error handled by global interceptor/alert if any
      }
    });
  }
}
