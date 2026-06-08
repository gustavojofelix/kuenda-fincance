import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { StateService, IMF, Branch, UserProfile } from '../../core/state.service';
import { NotificationService } from '../../core/notification.service';
import { Observable } from 'rxjs';

export interface TeamUser {
  name: string;
  email: string;
  phone?: string;
  status: 'Ativo' | 'Inativo' | 'Pendente';
  branchRoles: { branchId: number; role: string }[];
}

@Component({
  selector: 'app-settings',
  imports: [CommonModule, FormsModule],
  templateUrl: './settings.html'
})
export class Settings implements OnInit {
  private stateService = inject(StateService);
  private notificationService = inject(NotificationService);

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
  users: TeamUser[] = [
    { 
      name: 'Carlos Administrador', 
      email: 'carlos@kuenda.co.mz', 
      phone: '+258 84 111 2222',
      status: 'Ativo', 
      branchRoles: [
        { branchId: 1, role: 'Administrador' },
        { branchId: 2, role: 'Administrador' }
      ]
    },
    { 
      name: 'Ana Agente', 
      email: 'ana@kuenda.co.mz', 
      phone: '+258 84 333 4444',
      status: 'Ativo', 
      branchRoles: [
        { branchId: 1, role: 'Agente de Crédito' }
      ]
    },
    { 
      name: 'Pedro Gestor', 
      email: 'pedro@kuenda.co.mz', 
      phone: '+258 84 555 6666',
      status: 'Pendente', 
      branchRoles: [
        { branchId: 2, role: 'Gestor de Risco' }
      ]
    }
  ];

  // User Management Modal and Form states
  showUserModal = false;
  isEditingUser = false;
  selectedUser: TeamUser = { name: '', email: '', phone: '', status: 'Pendente', branchRoles: [] };
  userErrors: { [key: string]: string } = {};
  allBranches: Branch[] = [];
  standardRoles = ['Agente de Crédito', 'Gestor de Risco', 'Administrador', 'Gerente de Agência', 'Tesoureiro'];

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

    this.branches$.subscribe(b => {
      this.allBranches = b;
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
    this.notificationService.showToast('Branding Guardado', 'Branding e perfil da IMF atualizados globalmente!', 'success');
  }

  saveCredit() {
    this.notificationService.showToast('Parâmetros Guardados', 'Parâmetros de crédito actualizados globalmente.', 'success');
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
      this.notificationService.showToast('Campos em Falta', 'Por favor, preencha o Nome e o E-mail.', 'warning');
      return;
    }
    this.stateService.updateCurrentUser(this.currentUser);
    this.notificationService.showToast('Perfil Atualizado', 'Perfil do utilizador atualizado com sucesso!', 'success');
  }

  changePassword() {
    if (!this.currentPassword || !this.newPassword || !this.confirmPassword) {
      this.notificationService.showToast('Campos em Falta', 'Por favor, preencha todos os campos de palavra-passe.', 'warning');
      return;
    }
    if (this.newPassword !== this.confirmPassword) {
      this.notificationService.showToast('Erro de Validação', 'A nova palavra-passe e a sua confirmação não coincidem.', 'warning');
      return;
    }
    this.notificationService.showToast('Sucesso', 'Palavra-passe alterada com sucesso!', 'success');
    this.currentPassword = '';
    this.newPassword = '';
    this.confirmPassword = '';
  }

  getBranchName(branchId: number): string {
    const found = this.allBranches.find(b => b.id === branchId);
    return found ? found.name : `Agência #${branchId}`;
  }

  openInviteUser() {
    this.isEditingUser = false;
    this.selectedUser = { name: '', email: '', phone: '', status: 'Pendente', branchRoles: [] };
    this.userErrors = {};
    this.showUserModal = true;
  }

  openEditUser(user: TeamUser) {
    this.isEditingUser = true;
    this.selectedUser = { 
      ...user, 
      branchRoles: user.branchRoles.map(br => ({ ...br })) 
    };
    this.userErrors = {};
    this.showUserModal = true;
  }

  hasBranch(branchId: number): boolean {
    return this.selectedUser.branchRoles.some(br => br.branchId === branchId);
  }

  toggleUserBranch(branchId: number) {
    const idx = this.selectedUser.branchRoles.findIndex(br => br.branchId === branchId);
    if (idx > -1) {
      this.selectedUser.branchRoles.splice(idx, 1);
    } else {
      this.selectedUser.branchRoles.push({ branchId, role: 'Agente de Crédito' });
    }
  }

  getBranchRole(branchId: number): string {
    const found = this.selectedUser.branchRoles.find(br => br.branchId === branchId);
    return found ? found.role : '';
  }

  setBranchRole(branchId: number, role: string) {
    const found = this.selectedUser.branchRoles.find(br => br.branchId === branchId);
    if (found) {
      found.role = role;
    }
  }

  validateUserForm(): boolean {
    this.userErrors = {};
    let isValid = true;

    if (!this.selectedUser.name || this.selectedUser.name.trim().length < 3) {
      this.userErrors['name'] = 'O nome completo deve ter pelo menos 3 caracteres.';
      isValid = false;
    }
    
    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    if (!this.selectedUser.email || !emailRegex.test(this.selectedUser.email)) {
      this.userErrors['email'] = 'Introduza um e-mail profissional válido.';
      isValid = false;
    }

    if (this.selectedUser.phone && this.selectedUser.phone.trim().length < 9) {
      this.userErrors['phone'] = 'O telemóvel deve ter pelo menos 9 algarismos.';
      isValid = false;
    }

    if (this.selectedUser.branchRoles.length === 0) {
      this.userErrors['branches'] = 'Deve associar o utilizador a pelo menos uma agência.';
      isValid = false;
    }

    return isValid;
  }

  saveUser() {
    if (!this.validateUserForm()) {
      return;
    }

    if (this.isEditingUser) {
      const idx = this.users.findIndex(u => u.email === this.selectedUser.email);
      if (idx > -1) {
        this.users[idx] = { ...this.selectedUser };
      }
    } else {
      if (this.users.some(u => u.email.toLowerCase() === this.selectedUser.email.toLowerCase())) {
        this.userErrors['email'] = 'Já existe um utilizador registado com este e-mail.';
        return;
      }
      this.users.push({ ...this.selectedUser });
    }
    this.showUserModal = false;
  }

  toggleUserStatus(user: TeamUser) {
    user.status = user.status === 'Ativo' ? 'Inativo' : 'Ativo';
  }

  deleteUser(user: TeamUser) {
    if (confirm(`Tem a certeza que deseja remover o utilizador ${user.name}?`)) {
      this.users = this.users.filter(u => u.email !== user.email);
    }
  }
}
