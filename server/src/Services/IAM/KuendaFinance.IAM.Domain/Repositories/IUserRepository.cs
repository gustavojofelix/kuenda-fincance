using KuendaFinance.IAM.Domain.Entities;

namespace KuendaFinance.IAM.Domain.Repositories;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    
    // The password is passed here so the infrastructure can hash it using Identity's UserManager
    Task<User> AddAsync(User user, string password, CancellationToken cancellationToken = default);
    
    Task UpdateAsync(User user, CancellationToken cancellationToken = default);
    Task DeleteAsync(User user, CancellationToken cancellationToken = default);
    Task<List<User>> GetTeamMembersAsync(Guid tenantId, CancellationToken cancellationToken = default);
    
    // Returns true if password is valid
    Task<bool> CheckPasswordAsync(User user, string password, CancellationToken cancellationToken = default);
}
