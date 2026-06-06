import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { StateService } from '../../core/state.service';
import * as ExcelJS from 'exceljs';

@Component({
  selector: 'app-reports',
  imports: [CommonModule, FormsModule],
  templateUrl: './reports.html'
})
export class Reports implements OnInit {
  private stateService = inject(StateService);

  totalClients$ = this.stateService.totalClientsCount$;
  totalPortfolio$ = this.stateService.totalActivePortfolio$;

  selectedReport: string | null = null;
  startDate = '2024-03-01';
  endDate = '2024-03-31';
  format = 'Excel (.xlsx)';
  
  isGenerating = false;

  clients: any[] = [];
  loans: any[] = [];
  transactions: any[] = [];
  activeImf: any = null;

  reportCategories = [
    {
      title: 'Regulatórios (Banco de Moçambique)',
      reports: [
        { id: 'MAP10', name: 'MAP 10 - Balancete de Instituições Financeiras', desc: 'Registo mensal de activos e passivos.' },
        { id: 'MAP14', name: 'MAP 14 - Relação de Grandes Exposições', desc: 'Créditos que excedem 10% do capital próprio.' }
      ]
    },
    {
      title: 'Gestão de Crédito',
      reports: [
        { id: 'PORTFOLIO', name: 'Relatório de Carteira Ativa', desc: 'Detalhe de todos os empréstimos em circulação.' },
        { id: 'PAR', name: 'Relatório de PAR > 30 (Atrasos)', desc: 'Análise de risco e créditos em incumprimento.' },
        { id: 'DISBURSE', name: 'Novos Desembolsos por Período', desc: 'Volume de capital injectado no mercado.' }
      ]
    },
    {
      title: 'Financeiro & Contabilidade',
      reports: [
        { id: 'CASHFLOW', name: 'Fluxo de Caixa Consolidado', desc: 'Entradas, saídas e saldo operacional.' },
        { id: 'PL', name: 'Demonstração de Resultados (P&L)', desc: 'Lucros e perdas baseados em juros e custos.' }
      ]
    }
  ];

  exportHistory = [
    { name: 'Relatório de Carteira Ativa', date: '05 Jun 2026', user: 'Admin', format: 'Excel' },
    { name: 'MAP 10 - Balancete de Instituições Financeiras', date: '01 Jun 2026', user: 'Admin', format: 'Excel' }
  ];

  ngOnInit() {
    this.stateService.clients$.subscribe(data => this.clients = data);
    this.stateService.loans$.subscribe(data => this.loans = data);
    this.stateService.transactions$.subscribe(data => this.transactions = data);
    this.stateService.activeImf$.subscribe(data => this.activeImf = data);
  }

  selectReport(id: string) {
    this.selectedReport = id;
  }

  // Parse helper for local dates (e.g. "12 Fev 2024" or "2024-03-05")
  private parseDateString(dateStr: string): Date {
    if (!dateStr || dateStr === '-') return new Date(0);
    if (dateStr.includes('-')) return new Date(dateStr);
    
    // Parse "12 Fev 2024"
    const parts = dateStr.split(' ');
    if (parts.length === 3) {
      const day = parseInt(parts[0], 10);
      const year = parseInt(parts[2], 10);
      const months: { [key: string]: number } = {
        'jan': 0, 'feb': 1, 'fev': 1, 'mar': 2, 'apr': 3, 'abr': 3,
        'may': 4, 'mai': 4, 'jun': 5, 'jul': 6, 'aug': 7, 'ago': 7,
        'sep': 8, 'set': 8, 'oct': 9, 'out': 9, 'nov': 10, 'dec': 11, 'dez': 11
      };
      const monthStr = parts[1].toLowerCase().substring(0, 3);
      const month = months[monthStr] !== undefined ? months[monthStr] : 0;
      return new Date(year, month, day);
    }
    return new Date(dateStr);
  }

  private isWithinDateRange(dateStr: string): boolean {
    const date = this.parseDateString(dateStr);
    const start = new Date(this.startDate);
    const end = new Date(this.endDate);
    // Adjust end date to end of day
    end.setHours(23, 59, 59, 999);
    return date >= start && date <= end;
  }

  // Gets the report name
  getSelectedReportName(): string {
    return this.reportCategories
      .flatMap(c => c.reports)
      .find(r => r.id === this.selectedReport)?.name || '';
  }

