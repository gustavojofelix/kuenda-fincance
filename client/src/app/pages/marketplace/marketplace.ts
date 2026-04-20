import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common'; // Needed for forms binding in template typically, but using native forms
import { FormsModule } from '@angular/forms';
import { StateService } from '../../core/state.service';

@Component({
  selector: 'app-marketplace',
  imports: [FormsModule],
  templateUrl: './marketplace.html'
})
export class Marketplace {
  private stateService = inject(StateService);
  
  submitted = false;
  
  // Form fields
  fullName = '';
  business = '';
  phone = '';
  amount: number | null = null;

  submitForm(event: Event) {
    event.preventDefault();
    if (!this.fullName || !this.amount) return;

    // Create Customer
    this.stateService.addClient({
      name: this.fullName,
      business: this.business,
      phone: this.phone,
      bi: 'Em Progresso'
    });

    // Create Loan Lead
    this.stateService.addLoan({
      clientName: this.fullName,
      amount: this.amount
    });

    this.submitted = true;
  }
}
