import { signal, WritableSignal } from '@angular/core';
import { ZodSchema } from 'zod';
import { extractZodErrors } from './validate-form-error';

export class FormHelper<T extends object> {
  public model: WritableSignal<T>;
  public errors = signal<Record<string, string | undefined>>({});

  constructor(initialValue: T) {
    this.model = signal<T>(initialValue);
  }

  public updateField<K extends keyof T>(key: K, value: T[K]): void {
    this.model.update(m => ({ ...m, [key]: value }));
    this.clearError(key as string);
  }

  public clearError(key: string): void {
    if (this.errors()[key]) {
      this.errors.update(e => ({ ...e, [key]: undefined }));
    }
  }

  public validate(schema: ZodSchema): boolean {
    const result = schema.safeParse(this.model());
    if (result.success) {
      this.errors.set({});
      return true;
    } else {
      this.errors.set(extractZodErrors(result.error));
      return false;
    }
  }

  public reset(value: T): void {
    this.model.set(value);
    this.errors.set({});
  }
}
