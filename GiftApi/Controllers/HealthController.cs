using GiftApi.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace GiftApi.Controllers
{
    [ApiController]
    [Route("health")]
    public class HealthController : ControllerBase
    {
        readonly ApplicationDbContext _db;
        readonly ILogger<HealthController> _logger;
        readonly IHostEnvironment _env;
        static readonly DateTime _startedUtc = DateTime.UtcNow.AddHours(4);

        public HealthController(ApplicationDbContext db, ILogger<HealthController> logger, IHostEnvironment env)
        {
            _db = db;
            _logger = logger;
            _env = env;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Get(CancellationToken cancellationToken)
        {
            var assemblyVersion = typeof(Program).Assembly.GetName().Version?.ToString() ?? "n/a";
            var sw = Stopwatch.StartNew();

            bool dbOk = false;
            string? dbError = null;
            try
            {
                var conn = _db.Database.GetDbConnection();
                if (conn.State != System.Data.ConnectionState.Open)
                    await conn.OpenAsync(cancellationToken);

                dbOk = await _db.Database.CanConnectAsync(cancellationToken);

                await conn.CloseAsync();
            }
            catch (Exception ex)
            {
                dbOk = false;
                dbError = ex.GetType().Name + ": " + ex.Message;
            }
            sw.Stop();

            var response = new
            {
                status = dbOk ? "Healthy" : "Degraded",
                timestampUtc = DateTime.UtcNow.AddHours(4),
                environment = _env.EnvironmentName,
                version = assemblyVersion,
                uptimeSeconds = (DateTime.UtcNow - _startedUtc).TotalSeconds,
                db = new
                {
                    provider = _db.Database.ProviderName,
                    reachable = dbOk,
                    latencyMs = sw.ElapsedMilliseconds,
                    error = dbError
                }
            };

            if (!dbOk)
            {
                _logger.LogWarning("Health check failed: {Error}", dbError);
                return StatusCode(503, response);
            }

            _logger.LogInformation("Health check OK (latency {Latency} ms)", sw.ElapsedMilliseconds);
            return Ok(response);
        }
    }
}