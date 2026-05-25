import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { StateService } from '../../core/state.service';
import { map } from 'rxjs';

@Component({
  selector: 'app-dashboard',
  imports: [CommonModule, RouterLink],
  templateUrl: './dashboard.html'
})
export class Dashboard {
  stateService = inject(StateService);
  
  // High-Level KPIs
  totalPortfolio$ = this.stateService.totalActivePortfolio$;
  cashBalance$ = this.stateService.cashBalance$;
  par30Rate$ = this.stateService.par30Rate$;
  totalClients$ = this.stateService.totalClientsCount$;
  monthlyInflow$ = this.stateService.monthlyInflow$;
  monthlyOutflow$ = this.stateService.monthlyOutflow$;

  // Privacy Mode
  privacyMode = false;

  recentLoans$ = this.stateService.loans$.pipe(
    map(loans => loans.slice(0, 5))
  );

  // Simplified chart data for SVG
  // Represents 6 months of cash flow
  cashFlowData = [
    { month: 'Jan', inflow: 45000, outflow: 30000 },
    { month: 'Fev', inflow: 52000, outflow: 35000 },
    { month: 'Mar', inflow: 48000, outflow: 42000 },
    { month: 'Abr', inflow: 61000, outflow: 38000 },
    { month: 'Mai', inflow: 55000, outflow: 40000 },
    { month: 'Jun', inflow: 68000, outflow: 45000 },
  ];

  togglePrivacy() {
    this.privacyMode = !this.privacyMode;
  }

  simulate30Days() {
    this.stateService.simulate30DaysLater();
    alert('Simulação de 30 dias concluída! Os contratos ativos venceram e o risco foi recalculado.');
  }

  getMaskedValue(value: any): string {
    if (this.privacyMode) return '••••••';
    if (typeof value === 'number') return value.toLocaleString('pt-PT') + ' MZN';
    return value;
  }
}
