import { Injectable, inject } from '@angular/core';
import { BehaviorSubject, map } from 'rxjs';
import { MOCK_CLIENTS, MOCK_LOANS, MOCK_METRICS } from './mock-data';
import { NotificationService } from './notification.service';

export interface IMFProfile {
  name: string;
  nuit: string;
  email: string;
  phone: string;
  address: string;
  logoUrl?: string;
}

export interface Branch {
  id: number;
  name: string;
  city: string;
  address: string;
  manager: string;
  status: 'Ativa' | 'Inativa';
}

export interface Client {
  id: number;
  name: string;
  bi: string;
  biUrl?: string;
  photoUrl?: string;
  phone: string;
  province?: string;
  district?: string;
  neighborhood?: string;
  address?: string;
  business: string;
  businessYears?: string;
  income?: string;
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

export interface Transaction {
  id: string;
  description: string;
  amount: number;
  date: string;
  category: 'Salários' | 'Energia' | 'Água' | 'Renda' | 'Impostos' | 'Empréstimo' | 'Amortização' | 'Receita Juros' | 'Outros';
  type: 'Entrada' | 'Saída';
  isAuto?: boolean;
}

@Injectable({
  providedIn: 'root'
})
export class StateService {
  private notificationService = inject(NotificationService);

  private profileSubj = new BehaviorSubject<IMFProfile>({
    name: 'Kuenda Microfinanças',
    nuit: '400567123',
    email: 'contacto@kuenda.co.mz',
    phone: '+258 84 000 0000',
    address: 'Av. Eduardo Mondlane, 1234, Maputo'
  });
  profile$ = this.profileSubj.asObservable();

  private branchesSubj = new BehaviorSubject<Branch[]>([
    { id: 1, name: 'Sede Maputo', city: 'Maputo', address: 'Av. Eduardo Mondlane', manager: 'Carlos Mutemba', status: 'Ativa' },
    { id: 2, name: 'Agência Matola', city: 'Matola', address: 'Rua da Beira', manager: 'Anabela Sitoe', status: 'Ativa' },
    { id: 3, name: 'Agência Beira', city: 'Beira', address: 'Av. das Indústrias', manager: 'João Chissano', status: 'Inativa' }
  ]);
  branches$ = this.branchesSubj.asObservable();

  private clientsSubj = new BehaviorSubject<Client[]>(MOCK_CLIENTS);
  clients$ = this.clientsSubj.asObservable();

  private loansSubj = new BehaviorSubject<Loan[]>(MOCK_LOANS as any);
  loans$ = this.loansSubj.asObservable();

  private transactionsSubj = new BehaviorSubject<Transaction[]>([
    { id: 'T-101', description: 'Pagamento Salários Março', amount: 45000, date: '2024-03-05', category: 'Salários', type: 'Saída' },
    { id: 'T-102', description: 'Factura EDM - Maputo', amount: 3200, date: '2024-03-02', category: 'Energia', type: 'Saída' }
  ]);
  transactions$ = this.transactionsSubj.asObservable();

  private metricsSubj = new BehaviorSubject(MOCK_METRICS);
  metrics$ = this.metricsSubj.asObservable();

  // KPIs and other methods...

  totalClientsCount$ = this.clients$.pipe(map(clients => clients.length));
  activeClientsCount$ = this.clients$.pipe(map(clients => clients.filter(c => c.status === 'Em Dia' || c.status === 'Atrasado').length));
  atRiskClients$ = this.clients$.pipe(map(clients => clients.filter(c => c.status === 'Atrasado').length));
  
  totalActivePortfolio$ = this.loans$.pipe(map(loans => loans.filter(l => l.status === 'Ativo' || l.status === 'Atrasado').reduce((acc, l) => acc + (l.totalToPay - l.paidAmount), 0)));
  dueTodayCount$ = this.loans$.pipe(map(loans => loans.filter(l => l.status === 'Ativo' && l.nextPayment.includes(new Date().getDate().toString())).length));
  par30Rate$ = this.loans$.pipe(map(loans => {
      const active = loans.filter(l => l.status === 'Ativo' || l.status === 'Atrasado').length;
      const late = loans.filter(l => l.status === 'Atrasado').length;
      return active > 0 ? (late / active) * 100 : 0;
  }));

