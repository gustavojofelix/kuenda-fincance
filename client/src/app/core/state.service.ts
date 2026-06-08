import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, map, combineLatest, shareReplay, tap } from 'rxjs';
import { NotificationService } from './notification.service';

export interface UserProfile {
  name: string;
  role: string;
  email: string;
  phone: string;
  photoUrl?: string;
}

export interface IMF {
  id: string; // Tenant unique code (e.g. 'imf-20260526kd')
  name: string;
  nuit: string;
  email: string;
  phone: string;
  address: string;
  logoUrl?: string;
  primaryColor: string;
  secondaryColor: string;
}

export interface Branch {
  id: any;
  imfId: string;
  name: string;
  city: string;
  address: string;
  manager: string;
  phone?: string;
  email?: string;
  status: 'Ativa' | 'Inativa' | string;
}

export interface GuaranteeItem {
  name: string;
  photoUrl: string;
  value: number;
}

export interface Client {
  id: any;
  imfId: string;
  branchId?: any;
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
  id: any;
  imfId: string;
  branchId?: any;
  clientId?: any;
  clientName: string;
  amount: number;
  interestRate: number;
  term: number;
  totalToPay: number;
  paidAmount: number;
  installmentsCount: number;
  paidInstallments: number;
  monthlyPayment: number;
  date: string;
  status: 'Em Análise' | 'Aprovado' | 'Ativo' | 'Atrasado' | 'Liquidado' | string;
  nextPayment: string;
  disbursementMethod?: 'M-Pesa' | 'E-Mola' | 'Banco' | string;
}

export interface Transaction {
  id: any;
  imfId: string;
  branchId?: any;
  description: string;
  amount: number;
  date: string;
  category: string;
  type: 'Entrada' | 'Saída' | string;
  isAuto?: boolean;
}

@Injectable({
  providedIn: 'root'
})
export class StateService {
  private http = inject(HttpClient);
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
  private imfsSubj = new BehaviorSubject<IMF[]>([]);
  imfs$ = this.imfsSubj.asObservable();

  // Active IMF Context
  private activeImfIdSubj = new BehaviorSubject<string>('all');
  activeImfId$ = this.activeImfIdSubj.asObservable();

  activeImf$ = combineLatest([this.imfs$, this.activeImfId$]).pipe(
    map(([imfs, activeId]) => imfs.find(i => i.id.toLowerCase() === activeId.toLowerCase()))
  );

  // Active Branch Context
  private activeBranchIdSubj = new BehaviorSubject<any>('all');
  activeBranchId$ = this.activeBranchIdSubj.asObservable();

  switchBranch(branchId: any) {
    this.activeBranchIdSubj.next(branchId);
    if (branchId !== 'all') {
      localStorage.setItem('activeBranchId', branchId);
    } else {
      localStorage.removeItem('activeBranchId');
    }
  }

  // Master Data Subjects
  private branchesSubj = new BehaviorSubject<Branch[]>([]);
  private clientsSubj = new BehaviorSubject<Client[]>([]);
  private loansSubj = new BehaviorSubject<Loan[]>([]);
  private transactionsSubj = new BehaviorSubject<Transaction[]>([]);

  // Reactive Filters based on active IMF ID and active Branch ID
  branches$ = combineLatest([this.branchesSubj, this.activeImfId$]).pipe(
    map(([branches, imfId]) => imfId === 'all' ? branches : branches.filter(b => b.imfId === imfId)),
    shareReplay(1)
  );

  clients$ = combineLatest([this.clientsSubj, this.activeImfId$, this.activeBranchId$]).pipe(
    map(([clients, imfId, branchId]) => {
      let filtered = clients;
      if (imfId !== 'all') {
        filtered = filtered.filter(c => c.imfId === imfId);
      }
      return branchId === 'all' ? filtered : filtered.filter(c => c.branchId === branchId);
    }),
    shareReplay(1)
  );

  loans$ = combineLatest([this.loansSubj, this.activeImfId$, this.activeBranchId$]).pipe(
    map(([loans, imfId, branchId]) => {
      let filtered = loans;
      if (imfId !== 'all') {
        filtered = filtered.filter(l => l.imfId === imfId);
      }
      return branchId === 'all' ? filtered : filtered.filter(l => l.branchId === branchId);
    }),
    shareReplay(1)
  );

