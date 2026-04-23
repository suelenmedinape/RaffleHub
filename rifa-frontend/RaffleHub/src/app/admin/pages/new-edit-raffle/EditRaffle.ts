import {Component, inject, OnInit, signal} from '@angular/core';
import {FormsModule} from "@angular/forms";
import {NewEditRaffleComponent} from "./new-edit-raffle-component";
import {CreateRaffle, RaffleSchema} from "../../../models/raffle-model";
import {InputComponent} from "../../../components/ui/input/input-component";
import {FormHelper} from "../../../core/form-helper";
import {ApiResponseModel} from "../../../models/api-response-model";
import {HttpErrorResponse} from "@angular/common/http";
import { ApiRoutesService } from '../../../core/api-routes-service';

@Component({
    selector: 'app-edit-raffle',
    imports: [InputComponent, FormsModule],
    templateUrl: './new-edit-raffle-component.html',
    styleUrl: './new-edit-raffle-component.css',
})
export class EditRaffle extends NewEditRaffleComponent<Partial<CreateRaffle>> implements OnInit {
    protected override isEdit = signal(true);
    private raffleId: string | null = null;

    private readonly url = inject(ApiRoutesService);
    
    protected override form = new FormHelper<Partial<CreateRaffle>>({
        raffleName: '',
        ticketPrice: 0,
        drawDate: '',
        description: '',
        totalTickets: 0
    });

    ngOnInit(): void {
        this.raffleId = this.route.snapshot.paramMap.get('id');
        if (this.raffleId) {
            this.loadRaffleInfo(this.raffleId);
        }
    }

    private loadRaffleInfo(id: string): void {
        this.raffleService.listById(id).subscribe({
            next: (response) => {
                const data = response.data;
                this.form.reset({
                    raffleName: data.raffleName,
                    ticketPrice: data.ticketPrice,
                    drawDate: data.drawDate ? new Date(data.drawDate).toISOString().slice(0, 16) : '',
                    description: data.description,
                    totalTickets: data.totalTickets
                });
                if (data.imageUrl) {
                    const fullUrl = data.imageUrl.startsWith('http') 
                        ? data.imageUrl 
                        : `${this.url.baseApiUrl}/${data.imageUrl}`;
                    this.filePreview.set(fullUrl);
                }
            },
            error: (error: HttpErrorResponse) => {
                this.alertComponent.errorToast(this.errorHandle.getErrorMessage(error));
                this.router.navigate(['/admin/dashboard/list-raffles']);
            }
        });
    }

    override submitForm(): void {
        if (this.validateForm() && this.raffleId) {
            this.handleEditRaffle();
        } else {
            this.alertComponent.errorToast('Por favor, preencha todos os campos obrigatórios corretamente.');
        }
    }

    protected override validateForm(): boolean {
        return this.form.validate(RaffleSchema);
    }

    private handleEditRaffle(): void {
        const model = this.form.model();
        
        const payload: Partial<CreateRaffle> = {
            raffleName: model.raffleName,
            description: model.description,
            totalTickets: model.totalTickets,
            ticketPrice: model.ticketPrice,
            drawDate: new Date(model.drawDate!).toISOString(),
        };

        this.raffleService.updateRaffle(this.raffleId!, payload)
            .subscribe({
                next: (response: ApiResponseModel<void>)=> {
                    this.alertComponent.successToast(response.message);
                    this.router.navigate(['/admin/dashboard/list-raffles']);
                },
                error: (error: HttpErrorResponse)=> {
                    this.alertComponent.errorToast(this.errorHandle.getErrorMessage(error));
                }
            });
    }
}
