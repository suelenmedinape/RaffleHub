import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { ApiRoutesService } from '../core/api-routes-service';
import { ApiResponseModel } from '../models/api-response-model';

@Injectable({
  providedIn: 'root',
})
export class DashboardService {
  private readonly http = inject(HttpClient);
  private readonly url = inject(ApiRoutesService);

  getGeneralStats(): Observable<ApiResponseModel> {
    return this.http.get<ApiResponseModel>(this.url.dashboardUrl.stats);
  }
}
