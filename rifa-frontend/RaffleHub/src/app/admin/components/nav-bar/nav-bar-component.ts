import {Component, inject, signal} from '@angular/core';
import {Router, RouterLink} from "@angular/router";
import {AuthService} from "../../../service/auth-service";

@Component({
  selector: 'app-nav-bar',
    imports: [
        RouterLink
    ],
  templateUrl: './nav-bar-component.html',
  styleUrl: './nav-bar-component.css',
})
export class NavBarComponent {
    private readonly authService = inject(AuthService);
    private readonly router = inject(Router);

    isSidebarOpen = signal(false);

    toggleSidebar(): void {
        this.isSidebarOpen.update(v => !v);
    }

    closeSidebar(): void {
        this.isSidebarOpen.set(false);
    }

    logout(): void {
        this.authService.logout();
        this.router.navigate(['/auth/login']);
    }
}
