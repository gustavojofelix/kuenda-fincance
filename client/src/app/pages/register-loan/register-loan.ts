import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { StateService } from '../../core/state.service';
import { NotificationService } from '../../core/notification.service';
import { map } from 'rxjs';

@Component({
  selector: 'app-register-loan',
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './register-loan.html'
})
export class RegisterLoan {
  private router = inject(Router);
  private stateService = inject(StateService);
  private notificationService = inject(NotificationService);

  clients$ = this.stateService.clients$;
  
  // Form Model
  loanData = {
    clientName: '',
    amount: 5000,
    interestRate: 5,
    term: 6,
    purpose: 'Capital de Giro'
  };

  // Simulation results
  simulation = {
    totalToPay: 0,
    monthlyPayment: 0
  };

  constructor() {
    this.calculatePrice();
  }

  calculatePrice() {
    const amount = this.loanData.amount;
    const rate = this.loanData.interestRate / 100;
    const term = this.loanData.term;
    
    if (rate === 0) {
      this.simulation.monthlyPayment = amount / term;
    } else {
      this.simulation.monthlyPayment = (amount * rate) / (1 - Math.pow(1 + rate, -term));
    }
    
    this.simulation.monthlyPayment = Math.round(this.simulation.monthlyPayment * 100) / 100;
    this.simulation.totalToPay = Math.round((this.simulation.monthlyPayment * term) * 100) / 100;
  }

  submitLoan() {
    if (!this.loanData.clientName) {
        this.notificationService.showToast('Cliente em Falta', 'Por favor, selecione um cliente.', 'warning');
        return;
    }
    
    this.stateService.addLoan({
        clientName: this.loanData.clientName,
        amount: this.loanData.amount,
        interestRate: this.loanData.interestRate,
        term: this.loanData.term
    });
    
    this.router.navigate(['/admin/loans']);
  }
}