  transactions$ = combineLatest([this.transactionsSubj, this.activeImfId$, this.activeBranchId$]).pipe(
    map(([transactions, imfId, branchId]) => {
      let filtered = transactions;
      if (imfId !== 'all') {
        filtered = filtered.filter(t => t.imfId === imfId);
      }
      return branchId === 'all' ? filtered : filtered.filter(t => t.branchId === branchId);
    }),
    shareReplay(1)
  );

  // KPIs calculated dynamically on the active filtered streams
  totalClientsCount$ = this.clients$.pipe(map(clients => clients.length));
  activeClientsCount$ = this.clients$.pipe(map(clients => clients.filter(c => c.status === 'Em Dia' || c.status === 'Atrasado' || c.status === 'Avaliação' || c.status === 'Ativo').length));
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

  constructor() {
    // Check if token exists on init to load data
    const token = localStorage.getItem('token');
    const savedImfId = localStorage.getItem('activeImfId');
    const savedBranchId = localStorage.getItem('activeBranchId');

    if (token) {
      if (savedImfId) this.activeImfIdSubj.next(savedImfId);
      if (savedBranchId) this.activeBranchIdSubj.next(savedBranchId);
      this.loadAllData();
    }

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
    this.activeImfIdSubj.next(id);
    localStorage.setItem('activeImfId', id);
  }

  login(imfCode: string, email: string, password: string) {
    return this.http.post<any>('/api/auth/login', { imfCode, email, password }).pipe(
      tap({
        next: (res) => {
          localStorage.setItem('token', res.token);
          localStorage.setItem('activeImfId', res.tenant.code.toLowerCase());
          
          if (res.user.branchRoles && res.user.branchRoles.length > 0) {
            localStorage.setItem('activeBranchId', res.user.branchRoles[0].branchId);
            this.activeBranchIdSubj.next(res.user.branchRoles[0].branchId);
          } else {
            this.activeBranchIdSubj.next('all');
          }

          this.currentUserSubj.next({
            name: `${res.user.firstName} ${res.user.lastName}`,
            role: res.user.branchRoles?.[0]?.role || 'Admin',
            email: res.user.email,
            phone: '',
            photoUrl: ''
          });

          this.activeImfIdSubj.next(res.tenant.code.toLowerCase());
          
          const imfObj: IMF = {
            id: res.tenant.code.toLowerCase(),
            name: res.tenant.name,
            nuit: '',
            email: res.user.email,
            phone: '',
            address: '',
            primaryColor: res.tenant.primaryColor || '#10b981',
            secondaryColor: res.tenant.secondaryColor || '#059669'
          };
          this.imfsSubj.next([imfObj]);
          this.applyImfTheme(imfObj);

          this.loadAllData();
        }
      })
    );
  }

  registerIMF(name: string, nuit: string, email: string, adminPhone: string, city: string, address: string, adminName: string): IMF {
    // Keep in-memory representation for UI component fallback
    const mockImf: IMF = {
      id: 'PENDING',
      name, nuit, email, phone: adminPhone, address, primaryColor: '#10b981', secondaryColor: '#059669'
    };
    return mockImf;
  }

  register(imfName: string, nuit: string, adminName: string, adminEmail: string, adminPhone: string, password: string, province: string, city: string, address: string) {
    const command = {
      imfName,
      nuit,
      adminName,
      adminEmail,
      adminCellphone: adminPhone,
      password,
      province,
      city,
      fullAddress: address
    };
    return this.http.post<any>('/api/auth/register', command).pipe(
      tap({
        next: (res) => {
          localStorage.setItem('token', res.token);
          localStorage.setItem('activeImfId', res.tenant.code.toLowerCase());
          
          if (res.user.branchRoles && res.user.branchRoles.length > 0) {
            localStorage.setItem('activeBranchId', res.user.branchRoles[0].branchId);
            this.activeBranchIdSubj.next(res.user.branchRoles[0].branchId);
          } else {
            this.activeBranchIdSubj.next('all');
          }

          this.currentUserSubj.next({
            name: `${res.user.firstName} ${res.user.lastName}`,
            role: 'Admin',
            email: res.user.email,
            phone: adminPhone,
            photoUrl: ''
          });

          this.activeImfIdSubj.next(res.tenant.code.toLowerCase());
          
          const imfObj: IMF = {
            id: res.tenant.code.toLowerCase(),
            name: res.tenant.name,
            nuit: res.tenant.nuit || nuit,
            email: res.tenant.email || adminEmail,
            phone: res.tenant.cellphone || adminPhone,
            address: res.tenant.fullAddress || address,
            primaryColor: res.tenant.primaryColor || '#10b981',
            secondaryColor: res.tenant.secondaryColor || '#059669'
          };
          this.imfsSubj.next([imfObj]);
          this.applyImfTheme(imfObj);

          this.loadAllData();
        }
      })
    );
  }

