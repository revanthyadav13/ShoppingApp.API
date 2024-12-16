namespace ShoppingApp.API.Models
{
    public class Item
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public bool Deleted { get; set; }

        // Foreign Key for Users
        public int UserId { get; set; }
    }
}
