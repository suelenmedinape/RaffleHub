import { RaffleModel } from "./raffle-model"
import {ParticipantModel} from "./participant-model";
import {BookingModel} from "../participant/models/booking-model";

export interface TicketModel {
    id: string;
    ticketNumber: number;
    raffleId: string;
    raffle?: RaffleModel
    participantId?: string | null;
    participant?: ParticipantModel;
    bookingId?: string | null;
    booking?: BookingModel | null;
}

export interface ListTicketsByParticipant extends Pick<TicketModel, 'ticketNumber'> {}