  updateIMF(updates: Partial<IMF>) {
    // No-op for local demo settings
  }

  loadAllData() {
    this.http.get<any[]>('/api/branches').subscribe({
      next: (branches) => {
        const mappedBranches = branches.map(b => ({
          id: b.id,
          imfId: this.activeImfIdSubj.value,
          name: b.name,
          city: b.city,
          address: b.address,
          manager: b.manager,
          phone: b.cellphone,
          email: b.email,
          status: b.status
        }));
        this.branchesSubj.next(mappedBranches);
      },
      error: (err) => console.error('Error loading branches:', err)
    });

    this.http.get<any>('/api/clients').subscribe({
      next: (res) => {
        // Handle paginated envelope if any, e.g. { items: [], totalCount: 0 }
        const items = Array.isArray(res) ? res : (res.items || []);
        const mappedClients = items.map((c: any) => ({
          id: c.id,
          imfId: this.activeImfIdSubj.value,
          branchId: c.branchId,
          name: c.name,
          bi: c.bi,
          phone: c.phone,
          province: c.province,
          district: c.district,
          neighborhood: c.neighborhood,
          address: c.address,
          business: c.business,
          businessYears: c.businessYears,
          income: c.income,
          emergencyName: c.emergencyName,
          emergencyRelation: c.emergencyRelation,
          emergencyPhone: c.emergencyPhone,
          maritalStatus: c.maritalStatus,
          status: c.status,
          loanCycle: c.loanCycle,
          guarantees: c.guarantees
        }));
        this.clientsSubj.next(mappedClients);
      },
      error: (err) => console.error('Error loading clients:', err)
    });

    this.http.get<any>('/api/loans').subscribe({
      next: (res) => {
        const items = Array.isArray(res) ? res : (res.items || []);
        const mappedLoans = items.map((l: any) => ({
          id: l.id,
          imfId: this.activeImfIdSubj.value,
          branchId: l.branchId,
          clientId: l.clientId,
          clientName: l.clientName || 'Cliente',
          amount: l.amount,
          interestRate: l.interestRate,
          term: l.termMonths,
          totalToPay: l.totalToPay,
          paidAmount: l.paidAmount,
          installmentsCount: l.installments?.length || l.termMonths,
          paidInstallments: l.installments?.filter((inst: any) => inst.status === 'Paid').length || 0,
          monthlyPayment: l.installments?.[0]?.totalAmount || (l.totalToPay / l.termMonths),
          date: l.disbursedAt ? new Date(l.disbursedAt).toLocaleDateString() : '-',
          status: l.status,
          nextPayment: l.nextPaymentDate ? new Date(l.nextPaymentDate).toLocaleDateString() : '-',
          disbursementMethod: l.disbursementMethod
        }));
        this.loansSubj.next(mappedLoans);
      },
      error: (err) => console.error('Error loading loans:', err)
    });

    this.http.get<any>('/api/transactions').subscribe({
      next: (res) => {
        const items = Array.isArray(res) ? res : (res.items || []);
        const mappedTransactions = items.map((t: any) => ({
          id: t.id,
          imfId: this.activeImfIdSubj.value,
          branchId: t.branchId,
          description: t.description,
          amount: t.amount,
          date: new Date(t.date).toLocaleDateString(),
          category: t.category,
          type: t.type
        }));
        this.transactionsSubj.next(mappedTransactions);
      },
      error: (err) => console.error('Error loading transactions:', err)
    });
  }

  addBranch(branch: Partial<Branch>) {
    const payload = {
      name: branch.name,
      cellphone: branch.phone,
      email: branch.email,
      city: branch.city,
      address: branch.address,
      manager: branch.manager,
      status: 'Active'
    };
    this.http.post<any>('/api/branches', payload).subscribe({
      next: () => {
        this.loadAllData();
        this.notificationService.addNotification({
          title: 'Agência Criada',
          message: `A agência ${branch.name} foi registrada com sucesso.`,
          type: 'success'
        });
      },
      error: (err) => console.error('Error adding branch:', err)
    });
  }

