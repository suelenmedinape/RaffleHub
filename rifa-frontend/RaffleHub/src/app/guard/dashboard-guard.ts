import {CanActivateFn, Router} from '@angular/router';
import {inject} from "@angular/core";
import {AuthService} from "../service/auth-service";

export const dashboardGuard: CanActivateFn = (route, state) => {
    const authService = inject(AuthService);
    const router = inject(Router);

    if (!authService.isValidToken()) {
        authService.logout();
        return router.createUrlTree(['auth/login']);
    }

    const isAdminOrOperator = authService.hasAnyRole(['ADMIN', 'OPERATOR']);

    if (!isAdminOrOperator) {
        authService.logout();
        return router.createUrlTree(['auth/login']);
    }

    return true;
};
