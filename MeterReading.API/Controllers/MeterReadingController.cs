using MeterReading.API.DTOs;
using MeterReading.Domain;
using MeterReading.Infrastructure.Services;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace MeterReading.API.Controllers;

/// <summary>
/// Controller for managing meter reading uploads and data
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Tags("Meter Readings")]
public class MeterReadingsController : ControllerBase
{
    private readonly IMeterReadingService meterReadingService;

    public MeterReadingsController(IMeterReadingService meterReadingService)
    {
        this.meterReadingService = meterReadingService;
    }

    /// <summary>
    /// Upload meter readings from a CSV file
    /// </summary>
    /// <param name="file">CSV file containing meter readings with headers: AccountId, MeterReadingDateTime, MeterReadValue</param>
    /// <returns>Processing result with validation and commit statistics</returns>
    /// <response code="200">File processed successfully, returns processing results</response>
    /// <response code="400">Invalid file or validation errors</response>
    /// <response code="500">Internal server error during processing</response>
    [HttpPost("meter-reading-uploads")]
    [ProducesResponseType(typeof(MeterReadingResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> UploadMeterReadings(
        [Required][FromForm] IFormFile file)
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

            // Validate CSV headers
            var headerValidation = CsvHeaderValidator.ValidateHeaders(stream);
            if (!headerValidation.IsSuccess)
            {
                return BadRequest(headerValidation.ErrorMessage);
            }

            var processingResult = await meterReadingService.ProcessMeterReadingsAsync(stream);
          
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