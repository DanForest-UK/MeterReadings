namespace MeterReading.API.DTOs
{
    /// <summary>
    /// DTO for API response for meter reading submission
    /// </summary>
    public record MeterReadingResponse(
       int Validated,
       int Failed,
       int Committed,
       string[] Errors);
}
