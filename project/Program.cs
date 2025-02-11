                                    //1
public interface IAssortment
{
    int Id { get; set; }
    
    string Name { get; set; }
    
    decimal Price { get; set; }
    
    int Quantity { get; set; }

}

                                                    //2

// Strategy Interface
public interface IPricingStrategy
{
    decimal CalculatePrice(decimal basePrice);
}
public class RegularPricingStrategy : IPricingStrategy
{
    public decimal CalculatePrice(decimal basePrice){
        return basePrice;
    } 
}
public class DiscountPricingStrategy : IPricingStrategy
{

    public decimal CalculatePrice(decimal basePrice)
    {
        return basePrice *0.8m;
    }
}
public class SeasonalPricingStrategy : IPricingStrategy
{
    public decimal CalculatePrice(decimal basePrice)
    {
        return basePrice * 1.1m;
    }
}

public class Product : IAssortment
{
    public int Id { get; set; }
    public string Name { get; set; }
    public decimal Price { get; set; }
    public int Quantity { get; set; }

    private IPricingStrategy _pricingStrategy;

    public Product(int id, string name, decimal price, int quantity)
    {
        Id = id;
        Name = name;
        Price = price;
        Quantity = quantity;
        _pricingStrategy = new RegularPricingStrategy(); // Default strategy
    }

    public void SetPricingStrategy(IPricingStrategy strategy)
    {
        _pricingStrategy = strategy;

        Price=_pricingStrategy.CalculatePrice(Price);
    }

    public override string ToString()
    {
        return $"Product: {Name} (ID: {Id}) - Price: {Price}, Quantity: {Quantity}";
    }
}

                                                //3 

public enum ProductType
{
    Phone,
    Laptop,
    Tv,
    Tablet,
    Headphones,
    Custom
}

public abstract class ElectronicProductFactory
{
    public static Product CreateProduct(ProductType type, string customName = null, decimal? customPrice = null, int? customQuantity = null, IPricingStrategy pricingStrategy = null,int? customID=null)
    {
        Product baseProduct;

        switch (type)
        {
            case ProductType.Phone:
                baseProduct = new Product(1, "Iphone XYZ", 799.99m, 50);
                break;
            
            case ProductType.Laptop:
                baseProduct = new Product(2, "MacBook XYZ", 1299.99m, 30);
                break;
            
            case ProductType.Tv:
                baseProduct = new Product(3, "AppleTv XYZ", 599.99m, 40);
                break;
            
            case ProductType.Tablet:
                baseProduct = new Product(4, "Ipad XYZ", 399.99m, 25);
                break;
            
            case ProductType.Headphones:
                baseProduct = new Product(5, "Apple Ear XYZ", 349.99m, 60);
                break;
            
            default:
                throw new ArgumentException("Invalid product type");
        }

        if (customID != null)
            baseProduct.Id = customID.Value;

        if (customName != null)
            baseProduct.Name = customName;
        
        if (customPrice.HasValue)
            baseProduct.Price = customPrice.Value;
        
        if (customQuantity.HasValue)
            baseProduct.Quantity = customQuantity.Value;

        if (pricingStrategy != null)
            baseProduct.SetPricingStrategy(pricingStrategy);

        return baseProduct;
    }
}


                                        //4          

public class ShoppingCart
{
    private static ShoppingCart _instance;
    private static readonly object _lock = new object();
    private List<CartItem> _items;

    private ShoppingCart()
    {
        _items = new List<CartItem>();
    }

    public static ShoppingCart GetInstance()
    {
        if (_instance == null)
        {
            lock (_lock)
            {
                if (_instance == null)
                {
                    _instance = new ShoppingCart();
                }
            }
        }
        return _instance;
    }

    public List<CartItem> GetItems()
    {
        return _items;
    }

    public class CartItem
    {
        public Product Product { get; set; }
        public int Quantity { get; set; }
        public decimal NetPrice => Product.Price * Quantity;
        public decimal TaxRate => 0.18m; 
        public decimal GrossPrice => NetPrice * (1 + TaxRate);
    }

    public void AddProduct(Product product, int quantity)
    {
        var existingItem = _items.FirstOrDefault(i => i.Product.Id == product.Id);
        if (existingItem != null)
        {
            existingItem.Quantity += quantity;
        }
        else
        {
            _items.Add(new CartItem { Product = product, Quantity = quantity });
        }
    }

    public void RemoveProduct(Product product)
    {
        _items.RemoveAll(i => i.Product.Id == product.Id);
    }

    public void UpdateQuantity(Product product, int newQuantity)
    {
        var item = _items.FirstOrDefault(i => i.Product.Id == product.Id);
        if (item != null)
        {
            if (newQuantity <= 0)
            {
                RemoveProduct(product);
            }
            else
            {
                item.Quantity = newQuantity;
            }
        }
    }

    public void DisplayCart()
    {
        foreach (var item in _items)
        {
            Console.WriteLine($"Product: {item.Product.Name}, Quantity: {item.Quantity}, Net Price: {item.NetPrice}, Gross Price: {item.GrossPrice}");
        }
    }

    public decimal GetTotalNetPrice()
    {
        return _items.Sum(i => i.NetPrice);
    }

    public decimal GetTotalGrossPrice()
    {
        return _items.Sum(i => i.GrossPrice);
    }

