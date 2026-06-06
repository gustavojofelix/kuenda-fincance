import { Injectable, inject } from '@angular/core';
import { BehaviorSubject, map, combineLatest, shareReplay } from 'rxjs';
import { MOCK_CLIENTS, MOCK_LOANS, MOCK_METRICS } from './mock-data';
import { NotificationService } from './notification.service';

export interface UserProfile {
  name: string;
  role: string;
  email: string;
  phone: string;
  photoUrl?: string;
}

export interface IMF {
  id: string; // Tenant unique ID, e.g. 'kuenda', 'socinal'
  name: string;
  nuit: string;
  email: string;
  phone: string;
  address: string;
  logoUrl?: string;
  primaryColor: string; // Dynamic brand primary color (e.g. Hex)
  secondaryColor: string; // Dynamic brand secondary/dark color (e.g. Hex)
}

export interface Branch {
  id: number;
  imfId: string;
  name: string;
  city: string;
  address: string;
  manager: string;
  phone?: string;
  email?: string;
  status: 'Ativa' | 'Inativa';
}

export interface GuaranteeItem {
  name: string;
  photoUrl: string;
  value: number;
}

export interface Client {
  id: number;
  imfId: string;
  branchId?: number;
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
  guarantees?: GuaranteeItem[];
  requestedTerm?: number;
  estimatedMonthlyRevenue?: number;
  scoreSimulado?: number;
}

export interface Loan {
  id: string;
  imfId: string;
  branchId?: number;
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
  imfId: string;
  branchId?: number;
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

  // Active User Profile Context
  private currentUserSubj = new BehaviorSubject<UserProfile>({
    name: 'Carlos Felix',
    role: 'Admin',
    email: 'carlos.felix@kuenda.co.mz',
    phone: '+258 84 999 8888',
    photoUrl: ''
  });
  currentUser$ = this.currentUserSubj.asObservable();

  updateCurrentUser(updates: Partial<UserProfile>) {
    this.currentUserSubj.next({ ...this.currentUserSubj.value, ...updates });
  }

  // Multi-Tenant IMF Database
  private imfsSubj = new BehaviorSubject<IMF[]>([
    { 
      id: 'imf-20260526kd', 
      name: 'Kuenda Microfinanças', 
      nuit: '400567123', 
      email: 'contacto@kuenda.co.mz', 
      phone: '+258 84 000 0000', 
      address: 'Av. Eduardo Mondlane, 1234, Maputo',
      primaryColor: '#10b981', // Emerald Green
      secondaryColor: '#059669'
    },
    { 
      id: 'imf-20260526sc', 
      name: 'Socinal Microfinanças', 
      nuit: '900456123', 
      email: 'contacto@socinal.co.mz', 
      phone: '+258 82 111 2222', 
      address: 'Av. 25 de Setembro, Matola',
      primaryColor: '#2563eb', // Royal Blue
      secondaryColor: '#1d4ed8'
    }
  ]);
  imfs$ = this.imfsSubj.asObservable();

  // Active IMF Context
  private activeImfIdSubj = new BehaviorSubject<string>('imf-20260526kd');
  activeImfId$ = this.activeImfIdSubj.asObservable();

  activeImf$ = combineLatest([this.imfs$, this.activeImfId$]).pipe(
    map(([imfs, activeId]) => imfs.find(i => i.id.toLowerCase() === activeId.toLowerCase()))
  );

  // Active Branch Context
  private activeBranchIdSubj = new BehaviorSubject<number | 'all'>('all');
  activeBranchId$ = this.activeBranchIdSubj.asObservable();

  switchBranch(branchId: number | 'all') {
    this.activeBranchIdSubj.next(branchId);
  }

