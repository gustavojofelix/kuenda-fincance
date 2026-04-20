import { Routes } from '@angular/router';
import { Login } from './pages/login/login';
import { Marketplace } from './pages/marketplace/marketplace';
import { AdminShell } from './pages/admin-shell/admin-shell';
import { Dashboard } from './pages/dashboard/dashboard';
import { Clients } from './pages/clients/clients';
import { Loans } from './pages/loans/loans';
import { Accounting } from './pages/accounting/accounting';
import { Reports } from './pages/reports/reports';
import { Settings } from './pages/settings/settings';
import { Leads } from './pages/leads/leads';
import { ClientProfile } from './pages/client-profile/client-profile';
import { ClientForm } from './pages/client-form/client-form';
import { RegisterLoan } from './pages/register-loan/register-loan';
import { LoanDetail } from './pages/loan-detail/loan-detail';

export const routes: Routes = [
  { path: '', redirectTo: 'login', pathMatch: 'full' },
  { path: 'login', component: Login },
  { path: 'marketplace', component: Marketplace },
  {
    path: 'admin',
    component: AdminShell,
    children: [
      { path: '', redirectTo: 'dashboard', pathMatch: 'full' },
      { path: 'dashboard', component: Dashboard },
      { path: 'leads', component: Leads },
      { path: 'clients', component: Clients },
      { path: 'clients/register', component: ClientForm },
      { path: 'clients/edit/:id', component: ClientForm },
      { path: 'clients/:id', component: ClientProfile },
      { path: 'loans', component: Loans },
      { path: 'loans/register', component: RegisterLoan },
      { path: 'loans/:id', component: LoanDetail },
      { path: 'accounting', component: Accounting },
      { path: 'reports', component: Reports },
      { path: 'settings', component: Settings },
    ]
  },
  { path: '**', redirectTo: 'login' }
];
