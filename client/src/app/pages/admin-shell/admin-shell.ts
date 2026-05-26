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
  
  imfs$ = this.stateService.imfs$;
  activeImf$ = this.stateService.activeImf$;
  activeImfId$ = this.stateService.activeImfId$;
  
  showNotifications = false;

  toggleNotifications() {
    this.showNotifications = !this.showNotifications;
    if (!this.showNotifications) {
      this.notificationService.markAllAsRead();
    }
  }

  markAsRead(id: string) {
    this.notificationService.markAsRead(id);
  }

  switchImf(id: string) {
    this.stateService.switchImf(id);
  }
}
