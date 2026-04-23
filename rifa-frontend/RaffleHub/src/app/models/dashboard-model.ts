export interface RecentSaleDto {
    bookingId: string;
    participantName: string;
    raffleName: string;
    amount: number;
    paidAt: string;
}

export interface DashboardStatsDto {
    totalRevenue: number;
    totalTicketsSold: number;
    totalParticipants: number;
    activeRafflesCount: number;
    recentSales: RecentSaleDto[];
}
