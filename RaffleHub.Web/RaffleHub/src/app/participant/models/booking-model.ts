import {ParticipantModel} from "../../models/participant-model";
import {ListTicketsByParticipant, TicketModel} from "../../models/ticket-model";
import {BookingStatus} from "../../enum/booking-status";

export interface BookingModel {
    id: string;
    totalAmount: number;
    status: BookingStatus;
    createdAt: string;
    paidAt?: string | null;
    transactionId?: string | null;
    pixQrCodeUrl?: string | null;
    pixCopyPaste?: string | null;
    participantId: string;
    participant?: ParticipantModel;
    raffleId: string;
    raffle?: any;
    tickets: TicketModel[];
}

export interface PaymentData {
  id: string;
  totalAmount: number;
  status: string | number;
  createdAt: string;
  paidAt: string | null;
  raffleId: string;
  raffleName: string;
  participantId: string;
  participantName: string;
  pixQrCodeUrl: string;
  pixCopyPaste: string;
  tickets: TicketData[];
}

export interface TicketData {
  ticketNumber: number;
}

export interface ListBookingPendingDto extends Pick<BookingModel,
    'id' | 'totalAmount' | 'status' | 'createdAt' | 'paidAt' | 'raffleId' | 'participantId' | 'pixQrCodeUrl' | 'pixCopyPaste'
> {
    raffleName: string;
    participantName: string;
    tickets: ListTicketsByParticipant[];
}
export interface MyBookingsDto extends Pick<BookingModel, 'id' | 'raffleId' | 'totalAmount' | 'createdAt' | 'participantId'> {
    raffleName: string;
    status: string;
    ticketNumbers: number[];
}

export interface ParticipantPurchaseResponseDto extends
    Pick<ParticipantModel, 'participantName' | 'phone'>,
    Pick<BookingModel, 'totalAmount' | 'status'>
{
    bookingId: string;
    participantId: string;
    raffleName: string;
    ticketNumbers: number[];
}