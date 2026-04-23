import {ListTicketsByParticipant, TicketModel} from "./ticket-model";
import {z} from "zod";

export interface ParticipantModel {
    id: string;
    participantName: string;
    phone: string;
    cpf: string;
    userId?: string | null;
    user?: any | null;
    tickets: TicketModel[];
}

export interface ListAllParticipantsDto extends Pick<ParticipantModel, 'id' | 'participantName' | 'phone'> {
    tickets: number[];
}
export interface ParticipantDetailDto extends Pick<ParticipantModel, 'id' | 'participantName' | 'phone'> {
    tickets: ListTicketsByParticipant[];
}

export interface CreateParticipantDto extends Pick<ParticipantModel, 'participantName' | 'phone'> {
    document: string;
    raffleId: string;
    ticketNumbers: number[];
}

const phoneRegex = /^\(?\d{2}\)?\s?\d{4,5}-?\d{4}$/;

const cpfRegex = /^\d{3}\.?\d{3}\.?\d{3}-?\d{2}$/;

export const ParticipantSchema = z.object({
    participantName: z.string().min(1, 'Nome é obrigatório'),
    phone: z.string()
        .min(1, 'Telefone é obrigatório')
        .regex(phoneRegex, 'Telefone deve conter DDD (XX) e estar em um formato válido')
        .transform(phone => {
            const cleaned = phone.replace(/\D/g, '');
            
            const withoutCountry = cleaned.replace(/^55/, '');
            
            const ddd = withoutCountry.slice(0, 2);
            const firstPart = withoutCountry.slice(2, -4);
            const lastPart = withoutCountry.slice(-4);
            
            return `${ddd}${firstPart}${lastPart}`;
        }),
    cpf: z.string().min(14, "CPF inválido").regex(cpfRegex, "Adicione um cpf válido")
})