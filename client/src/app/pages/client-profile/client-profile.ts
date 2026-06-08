import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { StateService } from '../../core/state.service';
import { NotificationService } from '../../core/notification.service';
import { switchMap, map } from 'rxjs/operators';
import { of } from 'rxjs';

@Component({
  selector: 'app-client-profile',
  imports: [CommonModule, RouterLink, FormsModule],
  templateUrl: './client-profile.html'
})
export class ClientProfile {
  private route = inject(ActivatedRoute);
  private stateService = inject(StateService);
  private notificationService = inject(NotificationService);

  client$ = this.route.paramMap.pipe(
    switchMap(params => {
      const id = Number(params.get('id'));
      return this.stateService.getClientById(id);
    })
  );

  activeLoan$ = this.client$.pipe(
    switchMap(client => {
      if (!client) return of(null);
      return this.stateService.getLoansByClientName(client.name).pipe(
        map(loans => loans.find(l => l.status === 'Ativo' || l.status === 'Atrasado') || null)
      );
    })
  );

  loans$ = this.client$.pipe(
    switchMap(client => {
      if (!client) return of([]);
      return this.stateService.getLoansByClientName(client.name);
    })
  );

  history$ = this.client$.pipe(
    switchMap(client => {
      if (!client) return of([]);
      return this.stateService.transactions$.pipe(
        map(transactions => 
          transactions.filter(t => t.description.toLowerCase().includes(client.name.toLowerCase()))
        )
      );
    })
  );

  activeTab = 'Informação';

  // Payment Modal State
  showPaymentModal = false;
  paymentAmount: number = 0;
  selectedLoanId: string = '';
  selectedChannel: 'M-Pesa' | 'E-Mola' | 'Banco' = 'M-Pesa';

  openPayment(loan: any) {
    if (!loan) {
      this.notificationService.showToast('Sem Empréstimo', 'Este cliente não possui um empréstimo ativo no momento.', 'warning');
      return;
    }
    this.selectedLoanId = loan.id;
    this.paymentAmount = loan.monthlyPayment;
    this.selectedChannel = 'M-Pesa';
    this.showPaymentModal = true;
  }

  confirmPayment() {
    if (this.selectedLoanId && this.paymentAmount > 0) {
      this.stateService.registerPayment(this.selectedLoanId, this.paymentAmount, this.selectedChannel);
      this.showPaymentModal = false;
      this.notificationService.showToast('Sucesso', 'Amortização registada com sucesso!', 'success');
    }
  }
}
