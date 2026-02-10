namespace PiastaNet.API.Models
{
    public class Category
    {
        public int ItemId { get; set; }
        public string Name { get; set; } = null!;

        public Item Item { get; set; } = null!;
    }
}
