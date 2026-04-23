import { Component, computed, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { DashboardService } from '../../../service/dashboard-service';
import { ParticipantService } from '../../../service/participant-service';
import { RaffleService } from '../../../service/raffle-service';
import { DashboardStatsDto } from '../../../models/dashboard-model';
import { ListAllRaffleDto } from '../../../models/raffle-model';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-analytics',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './analytics-component.html',
  styleUrl: './analytics-component.css',
})
export class AnalyticsComponent implements OnInit {
  private readonly dashboardService = inject(DashboardService);
  private readonly participantService = inject(ParticipantService);
  private readonly raffleService = inject(RaffleService);

  stats = signal<DashboardStatsDto | null>(null);
  raffles = signal<ListAllRaffleDto[]>([]);
  selectedRaffleId = signal<string>('');
  participants = signal<any[]>([]);
  isLoading = signal<boolean>(false);
  isLoadingParticipants = signal<boolean>(false);

  selectedRaffle = signal<ListAllRaffleDto | null>(null);

  chartSlices = computed(() => {
    const raffle = this.selectedRaffle();
    const participantsList = this.participants();
    
    if (!raffle) return [];

    const total = raffle.totalTickets;
    const colors = [
      '#6366f1', '#10b981', '#f59e0b', '#ef4444', '#0ea5e9',
      '#8b5cf6', '#ec4899', '#f97316', '#14b8a6', '#475569'
    ];
    let cumulative = 0;
    const slices: any[] = [];

    // Sort participants by number of tickets (from current page)
    const sortedParticipants = [...participantsList].sort((a, b) => 
      (b.tickets?.length || 0) - (a.tickets?.length || 0)
    );

    // Top 10 participants
    const topParticipants = sortedParticipants.slice(0, 10);
    let top5Sum = 0;
    topParticipants.forEach((p, i) => {
      const count = p.tickets?.length || 0;
      if (count > 0) {
        const percentage = (count / total) * 100;
        slices.push({
          label: p.participantName,
          count,
          percentage,
          color: colors[i],
          dasharray: `${percentage} ${100 - percentage}`,
          dashoffset: -cumulative
        });
        cumulative += percentage;
        top5Sum += count;
      }
    });

    // Others: (Total Sold - Top 10)
    // This accounts for all participants not in the Top 10, including those not in the current page.
    const othersCount = Math.max(0, raffle.soldTicketsCount - top5Sum);
    if (othersCount > 0) {
      const percentage = (othersCount / total) * 100;
      slices.push({
        label: 'Outros Compradores',
        count: othersCount,
        percentage,
        color: '#94a3b8', // Slate-400 for grouped others
        dasharray: `${percentage} ${100 - percentage}`,
        dashoffset: -cumulative
      });
      cumulative += percentage;
    }

    // Unsold: (Total - Total Sold)
    const unsoldCount = Math.max(0, total - raffle.soldTicketsCount);
    if (unsoldCount > 0) {
      const percentage = (unsoldCount / total) * 100;
      slices.push({
        label: 'Não Vendido',
        count: unsoldCount,
        percentage,
        color: 'transparent',
        isUnsold: true,
        dasharray: `${percentage} ${100 - percentage}`,
        dashoffset: -cumulative
      });
    }

    return slices;
  });

  ngOnInit(): void {
    this.loadStats();
    this.loadRaffles();
  }

  loadStats(): void {
    this.isLoading.set(true);
    this.dashboardService.getGeneralStats().subscribe({
      next: (res) => {
          this.stats.set(res.data);

        this.isLoading.set(false);
      },
      error: () => this.isLoading.set(false)
    });
  }

  loadRaffles(): void {
    this.raffleService.listAllRaffles().subscribe({
      next: (res) => {
          this.raffles.set(res.data);
      }
    });
  }

  onRaffleChange(event: Event): void {
    const selectElement = event.target as HTMLSelectElement;
    const raffleId = selectElement.value;
    this.selectedRaffleId.set(raffleId);
    
    if (raffleId) {
      const raffle = this.raffles().find(r => r.id === raffleId);
      this.selectedRaffle.set(raffle || null);
      this.loadParticipants(raffleId);
    } else {
      this.selectedRaffle.set(null);
      this.participants.set([]);
    }
  }

  loadParticipants(raffleId: string): void {
    this.isLoadingParticipants.set(true);
    this.participantService.listByRaffle(raffleId, 1, 500).subscribe({
      next: (res) => {
          this.participants.set(res.data);
        this.isLoadingParticipants.set(false);
      },
      error: () => this.isLoadingParticipants.set(false)
    });
  }

  getInitials(name: string): string {
    if (!name) return '';
    const parts = name.trim().split(' ');
    if (parts.length === 1) return parts[0].substring(0, 2).toUpperCase();
    return (parts[0][0] + parts[parts.length - 1][0]).toUpperCase();
  }
}
