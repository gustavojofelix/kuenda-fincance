import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { StateService } from '../../core/state.service';
import { map } from 'rxjs';

@Component({
  selector: 'app-dashboard',
  imports: [CommonModule, RouterLink],
  templateUrl: './dashboard.html'
})
export class Dashboard {
  stateService = inject(StateService);
  metrics$ = this.stateService.metrics$;
  recentLoans$ = this.stateService.loans$.pipe(
    map(loans => loans.slice(0, 3))
  );
}
