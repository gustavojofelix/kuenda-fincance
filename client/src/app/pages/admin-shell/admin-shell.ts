import { Component, inject } from '@angular/core';
import { RouterOutlet, RouterLink, RouterLinkActive } from '@angular/router';
import { CommonModule } from '@angular/common';
import { NotificationService } from '../../core/notification.service';
import { StateService } from '../../core/state.service';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-admin-shell',
  imports: [RouterOutlet, RouterLink, RouterLinkActive, CommonModule, FormsModule],
  templateUrl: './admin-shell.html'
})
export class AdminShell {
  notificationService = inject(NotificationService);
  stateService = inject(StateService);
  
  notifications$ = this.notificationService.notifications$;
  unreadCount$ = this.notificationService.unreadCount$;
  
  activeImf$ = this.stateService.activeImf$;
  activeImfId$ = this.stateService.activeImfId$;
  currentUser$ = this.stateService.currentUser$;
  
  branches$ = this.stateService.branches$;
  activeBranchId$ = this.stateService.activeBranchId$;
  
  showNotifications = false;

  onBranchSelected(event: any) {
    const val = event.target.value;
    if (val === 'all') {
      this.stateService.switchBranch('all');
    } else {
      this.stateService.switchBranch(Number(val));
    }
  }

  toggleNotifications() {
    this.showNotifications = !this.showNotifications;
    if (!this.showNotifications) {
      this.notificationService.markAllAsRead();
    }
  }

  markAsRead(id: string) {
    this.notificationService.markAsRead(id);
  }
}
