import { ComponentFixture, TestBed } from '@angular/core/testing';
import { signal } from '@angular/core';
import { PaymentPageComponent } from './payment-page-component';
import { BookingService } from '../../../service/booking-service';
import { AlertService } from '../../../core/alert-service';
import { SignalrService } from '../../../core/signalr-service';
import { LoadingService } from '../../../core/loading-service';
import { of, throwError, Subject } from 'rxjs';

describe('PaymentPageComponent', () => {
  let component: PaymentPageComponent;
  let fixture: ComponentFixture<PaymentPageComponent>;
  let mockBookingService: any;
  let mockAlertService: any;
  let mockSignalrService: any;
  let mockLoadingService: any;
  let paymentConfirmedSubject: Subject<any>;

  beforeEach(async () => {
    paymentConfirmedSubject = new Subject<any>();

    mockBookingService = {
      getBookingPayment: vi.fn()
    };

    mockAlertService = {
      errorToast: vi.fn()
    };

    mockSignalrService = {
      startConnection: vi.fn(),
      stopConnection: vi.fn(),
      paymentConfirmed$: paymentConfirmedSubject.asObservable()
    };

    mockLoadingService = {
      isLoading: signal(false)
    };

    await TestBed.configureTestingModule({
      imports: [PaymentPageComponent],
      providers: [
        { provide: BookingService, useValue: mockBookingService },
        { provide: AlertService, useValue: mockAlertService },
        { provide: SignalrService, useValue: mockSignalrService },
        { provide: LoadingService, useValue: mockLoadingService }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(PaymentPageComponent);
    component = fixture.componentInstance;
    fixture.componentRef.setInput('participantId', '1234');
  });

  it('should create', () => {
    mockBookingService.getBookingPayment.mockReturnValue(of({ data: { status: 'PENDING', id: '123' } }));
    fixture.detectChanges();
    expect(component).toBeTruthy();
  });

  it('should set isPaymentConfirmed to true and populate data if booking is already PAID', () => {
    const mockPaidData = {
      status: 'PAID',
      raffleName: 'Rifa Super Carro',
      tickets: [{ ticketNumber: 42 }, { ticketNumber: 88 }]
    };
    mockBookingService.getBookingPayment.mockReturnValue(of({ data: mockPaidData }));

    fixture.detectChanges(); // Triggers ngOnInit -> getBooking()

    expect(component['isPaymentConfirmed']()).toBe(true);
    expect(component['raffleName']()).toBe('Rifa Super Carro');
    expect(component['acquiredNumbers']()).toEqual(['42', '88']);
    expect(mockSignalrService.startConnection).not.toHaveBeenCalled();
  });

  it('should connect to SignalR if booking status is PENDING', () => {
    const mockPendingData = {
      id: 'booking-abc',
      status: 'PENDING',
      raffleName: 'Rifa Super Carro',
      tickets: []
    };
    mockBookingService.getBookingPayment.mockReturnValue(of({ data: mockPendingData }));

    fixture.detectChanges();

    expect(component['isPaymentConfirmed']()).toBe(false);
    expect(mockSignalrService.startConnection).toHaveBeenCalledWith('booking-abc');
  });

  it('should update state when SignalR confirms payment', () => {
    const mockPendingData = {
      id: 'booking-abc',
      status: 'PENDING',
      raffleName: 'Rifa Super Carro',
      tickets: []
    };
    mockBookingService.getBookingPayment.mockReturnValue(of({ data: mockPendingData }));

    fixture.detectChanges();

    // Simular que o SignalR enviou confirmação de pagamento
    paymentConfirmedSubject.next({
      numbers: ['10', '20'],
      raffleName: 'Rifa Premiada'
    });

    expect(component['isPaymentConfirmed']()).toBe(true);
    expect(component['acquiredNumbers']()).toEqual(['10', '20']);
    expect(component['raffleName']()).toBe('Rifa Premiada');
  });

  it('should show error toast if loading booking fails', () => {
    const mockError = { message: 'Erro de conexão no servidor' };
    mockBookingService.getBookingPayment.mockReturnValue(throwError(() => mockError));

    fixture.detectChanges();

    expect(mockAlertService.errorToast).toHaveBeenCalledWith('Erro de conexão no servidor');
  });

  it('should stop SignalR connection on component destruction', () => {
    const mockPendingData = {
      id: 'booking-xyz',
      status: 'PENDING',
      raffleName: 'Rifa Show',
      tickets: []
    };
    mockBookingService.getBookingPayment.mockReturnValue(of({ data: mockPendingData }));

    fixture.detectChanges();
    fixture.destroy();

    expect(mockSignalrService.stopConnection).toHaveBeenCalledWith('booking-xyz');
  });
});
