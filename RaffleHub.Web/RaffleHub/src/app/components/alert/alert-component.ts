import {ChangeDetectorRef, Component, NgZone, OnInit} from '@angular/core';
import {AlertModel} from '../../models/alert-model';
import {AlertService} from '../../core/alert-service';

@Component({
  selector: 'app-alert-component',
  imports: [],
  standalone: true,
  templateUrl: './alert-component.html',
  styleUrl: './alert-component.css',
})
export class AlertComponent implements OnInit {

    toasts: AlertModel[] = [];
    modalAlert: AlertModel | null = null;

    constructor(private alertService: AlertService, private cdr: ChangeDetectorRef) {}

    ngOnInit() {
        this.alertService.alert$.subscribe(alert => {
            setTimeout(() => {
                if (alert.mode === 'toast') {
                    this.toasts = [...this.toasts, alert];

                    setTimeout(() => {
                        this.removeToast(alert);
                        this.cdr.markForCheck();
                    }, alert.duration ?? 3000);

                } else {
                    this.modalAlert = alert;
                }
                this.cdr.detectChanges();
            });
        });
    }

    removeToast(alert: AlertModel) {
        this.toasts = this.toasts.filter(t => t !== alert);
    }

    closeModal() {
        this.modalAlert = null;
    }
}
