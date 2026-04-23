import {Component, input} from '@angular/core';

@Component({
  selector: 'app-payment-confirmed',
  standalone: true,
  imports: [],
  templateUrl: './payment-confirmed-component.html',
  styleUrl: './payment-confirmed-component.css',
})
export class PaymentConfirmedComponent {
    raffleName = input<string>('');
    acquiredNumbers = input<string[]>([]);
}
