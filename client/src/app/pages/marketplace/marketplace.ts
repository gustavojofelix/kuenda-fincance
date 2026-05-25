import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { StateService } from '../../core/state.service';
import { NotificationService } from '../../core/notification.service';

@Component({
  selector: 'app-marketplace',
  imports: [CommonModule, FormsModule],
  templateUrl: './marketplace.html'
})
export class Marketplace {
  private stateService = inject(StateService);
  private notificationService = inject(NotificationService);

  fullName = '';
  business = '';
  phone = '';
  amount: number | null = null;
  submitted = false;

  submitForm(event: Event) {
    event.preventDefault();
    if (this.fullName && this.business && this.phone && this.amount) {
      // Create lead in state
      this.stateService.addClient({
        name: this.fullName,
        business: this.business,
        phone: this.phone,
        income: this.amount.toString(),
        status: 'Avaliação'
      });

      // Notify admin
      this.notificationService.addNotification({
          title: 'Novo Lead Marketplace',
          message: `${this.fullName} solicitou ${this.amount} MZN para o negócio de ${this.business}.`,
          type: 'info',
          link: '/admin/leads'
      });

      this.submitted = true;
    }
  }
}