  // Generates preview data dynamically
  getReportData(): { headers: string[], rows: any[][], title: string } {
    if (!this.selectedReport) return { headers: [], rows: [], title: '' };

    const title = this.getSelectedReportName();
    let headers: string[] = [];
    let rows: any[][] = [];

    switch (this.selectedReport) {
      case 'MAP10': {
        headers = ['Código de Conta', 'Descrição da Conta', 'Ativo / Devedor (MZN)', 'Passivo / Credor (MZN)'];
        
        // Calculate portfolio and cash dynamically
        const cashBalance = this.transactions.reduce((acc, t) => acc + (t.type === 'Entrada' ? t.amount : -t.amount), 500000);
        const activePortfolio = this.loans.filter(l => l.status === 'Ativo' || l.status === 'Atrasado').reduce((acc, l) => acc + (l.totalToPay - l.paidAmount), 0);
        const totalInterest = this.loans.reduce((acc, l) => acc + (l.totalToPay - l.amount), 0);
        
        rows = [
          ['11.100.00', 'Disponibilidades (Caixa e Bancos)', cashBalance.toLocaleString('pt-MZ') + ' MZN', '0,00 MZN'],
          ['14.200.00', 'Carteira de Crédito Bruta', activePortfolio.toLocaleString('pt-MZ') + ' MZN', '0,00 MZN'],
          ['14.900.00', 'Juros a Receber de Clientes', totalInterest.toLocaleString('pt-MZ') + ' MZN', '0,00 MZN'],
          ['21.100.00', 'Capital Social Autorizado', '0,00 MZN', '500.000,00 MZN'],
          ['29.100.00', 'Resultados Transitados (Lucro/Prejuízo)', '0,00 MZN', ((cashBalance + activePortfolio + totalInterest) - 500000).toLocaleString('pt-MZ') + ' MZN']
        ];
        break;
      }
      case 'MAP14': {
        headers = ['Nº Contrato', 'Cliente', 'Valor do Crédito (MZN)', 'Exposição sobre Capital (%)'];
        const equity = 500000;
        rows = this.loans
          .filter(l => l.amount >= 15000)
          .map(l => {
            const expRatio = ((l.amount / equity) * 100).toFixed(1) + '%';
            return [l.id, l.clientName, l.amount.toLocaleString('pt-MZ') + ' MZN', expRatio];
          });
        break;
      }
      case 'PORTFOLIO': {
        headers = ['Contrato', 'Cliente', 'Montante (MZN)', 'Taxa (%)', 'Prazo', 'Pago (MZN)', 'Saldo Devedor (MZN)', 'Estado'];
        rows = this.loans
          .filter(l => l.status === 'Ativo' || l.status === 'Atrasado')
          .map(l => [
            l.id,
            l.clientName,
            l.amount.toLocaleString('pt-MZ') + ' MZN',
            l.interestRate + '%',
            l.term + ' Meses',
            l.paidAmount.toLocaleString('pt-MZ') + ' MZN',
            (l.totalToPay - l.paidAmount).toLocaleString('pt-MZ') + ' MZN',
            l.status
          ]);
        break;
      }
      case 'PAR': {
        headers = ['Contrato', 'Cliente', 'Valor Contratado (MZN)', 'Saldo Devedor (MZN)', 'Próximo Pagamento', 'Estado'];
        rows = this.loans
          .filter(l => l.status === 'Atrasado')
          .map(l => [
            l.id,
            l.clientName,
            l.amount.toLocaleString('pt-MZ') + ' MZN',
            (l.totalToPay - l.paidAmount).toLocaleString('pt-MZ') + ' MZN',
            l.nextPayment,
            l.status
          ]);
        break;
      }
      case 'DISBURSE': {
        headers = ['Contrato', 'Cliente', 'Data de Desembolso', 'Valor Principal (MZN)', 'Prazo', 'Método'];
        rows = this.loans
          .filter(l => l.date !== '-' && this.isWithinDateRange(l.date))
          .map(l => [
            l.id,
            l.clientName,
            l.date,
            l.amount.toLocaleString('pt-MZ') + ' MZN',
            l.term + ' Meses',
            l.disbursementMethod || 'M-Pesa'
          ]);
        break;
      }
      case 'CASHFLOW': {
        headers = ['Transação', 'Data', 'Descrição', 'Categoria', 'Fluxo', 'Valor (MZN)'];
        rows = this.transactions
          .filter(t => this.isWithinDateRange(t.date))
          .map(t => [
            t.id,
            t.date,
            t.description,
            t.category,
            t.type,
            t.amount.toLocaleString('pt-MZ') + ' MZN'
          ]);
        break;
      }
      case 'PL': {
        headers = ['Rubrica de Proveito/Custo', 'Valor Acumulado (MZN)'];
        
        const interestInflow = this.transactions
          .filter(t => t.category === 'Receita Juros' || t.category === 'Amortização')
          .reduce((acc, t) => acc + t.amount, 0);
        
        const salaries = this.transactions.filter(t => t.category === 'Salários').reduce((acc, t) => acc + t.amount, 0);
        const rents = this.transactions.filter(t => t.category === 'Renda').reduce((acc, t) => acc + t.amount, 0);
        const utilities = this.transactions.filter(t => t.category === 'Energia' || t.category === 'Água').reduce((acc, t) => acc + t.amount, 0);
        
        rows = [
          ['(+) Receitas de Amortizações e Juros', interestInflow.toLocaleString('pt-MZ') + ' MZN'],
          ['(-) Despesas com Salários e Pessoal', salaries.toLocaleString('pt-MZ') + ' MZN'],
          ['(-) Despesas de Rendas de Instalações', rents.toLocaleString('pt-MZ') + ' MZN'],
          ['(-) Custos de Consumo (Água/EDM)', utilities.toLocaleString('pt-MZ') + ' MZN'],
          ['Resultado Líquido do Exercício', (interestInflow - (salaries + rents + utilities)).toLocaleString('pt-MZ') + ' MZN']
        ];
        break;
      }
    }

    return { headers, rows, title };
  }

