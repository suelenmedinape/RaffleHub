import { Component, inject } from '@angular/core';
import { NavigationEnd, Router } from "@angular/router";
import { filter, map } from "rxjs";
import { toSignal } from '@angular/core/rxjs-interop';

@Component({
  selector: 'app-footer',
  imports: [],
  templateUrl: './footer-component.html',
  styleUrl: './footer-component.css',
})
export class FooterComponent {
  private readonly router = inject(Router);

  public showFooter = toSignal(
    this.router.events.pipe(
      filter((event): event is NavigationEnd => event instanceof NavigationEnd),
      map(event => !['/login', '/register', '/404', '/dashboard'].some(path => event.url.includes(path)))
    ),
    { initialValue: true }
  );
}
