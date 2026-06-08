import { Injectable } from '@angular/core';
import { BehaviorSubject, map } from 'rxjs';

export interface KuendaNotification {
  id: string;
  title: string;
  message: string;
  type: 'info' | 'warning' | 'success' | 'danger';
  date: Date;
  read: boolean;
  link?: string;
}

export interface Toast {
  id: string;
  title: string;
  message: string;
  type: 'info' | 'warning' | 'success' | 'danger';
}

@Injectable({
  providedIn: 'root'
})
export class NotificationService {
  private notificationsSubj = new BehaviorSubject<KuendaNotification[]>([
    {
      id: '1',
      title: 'Novo Lead',
      message: 'António Silva solicitou 50.000 MZN via Marketplace.',
      type: 'info',
      date: new Date(),
      read: false,
      link: '/admin/leads'
    },
    {
      id: '2',
      title: 'Crédito em Atraso',
      message: 'O contrato L-10023 de Maria Santos venceu há 2 dias.',
      type: 'danger',
      date: new Date(Date.now() - 86400000),
      read: false,
      link: '/admin/loans/L-10023'
    }
  ]);

  notifications$ = this.notificationsSubj.asObservable();
  unreadCount$ = this.notifications$.pipe(map(ns => ns.filter(n => !n.read).length));

  // Global Toast System
  private toastsSubj = new BehaviorSubject<Toast[]>([]);
  toasts$ = this.toastsSubj.asObservable();

  showToast(title: string, message: string, type: 'info' | 'warning' | 'success' | 'danger' = 'info') {
    const id = Math.random().toString(36).substring(2, 11);
    const newToast: Toast = { id, title, message, type };
    this.toastsSubj.next([...this.toastsSubj.value, newToast]);
    
    // Auto-remove after 4 seconds
    setTimeout(() => {
      this.removeToast(id);
    }, 4000);
  }

  removeToast(id: string) {
    this.toastsSubj.next(this.toastsSubj.value.filter(t => t.id !== id));
  }

  addNotification(n: Omit<KuendaNotification, 'id' | 'date' | 'read'>) {
    const current = this.notificationsSubj.value;
    const newNote: KuendaNotification = {
        ...n,
        id: Math.random().toString(36).substring(2, 11),
        date: new Date(),
        read: false
    };
    this.notificationsSubj.next([newNote, ...current]);
    
    // Proactively show a toast for every new notification too
    this.showToast(n.title, n.message, n.type);
  }

  markAllAsRead() {
    const updated = this.notificationsSubj.value.map(n => ({ ...n, read: true }));
    this.notificationsSubj.next(updated);
  }

  markAsRead(id: string) {
    const updated = this.notificationsSubj.value.map(n => n.id === id ? { ...n, read: true } : n);
    this.notificationsSubj.next(updated);
  }
}