  // Triggers immediate download or print based on format
  exportReport(formatType: 'PDF' | 'EXCEL' | 'CSV') {
    const data = this.getReportData();
    if (data.rows.length === 0) {
      alert('Nenhum dado encontrado para exportar no período selecionado.');
      return;
    }

    if (formatType === 'CSV') {
      this.downloadCSV(data.title, data.headers, data.rows);
    } else if (formatType === 'EXCEL') {
      this.downloadExcel(data.title, data.headers, data.rows);
    } else if (formatType === 'PDF') {
      this.printPDF(data.title, data.headers, data.rows);
    }

    // Add to export history
    this.exportHistory.unshift({
      name: data.title,
      date: new Date().toLocaleDateString('pt-PT', { day: '2-digit', month: 'short', year: 'numeric' }),
      user: 'Admin',
      format: formatType === 'EXCEL' ? 'Excel' : formatType
    });
  }

  private downloadCSV(title: string, headers: string[], rows: any[][]) {
    // Generate clean CSV content
    const csvContent = [
      headers.join(';'),
      ...rows.map(row => row.map(val => `"${String(val).replace(/"/g, '""')}"`).join(';'))
    ].join('\n');
    
    const uniqueId = new Date().toISOString().replace(/[-:T]/g, '_').split('.')[0];
    const blob = new Blob([new Uint8Array([0xEF, 0xBB, 0xBF]), csvContent], { type: 'text/csv;charset=utf-8;' });
    const link = document.createElement('a');
    link.href = URL.createObjectURL(blob);
    link.download = `${title.toLowerCase().replace(/[^a-z0-9]/g, '_')}_${uniqueId}.csv`;
    link.click();
  }

