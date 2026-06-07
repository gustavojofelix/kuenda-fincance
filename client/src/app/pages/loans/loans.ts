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

  // Pagination
  currentPage = 1;
  pageSize = 5;
  totalPages = 1;
  totalCount = 0;
  Math = Math;

  getFiltered(loans: Loan[] | null): Loan[] {
    if (!loans) {
      this.totalCount = 0;
      this.totalPages = 1;
      return [];
    }
    const filtered = loans.filter(l => {
      const matchSearch = !this.searchTerm || l.clientName.toLowerCase().includes(this.searchTerm.toLowerCase()) || l.id.toLowerCase().includes(this.searchTerm.toLowerCase());
      const matchStatus = this.selectedStatus === 'Todos' || l.status === this.selectedStatus;
      return matchSearch && matchStatus;
    });

    this.totalCount = filtered.length;
    this.totalPages = Math.ceil(this.totalCount / this.pageSize) || 1;

    if (this.currentPage > this.totalPages) {
      this.currentPage = this.totalPages;
    }

    const startIndex = (this.currentPage - 1) * this.pageSize;
    return filtered.slice(startIndex, startIndex + this.pageSize);
  }

  previousPage() {
    if (this.currentPage > 1) {
      this.currentPage--;
    }
  }

  nextPage() {
    if (this.currentPage < this.totalPages) {
      this.currentPage++;
    }
  }

  goToPage(page: number) {
    this.currentPage = page;
  }

  // Modals state
  showDisburseModal = false;
  showPaymentModal = false;
  selectedLoan: Loan | null = null;
  paymentAmount: number = 0;
  selectedChannel: 'M-Pesa' | 'E-Mola' | 'Banco' = 'M-Pesa';

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
    this.selectedChannel = 'M-Pesa';
    this.showPaymentModal = true;
  }

  confirmPayment() {
    if (this.selectedLoan && this.paymentAmount > 0) {
      this.stateService.registerPayment(this.selectedLoan.id, this.paymentAmount, this.selectedChannel);
      this.showPaymentModal = false;
      this.selectedLoan = null;
    }
  }
}
