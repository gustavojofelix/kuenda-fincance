import { Component, inject, OnInit } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { StateService } from '../../core/state.service';
import { NotificationService } from '../../core/notification.service';

@Component({
  selector: 'app-login',
  imports: [RouterLink, FormsModule, CommonModule],
  templateUrl: './login.html'
})
export class Login implements OnInit {
  private router = inject(Router);
  private stateService = inject(StateService);
  private notificationService = inject(NotificationService);

  currentTab: 'login' | 'register' = 'login';
  
  imfId = '';
  email = '';
  password = '';
  isSubdomainLocked = false;

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
  showRegPassword = false;
  
  registeredCode = '';

  // Payment Modal controls
  showPaymentModal = false;
  paymentMethod: 'mpesa' | 'card' | 'bank' = 'mpesa';
  paymentPhone = '';
  paymentCardNumber = '';
  paymentCardExpiry = '';
  paymentCardCvv = '';
  paymentSimulating = false;

  // Validation helpers (RegEx)
  emailPattern = /^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,4}$/;
  nuitPattern = /^\d{9}$/;

  ngOnInit() {
    if (typeof window !== 'undefined' && window.location) {
      const hostname = window.location.hostname;
      const parts = hostname.split('.');
      
      // Check if we are on a subdomain (e.g. tenant.kuenda.co.mz or tenant.localhost)
      if (parts.length > 2 || (parts.length === 2 && parts[1] === 'localhost')) {
        const potentialSubdomain = parts[0].trim().toLowerCase();
        
        if (potentialSubdomain !== 'www' && potentialSubdomain !== 'app' && potentialSubdomain !== 'admin') {
          this.stateService.imfs$.subscribe(imfs => {
            const foundImf = imfs.find(i => {
              const cleanedId = i.id.toLowerCase();
              return cleanedId === potentialSubdomain || cleanedId === `imf-${potentialSubdomain}`;
            });

            if (foundImf) {
              this.imfId = foundImf.id.toUpperCase();
              this.isSubdomainLocked = true;
              this.stateService.applyImfTheme(foundImf);
            }
          }).unsubscribe();
        }
      }
    }
  }

  // Inline validators
  isEmailValid(val: string): boolean {
    return this.emailPattern.test(val);
  }

  isNuitValid(val: string): boolean {
    return this.nuitPattern.test(val);
  }

  isPasswordValid(val: string): boolean {
    if (!val || val.length < 8) return false;
    const hasDigit = /\d/.test(val);
    const hasUpper = /[A-Z]/.test(val);
    const hasLower = /[a-z]/.test(val);
    return hasDigit && hasUpper && hasLower;
  }

  isRegisterFormValid(): boolean {
    return !!(
      this.regName.trim() &&
      this.isNuitValid(this.regNuit) &&
      this.regAdminName.trim() &&
      this.isEmailValid(this.regEmail) &&
      this.regAdminPhone.trim() &&
      this.regProvince.trim() &&
      this.regCity.trim() &&
      this.regAddress.trim() &&
      this.isPasswordValid(this.regPassword)
    );
  }

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

    if (!id || !this.email || !this.password) {
      this.notificationService.showToast('Campos em falta', 'Por favor, preencha todos os campos.', 'warning');
      return;
    }

    if (!this.isEmailValid(this.email)) {
      this.notificationService.showToast('E-mail Inválido', 'Por favor, insira um endereço de e-mail válido.', 'warning');
      return;
    }

    this.stateService.login(id, this.email, this.password).subscribe({
      next: () => {
        this.router.navigate(['/admin/dashboard']);
      },
      error: (err) => {
        let errMsg = 'Código da IMF, email ou palavra-passe incorretos.';
        if (err.error?.errors) {
          const messages: string[] = [];
          for (const key in err.error.errors) {
            if (Array.isArray(err.error.errors[key])) {
              messages.push(...err.error.errors[key]);
            }
          }
          if (messages.length > 0) errMsg = messages.join('\n');
        } else {
          errMsg = err.error?.Message || err.error?.message || errMsg;
        }
        this.notificationService.showToast('Erro de Autenticação', errMsg, 'danger');
      }
    });
  }

  onSubmitRegister() {
    if (!this.isRegisterFormValid()) {
      this.notificationService.showToast('Campos Inválidos', 'Por favor, verifique se preencheu todos os campos corretamente.', 'warning');
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
        this.stateService.register(
          this.regName,
          this.regNuit,
          this.regAdminName,
          this.regEmail,
          this.regAdminPhone,
          this.regPassword,
          this.regProvince,
          this.regCity,
          fullAddress
        ).subscribe({
          next: () => {
            this.notificationService.showToast('Subscrição Ativa', 'Subscrição e Pagamento concluídos com sucesso! Bem-vindo ao Kuenda.', 'success');
            
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
          },
          error: (err) => {
            let errMsg = 'Por favor, verifique os dados e tente novamente.';
            if (err.error?.errors) {
              const messages: string[] = [];
              for (const key in err.error.errors) {
                if (Array.isArray(err.error.errors[key])) {
                  messages.push(...err.error.errors[key]);
                }
              }
              if (messages.length > 0) errMsg = messages.join('\n');
            } else {
              errMsg = err.error?.Message || err.error?.message || errMsg;
            }
            this.notificationService.showToast('Erro de Registo', 'Ocorreu um erro ao registar a IMF: ' + errMsg, 'danger');
          }
        });
      } catch (error) {
        this.notificationService.showToast('Erro de Registo', 'Ocorreu um erro ao processar o registo. Por favor, tente novamente.', 'danger');
      }
    }, 1500);
  }

  closePaymentModal() {
    this.showPaymentModal = false;
  }
}
