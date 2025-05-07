namespace WebAppForMiniProfiler.Model
{
    public class Room
    {
        public int RoomId { get; set; }
        public string? RoomName { get; set; }
        public int Capacity { get; set; }
        public ICollection<Book>? Books { get; set; }
    }
}
