using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Models
{
    public class Product
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ProductID { get; set; }
        public string? Name { get; set; }
        public double Price { get; set; }
        public int Quantity { get; set; }

        public Product() { }       
    }

    public record ProductDto(string Name, double Price, int Quantity);
}