using Dapper;
using WebAppForMiniProfiler.Abstract;
using WebAppForMiniProfiler.Context;
using WebAppForMiniProfiler.Model;

namespace WebAppForMiniProfiler.Service
{
    public class TestService : ITestService
    {
        private readonly DapperContext _dapperContext;
        public TestService(DapperContext dapperContext)
        {
            _dapperContext = dapperContext;
        }
        public async Task<IEnumerable<Room>> GetRoomsAsync()
        {
            using var connection = _dapperContext.CreateConnection();
            return await connection.QueryAsync<Room>("SELECT * FROM Room");
        }
        public async Task<IEnumerable<Book>> GetBooksAsync()
        {
            using var connection = _dapperContext.CreateConnection();
            return await connection.QueryAsync<Book>("SELECT * FROM Books");
        }

        public async Task<(IEnumerable<Book> books, IEnumerable<Room> rooms)> GetBooksAndRoomsAsync()
        {
            using var connection = _dapperContext.CreateConnection();

            var books = await connection.QueryAsync<Book>("SELECT * FROM Books");
            var rooms = await connection.QueryAsync<Room>("SELECT * FROM Rooms");

            return ((books, rooms));
        }

    }
}
