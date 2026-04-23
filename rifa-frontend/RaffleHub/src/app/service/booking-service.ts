import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { ApiRoutesService } from '../core/api-routes-service';
import { Observable } from 'rxjs';
import { ApiResponseModel } from '../models/api-response-model';
import { PaymentData } from '../participant/models/booking-model';
import { AuthCookieService } from './auth-cookie-service';

@Injectable({
  providedIn: 'root',
})
export class BookingService {
  private readonly url = inject(ApiRoutesService);
  private readonly http = inject(HttpClient);

  getBookingPayment(id: string): Observable<ApiResponseModel<PaymentData>> {
    return this.http.post<ApiResponseModel<PaymentData>>(`${this.url.bookingUrl.generatePix}/${id}`, {});
  }

  getMyBookings(): Observable<ApiResponseModel<any[]>> {
    return this.http.get<ApiResponseModel<any[]>>(this.url.bookingUrl.myBookings);
  }
}
