using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StackExchange.Profiling.Storage;
using WebAppForMiniProfiler.Extensions;
using WebAppForMiniProfiler.Helpers;

namespace WebAppForMiniProfiler.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    public class ProfilerController : Controller
    {
        private readonly CustomMiniProfilerMemoryStorage _storage;
        public ProfilerController(IAsyncStorage storage)
        {
            _storage = (CustomMiniProfilerMemoryStorage)storage;
        }
        [HttpGet()]
        [AllowAnonymous]
        public async Task<IActionResult> ProfilerListAsync(
        [FromQuery] int skip = 0,
        [FromQuery] int take = 10,
        [FromQuery] string? search = null,
        [FromQuery] string? start = null,
        [FromQuery] string? finish = null,
        [FromQuery] string? sort = null)
        {
            DateTime? startDate = start.ExtToDateTime();
            DateTime? finishDate = finish.ExtToDateTime();
            var (items, totalCount) = await _storage.CustomListPagedSlimAsync(skip, take, search, startDate, finishDate, sort);
            return Ok(new { items, totalCount });
        }
    }
}
