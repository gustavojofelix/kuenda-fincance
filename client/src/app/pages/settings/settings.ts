import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { StateService, IMF, Branch, UserProfile } from '../../core/state.service';
import { Observable } from 'rxjs';

@Component({
  selector: 'app-settings',
  imports: [CommonModule, FormsModule],
  templateUrl: './settings.html'
})
export class Settings implements OnInit {
  private stateService = inject(StateService);

  activeTab: 'perfil' | 'credito' | 'agencias' | 'equipa' | 'utilizador' = 'perfil';

  imf: IMF = { id: '', name: '', nuit: '', email: '', phone: '', address: '', logoUrl: '', primaryColor: '#10b981', secondaryColor: '#059669' };
  branches$: Observable<Branch[]> = this.stateService.branches$;

  // Branch Management Modal and Form states
  showBranchModal = false;
  showBranchDetailsModal = false;
  isEditingBranch = false;
  selectedBranch: Branch = { id: 0, imfId: '', name: '', city: '', address: '', manager: '', phone: '', email: '', status: 'Ativa' };
  branchErrors: { [key: string]: string } = {};

  // User Profile bindings
  currentUser: UserProfile = { name: '', role: '', email: '', phone: '', photoUrl: '' };
  
  // Password change bindings
  currentPassword = '';
  newPassword = '';
  confirmPassword = '';
  
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

    this.stateService.currentUser$.subscribe(u => {
      if (u) {
        this.currentUser = { ...u };
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

  openAddBranch() {
    this.isEditingBranch = false;
    this.selectedBranch = { id: 0, imfId: '', name: '', city: '', address: '', manager: '', phone: '', email: '', status: 'Ativa' };
    this.branchErrors = {};
    this.showBranchModal = true;
  }

  openEditBranch(branch: Branch) {
    this.isEditingBranch = true;
    this.selectedBranch = { ...branch };
    this.branchErrors = {};
    this.showBranchModal = true;
  }

  openBranchDetails(branch: Branch) {
    this.selectedBranch = { ...branch };
    this.showBranchDetailsModal = true;
  }

  validateBranchForm(): boolean {
    this.branchErrors = {};
    let isValid = true;
    
    if (!this.selectedBranch.name || this.selectedBranch.name.trim().length < 3) {
      this.branchErrors['name'] = 'O nome da agência deve ter pelo menos 3 caracteres.';
      isValid = false;
    }
    if (!this.selectedBranch.city || this.selectedBranch.city.trim().length === 0) {
      this.branchErrors['city'] = 'A cidade é obrigatória.';
      isValid = false;
    }
    if (!this.selectedBranch.address || this.selectedBranch.address.trim().length === 0) {
      this.branchErrors['address'] = 'O endereço completo é obrigatório.';
      isValid = false;
    }
    if (!this.selectedBranch.manager || this.selectedBranch.manager.trim().length === 0) {
      this.branchErrors['manager'] = 'O nome do responsável é obrigatório.';
      isValid = false;
    }
    if (this.selectedBranch.email && !/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(this.selectedBranch.email)) {
      this.branchErrors['email'] = 'Introduza um e-mail válido (ex: agência@dominio.com).';
      isValid = false;
    }
    if (!this.selectedBranch.phone || this.selectedBranch.phone.trim().length < 9) {
      this.branchErrors['phone'] = 'O contacto telefónico deve ter pelo menos 9 algarismos.';
      isValid = false;
    }
    
    return isValid;
  }

  saveBranch() {
    if (!this.validateBranchForm()) {
      return;
    }
    
    if (this.isEditingBranch) {
      this.stateService.updateBranch(this.selectedBranch);
    } else {
      this.stateService.addBranch(this.selectedBranch);
    }
    this.showBranchModal = false;
  }

  toggleBranchStatus(branch: Branch) {
    const updated = { ...branch, status: branch.status === 'Ativa' ? 'Inativa' as const : 'Ativa' as const };
    this.stateService.updateBranch(updated);
  }

  deleteBranch(branch: Branch) {
    if (confirm(`Tem a certeza que deseja apagar a agência ${branch.name}?`)) {
      this.stateService.deleteBranch(branch.id);
    }
  }

  onPhotoSelected(event: any) {
    const file = event.target.files[0];
    if (file) {
      const reader = new FileReader();
      reader.onload = (e: any) => {
        this.currentUser.photoUrl = e.target.result;
      };
      reader.readAsDataURL(file);
    }
  }

  removePhoto() {
    this.currentUser.photoUrl = '';
  }

  onLogoSelected(event: any) {
    const file = event.target.files[0];
    if (file) {
      const reader = new FileReader();
      reader.onload = (e: any) => {
        this.imf.logoUrl = e.target.result;
      };
      reader.readAsDataURL(file);
    }
  }

  removeLogo() {
    this.imf.logoUrl = '';
  }

  saveUserProfile() {
    if (!this.currentUser.name || !this.currentUser.email) {
      alert('Por favor, preencha o Nome e o E-mail.');
      return;
    }
    this.stateService.updateCurrentUser(this.currentUser);
    alert('Perfil do utilizador atualizado com sucesso!');
  }

  changePassword() {
    if (!this.currentPassword || !this.newPassword || !this.confirmPassword) {
      alert('Por favor, preencha todos os campos de palavra-passe.');
      return;
    }
    if (this.newPassword !== this.confirmPassword) {
      alert('A nova palavra-passe e a sua confirmação não coincidem.');
      return;
    }
    alert('Palavra-passe alterada com sucesso!');
    this.currentPassword = '';
    this.newPassword = '';
    this.confirmPassword = '';
  }
}
