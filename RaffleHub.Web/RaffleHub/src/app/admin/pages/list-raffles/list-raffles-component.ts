import { Component, inject, OnInit, signal, computed, ChangeDetectionStrategy, ElementRef, viewChild } from '@angular/core';
import { RaffleService } from '../../../service/raffle-service';
import { AlertService } from '../../../core/alert-service';
import { ErrorHandleService } from '../../../core/error-handle-service';
import { ListAllRaffleDto, RaffleSchema } from '../../../models/raffle-model';
import { StatusRaffle } from '../../../enum/status-raffle';
import { CommonModule, CurrencyPipe } from '@angular/common';
import { FormHelper } from '../../../core/form-helper';
import { FormsModule } from '@angular/forms';
import { finalize } from 'rxjs';

import { LoadingService } from '../../../core/loading-service';
import {RouterLink} from "@angular/router";

@Component({
  selector: 'app-list-raffles',
  standalone: true,
    imports: [CommonModule, CurrencyPipe, FormsModule, RouterLink],
  templateUrl: './list-raffles-component.html',
  styleUrl: './list-raffles-component.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ListRafflesComponent implements OnInit {
  private readonly raffleService = inject(RaffleService);
  private readonly alertService = inject(AlertService);
  private readonly errorHandle = inject(ErrorHandleService);
  private readonly loadingService = inject(LoadingService);

  protected raffles = signal<ListAllRaffleDto[]>([]);
  protected isLoading = this.loadingService.isLoading;
  protected StatusRaffle = StatusRaffle;

  protected totalAcquired = computed(() => {
    return this.raffles().reduce((acc, raffle) => acc + (raffle.soldTicketsCount * raffle.ticketPrice), 0);
  });

  ngOnInit(): void {
    this.loadRaffles();
  }

  loadRaffles(): void {
    this.raffleService.listAllRaffles()
      .subscribe({
        next: (response) => {
          if (response.data) {
            this.raffles.set(response.data);
          }
        },
        error: (error) => {
          const message = this.errorHandle.getErrorMessage(error);
          this.alertService.errorModal(message);
        }
      });
  }

  changeStatus(id: string, newStatus: StatusRaffle): void {
    this.raffleService.updateStatus(id, newStatus)
      .subscribe({
        next: (response) => {
          this.alertService.successToast(response.message || 'Status atualizado com sucesso');
          this.loadRaffles();
        },
        error: (error) => {
          const message = this.errorHandle.getErrorMessage(error);
          this.alertService.errorModal(message);
        }
      });
  }

  getStatusBadgeClass(status: StatusRaffle): string {
    switch (status) {
      case StatusRaffle.ACTIVE: return 'bg-green-100 text-green-800 dark:bg-green-900 dark:text-green-300';
      case StatusRaffle.COMPLETED: return 'bg-blue-100 text-blue-800 dark:bg-blue-900 dark:text-blue-300';
      case StatusRaffle.CANCELLED: return 'bg-red-100 text-red-800 dark:bg-red-900 dark:text-red-300';
      case StatusRaffle.EXPIRED: return 'bg-gray-100 text-gray-800 dark:bg-gray-900 dark:text-gray-300';
      default: return 'bg-gray-100 text-gray-800';
    }
  }

  getStatusText(status: StatusRaffle): string {
    switch (status) {
      case StatusRaffle.ACTIVE: return 'Ativa';
      case StatusRaffle.COMPLETED: return 'Concluída';
      case StatusRaffle.CANCELLED: return 'Cancelada';
      case StatusRaffle.EXPIRED: return 'Expirada';
      default: return 'Desconhecido';
    }
  }
}
