import {Component, signal} from '@angular/core';
import {FormsModule} from "@angular/forms";
import {NewEditRaffleComponent} from "./new-edit-raffle-component";
import {FormHelper} from "../../../core/form-helper";
import {ApiResponseModel} from "../../../models/api-response-model";
import {HttpErrorResponse} from "@angular/common/http";
import {CreateRaffleDto, RaffleData, RaffleSchema} from "../../../models/raffle-model";
import {InputComponent} from "../../../components/ui/input/input-component";

@Component({
    selector: 'app-new-raffle',
    imports: [InputComponent, FormsModule],
    templateUrl: './new-edit-raffle-component.html',
    styleUrl: './new-edit-raffle-component.css',
})

export class NewRaffle extends NewEditRaffleComponent<CreateRaffleDto>{
    protected override isEdit = signal(false);
    protected override form = new FormHelper<CreateRaffleDto>({
        raffleName: '',
        ticketPrice: 0,
        drawDate: '',
        description: '',
        totalTickets: 0
    });

    override submitForm(): void {
        if (this.validateForm()){
            this.handleNewRaffle();
        } else {
            this.alertComponent.errorToast('Por favor, preencha todos os campos obrigatórios corretamente.');
        }
    }

    protected override validateForm(): boolean {
        return this.form.validate(RaffleSchema);
    }

    private handleNewRaffle(): void {
        const model = this.form.model();
        const dto: CreateRaffleDto = {
            file: this.selectedFile() ?? undefined,
            raffleName: model.raffleName,
            description: model.description,
            totalTickets: model.totalTickets,
            ticketPrice: model.ticketPrice,
            drawDate: new Date(model.drawDate).toISOString(),
        };

        this.raffleService.createRaffle(dto)
            .subscribe({
                next: (response: ApiResponseModel<RaffleData>)=> {
                    this.alertComponent.successToast(response.message);
                    this.form.reset({
                        raffleName: '',
                        ticketPrice: 0,
                        drawDate: '',
                        description: '',
                        totalTickets: 0
                    });
                    this.selectedFile.set(null);
                    this.filePreview.set(null);
                },
                error: (error: HttpErrorResponse)=> {
                    const message = this.errorHandle.getErrorMessage(error);
                    this.alertComponent.errorToast(message);
                    console.log('erro');
                }
            })
    }
}
