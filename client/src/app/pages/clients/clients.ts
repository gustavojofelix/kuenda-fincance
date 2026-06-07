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

  // Pagination
  currentPage = 1;
  pageSize = 5;
  totalPages = 1;
  totalCount = 0;
  Math = Math;

  getFiltered(clients: Client[] | null): Client[] {
    if (!clients) {
      this.totalCount = 0;
      this.totalPages = 1;
      return [];
    }
    const filtered = clients.filter(c => {
      const matchSearch = !this.searchTerm || c.name.toLowerCase().includes(this.searchTerm.toLowerCase()) || c.bi.toLowerCase().includes(this.searchTerm.toLowerCase());
      const matchProvince = this.selectedProvince === 'Todas' || c.province === this.selectedProvince;
      const matchStatus = this.selectedStatus === 'Todos' || c.status === this.selectedStatus;
      
      return matchSearch && matchProvince && matchStatus;
    });

    this.totalCount = filtered.length;
    this.totalPages = Math.ceil(this.totalCount / this.pageSize) || 1;

    if (this.currentPage > this.totalPages) {
      this.currentPage = this.totalPages;
    }

    const startIndex = (this.currentPage - 1) * this.pageSize;
    return filtered.slice(startIndex, startIndex + this.pageSize);
  }

  previousPage() {
    if (this.currentPage > 1) {
      this.currentPage--;
    }
  }

  nextPage() {
    if (this.currentPage < this.totalPages) {
      this.currentPage++;
    }
  }

  goToPage(page: number) {
    this.currentPage = page;
  }

  deactivateClient(id: number) {
    if (confirm('Tem certeza que deseja desativar ou congelar esta conta?')) {
      this.stateService.updateClient(id, { status: 'Congelado' });
    }
  }
}
