import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { StateService, Client } from '../../core/state.service';
import { map } from 'rxjs';

@Component({
  selector: 'app-leads',
  imports: [CommonModule],
  templateUrl: './leads.html'
})
export class Leads {
  private stateService = inject(StateService);
  private router = inject(Router);

  // Filter clients who came from marketplace (status: Avaliação)
  leads$ = this.stateService.clients$.pipe(
    map(clients => clients.filter(c => c.status === 'Avaliação'))
  );

  // KPI calculations
  totalLeads$ = this.leads$.pipe(map(ls => ls.length));
  pipelineValue$ = this.leads$.pipe(
    map(ls => ls.reduce((acc, l) => acc + (parseFloat(l.income || '0') || 0), 0))
  );

  approveLead(id: number) {
    this.stateService.updateClient(id, { status: 'Em Análise' });
    this.router.navigate(['/admin/clients', id]);
  }

  rejectLead(id: number) {
    if (confirm('Tem a certeza que deseja arquivar este lead?')) {
        this.stateService.updateClient(id, { status: 'Arquivado' });
    }
  }
}
