using KuendaFinance.Operations.Application.DTOs;
using KuendaFinance.Operations.Domain.Entities;

namespace KuendaFinance.Operations.Application.DTOs;

public static class TransactionMappingExtensions
{
    public static TransactionDto ToDto(this Transaction tx)
    {
        return new TransactionDto
        {
            Id = tx.Id,
            TenantId = tx.TenantId,
            BranchId = tx.BranchId,
            Description = tx.Description,
            Amount = tx.Amount,
            TransactionDate = tx.TransactionDate,
            Category = tx.Category,
            Type = tx.Type,
            IsAuto = tx.IsAuto,
            CreatedAt = tx.CreatedAt
        };
    }
}
