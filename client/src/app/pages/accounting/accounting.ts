import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { StateService, Transaction } from '../../core/state.service';
import { map } from 'rxjs';

@Component({
  selector: 'app-accounting',
  imports: [CommonModule, FormsModule],
  templateUrl: './accounting.html'
})
export class Accounting {
  stateService = inject(StateService);

  // KPIs
  cashBalance$ = this.stateService.cashBalance$;
  monthlyInflow$ = this.stateService.monthlyInflow$;
  monthlyOutflow$ = this.stateService.monthlyOutflow$;
  
  transactions$ = this.stateService.transactions$;

  // Pagination
  currentPage = 1;
  pageSize = 10;
  totalCount = 0;
  totalPages = 1;
  Math = Math;

  // Filters
  searchTerm = '';
  selectedCategory = 'Todas';

  // Modal State
  showModal = false;
  newTx: Partial<Transaction> = this.resetForm();

  goToPage(page: number) {
    this.currentPage = page;
  }

  nextPage() {
    if (this.currentPage < this.totalPages) {
      this.currentPage++;
    }
  }

  previousPage() {
    if (this.currentPage > 1) {
      this.currentPage--;
    }
  }

  getPagedTransactions(transactions: Transaction[] | null): Transaction[] {
    if (!transactions) return [];
    const filtered = this.getFiltered(transactions);
    this.totalCount = filtered.length;
    this.totalPages = Math.ceil(this.totalCount / this.pageSize) || 1;
    
    // Reset page index if filters reduce count below current page limit
    if (this.currentPage > this.totalPages) {
      this.currentPage = 1;
    }
    
    const start = (this.currentPage - 1) * this.pageSize;
    return filtered.slice(start, start + this.pageSize);
  }

  // Dynamic Categories
  inflowCategories = ['Amortização', 'Receita Juros', 'Injeção de Capital', 'Outros'];
  outflowCategories = ['Salários', 'Energia', 'Água', 'Renda', 'Impostos', 'Empréstimo', 'Tecnologia', 'Outros'];

  getFiltered(transactions: Transaction[] | null): Transaction[] {
    if (!transactions) return [];
    return transactions.filter(t => {
      const matchSearch = !this.searchTerm || t.description.toLowerCase().includes(this.searchTerm.toLowerCase());
      const matchCategory = this.selectedCategory === 'Todas' || t.category === this.selectedCategory;
      return matchSearch && matchCategory;
    });
  }

  saveTransaction() {
    if (this.newTx.amount && this.newTx.description) {
      this.stateService.addManualTransaction(this.newTx);
      this.showModal = false;
      this.newTx = this.resetForm();
    }
  }

  changeType(type: 'Entrada' | 'Saída') {
      this.newTx.type = type;
      // Reset category to the first one available for the new type
      this.newTx.category = type === 'Entrada' ? (this.inflowCategories[0] as any) : (this.outflowCategories[0] as any);
  }

  private resetForm(): Partial<Transaction> {
    return {
      description: '',
      amount: 0,
      category: 'Salários',
      type: 'Saída',
      date: new Date().toISOString().split('T')[0]
    };
  }
}
