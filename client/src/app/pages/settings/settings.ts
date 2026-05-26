import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { StateService, IMF, Branch } from '../../core/state.service';
import { Observable } from 'rxjs';

@Component({
  selector: 'app-settings',
  imports: [CommonModule, FormsModule],
  templateUrl: './settings.html'
})
export class Settings implements OnInit {
  private stateService = inject(StateService);

  activeTab: 'perfil' | 'credito' | 'agencias' | 'equipa' = 'perfil';

  imf: IMF = { id: '', name: '', nuit: '', email: '', phone: '', address: '', primaryColor: '#10b981', secondaryColor: '#059669' };
  branches$: Observable<Branch[]> = this.stateService.branches$;
  
  // Curated SaaS Color Presets
  colorPresets = [
    { name: 'Esmeralda', primary: '#10b981', secondary: '#059669', bgClass: 'bg-emerald-500' },
    { name: 'Azul Real', primary: '#2563eb', secondary: '#1d4ed8', bgClass: 'bg-blue-600' },
    { name: 'Coral Tinto', primary: '#f43f5e', secondary: '#e11d48', bgClass: 'bg-rose-500' },
    { name: 'Ametista', primary: '#8b5cf6', secondary: '#7c3aed', bgClass: 'bg-purple-500' }
  ];

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
    this.stateService.activeImf$.subscribe(i => {
      if (i) {
        this.imf = { ...i };
      }
    });
  }

  selectPreset(preset: { primary: string, secondary: string }) {
    this.imf.primaryColor = preset.primary;
    this.imf.secondaryColor = preset.secondary;
    this.stateService.applyImfTheme(this.imf);
  }

  onCustomColorChange() {
    this.imf.secondaryColor = this.imf.primaryColor;
    this.stateService.applyImfTheme(this.imf);
  }

  saveProfile() {
    this.stateService.updateIMF(this.imf);
    alert('Branding e perfil da IMF atualizados globalmente!');
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
