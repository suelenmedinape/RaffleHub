import { Component, input, computed } from '@angular/core';
import { StatusRaffle } from '../../enum/status-raffle';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-status',
  standalone: true,
  imports: [CommonModule],
  template: `
    <span
      class="inline-flex items-center gap-1 rounded-full px-2 py-1 text-xs font-medium"
      [ngClass]="statusClass()"
    >
      {{ statusLabel() }}
    </span>
  `,
})
export class StatusComponent {
  status = input.required<StatusRaffle>();

  statusLabel = computed(() => {
    switch (this.status()) {
      case StatusRaffle.ACTIVE: return 'Ativa';
      case StatusRaffle.COMPLETED: return 'Finalizada';
      case StatusRaffle.CANCELLED: return 'Cancelada';
      case StatusRaffle.EXPIRED: return 'Expirada';
      default: return 'Desconhecida';
    }
  });

  statusClass = computed(() => {
    switch (this.status()) {
      case StatusRaffle.ACTIVE: return 'bg-indigo-100 text-indigo-800 dark:bg-verde-900 dark:text-verde-300';
      case StatusRaffle.COMPLETED: return 'bg-green-100 text-green-800 dark:bg-green-900 dark:text-green-300';
      case StatusRaffle.CANCELLED: return 'bg-yellow-100 text-yellow-800 dark:bg-yellow-900 dark:text-yellow-300';
      case StatusRaffle.EXPIRED: return 'bg-blue-100 text-blue-800 dark:bg-blue-900 dark:text-blue-300';
      default: return 'bg-gray-100 text-gray-800 dark:bg-gray-800 dark:text-gray-300';
    }
  });
}
