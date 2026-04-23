import { Injectable } from '@angular/core';
import {HttpErrorResponse} from "@angular/common/http";

@Injectable({
  providedIn: 'root',
})
export class ErrorHandleService {
    getErrorMessage(error: HttpErrorResponse): string {
        switch(error.status) {
            case 400:
                if (error.error?.errors) {
                    return Object.keys(error.error.errors)
                        .map(key => error.error.errors[key].join(', '))
                        .join('; ');
                }
                return error.error?.message || 'Dados inválidos';

            case 401:
                return 'Credenciais de autenticação inválidas';

            case 403:
                return 'Você não tem permissão para esta ação';

            case 404:
                return 'Recurso não encontrado';

            case 409:
                return error.error?.message || 'Conflito de dados';

            case 500:
                return 'Erro no servidor. Tente novamente mais tarde.';

            case 0:
                return 'Erro de conexão. Verifique sua internet.';

            default:
                return error.error?.message || 'Erro desconhecido';
        }
    }
}
