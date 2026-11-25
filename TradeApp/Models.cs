using Microsoft.EntityFrameworkCore;

namespace TradeApp;

public sealed class User
{
    public int Id { get; set; }
    public string LastName { get; set; } = "";
    public string FirstName { get; set; } = "";
    public string Email { get; set; } = "";
    public string PasswordHash { get; set; } = "";
    public string CodeWordHash { get; set; } = "";
    public int SecretQuestionId { get; set; }
    public string SecretAnswerHash { get; set; } = "";
    public SecretQuestion SecretQuestion { get; set; } = null!;
}
public sealed class SecretQuestion
{
    public int Id { get; set; }
    public string Text { get; set; } = "";
    public override string ToString() => Text;
}
public sealed class Buyer
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Address { get; set; } = "";
    public string Phone { get; set; } = "";
    public string ContactPerson { get; set; } = "";
    public override string ToString() => Name;
}
public sealed class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public decimal WholesalePrice { get; set; }
    public decimal RetailPrice { get; set; }
    public string ReferenceInfo { get; set; } = "";
}
public sealed class Sale
{
    public int Id { get; set; }
    public int BuyerId { get; set; }
    public string BuyerName { get; set; } = "";
    public string BuyerAddress { get; set; } = "";
    public string BuyerPhone { get; set; } = "";
    public string BuyerContact { get; set; } = "";
    public DateTime PurchaseDate { get; set; }
    public string Mode { get; set; } = "";
    public decimal Subtotal { get; set; }
    public decimal DiscountRate { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal Total { get; set; }
    public List<SaleItem> Lines { get; set; } = new();
}
public sealed class SaleItem
{
    public int Id { get; set; }
    public int SaleId { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = "";
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}

public sealed class TradeContext : DbContext
{
    public DbSet<User> Users => Set<User>();
    public DbSet<SecretQuestion> SecretQuestions => Set<SecretQuestion>();
    public DbSet<Buyer> Buyers => Set<Buyer>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Sale> Sales => Set<Sale>();
    public DbSet<SaleItem> SaleItems => Set<SaleItem>();
    protected override void OnConfiguring(DbContextOptionsBuilder options) => options.UseSqlServer(
        Environment.GetEnvironmentVariable("TRADELAB_CONNECTION") ??
        "Server=.\\SQLEXPRESS;Database=DidykTradeLabs2026;Trusted_Connection=True;TrustServerCertificate=True");
    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<User>().ToTable("Пользователи");
        b.Entity<User>().HasIndex(u => u.Email).IsUnique();
        b.Entity<User>().Property(u => u.Email).HasMaxLength(100);
        b.Entity<User>().Property(u => u.FirstName).HasMaxLength(50);
        b.Entity<User>().Property(u => u.LastName).HasMaxLength(50);
        b.Entity<User>().HasOne(u => u.SecretQuestion).WithMany().HasForeignKey(u => u.SecretQuestionId).OnDelete(DeleteBehavior.Restrict);
        b.Entity<SecretQuestion>().ToTable("СекретныеВопросы");
        b.Entity<Buyer>().ToTable("Покупатели");
        b.Entity<Product>().ToTable("Товары");
        b.Entity<Sale>().ToTable("Сделки");
        b.Entity<SaleItem>().ToTable("ПозицииСделок");
        b.Entity<Sale>().HasMany(s => s.Lines).WithOne().HasForeignKey(l => l.SaleId);
        b.Entity<Sale>().HasOne<Buyer>().WithMany().HasForeignKey(s => s.BuyerId).OnDelete(DeleteBehavior.Restrict);
        b.Entity<SaleItem>().HasOne<Product>().WithMany().HasForeignKey(l => l.ProductId).OnDelete(DeleteBehavior.Restrict);
        foreach (var entity in b.Model.GetEntityTypes())
            foreach (var property in entity.GetProperties())
                if (property.ClrType == typeof(decimal)) { property.SetPrecision(18); property.SetScale(2); }
    }
    public void Initialize()
    {
        Database.EnsureCreated();
        if (!SecretQuestions.Any()) SecretQuestions.Add(new SecretQuestion { Text = "Название первого учебного заведения" });
        if (!Buyers.Any()) Buyers.Add(new Buyer { Name = "ООО Учебный клиент", Address = "Санкт-Петербург, Учебная улица, 1", Phone = "+7 000 000-00-00", ContactPerson = "Иванов Иван Иванович" });
        if (!Products.Any()) Products.AddRange(new Product { Name = "Товар А", WholesalePrice = 500, RetailPrice = 650, ReferenceInfo = "Учебная товарная позиция" }, new Product { Name = "Товар Б", WholesalePrice = 600, RetailPrice = 750, ReferenceInfo = "Учебная товарная позиция" });
        SaveChanges();
    }
}
