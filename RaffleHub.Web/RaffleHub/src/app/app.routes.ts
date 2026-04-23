import {Routes} from '@angular/router';
import {HomeComponent} from './pages/home/home-component';
import {NotFoundComponent} from './components/not-found/not-found-component';
import { participantGuard } from './guard/participant-guard';
import { paymentGuard } from './guard/payment-guard';
import {dashboardGuard} from "./guard/dashboard-guard";

export const routes: Routes = [
    {path: '', component: HomeComponent},
    {
        path: 'my-bookings',
        canActivate: [participantGuard],
        loadComponent: () => import('./participant/pages/my-bookings/my-bookings.component').then(m => m.MyBookingsComponent)
    },
    {
        path: 'gallery',
        loadComponent: () => import('./pages/gallery/gallery-component').then(m => m.GalleryComponent)
    },
    {
      path: 'raffle',
      loadComponent:
          () => import('./pages/raffle-list/raffle-list-component')
          .then(m => m.RaffleListComponent)
    },
    {
        path: 'raffle/:id',
        loadComponent:
            () => import('./pages/view-raffle-details-component/view-raffle-details-component')
                .then(m => m.ViewRaffleDetailsComponent)
    },
    {
        path: 'payment/:participantId',
        canActivate: [paymentGuard],
        loadComponent:
            () => import('./participant/pages/payment-page/payment-page-component')
                .then(m => m.PaymentPageComponent)
    },
    {
        path: 'auth/login',
        loadComponent:
            () => import('./pages/auth/login')
            .then(m => m.LoginComponent)
    },
    {
        path: 'auth/register',
        loadComponent:
            () => import('./pages/auth/register')
            .then(m => m.RegisterComponent)
    },
    {
        path: 'dashboard',
        // canActivate: [dashboardGuard],
        loadComponent: () => import('./admin/pages/dashboard/dashboard-component').then(m => m.DashboardComponent),
        children: [
            {
                path: 'list-raffles',
                loadComponent: () => import('./admin/pages/list-raffles/list-raffles-component').then(m => m.ListRafflesComponent)
            },
            {
                path: 'new-raffle',
                loadComponent: () => import('./admin/pages/new-edit-raffle/NewRaffle').then(m => m.NewRaffle)
            },
            {
                path: 'edit-raffle/:id',
                loadComponent: () => import('./admin/pages/new-edit-raffle/EditRaffle').then(m => m.EditRaffle)
            },
            {
                path: 'analytics',
                loadComponent: () => import('./admin/pages/analytics/analytics-component').then(m => m.AnalyticsComponent)
            },
            {
                path: 'list-participants/:id',
                loadComponent: () => import('./admin/pages/list-participants/list-participants-component').then(m => m.ListParticipantsComponent)
            }
        ]
    },

    {path: '404', component: NotFoundComponent},
    {path: '**', redirectTo: '404'}
];