  // Master Data Subjects
  private branchesSubj = new BehaviorSubject<Branch[]>([
    { id: 1, imfId: 'imf-20260526kd', name: 'Sede Maputo', city: 'Maputo', address: 'Av. Eduardo Mondlane, 1234', manager: 'Carlos Mutemba', phone: '+258 84 111 2222', email: 'maputo@kuenda.co.mz', status: 'Ativa' },
    { id: 2, imfId: 'imf-20260526kd', name: 'Agência Matola', city: 'Matola', address: 'Rua da Beira, 45', manager: 'Anabela Sitoe', phone: '+258 84 333 4444', email: 'matola@kuenda.co.mz', status: 'Ativa' },
    { id: 3, imfId: 'imf-20260526sc', name: 'Agência Beira', city: 'Beira', address: 'Av. das Indústrias, 89', manager: 'João Chissano', phone: '+258 82 555 6666', email: 'beira@socinal.co.mz', status: 'Ativa' },
    { id: 4, imfId: 'imf-20260526sc', name: 'Agência Nampula', city: 'Nampula', address: 'Rua da Estação, 12', manager: 'Filomena Sitoe', phone: '+258 82 777 8888', email: 'nampula@socinal.co.mz', status: 'Ativa' }
  ]);

  private clientsSubj = new BehaviorSubject<Client[]>(MOCK_CLIENTS);
  private loansSubj = new BehaviorSubject<Loan[]>(MOCK_LOANS as any);
  
  private transactionsSubj = new BehaviorSubject<Transaction[]>([
    { id: 'T-101', imfId: 'imf-20260526kd', branchId: 1, description: 'Pagamento Salários Março', amount: 45000, date: '2024-03-05', category: 'Salários', type: 'Saída' },
    { id: 'T-102', imfId: 'imf-20260526kd', branchId: 1, description: 'Factura EDM - Maputo', amount: 3200, date: '2024-03-02', category: 'Energia', type: 'Saída' },
    { id: 'T-201', imfId: 'imf-20260526sc', branchId: 3, description: 'Pagamento Renda Agência', amount: 25000, date: '2024-03-01', category: 'Renda', type: 'Saída' },
    { id: 'T-202', imfId: 'imf-20260526sc', branchId: 3, description: 'Consumo EDM - Beira', amount: 4800, date: '2024-03-04', category: 'Energia', type: 'Saída' }
  ]);

  private metricsSubj = new BehaviorSubject(MOCK_METRICS);
  metrics$ = this.metricsSubj.asObservable();

  // Reactive Filters based on active IMF ID and active Branch ID
  branches$ = combineLatest([this.branchesSubj, this.activeImfId$]).pipe(
    map(([branches, imfId]) => branches.filter(b => b.imfId === imfId)),
    shareReplay(1)
  );

  clients$ = combineLatest([this.clientsSubj, this.activeImfId$, this.activeBranchId$]).pipe(
    map(([clients, imfId, branchId]) => {
      const imfClients = clients.filter(c => c.imfId === imfId);
      return branchId === 'all' ? imfClients : imfClients.filter(c => c.branchId === branchId);
    }),
    shareReplay(1)
  );

  loans$ = combineLatest([this.loansSubj, this.activeImfId$, this.activeBranchId$]).pipe(
    map(([loans, imfId, branchId]) => {
      const imfLoans = loans.filter(l => l.imfId === imfId);
      return branchId === 'all' ? imfLoans : imfLoans.filter(l => l.branchId === branchId);
    }),
    shareReplay(1)
  );

  transactions$ = combineLatest([this.transactionsSubj, this.activeImfId$, this.activeBranchId$]).pipe(
    map(([transactions, imfId, branchId]) => {
      const imfTransactions = transactions.filter(t => t.imfId === imfId);
      return branchId === 'all' ? imfTransactions : imfTransactions.filter(t => t.branchId === branchId);
    }),
    shareReplay(1)
  );

  constructor() {
    // Dynamic Theme Color Application
    this.activeImf$.subscribe(imf => {
      if (imf) {
        this.applyImfTheme(imf);
      }
    });
  }

  applyImfTheme(imf: IMF) {
    if (typeof document !== 'undefined' && document.documentElement) {
      document.documentElement.style.setProperty('--color-primary', imf.primaryColor);
      document.documentElement.style.setProperty('--color-primary-dark', imf.secondaryColor || imf.primaryColor);
    }
  }

  switchImf(imfId: string) {
    const id = imfId.trim().toLowerCase();
    if (this.imfsSubj.value.some(i => i.id.toLowerCase() === id)) {
      this.activeImfIdSubj.next(id);
      this.activeBranchIdSubj.next('all');
    }
  }