  cashBalance$ = this.transactions$.pipe(map(ts => ts.reduce((acc, t) => acc + (t.type === 'Entrada' ? t.amount : -t.amount), 500000)));
  monthlyInflow$ = this.transactions$.pipe(map(ts => ts.filter(t => t.type === 'Entrada').reduce((acc, t) => acc + t.amount, 0)));
  monthlyOutflow$ = this.transactions$.pipe(map(ts => ts.filter(t => t.type === 'Saída').reduce((acc, t) => acc + t.amount, 0)));

  updateProfile(profile: IMFProfile) {
    this.profileSubj.next(profile);
  }

  addBranch(branch: Partial<Branch>) {
    const current = this.branchesSubj.value;
    const newBranch = { ...branch, id: current.length + 1, status: 'Ativa' } as Branch;
    this.branchesSubj.next([...current, newBranch]);
  }

  getClientById(id: number) {
    return this.clients$.pipe(map(clients => clients.find(c => c.id === id)));
  }

  getLoansByClientName(name: string) {
    return this.loans$.pipe(map(loans => loans.filter(l => l.clientName === name)));
  }

  addClient(client: Partial<Client>) {
    const currentList = this.clientsSubj.value;
    const newId = currentList.length > 0 ? Math.max(...currentList.map(c => c.id)) + 1 : 1;
    const newClient = { loanCycle: 1, status: 'Avaliação', ...client, id: newId } as Client;
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
    const amount = loan.amount || 0;
    const interestRate = loan.interestRate || 5;
    const monthlyRate = interestRate / 100;
    const term = loan.term || 6;
    
    let monthlyPayment = 0;
    if (monthlyRate === 0) {
      monthlyPayment = amount / term;
    } else {
      monthlyPayment = (amount * monthlyRate) / (1 - Math.pow(1 + monthlyRate, -term));
    }
    
    monthlyPayment = Math.round(monthlyPayment * 100) / 100;
    const totalToPay = Math.round((monthlyPayment * term) * 100) / 100;
    
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
      this.addInternalTransaction({ description: `Desembolso ${method}: ${updated[index].clientName}`, amount: updated[index].amount, category: 'Empréstimo', type: 'Saída', isAuto: true });
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
          if (updated[index].paidAmount >= updated[index].totalToPay) { updated[index].status = 'Liquidado'; }
          this.loansSubj.next(updated);
          this.addInternalTransaction({ description: `Amortização: ${updated[index].clientName}`, amount: amount, category: 'Amortização', type: 'Entrada', isAuto: true });
      }
  }

  addManualTransaction(tx: Partial<Transaction>) { this.addInternalTransaction(tx); }

  simulate30DaysLater() {
    const currentLoans = this.loansSubj.value;
    let activeLoansUpdatedCount = 0;
    const updatedLoans = currentLoans.map(l => {
      if (l.status === 'Ativo') {
        activeLoansUpdatedCount++;
        
        this.notificationService.addNotification({
          title: 'Crédito em Atraso',
          message: `O contrato ${l.id} de ${l.clientName} está vencido há mais de 30 dias.`,
          type: 'danger',
          link: `/admin/loans/${l.id}`
        });

        const currentClients = this.clientsSubj.value;
        const clientIndex = currentClients.findIndex(c => c.name === l.clientName);
        if (clientIndex > -1) {
          const updatedClients = [...currentClients];
          updatedClients[clientIndex].status = 'Atrasado';
          this.clientsSubj.next(updatedClients);
        }

        return { ...l, status: 'Atrasado' as const };
      }
      return l;
    });

    if (activeLoansUpdatedCount > 0) {
      this.loansSubj.next(updatedLoans);
    }
  }

  private addInternalTransaction(tx: Partial<Transaction>) {
      const current = this.transactionsSubj.value;
      const newId = `T-${100 + current.length + 1}`;
      const newTx = { id: newId, date: new Date().toISOString().split('T')[0], ...tx } as Transaction;
      this.transactionsSubj.next([newTx, ...current]);
  }
}
