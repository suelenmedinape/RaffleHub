import { CanActivateFn, Router } from '@angular/router';
import { inject } from '@angular/core';
import { AuthService } from '../service/auth-service';
import { AuthCookieService } from '../service/auth-cookie-service';

export const paymentGuard: CanActivateFn = (route, state) => {
  const authService = inject(AuthService);
  const cookieService = inject(AuthCookieService);
  const router = inject(Router);

  if (authService.isValidToken()) {
    return true;
  }
  const routeParticipantId = route.paramMap.get('participantId');
  const cookieParticipantId = cookieService.getParticipantId();

  if (routeParticipantId && cookieParticipantId === routeParticipantId) {
    return true;
  }

  router.navigate(['/']);
  return false;
};
