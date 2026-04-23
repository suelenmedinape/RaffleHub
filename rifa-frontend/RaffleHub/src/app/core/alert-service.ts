import { Injectable } from '@angular/core';
import {Subject} from 'rxjs';
import {AlertModel, NotificationType} from '../models/alert-model';

@Injectable({
  providedIn: 'root',
})
export class AlertService {
  private alertSubject = new Subject<AlertModel>();
  alert$ = this.alertSubject.asObservable();

  showToast(type: NotificationType, message: string, duration: number = 3000) {
    this.alertSubject.next({
      type,
      message,
      mode: 'toast',
      duration
    });
  }

  successToast(message: string, duration?: number) {
    this.showToast('success', message, duration);
  }

  errorToast(message: string, duration?: number) {
    this.showToast('error', message, duration);
  }

  showModal(type: NotificationType, message: string) {
    this.alertSubject.next({
      type,
      message,
      mode: 'modal'
    });
  }

  successModal(message: string) {
    this.showModal('success', message);
  }

  errorModal(message: string) {
    this.showModal('error', message);
  }
}
