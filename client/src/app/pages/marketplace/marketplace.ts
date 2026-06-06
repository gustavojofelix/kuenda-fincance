import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { StateService } from '../../core/state.service';
import { NotificationService } from '../../core/notification.service';

@Component({
  selector: 'app-marketplace',
  imports: [CommonModule, FormsModule],
  templateUrl: './marketplace.html'
})
export class Marketplace {
  private stateService = inject(StateService);
  private notificationService = inject(NotificationService);

  // Simulation Sliders
  amount = 5000;
  term = 3;
  interestRate = 5; // 5% monthly rate

  // Wizard Steps
  currentStep = 1;
  submitted = false;

  // Form Fields
  fullName = '';
  phone = '';
  business = '';
  businessYears = '';
  estimatedMonthlyRevenue: number | null = null;
  province = 'Maputo';
  district = '';
  neighborhood = '';
  consent = false;

  get monthlyPayment(): number {
    const monthlyRate = this.interestRate / 100;
    if (monthlyRate === 0) {
      return Math.round(this.amount / this.term);
    }
    const payment = (this.amount * monthlyRate) / (1 - Math.pow(1 + monthlyRate, -this.term));
    return Math.round(payment);
  }

  get totalToPay(): number {
    return this.monthlyPayment * this.term;
  }

  nextStep() {
    if (this.currentStep < 4) {
      this.currentStep++;
    }
  }

  prevStep() {
    if (this.currentStep > 1) {
      this.currentStep--;
    }
  }

  // Calculate a mock credit score between 300 and 850
  calculateMockScore(): number {
    let score = 450;
    
    // Years in business
    const years = parseFloat(this.businessYears) || 0;
    if (years >= 3) score += 120;
    else if (years >= 1) score += 60;
    
    // Revenue to loan ratio
    const rev = this.estimatedMonthlyRevenue || 0;
    if (rev > this.amount * 3) score += 180;
    else if (rev > this.amount * 1.5) score += 100;
    else if (rev > this.amount) score += 50;

    // Contact/Telecom factor
    if (this.phone.includes('84') || this.phone.includes('85')) {
      score += 40; // Vodacom score bump
    } else if (this.phone.includes('86') || this.phone.includes('87')) {
      score += 30; // Movitel/mcel score bump
    }

    return Math.min(850, score);
  }

  submitForm(event: Event) {
    event.preventDefault();
    if (this.fullName && this.business && this.phone && this.amount && this.consent) {
      const mockScore = this.calculateMockScore();

      // Create lead in state
      this.stateService.addClient({
        name: this.fullName,
        business: this.business,
        phone: this.phone,
        income: this.amount.toString(), // Request amount
        requestedTerm: this.term,
        estimatedMonthlyRevenue: this.estimatedMonthlyRevenue || 0,
        scoreSimulado: mockScore,
        province: this.province,
        district: this.district,
        neighborhood: this.neighborhood,
        businessYears: this.businessYears + ' anos',
        status: 'Avaliação'
      });

      // Notify admin
      this.notificationService.addNotification({
        title: 'Novo Lead Marketplace',
        message: `${this.fullName} solicitou ${this.amount} MZN em ${this.term} meses. Score Simulado: ${mockScore}.`,
        type: 'info',
        link: '/admin/leads'
      });

      this.submitted = true;
    }
  }

  resetForm() {
    this.fullName = '';
    this.phone = '';
    this.business = '';
    this.businessYears = '';
    this.estimatedMonthlyRevenue = null;
    this.province = 'Maputo';
    this.district = '';
    this.neighborhood = '';
    this.consent = false;
    this.currentStep = 1;
    this.submitted = false;
  }
}
