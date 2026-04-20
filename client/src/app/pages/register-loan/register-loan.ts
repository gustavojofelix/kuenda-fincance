import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { StateService } from '../../core/state.service';
import { map } from 'rxjs';

@Component({
  selector: 'app-register-loan',
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './register-loan.html'
})
export class RegisterLoan {
  private router = inject(Router);
  private stateService = inject(StateService);

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
    
    // Simplified Price Table Logic for Mock
    this.simulation.totalToPay = amount * (1 + rate);
    this.simulation.monthlyPayment = this.simulation.totalToPay / term;
  }

  submitLoan() {
    if (!this.loanData.clientName) {
        alert('Por favor, selecione um cliente.');
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
