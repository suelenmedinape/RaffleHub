import {inject, Injectable} from '@angular/core';
import {ApiRoutesService} from "../core/api-routes-service";
import {HttpClient} from "@angular/common/http";
import {Observable} from "rxjs";
import {ApiResponseModel} from "../models/api-response-model";

@Injectable({
  providedIn: 'root',
})
export class TicketService {
    private readonly url = inject(ApiRoutesService);
    private readonly http = inject(HttpClient);

    listTicketsSold(id: string): Observable<ApiResponseModel> {
        return this.http.get<ApiResponseModel>(`${this.url.ticketUrl.base}/${id}`);
    }
}
