using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace WebAppForMiniProfiler.Model
{
    public class Book
    {
        public int BookId { get; set; }
        [Required]
        public required string Title { get; set; }
        [Required]
        public required string Author { get; set; }
        [Required]
        [MaxLength(20)]
        public required string ISBN { get; set; }
        public int PublishedYear { get; set; }
        public int? RoomId { get; set; }
        [ForeignKey("RoomId")]
        public Room? Room { get; set; }
    }
}
