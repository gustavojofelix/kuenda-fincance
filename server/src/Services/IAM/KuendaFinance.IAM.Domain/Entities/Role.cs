namespace KuendaFinance.IAM.Domain.Entities;

public class Role
{
    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public string Description { get; private set; }

    private Role() { } // EF Core

    public Role(Guid id, string name, string description)
    {
        Id = id;
        Name = name;
        Description = description;
    }
}
