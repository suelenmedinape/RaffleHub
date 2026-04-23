import { Component, signal } from '@angular/core';
import { finalize } from 'rxjs';
import { AuthBaseComponent } from './auth-component';
import { LoginDto, LoginSchema } from '../../models/auth-model';
import { HttpErrorResponse } from '@angular/common/http';
import { FormsModule } from '@angular/forms';
import { InputComponent } from '../../components/ui/input/input-component';
import { ApiResponseModel } from '../../models/api-response-model';
import { AuthData } from '../../models/auth-model';
import { FormHelper } from '../../core/form-helper';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [FormsModule, InputComponent],
  templateUrl: './auth-component.html',
  styleUrl: './auth-component.css',
})
export class LoginComponent extends AuthBaseComponent<LoginDto> {
  protected override isRegister = signal(false);
  protected override form = new FormHelper<LoginDto>({ email: '', password: '' });

  override submitForm(): void {
    if (this.validateForm()) {
      this.handleLogin();
    }
  }

  protected override validateForm(): boolean {
    return this.form.validate(LoginSchema);
  }

  private handleLogin(): void {
    this.authService.login(this.form.model())
      .subscribe({
        next: (response: ApiResponseModel<AuthData>) => {
          this.alertComponent.successToast(response.message);
          setTimeout(() => this.router.navigate(['/']), 3000);
        }
      });
  }
}
