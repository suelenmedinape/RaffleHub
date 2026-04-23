import { HttpClient } from '@angular/common/http';
import { inject, Injectable, signal } from '@angular/core';
import { AuthCookieService } from './auth-cookie-service';
import { ApiRoutesService } from '../core/api-routes-service';
import { AuthData, LoginSchema, RegisterDto } from '../models/auth-model';
import { Observable, tap } from 'rxjs';
import { ApiResponseModel } from '../models/api-response-model';
import { jwtDecode } from 'jwt-decode';

@Injectable({
  providedIn: 'root',
})
export class AuthService {
  private readonly authCookieService = inject(AuthCookieService);
  private readonly httpClient = inject(HttpClient);
  private readonly apiUrl = inject(ApiRoutesService);

  public readonly isAuthenticated = signal<boolean>(this.authCookieService.hasToken());

  login(data: LoginSchema): Observable<ApiResponseModel<AuthData>> {
    return this.httpClient.post<ApiResponseModel<AuthData>>(this.apiUrl.authUrl.login, data)
    .pipe(
      tap((response: ApiResponseModel<AuthData>) => {
        this.authCookieService.setAuthData(response.data);
        this.isAuthenticated.set(true);
      })
    );
  }

  register(data: RegisterDto): Observable<ApiResponseModel<AuthData>> {
    return this.httpClient.post<ApiResponseModel<AuthData>>(this.apiUrl.authUrl.register, data)
    .pipe(
      tap((response: ApiResponseModel<AuthData>) => {
        this.authCookieService.setAuthData(response.data);
        this.isAuthenticated.set(true);
      })
    );
  }

  isValidToken(): boolean {
    const token = this.authCookieService.getToken();
    if (!token) return false;
    
    try {
      const decodedToken = jwtDecode(token);
      if (!decodedToken.exp) return false;
      const isExpired = decodedToken.exp * 1000 < Date.now();
      return !isExpired;
    } catch {
      return false;
    }
  }

  getDecodedToken(): any {
    const token = this.authCookieService.getToken();
    if (!token) return null;
    
    try {
      return jwtDecode(token);
    } catch {
      return null;
    }
  }

  hasRole(role: string): boolean {
    if (!this.isValidToken()) return false;

    // 1. Tenta pelo cookie de dados do usuário
    const userData = this.authCookieService.getUserData();
    if (userData && userData.roles) {
      const roles: any = userData.roles;
      if (Array.isArray(roles)) {
        if (roles.some((r: any) => String(r).toUpperCase() === role.toUpperCase())) return true;
      } else {
        if (String(roles).toUpperCase() === role.toUpperCase()) return true;
      }
    }

    // 2. Fallback para o token decodificado
    const decoded = this.getDecodedToken();
    if (!decoded) return false;

    // Diversas chaves possíveis para roles em diferentes provedores JWT
    const roles = decoded['http://schemas.microsoft.com/ws/2008/06/identity/claims/role']
               || decoded['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/role']
               || decoded['role']
               || decoded['roles'];

    if (Array.isArray(roles)) {
      return roles.some((r: any) => String(r).toUpperCase() === role.toUpperCase());
    }

    return roles && String(roles).toUpperCase() === role.toUpperCase();
  }

  hasAnyRole(roles: string[]): boolean {
    return roles.some(role => this.hasRole(role));
  }

  logout(): void {
    this.authCookieService.clearAuthData();
    this.isAuthenticated.set(false);
  }
}
