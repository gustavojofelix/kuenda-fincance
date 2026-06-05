import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { StateService, Client } from '../../core/state.service';
import { of } from 'rxjs';

@Component({
  selector: 'app-client-form',
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './client-form.html'
})
export class ClientForm implements OnInit {
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private stateService = inject(StateService);

  isEditMode = false;
  currentStep = 1;
  totalSteps = 4;
  
  clientId: number | null = null;
  client: Partial<Client> = this.resetForm();

  // Campos para novas garantias
  newGuaranteeName = '';
  newGuaranteeValue: number | null = null;
  newGuaranteePhoto = '';

  ngOnInit() {
    this.route.paramMap.subscribe(params => {
      const id = params.get('id');
      if (id) {
        this.isEditMode = true;
        this.clientId = Number(id);
        this.stateService.getClientById(this.clientId).subscribe(data => {
          if (data) {
            this.client = { ...data };
            if (!this.client.guarantees) {
              this.client.guarantees = [];
            }
          }
        });
      }
    });
  }

  nextStep() {
    if (this.currentStep < this.totalSteps) {
      this.currentStep++;
    } else {
      this.save();
    }
  }

  prevStep() {
    if (this.currentStep > 1) {
      this.currentStep--;
    }
  }

  save() {
    if (this.isEditMode && this.clientId) {
      this.stateService.updateClient(this.clientId, this.client);
    } else {
      this.stateService.addClient(this.client);
    }
    this.router.navigate(['/admin/clients']);
  }

  onGuaranteePhotoSelected(event: Event) {
    const input = event.target as HTMLInputElement;
    if (input.files && input.files[0]) {
      const reader = new FileReader();
      reader.onload = () => {
        this.newGuaranteePhoto = reader.result as string;
      };
      reader.readAsDataURL(input.files[0]);
    }
  }

  addGuarantee() {
    if (!this.newGuaranteeName || this.newGuaranteeValue === null) {
      alert('Por favor, preencha o nome e o valor da garantia.');
      return;
    }
    if (!this.client.guarantees) {
      this.client.guarantees = [];
    }
    this.client.guarantees.push({
      name: this.newGuaranteeName,
      value: this.newGuaranteeValue,
      photoUrl: this.newGuaranteePhoto || ''
    });
    // Limpar campos
    this.newGuaranteeName = '';
    this.newGuaranteeValue = null;
    this.newGuaranteePhoto = '';
  }

  removeGuarantee(index: number) {
    if (this.client.guarantees) {
      this.client.guarantees.splice(index, 1);
    }
  }

  private resetForm(): Partial<Client> {
    return { 
      name: '', 
      bi: '', 
      phone: '', 
      maritalStatus: 'Solteiro(a)', 
      province: 'Maputo Cidade', 
      district: 'KaMpfumo', 
      neighborhood: '', 
      address: '', 
      business: 'Mercearia', 
      income: '0 - 10.000 MZN', 
      businessYears: '1 a 3 anos',
      emergencyName: '', 
      emergencyRelation: 'Familiar', 
      emergencyPhone: '',
      guarantees: []
    };
  }
}
