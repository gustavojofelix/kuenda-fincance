import { Injectable } from '@angular/core';
import { BehaviorSubject, map } from 'rxjs';
import { MOCK_CLIENTS, MOCK_LOANS, MOCK_METRICS } from './mock-data';

export interface Client {
  id: number;
  name: string;
  bi: string;
  biUrl?: string; // foto do BI
  photoUrl?: string; // foto do rosto
  phone: string;
  
  // Contacto & Localização
  province?: string;
  district?: string;
  neighborhood?: string;
  address?: string; // physical address

  // Negócio
  business: string;
  businessYears?: string; // Anos de actividade
  income?: string; // avg income range

  // Contacto de Emergência
  emergencyName?: string;
  emergencyRelation?: string;
  emergencyPhone?: string;
  
  maritalStatus?: string;
  status: string;
  loanCycle: number;
}

export interface Loan {
  id: string;
  clientId?: number;
  clientName: string;
  amount: number; // Principal
  interestRate: number; // %
  term: number; // Meses
  totalToPay: number;
  paidAmount: number;
  installmentsCount: number;
  paidInstallments: number;
  monthlyPayment: number;
  date: string;
  status: 'Em Análise' | 'Aprovado' | 'Ativo' | 'Atrasado' | 'Liquidado';
  nextPayment: string;
  disbursementMethod?: 'M-Pesa' | 'E-Mola' | 'Banco';
}

@Injectable({
  providedIn: 'root'
})
export class StateService {
  private clientsSubj = new BehaviorSubject<Client[]>(MOCK_CLIENTS);
  clients$ = this.clientsSubj.asObservable();

  // Observable derived for KPIs
  totalClientsCount$ = this.clients$.pipe(map(clients => clients.length));
  activeClientsCount$ = this.clients$.pipe(map(clients => clients.filter(c => c.status === 'Em Dia' || c.status === 'Atrasado').length));
  atRiskClients$ = this.clients$.pipe(map(clients => clients.filter(c => c.status === 'Atrasado').length));

  private loansSubj = new BehaviorSubject<Loan[]>(MOCK_LOANS as any);
  loans$ = this.loansSubj.asObservable();

  private metricsSubj = new BehaviorSubject(MOCK_METRICS);
  metrics$ = this.metricsSubj.asObservable();

  // Derived KPIs for Loans
  totalActivePortfolio$ = this.loans$.pipe(map(loans => loans.filter(l => l.status === 'Ativo' || l.status === 'Atrasado').reduce((acc, l) => acc + (l.totalToPay - l.paidAmount), 0)));
  dueTodayCount$ = this.loans$.pipe(map(loans => loans.filter(l => l.status === 'Ativo' && l.nextPayment.includes(new Date().getDate().toString())).length));
  par30Rate$ = this.loans$.pipe(map(loans => {
      const active = loans.filter(l => l.status === 'Ativo' || l.status === 'Atrasado').length;
      const late = loans.filter(l => l.status === 'Atrasado').length;
      return active > 0 ? (late / active) * 100 : 0;
  }));

  getClientById(id: number) {
    return this.clients$.pipe(map(clients => clients.find(c => c.id === id)));
  }

  getLoansByClientName(name: string) {
    return this.loans$.pipe(map(loans => loans.filter(l => l.clientName === name)));
  }

  addClient(client: Partial<Client>) {
    const currentList = this.clientsSubj.value;
    const newId = currentList.length > 0 ? Math.max(...currentList.map(c => c.id)) + 1 : 1;
    const newClient = { 
      loanCycle: 1,
      status: 'Avaliação',
      ...client, 
      id: newId 
    } as Client;
    
    // Add to front of list
    this.clientsSubj.next([newClient, ...currentList]);
    return newClient;
  }
  
  updateClient(id: number, updates: Partial<Client>) {
    const currentList = this.clientsSubj.value;
    const index = currentList.findIndex(c => c.id === id);
    if (index > -1) {
      currentList[index] = { ...currentList[index], ...updates };
      this.clientsSubj.next([...currentList]);
    }
  }

  addLoan(loan: Partial<Loan>) {
    const currentList = this.loansSubj.value;
    const newIdStr = `L-${10023 + currentList.length}`;
    
    // Simple Price simulation for the mock
    const amount = loan.amount || 0;
    const rate = (loan.interestRate || 5) / 100;
    const term = loan.term || 6;
    const totalToPay = amount * (1 + rate); // Simplified for mock
    const monthlyPayment = totalToPay / term;

    const newLoan = { 
        date: '-', 
        status: 'Em Análise',
        nextPayment: '-',
        paidAmount: 0,
        paidInstallments: 0,
        totalToPay,
        monthlyPayment,
        installmentsCount: term,
        ...loan, 
        id: newIdStr 
    } as Loan;
    
    this.loansSubj.next([newLoan, ...currentList]);
    return newLoan;
  }

  disburseLoan(loanId: string, method: 'M-Pesa' | 'E-Mola' | 'Banco') {
    const currentLoans = this.loansSubj.value;
    const index = currentLoans.findIndex(l => l.id === loanId);
    if (index > -1) {
      const updated = [...currentLoans];
      updated[index].status = 'Ativo';
      updated[index].disbursementMethod = method;
      
      const date = new Date();
      updated[index].date = `${date.getDate()} ${date.toLocaleString('pt', { month: 'short' })} ${date.getFullYear()}`;
      
      const nextMonth = new Date(date);
      nextMonth.setMonth(nextMonth.getMonth() + 1);
      updated[index].nextPayment = `${nextMonth.getDate()} ${nextMonth.toLocaleString('pt', { month: 'short' })} ${nextMonth.getFullYear()}`;
      
      this.loansSubj.next(updated);

      // Link to client status
      const currentClients = this.clientsSubj.value;
      const clientIndex = currentClients.findIndex(c => c.name === updated[index].clientName);
      if (clientIndex > -1) {
        const updatedClients = [...currentClients];
        updatedClients[clientIndex].status = 'Em Dia';
        this.clientsSubj.next(updatedClients);
      }
    }
  }

  registerPayment(loanId: number | string, amount: number) {
      const currentLoans = this.loansSubj.value;
      const index = currentLoans.findIndex(l => l.id === loanId);
      if (index > -1) {
          const updated = [...currentLoans];
          updated[index].paidAmount += amount;
          updated[index].paidInstallments += 1;
          
          if (updated[index].paidAmount >= updated[index].totalToPay) {
              updated[index].status = 'Liquidado';
          }
          
          this.loansSubj.next(updated);
      }
  }
}
