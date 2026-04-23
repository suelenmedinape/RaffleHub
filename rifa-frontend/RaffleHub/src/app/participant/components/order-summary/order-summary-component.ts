import {Component, input, signal} from '@angular/core';
import {CurrencyPipe, DatePipe} from "@angular/common";
import {PaymentData} from "../../models/booking-model";

@Component({
  selector: 'app-order-summary',
    imports: [
        CurrencyPipe,
        DatePipe
    ],
  templateUrl: './order-summary-component.html',
  styleUrl: './order-summary-component.css',
})
export class OrderSummaryComponent {
    booking = input<any>();

    protected isCopied = signal<boolean>(false);

    copyPix() {
        const data = this.booking();
        if (data) {
            navigator.clipboard.writeText(data.pixCopyPaste);
            this.isCopied.set(true);
            setTimeout(() => this.isCopied.set(false), 2000);
        }
    }
}
