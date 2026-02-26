using Microsoft.AspNetCore.Mvc;
using PhishingDetector.Core.DTOs;
using PhishingDetector.Core.Interfaces;

namespace PhishingDetector.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PhishingController : ControllerBase
{
    private readonly IPhishingAnalysisService _service;
    private readonly IOllamaService _ollama;

    public PhishingController(IPhishingAnalysisService service, IOllamaService ollama)
    {
        _service = service;
        _ollama = ollama;
    }

    [HttpPost("analyze")]
    [ProducesResponseType(typeof(AnalyzeEmailResponse), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(503)]
    public async Task<IActionResult> Analyze([FromBody] AnalyzeEmailRequest request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Content) && string.IsNullOrWhiteSpace(request.Subject))
            return BadRequest("En az konu veya içerik alanı doldurulmalıdır.");

        if (!await _ollama.IsAvailableAsync(ct))
            return StatusCode(503, "Ollama servisi şu anda erişilemiyor. Lütfen Ollama'nın çalıştığından emin olun.");

        try
        {
            var result = await _service.AnalyzeEmailAsync(request, ct);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Analiz sırasında hata: {ex.Message}");
        }
    }

    [HttpGet("dashboard")]
    public async Task<IActionResult> Dashboard(CancellationToken ct)
    {
        var stats = await _service.GetDashboardStatsAsync(ct);
        return Ok(stats);
    }

    [HttpGet("history")]
    public async Task<IActionResult> History([FromQuery] int count = 20, CancellationToken ct = default)
    {
        var analyses = await _service.GetRecentAnalysesAsync(count, ct);
        return Ok(analyses);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var analysis = await _service.GetAnalysisByIdAsync(id, ct);
        if (analysis is null) return NotFound();
        return Ok(analysis);
    }

    [HttpGet("health")]
    public async Task<IActionResult> Health(CancellationToken ct)
    {
        var ollamaOk = await _ollama.IsAvailableAsync(ct);
        return Ok(new { status = "ok", ollama = ollamaOk ? "connected" : "disconnected" });
    }
}
