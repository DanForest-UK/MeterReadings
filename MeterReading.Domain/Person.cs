﻿namespace MeterReading.Domain;

/// <summary>
/// Person type
/// </summary>
public record Person(string FirstName, string LastName)
{    
    public string FullName => $"{FirstName} {LastName}";
}
