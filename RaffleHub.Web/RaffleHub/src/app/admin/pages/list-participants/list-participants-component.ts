import { Component, inject, signal, OnInit, OnDestroy } from '@angular/core';
import { CommonModule, CurrencyPipe } from '@angular/common';
import { ActivatedRoute, RouterModule } from '@angular/router';
import { FormsModule, ReactiveFormsModule, FormControl } from '@angular/forms';
import { ParticipantService } from '../../../service/participant-service';
import { RaffleService } from '../../../service/raffle-service';
import { ListAllParticipantsDto } from '../../../models/participant-model';
import { ListByIdRaffleDto } from '../../../models/raffle-model';
import { debounceTime, distinctUntilChanged, Subject, takeUntil } from 'rxjs';

@Component({
  selector: 'app-list-participants',
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule, ReactiveFormsModule, CurrencyPipe],
  templateUrl: './list-participants-component.html',
  styleUrl: './list-participants-component.css',
})
export class ListParticipantsComponent implements OnInit, OnDestroy {
    private readonly route = inject(ActivatedRoute);
    private readonly participantService = inject(ParticipantService);
    private readonly raffleService = inject(RaffleService);

    raffleId = signal<string | null>(null);
    raffle = signal<ListByIdRaffleDto | null>(null);
    participants = signal<ListAllParticipantsDto[]>([]);
    isLoading = signal(false);
    
    searchControl = new FormControl('');
    private destroy$ = new Subject<void>();

    ngOnInit(): void {
        const id = this.route.snapshot.paramMap.get('id');
        if (id) {
            this.raffleId.set(id);
            this.loadRaffleDetails(id);
            this.loadParticipants('');
        }

        this.searchControl.valueChanges.pipe(
            debounceTime(400),
            distinctUntilChanged(),
            takeUntil(this.destroy$)
        ).subscribe(term => {
            this.loadParticipants(term || '');
        });
    }

    ngOnDestroy(): void {
        this.destroy$.next();
        this.destroy$.complete();
    }

    loadRaffleDetails(id: string): void {
        this.raffleService.listById(id).subscribe({
            next: (response) => {
                this.raffle.set(response.data);
            }
        });
    }

    loadParticipants(term: string): void {
        const id = this.raffleId();
        if (!id) return;

        this.isLoading.set(true);
        this.participantService.listByRaffle(id, 1, 100, term).subscribe({
            next: (response) => {
                this.participants.set(response.data as ListAllParticipantsDto[]);
                this.isLoading.set(false);
            },
            error: () => {
                this.participants.set([]);
                this.isLoading.set(false);
            }
        });
    }

    getWhatsAppUrl(phone: string): string {
        const cleaned = phone.replace(/\D/g, '');
        const finalPhone = cleaned.length <= 11 ? `55${cleaned}` : cleaned;
        return `https://wa.me/${finalPhone}`;
    }

    copyToClipboard(text: string): void {
        navigator.clipboard.writeText(text);
    }
}
