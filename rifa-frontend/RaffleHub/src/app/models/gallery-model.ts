import {z} from "zod";
import { CategoryGalleryModel } from "./category-gallery-model";

export interface GalleryModel {
    id: string
    imageUrl: string
    nameImage: string
    descriptionImage?: string
    year: number
    category?: CategoryGalleryModel
    categoryId: string
}

export type CreateGalleryDto = Omit<GalleryModel, 'id'>;
export type ListAllGalleryDto = ReadonlyArray<GalleryModel>;
export type ListGalleryDto = Omit<GalleryModel, 'id' | 'imageUrl'>;

export const CreateGallerySchema = z.object({
    nameImage: z.string().min(1, 'Nome é obrigatório'),
    year: z.number().min(1999, 'Ano inválido')
})