import { Component, inject } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { StateService } from '../../core/state.service';

@Component({
  selector: 'app-login',
  imports: [RouterLink, FormsModule, CommonModule],
  templateUrl: './login.html'
})
export class Login {
  private router = inject(Router);
  private stateService = inject(StateService);

  currentTab: 'login' | 'register' = 'login';
  
  imfId = '';
  email = '';
  password = '';

  // Registration bindings
  regName = '';
  regNuit = '';
  regEmail = '';
  regAdminName = '';
  regAdminPhone = '';
  regProvince = '';
  regCity = '';
  regAddress = '';
  regPassword = '';
  
  registeredCode = '';

  // Payment Modal controls
  showPaymentModal = false;
  paymentMethod: 'mpesa' | 'card' | 'bank' = 'mpesa';
  paymentPhone = '';
  paymentCardNumber = '';
  paymentCardExpiry = '';
  paymentCardCvv = '';
  paymentSimulating = false;

  switchTab(tab: 'login' | 'register') {
    this.currentTab = tab;
    if (tab === 'login') {
      this.registeredCode = '';
    }
  }

  onImfIdInput() {
    const id = this.imfId.trim().toLowerCase();
    let found = false;
    
    this.stateService.imfs$.subscribe(imfs => {
      const imf = imfs.find(i => i.id.toLowerCase() === id);
      if (imf) {
        this.stateService.applyImfTheme(imf);
        found = true;
      }
    }).unsubscribe();

    if (!found) {
      if (typeof document !== 'undefined' && document.documentElement) {
        document.documentElement.style.setProperty('--color-primary', '#64748b'); // Neutral Slate
        document.documentElement.style.setProperty('--color-primary-dark', '#475569');
      }
    }
  }

  onSubmitLogin() {
    const id = this.imfId.trim().toLowerCase();
    let imfExists = false;
    
    this.stateService.imfs$.subscribe(imfs => {
      imfExists = imfs.some(i => i.id.toLowerCase() === id);
    }).unsubscribe();

    if (!imfExists) {
      alert('Código da IMF inválido! Por favor, insira um código válido (ex: IMF-20260526KD, IMF-20260526SC ou o código gerado na sua subscrição).');
      return;
    }

    if (!this.email || !this.password) {
      alert('Por favor, preencha todos os campos.');
      return;
    }

    this.stateService.switchImf(id);
    this.router.navigate(['/admin/dashboard']);
  }

  onSubmitRegister() {
    if (!this.regName || !this.regNuit || !this.regEmail || !this.regAdminName || !this.regAdminPhone || !this.regProvince || !this.regCity || !this.regAddress || !this.regPassword) {
      alert('Por favor, preencha todos os campos.');
      return;
    }
    
    this.paymentPhone = this.regAdminPhone;
    this.showPaymentModal = true;
  }

  onConfirmPayment() {
    this.paymentSimulating = true;
    setTimeout(() => {
      this.paymentSimulating = false;
      this.showPaymentModal = false;
      
      try {
        const fullAddress = `${this.regProvince}, ${this.regCity}, ${this.regAddress}`;
        const newImf = this.stateService.registerIMF(
          this.regName,
          this.regNuit,
          this.regEmail,
          this.regAdminPhone,
          this.regCity,
          fullAddress,
          this.regAdminName
        );
        this.registeredCode = newImf.id.toUpperCase();
        
        // Immediate login redirection
        this.imfId = this.registeredCode;
        this.email = this.regEmail;
        this.password = this.regPassword;
        this.onImfIdInput();
        this.stateService.switchImf(this.registeredCode);
        
        this.stateService.updateCurrentUser({
          name: this.regAdminName,
          email: this.regEmail,
          phone: this.regAdminPhone,
          role: 'Admin'
        });

        alert('Subscrição e Pagamento concluídos com sucesso! Bem-vindo ao Kuenda.');

        // Clear bindings
        this.regName = '';
        this.regNuit = '';
        this.regEmail = '';
        this.regAdminName = '';
        this.regAdminPhone = '';
        this.regProvince = '';
        this.regCity = '';
        this.regAddress = '';
        this.regPassword = '';
        this.registeredCode = '';

        this.router.navigate(['/admin/dashboard']);
      } catch (error) {
        alert('Ocorreu um erro ao registar a IMF. Por favor, tente novamente.');
      }
    }, 1500);
  }

  closePaymentModal() {
    this.showPaymentModal = false;
  }
}
