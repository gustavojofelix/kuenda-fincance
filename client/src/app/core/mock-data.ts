export const MOCK_CLIENTS = [
  { id: 1, name: 'João Chissano', bi: '100456123B', phone: '+258 84 123 4567', business: 'Mercearia', income: '10.001 - 25.000 MZN', maritalStatus: 'Casado', province: 'Maputo Cidade', district: 'KaMpfumo', neighborhood: 'Polana', address: 'Av. Julius Nyerere, 123', status: 'Em Dia', loanCycle: 2 },
  { id: 2, name: 'Anabela Mutemba', bi: '120987654M', phone: '+258 82 987 6543', business: 'Vestuário / Calçado', income: '25.001 - 50.000 MZN', maritalStatus: 'Solteira', province: 'Maputo Província', district: 'Matola', neighborhood: 'Fomento', address: 'Rua das Flores, 45', status: 'Atrasado', loanCycle: 1 },
  { id: 3, name: 'Carlos Sitoe', bi: '130345678C', phone: '+258 85 345 6789', business: 'Serviços / Manutenção', income: '25.001 - 50.000 MZN', maritalStatus: 'Casado', province: 'Maputo Província', district: 'Matola', neighborhood: 'Tchumene', address: 'Rua Principal, 89', status: 'Avaliação', loanCycle: 1 },
  { id: 4, name: 'Fátima Nhavene', bi: '140112233N', phone: '+258 84 112 2334', business: 'Estética / Cabeleireiro', income: '10.001 - 25.000 MZN', maritalStatus: 'Divorciada', province: 'Maputo Cidade', district: 'KaMaxakeni', neighborhood: 'Alto Maé', address: 'Av. Eduardo Mondlane, 400', status: 'Avaliação', loanCycle: 3 }
];

export const MOCK_LOANS = [
  { 
    id: 'L-10023', 
    clientName: 'João Chissano', 
    amount: 10000, 
    interestRate: 5, 
    term: 12, 
    totalToPay: 10500, 
    paidAmount: 8750, 
    monthlyPayment: 875, 
    installmentsCount: 12, 
    paidInstallments: 10,
    date: '12 Fev 2024', 
    status: 'Ativo', 
    nextPayment: '12 Mar 2024',
    disbursementMethod: 'M-Pesa'
  },
  { 
    id: 'L-10024', 
    clientName: 'Anabela Mutemba', 
    amount: 25000, 
    interestRate: 5, 
    term: 6, 
    totalToPay: 26250, 
    paidAmount: 13125, 
    monthlyPayment: 4375, 
    installmentsCount: 6, 
    paidInstallments: 3,
    date: '05 Jan 2024', 
    status: 'Atrasado', 
    nextPayment: '05 Fev 2024',
    disbursementMethod: 'E-mola'
  },
  { 
    id: 'L-10025', 
    clientName: 'Carlos Sitoe', 
    amount: 45000, 
    interestRate: 10, 
    term: 24, 
    totalToPay: 49500, 
    paidAmount: 0, 
    monthlyPayment: 2062.5, 
    installmentsCount: 24, 
    paidInstallments: 0,
    date: '-', 
    status: 'Em Análise', 
    nextPayment: '-'
  },
  { 
    id: 'L-10026', 
    clientName: 'Fátima Nhavene', 
    amount: 15000, 
    interestRate: 5, 
    term: 12, 
    totalToPay: 15750, 
    paidAmount: 0, 
    monthlyPayment: 1312.5, 
    installmentsCount: 12, 
    paidInstallments: 0,
    date: '-', 
    status: 'Em Análise', 
    nextPayment: '-'
  }
];

export const MOCK_METRICS = {
  activePortfolio: '1.240.500 MZN',
  defaultRate: '4.2%',
  newLeads: 8,
  upcomingDue: 14
};
