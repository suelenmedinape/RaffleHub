import { Component, inject, OnInit, signal } from '@angular/core';
import { RaffleService } from '../../service/raffle-service';
import { ListAllRaffleDto } from '../../models/raffle-model';
import { AlertService } from '../../core/alert-service';
import { HttpErrorResponse } from '@angular/common/http';
import { Router } from '@angular/router';
import { CurrencyPipe } from '@angular/common';
import { LoadingComponent } from '../../components/loading/loading-component';
import { StatusComponent } from '../../components/status/status-component';

import { LoadingService } from '../../core/loading-service';

@Component({
  selector: 'app-raffle-list',
  imports: [CurrencyPipe, StatusComponent, LoadingComponent],
  templateUrl: './raffle-list-component.html',
  styleUrl: './raffle-list-component.css',
})
export class RaffleListComponent implements OnInit{

  private readonly raffleService = inject(RaffleService);
  private readonly alertComponent = inject(AlertService);
  private readonly route = inject(Router);
  private readonly loadingService = inject(LoadingService);

  public raffles: ListAllRaffleDto[] = [];

  protected isLoading = this.loadingService.isLoading;
  

   ngOnInit() {
        this.getAllRaffles();
    }

    soldBar(totalSold: number, total: number): number {
        return (totalSold / total) * 100;
    }

    viewMore(id: string) {
        this.route.navigate([`/raffle/${id}`]);
    }

    getAllRaffles() {

        this.raffleService.listAllRaffles()
            .subscribe({
                next: (response) => {
                    this.raffles = response.data.map(raffle => ({
                        ...raffle,
                        soldPercentage: this.soldBar(raffle.soldTicketsCount, raffle.totalTickets)
                    }));
                    console.log(this.raffles)
                },
                error: (error: HttpErrorResponse) => {
                    this.alertComponent.errorToast(error.error.message);
                }
            })
    }
}
