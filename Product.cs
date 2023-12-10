using System;
namespace Application
{
    public class Product
    {
        public int ProductID { get; set; }
        public string Name { get; set; }
        public double Price { get; set; }
        public int Quantity { get; set; }

        public Product(int productId, string name, double price, int quantity)
        {
            ProductID = productId;
            Name = name;
            Price = price;
            Quantity = quantity;
        }

        public void UpdateStock(int quantity)
        {
            Quantity = quantity;
        }

        public string GetDetails()
        {
            return $"ProductID: {ProductID}, Name: {Name}, Price: {Price}, Quantity: {Quantity}";
        }
    }

}

