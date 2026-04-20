import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { StateService } from '../../core/state.service';

@Component({
  selector: 'app-reports',
  imports: [CommonModule, FormsModule],
  templateUrl: './reports.html'
})
export class Reports {
  private stateService = inject(StateService);

  totalClients$ = this.stateService.totalClientsCount$;
  totalPortfolio$ = this.stateService.totalActivePortfolio$;

  selectedReport: string | null = null;
  startDate = '2024-03-01';
  endDate = '2024-03-31';
  format = 'Excel (.xlsx)';
  
  isGenerating = false;

  reportCategories = [
    {
      title: 'Regulatórios (Banco de Moçambique)',
      reports: [
        { id: 'MAP10', name: 'MAP 10 - Balancete de Instituições Financeiras', desc: 'Registo mensal de activos e passivos.' },
        { id: 'MAP14', name: 'MAP 14 - Relação de Grandes Exposições', desc: 'Créditos que excedem 10% do capital próprio.' }
      ]
    },
    {
      title: 'Gestão de Crédito',
      reports: [
        { id: 'PORTFOLIO', name: 'Relatório de Carteira Ativa', desc: 'Detalhe de todos os empréstimos em circulação.' },
        { id: 'PAR', name: 'Relatório de PAR > 30 (Atrasos)', desc: 'Análise de risco e créditos em incumprimento.' },
        { id: 'DISBURSE', name: 'Novos Desembolsos por Período', desc: 'Volume de capital injectado no mercado.' }
      ]
    },
    {
      title: 'Financeiro & Contabilidade',
      reports: [
        { id: 'CASHFLOW', name: 'Fluxo de Caixa Consolidado', desc: 'Entradas, saídas e saldo operacional.' },
        { id: 'PL', name: 'Demonstração de Resultados (P&L)', desc: 'Lucros e perdas baseados em juros e custos.' }
      ]
    }
  ];

  exportHistory = [
    { name: 'Relatório de Carteira - Fev', date: '01 Mar 2024', user: 'Admin', format: 'Excel' },
    { name: 'MAP 10 - Janeiro', date: '05 Fev 2024', user: 'Admin', format: 'Excel' }
  ];

  selectReport(id: string) {
    this.selectedReport = id;
  }

  generateReport() {
    if (!this.selectedReport) return;
    
    this.isGenerating = true;
    
    // Simulate generation delay
    setTimeout(() => {
      this.isGenerating = false;
      const reportName = this.reportCategories
        .flatMap(c => c.reports)
        .find(r => r.id === this.selectedReport)?.name || 'Relatório';
        
      this.exportHistory.unshift({
        name: reportName,
        date: new Date().toLocaleDateString('pt-PT', { day: '2-digit', month: 'short', year: 'numeric' }),
        user: 'Admin',
        format: this.format.split(' ')[0]
      });
      this.selectedReport = null;
      alert('Relatório gerado com sucesso! O download iniciará automaticamente.');
    }, 2000);
  }
}
