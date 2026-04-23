export interface CategoryGalleryModel {
  id: string;
  categoryName: string;
}

export type ListAllCategoryGalleryDto = ReadonlyArray<CategoryGalleryModel>;
