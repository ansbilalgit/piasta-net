using System.ComponentModel.DataAnnotations;

namespace PiastaNet.API.Models
{
    public class Item
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; } = null!;

        public int? Length { get; set; }

        [Required]
        public string Description { get; set; } = "No description provided";

        public string? Thumbnail { get; set; } = "https://i.imgur.com/OJhoTqu.png";

        public ItemType Type { get; set; }

        public int? Copies { get; set; } = 1;

        public List<Category> Categories { get; set; } = new();

        public Boardgame? Boardgame { get; set; }
        public Videogame? Videogame { get; set; }
    }
}
