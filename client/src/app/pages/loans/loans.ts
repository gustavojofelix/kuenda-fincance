import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { StateService, Loan } from '../../core/state.service';

@Component({
  selector: 'app-loans',
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './loans.html'
})
export class Loans {
  stateService = inject(StateService);
  
  // KPIs
  totalPortfolio$ = this.stateService.totalActivePortfolio$;
  dueToday$ = this.stateService.dueTodayCount$;
  par30$ = this.stateService.par30Rate$;
  
  loans$ = this.stateService.loans$;

  // Filters
  searchTerm = '';
  selectedStatus = 'Todos';

  getFiltered(loans: Loan[] | null): Loan[] {
    if (!loans) return [];
    return loans.filter(l => {
      const matchSearch = !this.searchTerm || l.clientName.toLowerCase().includes(this.searchTerm.toLowerCase()) || l.id.toLowerCase().includes(this.searchTerm.toLowerCase());
      const matchStatus = this.selectedStatus === 'Todos' || l.status === this.selectedStatus;
      return matchSearch && matchStatus;
    });
  }

  // Modals state
  showDisburseModal = false;
  showPaymentModal = false;
  selectedLoan: Loan | null = null;
  paymentAmount: number = 0;

  openDisburse(loan: Loan) {
    this.selectedLoan = loan;
    this.showDisburseModal = true;
  }

  confirmDisbursement(method: 'M-Pesa' | 'E-Mola' | 'Banco') {
    if (this.selectedLoan) {
      this.stateService.disburseLoan(this.selectedLoan.id, method);
      this.showDisburseModal = false;
      this.selectedLoan = null;
    }
  }

  openPayment(loan: Loan) {
    this.selectedLoan = loan;
    this.paymentAmount = loan.monthlyPayment;
    this.showPaymentModal = true;
  }

  confirmPayment() {
    if (this.selectedLoan && this.paymentAmount > 0) {
      this.stateService.registerPayment(this.selectedLoan.id, this.paymentAmount);
      this.showPaymentModal = false;
      this.selectedLoan = null;
    }
  }
}
