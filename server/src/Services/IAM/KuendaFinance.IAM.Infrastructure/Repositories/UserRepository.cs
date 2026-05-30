using KuendaFinance.IAM.Domain.Entities;
using KuendaFinance.IAM.Domain.Repositories;
using KuendaFinance.IAM.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;

namespace KuendaFinance.IAM.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly UserManager<ApplicationUser> _userManager;

    public UserRepository(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<User> AddAsync(User user, string password, CancellationToken cancellationToken = default)
    {
        var appUser = new ApplicationUser
        {
            Id = user.Id,
            UserName = user.Email,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt
        };

        var result = await _userManager.CreateAsync(appUser, password);
        
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            throw new Exception($"Failed to create user: {errors}");
        }

        return user;
    }

    public async Task<bool> CheckPasswordAsync(User user, string password, CancellationToken cancellationToken = default)
    {
        var appUser = await _userManager.FindByIdAsync(user.Id.ToString());
        if (appUser == null) return false;

        return await _userManager.CheckPasswordAsync(appUser, password);
    }

    public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        var appUser = await _userManager.FindByEmailAsync(email);
        if (appUser == null) return null;

        return MapToDomain(appUser);
    }

    public async Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var appUser = await _userManager.FindByIdAsync(id.ToString());
        if (appUser == null) return null;

        return MapToDomain(appUser);
    }

    public async Task UpdateAsync(User user, CancellationToken cancellationToken = default)
    {
        var appUser = await _userManager.FindByIdAsync(user.Id.ToString());
        if (appUser == null) throw new Exception("User not found");

        appUser.FirstName = user.FirstName;
        appUser.LastName = user.LastName;
        appUser.IsActive = user.IsActive;

        await _userManager.UpdateAsync(appUser);
    }

    private User MapToDomain(ApplicationUser appUser)
    {
        // Using reflection or a mapping method to reconstruct domain entity
        // Since constructor requires fields, we'll use it
        var user = new User(appUser.Id, appUser.Email!, appUser.FirstName, appUser.LastName);
        if (!appUser.IsActive) user.Deactivate();
        return user;
    }
}
