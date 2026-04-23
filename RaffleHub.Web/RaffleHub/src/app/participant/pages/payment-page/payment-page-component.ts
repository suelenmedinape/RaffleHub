import { Component, inject, input, OnInit, OnDestroy, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { PaymentData } from '../../models/booking-model';
import { Router } from '@angular/router';
import { AlertService } from '../../../core/alert-service';
import { ErrorHandleService } from '../../../core/error-handle-service';
import { ParticipantService } from '../../../service/participant-service';
import { RaffleService } from '../../../service/raffle-service';
import { TicketService } from '../../../service/ticket-service';
import { BookingService } from '../../../service/booking-service';
import { LoadingComponent } from '../../../components/loading/loading-component';
import { SignalrService } from '../../../core/signalr-service';
import { Subscription } from 'rxjs';
import {PaymentConfirmedComponent} from "../../components/payment-confirmed/payment-confirmed-component";
import {OrderSummaryComponent} from "../../components/order-summary/order-summary-component";


import { LoadingService } from '../../../core/loading-service';

@Component({
  selector: 'app-payment-page',
  standalone: true,
    imports: [CommonModule, PaymentConfirmedComponent, OrderSummaryComponent],
  templateUrl: './payment-page-component.html',
  styleUrl: './payment-page-component.css',
})
export class PaymentPageComponent implements OnInit, OnDestroy {
  public booking: PaymentData | undefined;
  
  protected isPaymentConfirmed = signal<boolean>(false);
  protected acquiredNumbers = signal<string[]>([]);
  protected raffleName = signal<string>('');
  
  private readonly bookingService = inject(BookingService);
  private readonly alertComponent = inject(AlertService);
  private readonly signalrService = inject(SignalrService);
  private readonly loadingService = inject(LoadingService);

  protected isLoading = this.loadingService.isLoading;
  
  private signalRSubscription: Subscription | undefined;

  readonly participantId = input.required<string>({ alias: 'participantId' });

  ngOnInit(): void {
    this.getBooking();
  }

  getBooking() {
    this.bookingService.getBookingPayment(this.participantId()).subscribe({
      next: (response: any) => {
        this.booking = response.data;
        
        if (this.booking?.status === 'PAID' || this.booking?.status === 1) {
          this.isPaymentConfirmed.set(true);
          this.acquiredNumbers.set(this.booking.tickets.map((t: any) => t.ticketNumber.toString()));
          this.raffleName.set(this.booking.raffleName);
        } else if (this.booking?.id) {
          this.signalrService.startConnection(this.booking.id);
          this.signalRSubscription = this.signalrService.paymentConfirmed$.subscribe(
            (event: any) => {
               console.info('⚡ SignalR: Pagamento Recebido!', event);
               this.isPaymentConfirmed.set(true);
               this.acquiredNumbers.set(event.numbers);
               this.raffleName.set(event.raffleName);
            }
          );
        }
      },
      error: (error: any) => {
        this.alertComponent.errorToast(error.message);
      }
    });
  }



  ngOnDestroy(): void {
    if (this.signalRSubscription) {
      this.signalRSubscription.unsubscribe();
    }
    if (this.booking?.id) {
      this.signalrService.stopConnection(this.booking.id);
    }
  }
}