  registerIMF(name: string, nuit: string, email: string): IMF {
    const today = new Date();
    const yyyy = today.getFullYear();
    const mm = String(today.getMonth() + 1).padStart(2, '0');
    const dd = String(today.getDate()).padStart(2, '0');
    const dateStr = `${yyyy}${mm}${dd}`;
    
    const chars = 'ABCDEFGHIJKLMNOPQRSTUVWXYZ';
    const r1 = chars.charAt(Math.floor(Math.random() * chars.length));
    const r2 = chars.charAt(Math.floor(Math.random() * chars.length));
    
    const generatedId = `imf-${dateStr}${r1}${r2}`.toLowerCase();

    // Select dynamic brand primary color combinations (Hex)
    const colors = [
      { primary: '#6366f1', secondary: '#4f46e5' }, // Indigo
      { primary: '#8b5cf6', secondary: '#7c3aed' }, // Purple
      { primary: '#ec4899', secondary: '#db2777' }, // Pink
      { primary: '#06b6d4', secondary: '#0891b2' }, // Cyan
      { primary: '#10b981', secondary: '#059669' }, // Emerald
      { primary: '#f59e0b', secondary: '#d97706' }  // Amber
    ];
    const color = colors[Math.floor(Math.random() * colors.length)];

    const newImf: IMF = {
      id: generatedId,
      name,
      nuit,
      email,
      phone: '',
      address: '',
      primaryColor: color.primary,
      secondaryColor: color.secondary
    };

    const currentImfs = this.imfsSubj.value;
    this.imfsSubj.next([...currentImfs, newImf]);

    // Initialize a default main branch for this new tenant
    const currentBranches = this.branchesSubj.value;
    const nextBranchId = currentBranches.length > 0 ? Math.max(...currentBranches.map(b => b.id)) + 1 : 1;
    const defaultBranch: Branch = {
      id: nextBranchId,
      imfId: generatedId,
      name: 'Agência Sede',
      city: 'Maputo',
      address: 'Sede Principal',
      manager: 'Administrador',
      status: 'Ativa'
    };
    this.branchesSubj.next([...currentBranches, defaultBranch]);

    return newImf;
  }

  updateIMF(updates: Partial<IMF>) {
    const currentImfs = this.imfsSubj.value;
    const activeId = this.activeImfIdSubj.value;
    const index = currentImfs.findIndex(i => i.id === activeId);
    if (index > -1) {
      const updated = [...currentImfs];
      updated[index] = { ...updated[index], ...updates };
      this.imfsSubj.next(updated);
      this.applyImfTheme(updated[index]);
    }
  }

  // KPIs calculated dynamically on the active filtered streams
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

  addBranch(branch: Partial<Branch>) {
    const current = this.branchesSubj.value;
    const newId = current.length > 0 ? Math.max(...current.map(b => b.id)) + 1 : 1;
    const newBranch = { 
      ...branch, 
      id: newId, 
      imfId: this.activeImfIdSubj.value,
      status: branch.status || 'Ativa' 
    } as Branch;
    this.branchesSubj.next([...current, newBranch]);
    this.notificationService.addNotification({
      title: 'Agência Criada',
      message: `A agência ${newBranch.name} foi registrada com sucesso.`,
      type: 'success'
    });
  }

  updateBranch(updated: Branch) {
    const current = this.branchesSubj.value;
    const index = current.findIndex(b => b.id === updated.id);
    if (index > -1) {
      const updatedList = [...current];
      updatedList[index] = { ...updated };
      this.branchesSubj.next(updatedList);
      this.notificationService.addNotification({
        title: 'Agência Atualizada',
        message: `Os dados da agência ${updated.name} foram salvos.`,
        type: 'success'
      });
    }
  }

  deleteBranch(id: number) {
    const current = this.branchesSubj.value;
    const branchName = current.find(b => b.id === id)?.name || '';
    const filtered = current.filter(b => b.id !== id);
    this.branchesSubj.next(filtered);
    this.notificationService.addNotification({
      title: 'Agência Apagada',
      message: `A agência ${branchName} foi removida da plataforma.`,
      type: 'danger'
    });
  }

  getClientById(id: number) {
    return this.clientsSubj.asObservable().pipe(map(clients => clients.find(c => c.id === id)));
  }

