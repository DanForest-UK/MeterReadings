namespace MeterReading.Domain.Entities;

/// <summary>
/// Account type
/// </summary>
public record Account(AccountId AccountId, Person Person)
{
    // Parameterless constructor for
    private Account() : this(new AccountId(0), new Person("", "")) { }
};

