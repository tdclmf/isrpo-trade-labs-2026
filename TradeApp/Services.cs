using System.Net.Mail;
using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using TradeMath;

namespace TradeApp;

public static class Passwords
{
    public static string Hash(string value)
    {
        byte[] salt = RandomNumberGenerator.GetBytes(16);
        byte[] hash = Rfc2898DeriveBytes.Pbkdf2(value, salt, 100000, HashAlgorithmName.SHA256, 32);
        return "100000:" + Convert.ToBase64String(salt) + ":" + Convert.ToBase64String(hash);
    }
    public static bool Verify(string value, string stored)
    {
        var parts = stored.Split(':');
        if (parts.Length != 3 || !int.TryParse(parts[0], out int rounds) || rounds != 100000) return false;
        try
        {
            byte[] hash = Rfc2898DeriveBytes.Pbkdf2(value, Convert.FromBase64String(parts[1]), rounds, HashAlgorithmName.SHA256, 32);
            return CryptographicOperations.FixedTimeEquals(hash, Convert.FromBase64String(parts[2]));
        }
        catch (FormatException) { return false; }
    }
}
public static class Accounts
{
    public static User Register(TradeContext db, string last, string first, string email, string password,
        string repeat, string codeWord, int questionId, string answer)
    {
        email = email.Trim().ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(last) || string.IsNullOrWhiteSpace(first) || last.Length > 50 || first.Length > 50 ||
            email.Length > 100 || !MailAddress.TryCreate(email, out var address) || address.Address != email ||
            password.Length < 8 || password.Length > 128 || password != repeat ||
            string.IsNullOrWhiteSpace(codeWord) || string.IsNullOrWhiteSpace(answer))
            throw new ArgumentException("Заполните поля, укажите e-mail и совпадающие пароли длиной от 8 символов.");
        if (!db.SecretQuestions.Any(q => q.Id == questionId)) throw new ArgumentException("Выберите секретный вопрос.");
        if (db.Users.Any(u => u.Email == email)) throw new ArgumentException("Этот e-mail уже зарегистрирован.");
        var user = new User { LastName = last.Trim(), FirstName = first.Trim(), Email = email,
            PasswordHash = Passwords.Hash(password), CodeWordHash = Passwords.Hash(codeWord),
            SecretQuestionId = questionId, SecretAnswerHash = Passwords.Hash(answer) };
        db.Users.Add(user);
        db.SaveChanges();
        return user;
    }
    public static User? Login(TradeContext db, string email, string password)
    {
        var user = db.Users.SingleOrDefault(u => u.Email == email.Trim().ToLowerInvariant());
        return user != null && Passwords.Verify(password, user.PasswordHash) ? user : null;
    }
}
public static class Sales
{
    public static readonly DiscountRule[] Rules = { new(20, 0, 5), new(0, 10000, 7), new(20, 10000, 10) };
    public static Sale Save(TradeContext db, Buyer buyer, DateTime date, SaleMode mode, IReadOnlyList<(Product Product, int Quantity)> selected)
    {
        DebugPrint.PrintEntry("Sales.Save");
        var lines = selected.Select(l => new SaleLine(l.Product.Name, l.Quantity, l.Product.WholesalePrice, l.Product.RetailPrice)).ToArray();
        var totals = SalesCalculator.Calculate(lines, mode, Rules);
        var sale = new Sale { BuyerId = buyer.Id, BuyerName = buyer.Name, BuyerAddress = buyer.Address,
            BuyerPhone = buyer.Phone, BuyerContact = buyer.ContactPerson, PurchaseDate = date, Mode = mode.ToString(),
            Subtotal = totals.Subtotal, DiscountRate = totals.Rate, DiscountAmount = totals.Discount, Total = totals.Total,
            Lines = selected.Select(l => new SaleItem { ProductId = l.Product.Id, ProductName = l.Product.Name,
                Quantity = l.Quantity, UnitPrice = mode == SaleMode.Wholesale ? l.Product.WholesalePrice : l.Product.RetailPrice }).ToList() };
        db.Sales.Add(sale);
        db.SaveChanges();
        DebugPrint.PrintObject(sale.Id, "SaleId");
        DebugPrint.PrintExit("Sales.Save");
        return sale;
    }
}
