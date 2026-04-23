import { StatusRaffle } from "../enum/status-raffle"
import { TicketModel } from "./ticket-model"
import { z } from 'zod';

export interface RaffleModel {
  id: string
  imageUrl: string
  raffleName: string
  description: string
  totalTickets: number
  ticketPrice: number
  drawDate: string
  status: StatusRaffle
  createdAt: string
  tickets: TicketModel[]
}

export interface RaffleData {
    message: string
    data: string
}

export type CreateRaffle = Omit<Required<RaffleModel>, 'id' | 'imageUrl' | 'status' | 'createdAt' | 'tickets'>
export type CreateRaffleDto = Required<CreateRaffle> & {
    file?: File
}

export const RaffleSchema = z.object({
  raffleName: z.string().min(3, 'Nome deve ter pelo menos 3 caracteres'),
  ticketPrice: z.number().min(0.01, 'Preço deve ser maior que zero'),
  drawDate: z.string().min(1, 'Data do sorteio é obrigatória'),
  description: z.string().optional(),
  totalTickets: z.number().min(1, 'Mínimo de 1 rifa')
});

export type RaffleSchema = z.infer<typeof RaffleSchema>;

export type ListAllRaffleDto = Omit<RaffleModel, 'createdAt' | 'description'> & {
  soldTicketsCount: number;
  soldPercentage: number;
};

export type ListByIdRaffleDto = Omit<RaffleModel, 'createdAt'> & {
    soldTicketsCount: number;
};