  private async downloadExcel(title: string, headers: string[], rows: any[][]) {
    const workbook = new ExcelJS.Workbook();
    const worksheet = workbook.addWorksheet(title.substring(0, 31));

    // Get primary brand color
    const primaryColor = this.activeImf?.primaryColor ? this.activeImf.primaryColor.replace('#', '') : '10B981';

    // Corporate Header row
    const titleRow = worksheet.addRow([this.activeImf?.name || 'Kuenda Microfinanças']);
    titleRow.font = { name: 'Arial', size: 16, bold: true, color: { argb: 'FF0F172A' } };
    worksheet.mergeCells('A1:E1');
    worksheet.getRow(1).height = 30;

    // Report title
    const docTitleRow = worksheet.addRow([title]);
    docTitleRow.font = { name: 'Arial', size: 12, bold: true, color: { argb: 'FF475569' } };
    worksheet.mergeCells('A2:E2');
    worksheet.getRow(2).height = 20;

    // Period / Generation Date
    const metaRow = worksheet.addRow([`Período: ${this.startDate} a ${this.endDate} | Gerado em: ${new Date().toLocaleDateString('pt-PT')}`]);
    metaRow.font = { name: 'Arial', size: 10, italic: true, color: { argb: 'FF64748B' } };
    worksheet.mergeCells('A3:E3');
    worksheet.getRow(3).height = 20;

    // Blank line
    worksheet.addRow([]);

    // Table Header Row
    const headerRow = worksheet.addRow(headers);
    headerRow.height = 25;
    headerRow.eachCell((cell) => {
      cell.fill = {
        type: 'pattern',
        pattern: 'solid',
        fgColor: { argb: 'FF' + primaryColor }
      };
      cell.font = {
        name: 'Arial',
        size: 11,
        bold: true,
        color: { argb: 'FFFFFFFF' }
      };
      cell.alignment = { vertical: 'middle', horizontal: 'left' };
      cell.border = {
        bottom: { style: 'medium', color: { argb: 'FF0F172A' } }
      };
    });

    // Data Rows
    rows.forEach((rowData) => {
      const row = worksheet.addRow(rowData);
      row.height = 20;
      row.eachCell((cell) => {
        cell.font = { name: 'Arial', size: 10, color: { argb: 'FF334155' } };
        cell.alignment = { vertical: 'middle', horizontal: 'left' };
        cell.border = {
          bottom: { style: 'thin', color: { argb: 'FFE2E8F0' } }
        };
      });
    });

    // Auto column fitting
    worksheet.columns.forEach((column) => {
      let maxLen = 0;
      column.eachCell!((cell) => {
        if (cell.value) {
          const valStr = cell.value.toString();
          if (valStr.length > maxLen) {
            maxLen = valStr.length;
          }
        }
      });
      column.width = Math.max(maxLen + 4, 15);
    });

    // Generate output xlsx buffer
    const uniqueId = new Date().toISOString().replace(/[-:T]/g, '_').split('.')[0];
    const buffer = await workbook.xlsx.writeBuffer();
    const blob = new Blob([buffer], { type: 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet' });
    const link = document.createElement('a');
    link.href = URL.createObjectURL(blob);
    link.download = `${title.toLowerCase().replace(/[^a-z0-9]/g, '_')}_${uniqueId}.xlsx`;
    link.click();
  }



  private printPDF(title: string, headers: string[], rows: any[][]) {
    const printWindow = window.open('', '_blank');
    if (!printWindow) return;
    
    const imfName = this.activeImf?.name || 'Kuenda Microfinanças';
    const nuit = this.activeImf?.nuit || '400567123';
    const email = this.activeImf?.email || 'contacto@kuenda.co.mz';
    const uniqueId = new Date().toISOString().replace(/[-:T]/g, '_').split('.')[0];
    
    let html = `
      <html>
        <head>
          <title>${title}_${uniqueId}</title>
          <style>
            body { font-family: 'Helvetica Neue', Helvetica, Arial, sans-serif; color: #1e293b; padding: 40px; }
            .header-table { width: 100%; border: none; margin-bottom: 30px; }
            .header-left { font-size: 20px; font-weight: 900; color: #0f172a; }
            .header-right { text-align: right; font-size: 11px; color: #64748b; line-height: 1.5; }
            .title-section { border-top: 2px solid #10b981; padding-top: 15px; margin-bottom: 25px; }
            h1 { font-size: 22px; font-weight: 800; margin: 0 0 5px 0; color: #0f172a; }
            .meta { font-size: 12px; color: #64748b; }
            table.data-table { width: 100%; border-collapse: collapse; margin-top: 10px; }
            table.data-table th { background-color: #f8fafc; border-bottom: 2px solid #e2e8f0; color: #475569; font-weight: 800; text-align: left; padding: 12px 10px; font-size: 11px; text-transform: uppercase; letter-spacing: 0.5px; }
            table.data-table td { border-bottom: 1px solid #f1f5f9; padding: 12px 10px; font-size: 12px; color: #334155; }
            table.data-table tr:nth-child(even) td { background-color: #f8fafc; }
            .footer { margin-top: 50px; border-top: 1px solid #e2e8f0; padding-top: 15px; font-size: 10px; color: #94a3b8; text-align: center; }
            @media print {
              body { padding: 0; }
            }
          </style>
        </head>
        <body>
          <table class="header-table">
            <tr>
              <td class="header-left" style="border:none; padding:0;">${imfName}</td>
              <td class="header-right" style="border:none; padding:0;">
                NUIT: ${nuit}<br>
                Email: ${email}<br>
                Gerado por: Admin
              </td>
            </tr>
          </table>
          
          <div class="title-section">
            <h1>${title}</h1>
            <div class="meta">Período: ${this.startDate} a ${this.endDate} | Data de Emissão: ${new Date().toLocaleDateString('pt-PT')} | ID: ${uniqueId}</div>
          </div>
          
          <table class="data-table">
            <thead>
              <tr>
                ${headers.map(h => `<th>${h}</th>`).join('')}
              </tr>
            </thead>
            <tbody>
              ${rows.map(row => `<tr>${row.map(val => `<td>${val}</td>`).join('')}</tr>`).join('')}
            </tbody>
          </table>
          
          <div class="footer">
            Documento de uso interno gerado pela plataforma de gestão Kuenda Finance. © ${new Date().getFullYear()}
          </div>
          
          <script>
            window.onload = function() {
              window.print();
              setTimeout(function() { window.close(); }, 500);
            }
          </script>
        </body>
      </html>
    `;
    
    printWindow.document.write(html);
    printWindow.document.close();
  }

  generateReport() {
    if (!this.selectedReport) return;
    
    this.isGenerating = true;
    
    // Simulate generation delay
    setTimeout(() => {
      this.isGenerating = false;
      
      const formatExt = this.format === 'Excel (.xlsx)' ? 'EXCEL' : (this.format === 'PDF Document' ? 'PDF' : 'CSV');
      this.exportReport(formatExt);
    }, 1200);
  }
}

