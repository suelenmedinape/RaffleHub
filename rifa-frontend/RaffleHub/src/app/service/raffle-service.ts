import {HttpClient} from '@angular/common/http';
import {inject, Injectable} from '@angular/core';
import {ApiRoutesService} from '../core/api-routes-service';
import {Observable} from 'rxjs';
import {CreateRaffle, CreateRaffleDto, ListAllRaffleDto, ListByIdRaffleDto, RaffleData, RaffleModel} from '../models/raffle-model';
import {ApiResponseModel} from '../models/api-response-model';
import {StatusRaffle} from '../enum/status-raffle';

@Injectable({
    providedIn: 'root',
})
export class RaffleService {
    private readonly url = inject(ApiRoutesService);
    private readonly http = inject(HttpClient);

    listAllRaffles(): Observable<ApiResponseModel<ListAllRaffleDto[]>> {
        const finalUrl = this.url.buildUrl(this.url.raffleUrl.base);
        return this.http.get<ApiResponseModel<ListAllRaffleDto[]>>(finalUrl);
    }

    listById(id: string): Observable<ApiResponseModel<ListByIdRaffleDto>> {
        return this.http.get<ApiResponseModel<ListByIdRaffleDto>>(`${this.url.raffleUrl.base}/${id}`)
    }

    createRaffle(dto: CreateRaffleDto): Observable<ApiResponseModel<RaffleData>> {
        const finalUrl = this.url.buildUrl(this.url.raffleUrl.base);

        const formData = new FormData();

        if (dto.file) {
            formData.append('file', dto.file);
        }

        formData.append('RaffleName', dto.raffleName);
        formData.append('Description', dto.description);
        formData.append('TotalTickets', dto.totalTickets.toString());
        formData.append('TicketPrice', dto.ticketPrice.toString());
        formData.append('DrawDate', dto.drawDate);

        return this.http.post<ApiResponseModel<RaffleData>>(finalUrl, formData);
    }

    updateRaffle(id: string, raffle: Partial<CreateRaffle>): Observable<ApiResponseModel<void>> {
        const finalUrl = `${this.url.raffleUrl.base}/${id}`;
        return this.http.put<ApiResponseModel<void>>(finalUrl, raffle);
    }

    updateStatus(id: string, status: StatusRaffle): Observable<ApiResponseModel<void>> {
        return this.http.patch<ApiResponseModel<void>>(`${this.url.raffleUrl.changeStatus}/${id}`, {status});
    }
}
