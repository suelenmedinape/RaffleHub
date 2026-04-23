import { HttpInterceptorFn, HttpErrorResponse } from '@angular/common/http';
import { inject } from '@angular/core';
import { catchError, throwError } from 'rxjs';
import { AlertService } from '../alert-service';
import { ErrorHandleService } from '../error-handle-service';

export const errorInterceptor: HttpInterceptorFn = (req, next) => {
  const alertService = inject(AlertService);
  const errorHandle = inject(ErrorHandleService);

  return next(req).pipe(
    catchError((error: HttpErrorResponse) => {
      const message = errorHandle.getErrorMessage(error);
      alertService.errorToast(message);
      return throwError(() => error);
    })
  );
};
