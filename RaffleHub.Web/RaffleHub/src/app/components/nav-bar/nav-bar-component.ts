import { CUSTOM_ELEMENTS_SCHEMA, Component, inject, computed } from '@angular/core';
import { NavigationEnd, Router, RouterLink } from "@angular/router";
import { filter, map } from "rxjs";
import { toSignal } from '@angular/core/rxjs-interop';
import { AuthService } from '../../service/auth-service';
import { AuthCookieService } from '../../service/auth-cookie-service';

@Component({
  selector: 'app-navBar-component',
  imports: [RouterLink],
  schemas: [CUSTOM_ELEMENTS_SCHEMA],
  templateUrl: './nav-bar-component.html',
  styleUrl: './nav-bar-component.css',
})
export class NavBarComponent {
  private readonly router = inject(Router);
  private readonly authService = inject(AuthService);
  private readonly cookieService = inject(AuthCookieService);

  public readonly isAuthenticated = this.authService.isAuthenticated;
  
  public readonly isAdminOrOperator = computed(() => {
    if (!this.isAuthenticated()) return false;
    return this.authService.hasAnyRole(['ADMIN', 'OPERATOR']);
  });

  public showNavbar = toSignal(
    this.router.events.pipe(
      filter((event): event is NavigationEnd => event instanceof NavigationEnd),
      map(event => !['/login', '/register', '/404', '/dashboard'].some(path => event.url.includes(path)))
    ),
    { initialValue: true }
  );

  getParticipantId(): string | null {
    return this.cookieService.getParticipantId();
  }

  logout(): void {
    // Esvazia os cookies pra deslogar de fato
    this.authService.logout();
    // Redireciona pra tela de login
    this.router.navigate(['/auth/login']);
  }
}
