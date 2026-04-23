import {Component, Directive, inject, signal, WritableSignal} from '@angular/core';
import {DomSanitizer, SafeUrl} from '@angular/platform-browser';
import {InputComponent} from "../../../components/ui/input/input-component";
import {RaffleModel} from "../../../models/raffle-model";
import {LoadingService} from "../../../core/loading-service";
import {FormHelper} from "../../../core/form-helper";
import {RaffleService} from "../../../service/raffle-service";
import {ErrorHandleService} from "../../../core/error-handle-service";
import {Router, ActivatedRoute} from "@angular/router";
import {AlertService} from "../../../core/alert-service";

@Directive()
export abstract class NewEditRaffleComponent<T extends object = any> {
    protected abstract isEdit: WritableSignal<boolean>;

    private readonly loadingService = inject(LoadingService);
    protected isLoading = this.loadingService.isLoading;

    protected abstract form: FormHelper<T>;

    protected raffleService = inject(RaffleService);
    protected errorHandle = inject(ErrorHandleService);
    protected router = inject(Router);
    protected route = inject(ActivatedRoute);
    protected alertComponent = inject(AlertService);

    selectedFile = signal<File | null>(null);
    filePreview = signal<string | null>(null);
    private readonly sanitizer = inject(DomSanitizer);

    protected get sanitizedFilePreview(): SafeUrl | null {
        const preview = this.filePreview();
        return preview ? this.sanitizer.bypassSecurityTrustUrl(preview) : null;
    }

    abstract submitForm(): void;
    protected abstract validateForm(): boolean;

    onFileSelected(event: Event) {
        const file = (event.target as HTMLInputElement).files?.[0];
        if (file) {
            this.selectedFile.set(file);
            const reader = new FileReader();
            reader.onload = () => {
                this.filePreview.set(reader.result as string);
            };
            reader.readAsDataURL(file);
        }
    }
}
