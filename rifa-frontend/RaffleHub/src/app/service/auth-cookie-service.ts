import { Injectable, inject } from '@angular/core';
import { CookieService } from "ngx-cookie-service";
import { AuthData } from '../models/auth-model';
import { jwtDecode } from 'jwt-decode';

@Injectable({
  providedIn: 'root',
})
export class AuthCookieService {
  private readonly cookieService = inject(CookieService);
  
  private readonly TOKEN_KEY = 'RAFFLE_HUB_TOKEN';
  private readonly REFRESH_TOKEN_KEY = 'RAFFLE_HUB_REFRESH_TOKEN';
  private readonly USER_DATA_KEY = 'RAFFLE_HUB_USER';
  private readonly PARTICIPANT_ID_KEY = 'RAFFLE_HUB_PARTICIPANT_ID';

  setAuthData(data: AuthData): void {
    let tokenExpires: Date;
    
    try {
      tokenExpires = new Date(data.expiration);
      if (isNaN(tokenExpires.getTime())) {
        throw new Error('Invalid expiration date');
      }
    } catch {
      // Fallback: Tenta extrair do JWT se o campo expiration falhar
      try {
        const decoded: any = jwtDecode(data.token);
        tokenExpires = new Date(decoded.exp * 1000);
      } catch {
        // Fallback final: 1 dia
        tokenExpires = new Date();
        tokenExpires.setDate(tokenExpires.getDate() + 1);
      }
    }

    const refreshTokenExpires = 7; 

    this.cookieService.set(this.TOKEN_KEY, data.token, { expires: tokenExpires, path: '/' });
    this.cookieService.set(this.REFRESH_TOKEN_KEY, data.refreshToken, { expires: refreshTokenExpires, path: '/' });
    
    const userData = { phone: data.phone, fullName: data.fullName, roles: data.roles };
    this.cookieService.set(this.USER_DATA_KEY, JSON.stringify(userData), { expires: tokenExpires, path: '/' });
  }

  setParticipantId(participantId: string): void {
    this.cookieService.set(this.PARTICIPANT_ID_KEY, participantId, { expires: 7, path: '/' });
  }

  getParticipantId(): string | null {
    return this.cookieService.get(this.PARTICIPANT_ID_KEY) || null;
  }

  getToken(): string {
    return this.cookieService.get(this.TOKEN_KEY);
  }

  getRefreshToken(): string {
    return this.cookieService.get(this.REFRESH_TOKEN_KEY);
  }

  getUserData(): { phone: string; fullName: string; roles: string[] } | null {
    const userDataStr = this.cookieService.get(this.USER_DATA_KEY);
    if (!userDataStr) return null;
    
    try {
      return JSON.parse(userDataStr);
    } catch {
      return null;
    }
  }

  clearAuthData(): void {
    this.cookieService.delete(this.TOKEN_KEY, '/');
    this.cookieService.delete(this.REFRESH_TOKEN_KEY, '/');
    this.cookieService.delete(this.USER_DATA_KEY, '/');
    this.cookieService.delete(this.PARTICIPANT_ID_KEY, '/');
  }

  hasToken(): boolean {
    return this.cookieService.check(this.TOKEN_KEY);
  }
}
