import { Component, input, output } from '@angular/core';

@Component({
  selector: 'app-ticket-selector',
  standalone: true,
  imports: [],
  template: `
    <ul class="grid grid-cols-6 sm:grid-cols-8 md:grid-cols-15 gap-2 ul-numeros">
      @for (ticket of tickets(); track ticket.number) {
        <li>
          <input 
            type="checkbox" 
            [id]="'numero-' + ticket.number" 
            [value]="ticket.number"
            [disabled]="ticket.isSold" 
            [checked]="selectedTickets().includes(ticket.number)"
            (change)="toggle.emit(ticket.number)" 
            class="peer hidden" 
          />
          <label 
            [for]="'numero-' + ticket.number" 
            [class.opacity-50]="ticket.isSold"
            [class.cursor-not-allowed]="ticket.isSold" 
            [class.cursor-pointer]="!ticket.isSold"
            class="flex items-center justify-center w-10 h-10 sm:w-11 sm:h-11 rounded-lg select-none text-sm font-semibold border-2 border-gray-200 dark:border-gray-800 text-gray-600 bg-white hover:bg-gray-50 dark:hover:bg-gray-900 dark:bg-gray-900 dark:peer-checked:border-verde-500 peer-checked:border-indigo-500 dark:peer-checked:bg-verde-800 peer-checked:bg-indigo-50 dark:peer-checked:text-verde-700 peer-checked:text-indigo-700 peer-disabled:opacity-50 peer-disabled:cursor-not-allowed transition"
          >
            {{ ticket.number }}
          </label>
        </li>
      }
    </ul>
  `,
  styles: [`
    .md\\:grid-cols-15 {
      grid-template-columns: repeat(15, minmax(0, 1fr));
    }
  `]
})
export class TicketSelectorComponent {
  tickets = input.required<{ number: number, isSold: boolean }[]>();
  selectedTickets = input.required<number[]>();
  toggle = output<number>();
}
