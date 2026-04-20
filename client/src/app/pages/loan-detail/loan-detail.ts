import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { StateService, Loan } from '../../core/state.service';
import { Observable } from 'rxjs';

@Component({
  selector: 'app-loan-detail',
  imports: [CommonModule, RouterLink],
  templateUrl: './loan-detail.html'
})
export class LoanDetail implements OnInit {
  private route = inject(ActivatedRoute);
  private stateService = inject(StateService);

  loan$: Observable<Loan | undefined> = new Observable();

  ngOnInit() {
    this.route.paramMap.subscribe(params => {
      const id = params.get('id');
      if (id) {
        this.loan$ = this.stateService.loans$.pipe(
          map(loans => loans.find(l => l.id === id))
        );
      }
    });
  }

  // Simplified history for mock
  history = [
    { date: '12 Mar 2024', amount: 875, method: 'M-Pesa', status: 'Confirmado' },
    { date: '12 Fev 2024', amount: 875, method: 'M-Pesa', status: 'Confirmado' },
    { date: '12 Jan 2024', amount: 875, method: 'E-Mola', status: 'Confirmado' }
  ];
}

// Add map operator to the imports or use it from rxjs
import { map } from 'rxjs/operators';
