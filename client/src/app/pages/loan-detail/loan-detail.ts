import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { StateService, Loan } from '../../core/state.service';
import { Observable, map } from 'rxjs';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-loan-detail',
  imports: [CommonModule, RouterLink, FormsModule],
  templateUrl: './loan-detail.html'
})
export class LoanDetail implements OnInit {
  private route = inject(ActivatedRoute);
  private stateService = inject(StateService);

  loan$: Observable<Loan | undefined> = new Observable();
  
  // Payment Modal State
  showPaymentModal = false;
  paymentAmount: number = 0;
  selectedLoanId: string = '';

  ngOnInit() {
    this.route.paramMap.subscribe(params => {
      const id = params.get('id');
      if (id) {
        this.selectedLoanId = id;
        this.loan$ = this.stateService.loans$.pipe(
          map(loans => loans.find(l => l.id === id))
        );
      }
    });
  }

  // Simplified history for mock
  history = [
    { date: '12 Mar 2024', amount: 875, method: 'M-Pesa', status: 'Confirmado' },
    { date: '12 Fev 2024', amount: 875, method: 'M-Pesa', status: 'Confirmado' },
    { date: '12 Jan 2024', amount: 875, method: 'E-Mola', status: 'Confirmado' }
  ];

  openPayment(loan: Loan) {
    this.paymentAmount = loan.monthlyPayment;
    this.showPaymentModal = true;
  }

  getAmortizationSchedule(loan: Loan) {
    const schedule = [];
    const principal = loan.amount;
    const rate = loan.interestRate / 100;
    const term = loan.term;
    const pmt = loan.monthlyPayment;
    
    let remainingBalance = principal;
    
    let startDate = new Date();
    if (loan.date && loan.date !== '-') {
      const parts = loan.date.split(' ');
      if (parts.length === 3) {
        const day = parseInt(parts[0]);
        const year = parseInt(parts[2]);
        const monthsPt: { [key: string]: number } = {
          'jan': 0, 'fev': 1, 'mar': 2, 'abr': 3, 'mai': 4, 'jun': 5,
          'jul': 6, 'ago': 7, 'set': 8, 'out': 9, 'nov': 10, 'dez': 11,
          'jan.': 0, 'fev.': 1, 'mar.': 2, 'abr.': 3, 'mai.': 4, 'jun.': 5,
          'jul.': 6, 'ago.': 7, 'set.': 8, 'out.': 9, 'nov.': 10, 'dez.': 11
        };
        const monthStr = parts[1].toLowerCase();
        const month = monthsPt[monthStr] !== undefined ? monthsPt[monthStr] : 1;
        startDate = new Date(year, month, day);
      }
    }

    for (let i = 1; i <= term; i++) {
      const interestPortion = Math.round((remainingBalance * rate) * 100) / 100;
      const principalPortion = Math.round((pmt - interestPortion) * 100) / 100;
      remainingBalance = Math.max(0, Math.round((remainingBalance - principalPortion) * 100) / 100);
      
      const dueDate = new Date(startDate);
      dueDate.setMonth(startDate.getMonth() + i);
      const dueDateStr = `${dueDate.getDate()} ${dueDate.toLocaleString('pt', { month: 'short' })} ${dueDate.getFullYear()}`;
      
      let status = 'Pendente';
      if (i <= loan.paidInstallments) {
        status = 'Pago';
      } else if (loan.status === 'Atrasado' && i === loan.paidInstallments + 1) {
        status = 'Vencido';
      } else if (loan.status === 'Liquidado') {
        status = 'Pago';
      }

      schedule.push({
        installmentNumber: i,
        dueDate: dueDateStr,
        monthlyPayment: pmt,
        principalPortion,
        interestPortion,
        remainingBalance,
        status
      });
    }
    return schedule;
  }

  confirmPayment() {
    if (this.selectedLoanId && this.paymentAmount > 0) {
      this.stateService.registerPayment(this.selectedLoanId, this.paymentAmount);
      this.showPaymentModal = false;
      alert('Amortização registada com sucesso!');
    }
  }

  printContract() {
      window.print();
  }
}
