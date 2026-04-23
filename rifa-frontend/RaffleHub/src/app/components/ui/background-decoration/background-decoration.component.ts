import { Component, input } from '@angular/core';
import { NgClass } from '@angular/common';

@Component({
  selector: 'app-background-decoration',
  standalone: true,
  imports: [NgClass],
  template: `
    <div aria-hidden="true" [ngClass]="containerClass()" class="absolute inset-x-0 -z-10 transform-gpu overflow-hidden blur-3xl">
      <div [style.clip-path]="clipPath" [ngClass]="blobClass()"
           class="relative aspect-1155/678 w-144.5 -translate-x-1/2 bg-linear-to-tr dark:from-indigo-800 dark:to-indigo-600 from-verde-500 to-verde-500 opacity-50 sm:w-288.75">
      </div>
    </div>
  `
})
export class BackgroundDecorationComponent {
  position = input<'top' | 'bottom'>('top');
  variant = input<'left' | 'right'>('left');

  protected readonly clipPath = 'polygon(74.1% 44.1%, 100% 61.6%, 97.5% 26.9%, 85.5% 0.1%, 80.7% 2%, 72.5% 32.5%, 60.2% 62.4%, 52.4% 68.1%, 47.5% 58.3%, 45.2% 34.5%, 27.5% 76.7%, 0.1% 64.9%, 17.9% 100%, 27.6% 76.8%, 76.1% 97.7%, 74.1% 44.1%)';

  protected containerClass() {
    if (this.position() === 'top') {
      return '-top-40 sm:-top-80';
    }
    return 'top-[calc(100%-13rem)] sm:top-[calc(100%-30rem)]';
  }

  protected blobClass() {
    const alignment = this.variant() === 'left' ? 'left-[calc(50%-11rem)] sm:left-[calc(50%-30rem)]' : 'left-[calc(50%+3rem)] sm:left-[calc(50%+36rem)]';
    const rotation = this.position() === 'top' ? 'rotate-30' : '';
    return `${alignment} ${rotation}`;
  }
}
