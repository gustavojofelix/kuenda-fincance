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

  ngOnInit() {
    this.route.paramMap.subscribe(params => {
      const id = params.get('id');
      if (id) {
        this.isEditMode = true;
        this.clientId = Number(id);
        this.stateService.getClientById(this.clientId).subscribe(data => {
          if (data) {
            this.client = { ...data };
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
      emergencyPhone: ''
    };
  }
}
