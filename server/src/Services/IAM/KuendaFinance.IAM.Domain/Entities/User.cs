namespace KuendaFinance.IAM.Domain.Entities;

public class User
{
    public Guid Id { get; private set; }
    public string Email { get; private set; }
    public string FirstName { get; private set; }
    public string LastName { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }
    
    // We don't store PasswordHash here if Identity handles it, 
    // but we might need it depending on how much we abstract.
    // Cleanest way is to let Infrastructure handle passwords completely,
    // and Domain just represents the user's business state.

    private User() { } // EF Core

    public User(Guid id, string email, string firstName, string lastName)
    {
        Id = id;
        Email = email;
        FirstName = firstName;
        LastName = lastName;
        IsActive = true;
        CreatedAt = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        IsActive = false;
    }

    public void Activate()
    {
        IsActive = true;
    }
}
