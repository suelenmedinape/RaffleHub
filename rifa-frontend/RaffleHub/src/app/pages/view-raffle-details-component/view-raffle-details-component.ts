import {
    Component,
    inject,
    OnInit,
    signal,
    input,
    viewChild,
    ElementRef,
    CUSTOM_ELEMENTS_SCHEMA,
    computed,
    ChangeDetectionStrategy
} from '@angular/core';
import {RaffleService} from "../../service/raffle-service";
import {AlertService} from "../../core/alert-service";
import {ActivatedRoute, Router} from "@angular/router";
import {ListAllRaffleDto, ListByIdRaffleDto} from "../../models/raffle-model";
import { LoadingComponent } from "../../components/loading/loading-component";
import { forkJoin, of, switchMap, combineLatest, finalize } from "rxjs";
import { HttpErrorResponse } from "@angular/common/http";
import { ApiResponseModel } from "../../models/api-response-model";
import {TicketService} from "../../service/ticket-service";
import {ErrorHandleService} from "../../core/error-handle-service";
import {CurrencyPipe, DatePipe} from "@angular/common";
import {FormsModule} from "@angular/forms";
import { ParticipantService } from "../../service/participant-service";
import { CreateParticipantDto, ParticipantSchema } from "../../models/participant-model";
import { extractZodErrors } from "../../core/validate-form-error";
import { InputComponent } from "../../components/ui/input/input-component";
import { AuthCookieService } from '../../service/auth-cookie-service';
import { AuthService } from '../../service/auth-service';
import { toObservable, toSignal } from '@angular/core/rxjs-interop';
import { TicketSelectorComponent } from '../../components/ticket-selector/ticket-selector.component';
import { BackgroundDecorationComponent } from '../../components/ui/background-decoration/background-decoration.component';
import { FormHelper } from '../../core/form-helper';

interface ParticipantFormModel {
    participantName: string;
    phone: string;
    cpf: string;
}

import { LoadingService } from '../../core/loading-service';

@Component({
  selector: 'app-view-raffle-details-component',
    imports: [
        LoadingComponent,
        CurrencyPipe,
        DatePipe,
        FormsModule,
        InputComponent,
        TicketSelectorComponent,
        BackgroundDecorationComponent
    ],
  schemas: [CUSTOM_ELEMENTS_SCHEMA],
  templateUrl: './view-raffle-details-component.html',
  styleUrl: './view-raffle-details-component.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ViewRaffleDetailsComponent implements OnInit{

    private readonly raffleService = inject(RaffleService);
    private readonly ticketService = inject(TicketService);
    private readonly authService = inject(AuthService);
    private readonly participantService = inject(ParticipantService);
    private readonly alertComponent = inject(AlertService);
    private readonly errorHandle = inject(ErrorHandleService);
    private readonly cookieService = inject(AuthCookieService);
    private readonly router = inject(Router);
    private readonly loadingService = inject(LoadingService);

    protected isLoading = this.loadingService.isLoading;
    protected isProcessingPurchase = signal<boolean>(false);
    protected isAuthenticated = signal<boolean>(false);

    readonly raffleId = input.required<string>({ alias: 'id' });
    readonly paymentDialog = viewChild<ElementRef<HTMLDialogElement>>('paymentDialog');

    private readonly refreshTrigger = signal<number>(0);
    private readonly raffleId$ = toObservable(this.raffleId);
    protected readonly raffleData = toSignal<{
        raffle: ApiResponseModel<ListByIdRaffleDto>,
        sold: ApiResponseModel<number[]>
    }>(
        combineLatest([this.raffleId$, toObservable(this.refreshTrigger)]).pipe(
            switchMap(([id]) => {
                return forkJoin({
                    raffle: this.raffleService.listById(id),
                    sold: this.ticketService.listTicketsSold(id)
                });
            })
        )
    );

    protected readonly allTickets = computed(() => {
        const data = this.raffleData();
        if (!data?.raffle.data) return [];
        
        const soldSet = new Set(data.sold.data);
        return Array.from(
            {length: data.raffle.data.totalTickets},
            (_, i) => ({
                number: i + 1,
                isSold: soldSet.has(i + 1)
            })
        );
    });

    protected selectedTickets = signal<number[]>([]);
    protected totalAmount = computed(() => {
        const raffle = this.raffleData()?.raffle.data;
        return this.selectedTickets().length * (raffle?.ticketPrice ?? 0);
    });

    protected form = new FormHelper<ParticipantFormModel>({
        participantName: '',
        phone: '',
        cpf: ''
    });

    protected readonly formFields = [
        { id: 'participantName', label: 'Seu nome', key: 'participantName' as const },
        { id: 'phone', label: 'Seu telefone', key: 'phone' as const },
        { id: 'cpf', label: 'Seu CPF', key: 'cpf' as const }
    ];

    ngOnInit() {
        this.isAuthenticated.set(this.cookieService.hasToken());
        if(this.isAuthenticated()) {
            const user = this.cookieService.getUserData();
            this.form.updateField('participantName', user?.fullName || '');
            this.form.updateField('phone', user?.phone || '');
        }
    }

    onTicketToggle(ticketNumber: number): void {
        this.selectedTickets.update(tickets => {
            if (tickets.includes(ticketNumber)) {
                return tickets.filter(n => n !== ticketNumber);
            } else {
                return [...tickets, ticketNumber];
            }
        });

        this.form.clearError('ticketNumbers');
    }

    private validateForm(): boolean {
        if (!this.form.validate(ParticipantSchema)) return false;

        if (this.selectedTickets().length === 0) {
            this.form.errors.update(e => ({ ...e, ticketNumbers: 'Selecione pelo menos um número' }));
            return false;
        }

        return true;
    }

    submitForm() {
        if (!this.validateForm()) return;

        if (this.authService.hasAnyRole(['ADMIN', 'OPERATOR'])) {
            this.confirmPurchase(false);
            return;
        }

        const dialog = this.paymentDialog()?.nativeElement;
        if (dialog) {
            dialog.showModal();
        }
    }

    confirmPurchase(payNow: boolean) {
        this.isProcessingPurchase.set(true);

        const data: CreateParticipantDto = {
            participantName: this.form.model().participantName,
            phone: this.form.model().phone.replace(/\D/g, ''),
            document: this.form.model().cpf.replace(/\D/g, ''),
            raffleId: this.raffleId(),
            ticketNumbers: this.selectedTickets()
        };

        this.participantService.createParticipant(data)
            .pipe(finalize(() => this.isProcessingPurchase.set(false)))
            .subscribe({
                next: (response) => {
                    this.alertComponent.successToast(response.message);
                    this.resetForm();
                    this.refreshTrigger.update(v => v + 1);
                    
                    const isAdminOrOperator = this.authService.hasAnyRole(['ADMIN', 'OPERATOR']);
                    
                    if(!isAdminOrOperator && payNow) {
                        this.cookieService.setParticipantId(response.data.participantId);
                        this.router.navigate([`payment/${response.data.participantId}`]);
                    }
                }
            });
    }

    private resetForm(): void {
        this.form.reset({
            participantName: '',
            phone: '',
            cpf: ''
        });
        this.selectedTickets.set([]);
    }
}
