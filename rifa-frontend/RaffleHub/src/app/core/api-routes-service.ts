import {Injectable} from '@angular/core';
import { environment } from '../../environments/environment';

@Injectable({
    providedIn: 'root',
})
export class ApiRoutesService {
    private readonly baseUrl = environment.apiUrl;

    get baseApiUrl(): string {
        return this.baseUrl;
    }

    readonly authUrl = {
        login: `${this.baseUrl}/Auth/login`,
        register: `${this.baseUrl}/Auth/register`,
        refresh: `${this.baseUrl}/Auth/refresh`,
        revoke: `${this.baseUrl}/Auth/revoke`
    };

    readonly raffleUrl = {
        base: `${this.baseUrl}/Raffle`,
        names: `${this.baseUrl}/Raffle/names`,
        changeStatus: `${this.baseUrl}/Raffle/ChangeStatus`,
        ticketsSold: `${this.baseUrl}/tickets/listTicketsSold`
    };

    readonly participantUrl = {
        base: `${this.baseUrl}/Participant`,
        raffle: `${this.baseUrl}/Participant/Raffle`
    };

    readonly dashboardUrl = {
        stats: `${this.baseUrl}/Dashboard/stats`
    };

    readonly bookingUrl = {
        myBookings: `${this.baseUrl}/Booking/my-bookings`,
        pending: `${this.baseUrl}/Booking/pending`,
        generatePix: `${this.baseUrl}/Booking/generate-pix`
    };

    readonly categoriesGalleryUrl = {
        base: `${this.baseUrl}/CategoriesGallery`
    };

    readonly galleryUrl = {
        base: `${this.baseUrl}/Gallery`,
        byYears: `${this.baseUrl}/Gallery/by-years`
    };

    readonly ticketUrl = {
        base: `${this.baseUrl}/Ticket`
    };

    buildUrl(url: string, params?: Record<string, any>): string {
        if (!params) return url;

        const queryString = Object.entries(params)
            .filter(([_, value]) => value !== null && value !== undefined)
            .map(([key, value]) => `${encodeURIComponent(key)}=${encodeURIComponent(value)}`)
            .join('&');

        return queryString ? `${url}?${queryString}` : url;
    }
}
