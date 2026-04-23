import { Component, computed, inject, signal, Directive, WritableSignal } from '@angular/core';
import { AuthService } from '../../service/auth-service';
import { ErrorHandleService } from '../../core/error-handle-service';
import { Router } from '@angular/router';
import { AlertService } from '../../core/alert-service';
import { FormHelper } from '../../core/form-helper';
import { LoadingService } from '../../core/loading-service';
import {AuthModel} from "../../models/auth-model";

@Directive()
export abstract class AuthBaseComponent<T extends AuthModel = any> {
  protected viewPassword = signal<boolean>(false);
  protected abstract isRegister: WritableSignal<boolean>;
  protected inputType = computed(() => this.viewPassword() ? 'text' : 'password');
  
  private readonly loadingService = inject(LoadingService);
  protected isLoading = this.loadingService.isLoading;

  protected abstract form: FormHelper<T>;

  protected authService = inject(AuthService);
  protected errorHandle = inject(ErrorHandleService);
  protected router = inject(Router);
  protected alertComponent = inject(AlertService);

  showOrHidePassword(): void {
    this.viewPassword.update(v => !v);
  }

  abstract submitForm(): void;
  protected abstract validateForm(): boolean;
}
