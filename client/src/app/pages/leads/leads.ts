import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { StateService, Client } from '../../core/state.service';
import { BehaviorSubject, combineLatest, map } from 'rxjs';

@Component({
  selector: 'app-leads',
  imports: [CommonModule, FormsModule],
  templateUrl: './leads.html'
})
export class Leads {
  private stateService = inject(StateService);
  private router = inject(Router);

  activeTab$ = new BehaviorSubject<'Avaliação' | 'Em Análise' | 'Arquivado'>('Avaliação');
  selectedLead: Client | null = null;
  isEditing = false;
  editForm: Partial<Client> = {};

  leads$ = combineLatest([this.stateService.clients$, this.activeTab$]).pipe(
    map(([clients, tab]) => clients.filter(c => {
      const isLead = c.status === 'Avaliação' || c.status === 'Arquivado' || c.requestedTerm !== undefined;
      return isLead && c.status === tab;
    }))
  );

  // KPI calculations (always based on pending leads)
  pendingLeads$ = this.stateService.clients$.pipe(
    map(clients => clients.filter(c => c.status === 'Avaliação'))
  );
  totalLeads$ = this.pendingLeads$.pipe(map(ls => ls.length));
  pipelineValue$ = this.pendingLeads$.pipe(
    map(ls => ls.reduce((acc, l) => acc + (parseFloat(l.income || '0') || 0), 0))
  );

  setTab(tab: 'Avaliação' | 'Em Análise' | 'Arquivado') {
    this.activeTab$.next(tab);
  }

  viewDetails(lead: Client) {
    this.selectedLead = lead;
    this.isEditing = false;
  }

  closeDetails() {
    this.selectedLead = null;
    this.isEditing = false;
  }

  startEdit() {
    if (this.selectedLead) {
      this.editForm = { ...this.selectedLead };
      this.isEditing = true;
    }
  }

  cancelEdit() {
    this.isEditing = false;
  }

  saveEdit() {
    if (this.selectedLead && this.selectedLead.id) {
      // Re-calculate mock score in case business indicators changed
      const years = parseFloat(this.editForm.businessYears || '') || 0;
      const amount = parseFloat(this.editForm.income || '0') || 0;
      const rev = this.editForm.estimatedMonthlyRevenue || 0;
      const phone = this.editForm.phone || '';
      
      let score = 450;
      if (years >= 3) score += 120;
      else if (years >= 1) score += 60;
      
      if (rev > amount * 3) score += 180;
      else if (rev > amount * 1.5) score += 100;
      else if (rev > amount) score += 50;

      if (phone.includes('84') || phone.includes('85')) score += 40;
      else if (phone.includes('86') || phone.includes('87')) score += 30;

      this.editForm.scoreSimulado = Math.min(850, score);

      this.stateService.updateClient(this.selectedLead.id, this.editForm);
      this.selectedLead = { ...this.selectedLead, ...this.editForm } as Client;
      this.isEditing = false;
    }
  }

  approveLead(id: number) {
    this.stateService.updateClient(id, { status: 'Em Análise' });
    this.selectedLead = null;
    this.isEditing = false;
    this.router.navigate(['/admin/clients', id]);
  }

  rejectLead(id: number) {
    if (confirm('Tem a certeza que deseja arquivar este lead?')) {
        this.stateService.updateClient(id, { status: 'Arquivado' });
        this.selectedLead = null;
        this.isEditing = false;
    }
  }
}
