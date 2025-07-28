namespace MeterReading.Domain.Entities;

public record Account(AccountId AccountId, Person Person)
{
    // Parameterless constructor for EF
    private Account() : this(new AccountId(0), new Person("", "")) { }
};

