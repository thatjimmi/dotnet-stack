namespace Models
{
    public class InventoryManager
    {
        private List<Product> products = new List<Product>();
        
        public void AddProduct(Product product)
        {
            products.Add(product);
        }

        public void RemoveProduct(int productId)
        {
            products.RemoveAll(p => p.ProductID == productId);
        }

        public string CheckInventory()
        {
            return string.Join("\n", products.Select(p => p.GetDetails()));
        }

        public Product? GetProductById(int id)
        {
            return products.FirstOrDefault(p => p.ProductID == id);
        }
    }

}