import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { AuthCookieService } from '../../service/auth-cookie-service';

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const authCookieService = inject(AuthCookieService);
  const token = authCookieService.getToken();

  if (token) {
    const cloned = req.clone({
      setHeaders: {
        Authorization: `Bearer ${token}`
      }
    });
    return next(cloned);
  }

  return next(req);
};
