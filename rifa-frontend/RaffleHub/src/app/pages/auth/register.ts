import { Component, signal } from '@angular/core';
import { finalize } from 'rxjs';
import { AuthBaseComponent } from './auth-component';
import { RegisterDto, RegisterSchema } from '../../models/auth-model';
import { HttpErrorResponse } from '@angular/common/http';
import { FormsModule } from '@angular/forms';
import { InputComponent } from '../../components/ui/input/input-component';
import { ApiResponseModel } from '../../models/api-response-model';
import { AuthData } from '../../models/auth-model';
import { FormHelper } from '../../core/form-helper';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [FormsModule, InputComponent],
  templateUrl: './auth-component.html',
  styleUrl: './auth-component.css',
})
export class RegisterComponent extends AuthBaseComponent<RegisterDto> {
  protected override isRegister = signal(true);
  protected override form = new FormHelper<RegisterDto>({ fullName: '', email: '', phone: '', password: '' });

  override submitForm(): void {
    if (this.validateForm()) {
      this.handleRegister();
    }
  }

  protected override validateForm(): boolean {
    return this.form.validate(RegisterSchema);
  }

  private handleRegister(): void {
    this.authService.register(this.form.model())
      .subscribe({
        next: (response: ApiResponseModel<AuthData>) => {
          this.alertComponent.successToast(response.message);
          setTimeout(() => this.router.navigate(['/']), 3000);
        }
      });
  }
}
