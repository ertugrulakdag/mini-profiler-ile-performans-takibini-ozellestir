using Microsoft.AspNetCore.Mvc;
using StackExchange.Profiling;
using WebAppForMiniProfiler.Abstract;

namespace WebAppForMiniProfiler.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    public class LibraryController : Controller
    {
        private readonly ITestService _myService;

        public LibraryController(ITestService myService)
        {
            _myService = myService;
        }

        [HttpGet]
        public async Task<IActionResult> BooksAsync()
        {
            var data = await _myService.GetBooksAsync();
            return Ok(data);
        }
        [HttpGet]
        public async Task<IActionResult> BooksAndRoomsAsync()
        {
            using (MiniProfiler.Current.Step("Controller: Kitap ve Oda Verilerini Getir"))
            {
                var (books, rooms) = await _myService.GetBooksAndRoomsAsync();
                return Ok(new
                {
                    Books = books,
                    Rooms = rooms
                });
            }
        }

    }
}
