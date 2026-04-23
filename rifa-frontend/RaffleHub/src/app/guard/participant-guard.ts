import { CanActivateFn, Router } from '@angular/router';
import { inject } from '@angular/core';
import { AuthService } from '../service/auth-service';

export const participantGuard: CanActivateFn = (route, state) => {
  const authService = inject(AuthService);
  const router = inject(Router);
  
  const isLogued = authService.isValidToken();

  if (isLogued) {
    return true;
  }
   
  router.navigate(["/auth/login"]);
  return false;
};
