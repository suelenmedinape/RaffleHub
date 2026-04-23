import { Injectable, inject } from '@angular/core';
import { HubConnection, HubConnectionBuilder, HubConnectionState } from '@microsoft/signalr';
import { Subject } from 'rxjs';
import { environment } from '../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class SignalrService {
  private hubConnection: HubConnection | undefined;
  public paymentConfirmed$ = new Subject<{ numbers: string[], raffleName: string }>();

  public startConnection(bookingId: string) {
    if (this.hubConnection?.state === HubConnectionState.Connected) return;

    this.hubConnection = new HubConnectionBuilder()
      .withUrl(`${environment.apiUrl}/payment-hub`, {
        withCredentials: true
      })
      .withAutomaticReconnect()
      .build();

    this.hubConnection
      .start()
      .then(() => {
        this.hubConnection?.invoke('JoinPaymentGroup', bookingId);
      })
      .catch(err => console.error('SignalR: Erro ao conectar:', err));

    this.hubConnection.on('PaymentConfirmed', (data: any) => {
      const numbers = data.numbers || data.Numbers || [];
      const raffleName = data.raffleName || data.RaffleName || '';

      this.paymentConfirmed$.next({
        numbers: numbers.map((n: any) => n.toString()),
        raffleName: raffleName
      });
    });
  }

  public stopConnection(bookingId: string) {
    if (this.hubConnection?.state === HubConnectionState.Connected) {
      this.hubConnection.invoke('LeavePaymentGroup', bookingId)
        .then(() => this.hubConnection?.stop());
    }
  }
}
