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

  // Filters
  searchTerm = '';
  selectedCategory = 'Todas';

  // Modal State
  showModal = false;
  newTx: Partial<Transaction> = this.resetForm();

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
