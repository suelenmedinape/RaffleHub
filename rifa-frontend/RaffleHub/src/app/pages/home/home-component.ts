import { ChangeDetectionStrategy, Component, signal, effect } from '@angular/core';
import { BackgroundDecorationComponent } from '../../components/ui/background-decoration/background-decoration.component';

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [BackgroundDecorationComponent],
  templateUrl: './home-component.html',
  styleUrl: './home-component.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class HomeComponent {
  private readonly text = 'Iff Road';
  protected displayedText = signal('');

  private position = 0;
  private isDeleting = false;

  private readonly speed = 100;
  private readonly pauseAfterTyping = 3000;
  private readonly pauseAfterDeleting = 500;

  constructor() {
    this.animateText();
  }

  private animateText(): void {
    if (this.isDeleting) {
      if (this.position > 0) {
        this.displayedText.update(t => t.slice(0, -1));
        this.position--;
        setTimeout(() => this.animateText(), this.speed);
      } else {
        this.isDeleting = false;
        setTimeout(() => this.animateText(), this.pauseAfterDeleting);
      }
    } else {
      if (this.position < this.text.length) {
        this.displayedText.update(t => t + this.text[this.position++]);
        setTimeout(() => this.animateText(), this.speed);
      } else {
        setTimeout(() => {
          this.isDeleting = true;
          this.animateText();
        }, this.pauseAfterTyping);
      }
    }
  }
}
