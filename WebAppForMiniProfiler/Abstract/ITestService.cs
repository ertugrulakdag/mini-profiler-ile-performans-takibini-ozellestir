using WebAppForMiniProfiler.Model;

namespace WebAppForMiniProfiler.Abstract
{
    public interface ITestService
    {
        Task<IEnumerable<Room>> GetRoomsAsync();
        Task<IEnumerable<Book>> GetBooksAsync();
        Task<(IEnumerable<Book> books, IEnumerable<Room> rooms)> GetBooksAndRoomsAsync();
    }
}
