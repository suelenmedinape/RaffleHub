import { Component, CUSTOM_ELEMENTS_SCHEMA, inject, OnInit, signal } from '@angular/core';
import { GalleryService } from '../../service/gallery-service';
import { ListAllGalleryDto, GalleryModel } from '../../models/gallery-model';
import { CategoryGalleryService } from '../../service/category-gallery-service';
import { LoadingComponent } from '../../components/loading/loading-component';
import { LoadingService } from '../../core/loading-service';

@Component({
  selector: 'app-gallery',
  imports: [LoadingComponent],
  templateUrl: './gallery-component.html',
  styleUrl: './gallery-component.css',
  schemas: [CUSTOM_ELEMENTS_SCHEMA]
})
export class GalleryComponent implements OnInit{

  private readonly galleryService = inject(GalleryService);
  private readonly categoryService = inject(CategoryGalleryService);
  private readonly loadingService = inject(LoadingService);
  
  public gallery: ListAllGalleryDto = [];
  public selectedImage: GalleryModel | null = null;
  
  protected isModalOpen = signal<boolean>(false);
  protected isLoading = this.loadingService.isLoading;

  public availableYears: number[] = [];
  public selectedYears: Set<number> = new Set<number>();
  public isDropdownOpen = false;

  ngOnInit() {
    this.generateYears();
    this.getGallery();
  }

  generateYears() {
    const startYear = 2023;
    const currentYear = new Date().getFullYear();
    for (let i = currentYear; i >= startYear; i--) {
        this.availableYears.push(i);
    }
  }

  toggleYear(year: number) {
    if (this.selectedYears.has(year)) {
        this.selectedYears.delete(year);
    } else {
        this.selectedYears.add(year);
    }
    this.getGallery();
  }

  toggleDropdown() {
    this.isDropdownOpen = !this.isDropdownOpen;
  }

  get columns(): GalleryModel[][] {
    const cols: GalleryModel[][] = [[], [], [], []];
    this.gallery.forEach((item, index) => {
      cols[index % 4].push(item);
    });
    return cols;
  }

  getGallery() {

    if (this.selectedYears.size === 0) {
        return this.galleryService.getGallery(1, 50).subscribe({
            next: (response: any) => {
                this.gallery = response.data;
            },
            error: (err) => {
                console.error('Error fetching gallery', err);
            }
        });
    } else {
        const yearsArray = Array.from(this.selectedYears);
        return this.galleryService.getGalleryByYears(yearsArray).subscribe({
            next: (response: any) => {
                this.gallery = response.data;
            },
            error: (err) => {
                console.error('Error fetching filtered gallery', err);
            }
        });
    }
  }

  openModal(id: string) {
    this.galleryService.getGalleryById(id).subscribe((response: any) => {
      const selected = response.data;

      this.categoryService.getCategoryById(selected.categoryId).subscribe({
        next: (catResp: any) => {
          selected.category = catResp.data;
          this.selectedImage = selected;
          this.isModalOpen.set(true);
        },
        error: (err) => {
          console.error('Error fetching category', err);
          this.selectedImage = selected;
          this.isModalOpen.set(true);
        }
      });
    });
  }

  closeModal() {
    this.isModalOpen.set(false);
    this.selectedImage = null;
  }
}