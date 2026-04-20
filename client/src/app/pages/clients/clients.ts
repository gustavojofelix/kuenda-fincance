import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { StateService, Client } from '../../core/state.service';

@Component({
  selector: 'app-clients',
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './clients.html'
})
export class Clients {
  stateService = inject(StateService);
  
  // KPIs
  totalClients$ = this.stateService.totalClientsCount$;
  activeClients$ = this.stateService.activeClientsCount$;
  atRiskClients$ = this.stateService.atRiskClients$;
  clients$ = this.stateService.clients$;

  // Filters
  searchTerm = '';
  selectedProvince = 'Todas';
  selectedStatus = 'Todos';

  getFiltered(clients: Client[] | null): Client[] {
    if (!clients) return [];
    return clients.filter(c => {
      const matchSearch = !this.searchTerm || c.name.toLowerCase().includes(this.searchTerm.toLowerCase()) || c.bi.toLowerCase().includes(this.searchTerm.toLowerCase());
      const matchProvince = this.selectedProvince === 'Todas' || c.province === this.selectedProvince;
      const matchStatus = this.selectedStatus === 'Todos' || c.status === this.selectedStatus;
      
      return matchSearch && matchProvince && matchStatus;
    });
  }

  deactivateClient(id: number) {
    if (confirm('Tem certeza que deseja desativar ou congelar esta conta?')) {
      this.stateService.updateClient(id, { status: 'Congelado' });
    }
  }
}
