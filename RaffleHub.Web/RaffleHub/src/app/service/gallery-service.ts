import { inject, Injectable } from '@angular/core';
import { ApiRoutesService } from '../core/api-routes-service';
import { HttpClient, HttpParams } from '@angular/common/http';
import {Observable} from "rxjs";
import {ApiResponseModel} from "../models/api-response-model";
import {CreateGalleryDto, ListAllGalleryDto, GalleryModel} from "../models/gallery-model";

@Injectable({
  providedIn: 'root',
})
export class GalleryService {
  
  private readonly url = inject(ApiRoutesService);
  private readonly http = inject(HttpClient);

  getGallery(page: number = 1, pageSize: number = 50): Observable<ApiResponseModel<ListAllGalleryDto>> {
      const finalUrl = this.url.buildUrl(this.url.galleryUrl.base, {page, pageSize});
      return this.http.get<ApiResponseModel<ListAllGalleryDto>>(finalUrl);
  }

  getGalleryById(id: string): Observable<ApiResponseModel<ListAllGalleryDto>> {
      const finalUrl = `${this.url.galleryUrl.base}/${id}`;
      return this.http.get<ApiResponseModel<ListAllGalleryDto>>(finalUrl);
  }

  getGalleryByYears(years: number[]): Observable<ApiResponseModel<ListAllGalleryDto>> {
      let params = new HttpParams();
      years.forEach(year => {
          params = params.append('years', year.toString());
      });
      return this.http.get<ApiResponseModel<ListAllGalleryDto>>(this.url.galleryUrl.byYears, { params });
  }
}
