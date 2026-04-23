import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { ApiRoutesService } from '../core/api-routes-service';
import { Observable } from 'rxjs';
import { ApiResponseModel } from '../models/api-response-model';
import { CategoryGalleryModel } from '../models/category-gallery-model';

@Injectable({
  providedIn: 'root',
})
export class CategoryGalleryService {
  private readonly url = inject(ApiRoutesService);
  private readonly http = inject(HttpClient);

  getCategoryById(id: string): Observable<ApiResponseModel<CategoryGalleryModel>> {
    const finalUrl = `${this.url.categoriesGalleryUrl.base}/${id}`;
    return this.http.get<ApiResponseModel<CategoryGalleryModel>>(finalUrl);
  }
}