  getLoansByClientName(name: string) {
    return this.loansSubj.asObservable().pipe(map(loans => loans.filter(l => l.clientName === name)));
  }

  addClient(client: Partial<Client>) {
    const currentList = this.clientsSubj.value;
    const newId = currentList.length > 0 ? Math.max(...currentList.map(c => c.id)) + 1 : 1;
    
    const activeBranchId = this.activeBranchIdSubj.value;
    let branchId: number;
    if (activeBranchId === 'all') {
      const activeImfId = this.activeImfIdSubj.value;
      const firstBranch = this.branchesSubj.value.find(b => b.imfId === activeImfId);
      branchId = firstBranch ? firstBranch.id : 1;
    } else {
      branchId = activeBranchId;
    }

    const newClient = { 
      loanCycle: 1, 
      status: 'Avaliação', 
      imfId: this.activeImfIdSubj.value, 
      branchId,
      ...client, 
      id: newId 
    } as Client;
    this.clientsSubj.next([newClient, ...currentList]);
    return newClient;
  }
  
  updateClient(id: number, updates: Partial<Client>) {
    const currentList = this.clientsSubj.value;
    const index = currentList.findIndex(c => c.id === id);
    if (index > -1) {
      const updated = [...currentList];
      updated[index] = { ...updated[index], ...updates };
      this.clientsSubj.next(updated);
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
    
    const activeBranchId = this.activeBranchIdSubj.value;
    let branchId: number;
    if (activeBranchId === 'all') {
      const activeImfId = this.activeImfIdSubj.value;
      const firstBranch = this.branchesSubj.value.find(b => b.imfId === activeImfId);
      branchId = firstBranch ? firstBranch.id : 1;
    } else {
      branchId = activeBranchId;
    }

    const newLoan = { 
      date: '-', 
      status: 'Em Análise', 
      nextPayment: '-', 
      paidAmount: 0, 
      paidInstallments: 0, 
      totalToPay, 
      monthlyPayment, 
      installmentsCount: term, 
      imfId: this.activeImfIdSubj.value,
      branchId,
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
      
      this.addInternalTransaction({ 
        description: `Desembolso ${method}: ${updated[index].clientName}`, 
        amount: updated[index].amount, 
        category: 'Empréstimo', 
        type: 'Saída', 
        isAuto: true 
      });
      
      const currentClients = this.clientsSubj.value;
      const clientIndex = currentClients.findIndex(c => c.name === updated[index].clientName);
      if (clientIndex > -1) {
        const updatedClients = [...currentClients];
        updatedClients[clientIndex].status = 'Em Dia';
        this.clientsSubj.next(updatedClients);
      }
    }
  }

  registerPayment(loanId: number | string, amount: number, channel: 'M-Pesa' | 'E-Mola' | 'Banco' | string = 'M-Pesa') {
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
          
          this.addInternalTransaction({ 
            description: `Amortização (${channel}): ${updated[index].clientName}`, 
            amount: amount, 
            category: 'Amortização', 
            type: 'Entrada', 
            isAuto: true 
          });
      }
  }

  addManualTransaction(tx: Partial<Transaction>) { 
    this.addInternalTransaction(tx); 
  }

  simulate30DaysLater() {
    const currentLoans = this.loansSubj.value;
    const currentImfId = this.activeImfIdSubj.value;
    let activeLoansUpdatedCount = 0;
    
    const updatedLoans = currentLoans.map(l => {
      // Only affect the active IMF's active loans
      if (l.imfId === currentImfId && l.status === 'Ativo') {
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
      
      const activeBranchId = this.activeBranchIdSubj.value;
      let branchId: number;
      if (activeBranchId === 'all') {
        const activeImfId = this.activeImfIdSubj.value;
        const firstBranch = this.branchesSubj.value.find(b => b.imfId === activeImfId);
        branchId = firstBranch ? firstBranch.id : 1;
      } else {
        branchId = activeBranchId;
      }

      const newTx = { 
        id: newId, 
        imfId: this.activeImfIdSubj.value,
        branchId,
        date: new Date().toISOString().split('T')[0], 
        ...tx 
      } as Transaction;
      this.transactionsSubj.next([newTx, ...current]);
  }
}
