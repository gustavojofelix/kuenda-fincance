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

  switchTab(tab: 'login' | 'register') {
    this.currentTab = tab;
  }

  onImfIdInput() {
    const id = this.imfId.trim().toLowerCase();
    let found = false;
    
    this.stateService.imfs$.subscribe(imfs => {
      const imf = imfs.find(i => i.id === id);
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
      imfExists = imfs.some(i => i.id === id);
    }).unsubscribe();

    if (!imfExists) {
      alert('Código da IMF inválido! Utilize "kuenda" ou "socinal".');
      return;
    }

    if (!this.email || !this.password) {
      alert('Por favor, preencha todos os campos.');
      return;
    }

    this.stateService.switchImf(id);
    this.router.navigate(['/admin/dashboard']);
  }
}
