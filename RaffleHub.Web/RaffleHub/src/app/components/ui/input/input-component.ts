import {Component, input, model, computed} from '@angular/core';
import {FormsModule} from "@angular/forms";
import {NgClass} from "@angular/common";

@Component({
    selector: 'app-input',
    imports: [FormsModule, NgClass],
    standalone: true,
    template: `
        <div>
            <div class="flex items-center justify-between mb-2">
                <label
                        [for]="inputId()"
                        class="block text-sm/6 font-medium text-gray-900 dark:text-gray-100"
                >
                    {{ label() }}
                </label>
                <ng-content select="[labelAction]"></ng-content>
            </div>
            <div>
                <input
                        [(ngModel)]="internalValue"
                        [id]="inputId()"
                        [type]="type()"
                        [name]="name()"
                        [placeholder]="placeholder()"
                        [autocomplete]="computedAutocomplete()"
                        [ngClass]="{
            'outline-red-500 focus:outline-red-500': hasError(),
            'outline-gray-300 focus:outline-indigo-600 dark:focus:outline-verde-500': !hasError()
          }"
                        class="block w-full rounded-md bg-white px-3 py-1.5 text-base text-gray-900 outline-1 -outline-offset-1 placeholder:text-gray-400 focus:outline-2 focus:-outline-offset-2 sm:text-sm/6 dark:bg-white/5 dark:text-white dark:outline-white/10 dark:placeholder:text-gray-500"
                />
            </div>
            @if (errorMessage()) {
                <p class="mt-2 text-sm text-red-600 dark:text-red-400">
                    • {{ errorMessage() }}
                </p>
            }
        </div>
    `
})
export class InputComponent {

    value = model<string>('');

    inputId = input.required<string>();
    label = input.required<string>();
    type = input<string>('text');
    name = input<string>('');
    placeholder = input<string>('');
    autocomplete = input<string>('');
    errorMessage = input<string>('');

    hasError = computed(() => !!this.errorMessage());

    computedAutocomplete = computed(() => {
        if (this.autocomplete()) {
            return this.autocomplete();
        }
        return this.type() === 'password' ? 'off' : 'on';
    });

    get internalValue(): string {
        return this.value();
    }

    set internalValue(val: string) {
        this.value.set(val);
    }
}