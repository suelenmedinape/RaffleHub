import {inject, Injectable} from '@angular/core';
import {ApiRoutesService} from "../core/api-routes-service";
import {HttpClient} from "@angular/common/http";
import {Observable} from "rxjs";
import {ApiResponseModel} from "../models/api-response-model";
import {CreateParticipantDto} from "../models/participant-model";

@Injectable({
  providedIn: 'root',
})
export class ParticipantService {
    private readonly url = inject(ApiRoutesService);
    private readonly http = inject(HttpClient);

    createParticipant(data: CreateParticipantDto): Observable<ApiResponseModel> {
        return this.http.post<ApiResponseModel>(this.url.participantUrl.base, data);
    }

    getParticipantById(id: string): Observable<ApiResponseModel> {
        return this.http.get<ApiResponseModel>(`${this.url.participantUrl.base}/${id}`);
    }

    listByRaffle(raffleId: string, page: number = 1, pageSize: number = 50, search: string = ''): Observable<ApiResponseModel> {
        const url = `${this.url.participantUrl.raffle}/${raffleId}`;
        const params = { page: page.toString(), pageSize: pageSize.toString(), searchTerm: search };
        return this.http.get<ApiResponseModel>(url, { params });
    }
}
