using MeterReading.API.DTOs;
using MeterReading.Domain;
using MeterReading.Infrastructure.Services;
using Microsoft.AspNetCore.Mvc;

namespace MeterReading.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MeterReadingsController : ControllerBase
{
    private readonly IMeterReadingService meterReadingService;

    public MeterReadingsController(IMeterReadingService meterReadingService)
    {
        this.meterReadingService = meterReadingService;
    }

    [HttpPost("meter-reading-uploads")]
    public async Task<IActionResult> UploadMeterReadings(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest("No file uploaded");
        }

        if (!file.FileName.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest("Only CSV files are allowed");
        }

        try
        {
            using var stream = file.OpenReadStream();
            var processingResult = await meterReadingService.ProcessMeterReadingsAsync(stream);

            // Map infrastructure result to API DTO
            var apiResult = new MeterReadingResponse(
                processingResult.Validated,
                processingResult.Failed,
                processingResult.Committed,
                processingResult.Errors);

            return Ok(apiResult);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error processing file: {ex.Message}");
        }
    }
}