    public void Clear()
    {
        _items.Clear();
    }
}


//5

public class Store
{
    private List<Product> _inventory;

    public Store()
    {
        _inventory = new List<Product>();
    }

    // a. Take delivery of goods
    public void TakeDelivery(List<Product> newProducts)
    {
        foreach (var product in newProducts)
        {
            var existingProduct = _inventory.FirstOrDefault(p => p.Id == product.Id);
            if (existingProduct != null)
            {
                existingProduct.Quantity += product.Quantity;
            }
            else
            {
                _inventory.Add(product);
            }
        }

        DeliveryToFile(newProducts);
    }

    private void DeliveryToFile(List<Product> deliveredProducts)
    {
        using (StreamWriter writer = new StreamWriter("delivery.txt"))
        {
            foreach (var product in deliveredProducts)
            {
                writer.WriteLine($"Product ID: {product.Id}, Name: {product.Name}, Quantity: {product.Quantity}, Date: {DateTime.Now}");
            }
        }
    }

    // b. Generate order for low stock items
    public void GenerateLowStockOrder()
    {
        var lowStockItems = _inventory.Where(p => p.Quantity < 10).ToList();

        using (StreamWriter writer = new StreamWriter("low_stock.txt"))
        {
            foreach (var product in lowStockItems)
            {
                writer.WriteLine($"Reorder: {product.Name}, Current Stock: {product.Quantity}, Product ID: {product.Id}");
            }
        }
    }

    // c. Purchase process
    private Dictionary<int, int> _salesHistory = new Dictionary<int, int>();

    public void ProcessPurchase(ShoppingCart cart)
{
    foreach (var item in cart.GetItems())
    {
        var inventoryProduct = _inventory.FirstOrDefault(p => p.Id == item.Product.Id);
        if (inventoryProduct != null)
        {
            var tempItem=inventoryProduct.Quantity;
            inventoryProduct.Quantity -= item.Quantity;

            if (!_salesHistory.ContainsKey(item.Product.Id)){
                _salesHistory[item.Product.Id] = 0;
            }

            if(inventoryProduct.Quantity <=0){
                inventoryProduct.Quantity =0;
                item.Quantity=tempItem;
                Console.WriteLine($"Only {tempItem} amount of left in the {item.Product.Name}. You can not buy more");
            }
            
            _salesHistory[item.Product.Id] += item.Quantity;

        }else{
            Console.WriteLine("The item in shopping cart is not avaible in the moment.");
        }
    }

    GenerateInvoice(cart);
    GenerateLowStockOrder();
    UpdateSalesStatistics(cart);
}

    private void GenerateInvoice(ShoppingCart cart)
    {
        using (StreamWriter writer = new StreamWriter("invoice.txt"))
        {
            writer.WriteLine("Invoice");
            writer.WriteLine($"Date: {DateTime.Now}");
            writer.WriteLine("               ");
            foreach (var item in cart.GetItems())
            {
                writer.WriteLine($"{item.Product.Name} - Quantity: {item.Quantity}, Total: {item.GrossPrice}");
            }
            writer.WriteLine($"Total: {cart.GetTotalGrossPrice()}");
        }
    }

    private void UpdateSalesStatistics(ShoppingCart cart)
    {
        using (StreamWriter writer = new StreamWriter("sales_history.txt", true))
        {
            writer.WriteLine($"Sales Record at {DateTime.Now}:");
            foreach (var sale in _salesHistory)
            {
                writer.WriteLine($"Product ID {sale.Key}: {sale.Value} units sold");
            }
            writer.WriteLine("----------------------------");
        }
    
        Console.WriteLine("Sales statistics updated");
    }

    // d. Generate stock on hand report
    public void GenerateStockReport()
    {
        using (StreamWriter writer = new StreamWriter("stock_report.txt"))
        {
            foreach (var product in _inventory)
            {
                writer.WriteLine($"Product: {product.Name}, Quantity: {product.Quantity}, Price per Unit: {product.Price}");
            }
        }
    }

    // e. Compare products
    public List<Product> CompareProductsByPricePerKilo()
    {
        return _inventory.OrderBy(p => p.Price / p.Quantity).ToList();
    }
}


class Program
{
    static void Main(string[] args)
    {
        Store store = new Store();

        Product smartphone = ElectronicProductFactory.CreateProduct(
            ProductType.Phone, 
            pricingStrategy: new DiscountPricingStrategy()
        );
        Product laptop = ElectronicProductFactory.CreateProduct(
            ProductType.Laptop, 
            customName: "Custom Laptop", 
            customPrice: 1500m,
            pricingStrategy: new SeasonalPricingStrategy()
        );

        store.TakeDelivery(new List<Product> { smartphone, laptop });

        ShoppingCart cart = ShoppingCart.GetInstance();

        cart.AddProduct(smartphone, 2);
        cart.AddProduct(laptop, 1);

        cart.DisplayCart();

        store.ProcessPurchase(cart);

        store.GenerateStockReport();

        cart.Clear();

        cart.AddProduct(smartphone, 5);
        cart.AddProduct(laptop, 6);

        cart.DisplayCart();

        store.ProcessPurchase(cart);

        store.GenerateStockReport();



        var comparedProducts = store.CompareProductsByPricePerKilo();
    }
}