  updateBranch(updated: Branch) {
    this.http.put<any>(`/api/branches/${updated.id}`, updated).subscribe({
      next: () => {
        this.loadAllData();
      },
      error: (err) => console.error('Error updating branch:', err)
    });
  }

  deleteBranch(id: number) {
    this.http.delete<any>(`/api/branches/${id}`).subscribe({
      next: () => {
        this.loadAllData();
      },
      error: (err) => console.error('Error deleting branch:', err)
    });
  }

  getClientById(id: any) {
    return this.clientsSubj.asObservable().pipe(map(clients => clients.find(c => c.id === id)));
  }

  getLoansByClientName(name: string) {
    return this.loansSubj.asObservable().pipe(map(loans => loans.filter(l => l.clientName === name)));
  }

  addClient(client: Partial<Client>) {
    const payload = {
      name: client.name,
      bi: client.bi,
      phone: client.phone,
      maritalStatus: client.maritalStatus,
      province: client.province,
      district: client.district,
      neighborhood: client.neighborhood,
      address: client.address,
      business: client.business,
      businessYears: client.businessYears,
      income: client.income,
      emergencyName: client.emergencyName,
      emergencyRelation: client.emergencyRelation,
      emergencyPhone: client.emergencyPhone,
      guarantees: client.guarantees || []
    };
    this.http.post<any>('/api/clients', payload).subscribe({
      next: () => {
        this.loadAllData();
        this.notificationService.addNotification({
          title: 'Cliente Criado',
          message: `O cliente ${client.name} foi registrado com sucesso.`,
          type: 'success'
        });
      },
      error: (err) => console.error('Error adding client:', err)
    });
  }
  
  updateClient(id: any, updates: Partial<Client>) {
    this.http.put<any>(`/api/clients/${id}`, updates).subscribe({
      next: () => {
        this.loadAllData();
      },
      error: (err) => console.error('Error updating client:', err)
    });
  }

  addLoan(loan: Partial<Loan>) {
    const payload = {
      clientId: loan.clientId,
      amount: loan.amount,
      interestRate: loan.interestRate,
      termMonths: loan.term
    };
    this.http.post<any>('/api/loans', payload).subscribe({
      next: () => {
        this.loadAllData();
      },
      error: (err) => console.error('Error adding loan:', err)
    });
  }

  disburseLoan(loanId: string, method: 'M-Pesa' | 'E-Mola' | 'Banco') {
    const payload = {
      disbursementMethod: method,
      reference: 'REF-' + Math.random().toString(36).substring(7).toUpperCase()
    };
    this.http.post<any>(`/api/loans/${loanId}/disburse`, payload).subscribe({
      next: () => {
        this.loadAllData();
      },
      error: (err) => console.error('Error disbursing loan:', err)
    });
  }

  registerPayment(loanId: number | string, amount: number, channel: 'M-Pesa' | 'E-Mola' | 'Banco' | string = 'M-Pesa') {
    const payload = {
      amount: amount,
      channel: channel,
      reference: 'PAY-' + Math.random().toString(36).substring(7).toUpperCase()
    };
    this.http.post<any>(`/api/loans/${loanId}/payments`, payload).subscribe({
      next: () => {
        this.loadAllData();
      },
      error: (err) => console.error('Error registering payment:', err)
    });
  }

  addManualTransaction(tx: Partial<Transaction>) {
    const payload = {
      description: tx.description,
      amount: tx.amount,
      category: tx.category,
      type: tx.type,
      transactionDate: tx.date ? new Date(tx.date).toISOString() : new Date().toISOString()
    };
    this.http.post<any>('/api/transactions', payload).subscribe({
      next: () => {
        this.loadAllData();
        this.notificationService.addNotification({
          title: 'Transação Registrada',
          message: `A transação "${tx.description}" foi registrada com sucesso.`,
          type: 'success'
        });
      },
      error: (err) => console.error('Error adding transaction:', err)
    });
  }

  simulate30DaysLater() {
    this.http.post<any>('/api/loans/process-overdue', {}).subscribe({
      next: () => {
        this.loadAllData();
        this.notificationService.addNotification({
          title: 'Simulação Executada',
          message: 'Processamento de atraso concluído no servidor.',
          type: 'info'
        });
      },
      error: (err) => console.error('Error simulating overdue:', err)
    });
  }
}
