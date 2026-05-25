import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { StateService, IMFProfile, Branch } from '../../core/state.service';
import { Observable } from 'rxjs';

@Component({
  selector: 'app-settings',
  imports: [CommonModule, FormsModule],
  templateUrl: './settings.html'
})
export class Settings implements OnInit {
  private stateService = inject(StateService);

  activeTab: 'perfil' | 'credito' | 'agencias' | 'equipa' = 'perfil';

  profile: IMFProfile = { name: '', nuit: '', email: '', phone: '', address: '' };
  branches$: Observable<Branch[]> = this.stateService.branches$;
  
  // Mock Credit Params
  creditParams = {
    defaultRate: 5,
    lateRate: 2,
    originationFee: 500,
    maxTerm: 24,
    currency: 'MZN'
  };

  // Mock Users
  users = [
    { name: 'Carlos Administrador', role: 'Administrador', email: 'carlos@kuenda.co.mz', status: 'Ativo' },
    { name: 'Ana Agente', role: 'Agente de Crédito', email: 'ana@kuenda.co.mz', status: 'Ativo' },
    { name: 'Pedro Gestor', role: 'Gestor de Risco', email: 'pedro@kuenda.co.mz', status: 'Pendente' }
  ];

  ngOnInit() {
    this.stateService.profile$.subscribe(p => this.profile = { ...p });
  }

  saveProfile() {
    this.stateService.updateProfile(this.profile);
    alert('Perfil da IMF actualizado com sucesso!');
  }

  saveCredit() {
    alert('Parâmetros de crédito actualizados globalmente.');
  }

  addBranch() {
    const name = prompt('Nome da Agência:');
    if (name) {
      this.stateService.addBranch({ name, city: 'Maputo', address: '-', manager: 'Pendente' });
    }
  }
}
