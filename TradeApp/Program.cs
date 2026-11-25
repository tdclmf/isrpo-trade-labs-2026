using TradeMath;
using Microsoft.EntityFrameworkCore;

namespace TradeApp;

internal static class Program
{
    [STAThread]
    static int Main(string[] args)
    {
        try
        {
            using var db = new TradeContext();
            db.Initialize();
            if (args.Contains("--schema")) { Console.WriteLine(db.Database.GenerateCreateScript()); return 0; }
            if (args.Contains("--self-test") || Environment.GetEnvironmentVariable("TRADELAB_DEBUG_TEST") == "1") { SelfTest.Run(db); return 0; }
            ApplicationConfiguration.Initialize();
            Application.Run(new MainForm(db, args.Contains("--demo"), args.Contains("--capture")));
            return 0;
        }
        catch (Exception e) { Console.Error.WriteLine(e.Message); return 1; }
    }
}

internal static class SelfTest
{
    public static void Run(TradeContext db)
    {
        const string email = "didyk@example.test", password = "TestTrade2026!";
        int q = db.SecretQuestions.First().Id;
        var user = db.Users.SingleOrDefault(u => u.Email == email) ?? Accounts.Register(db, "Дидык", "Глеб", email, password, password, "учебное", q, "школа");
        Check(user.Id > 0, "user saved by Entity Framework");
        Check(user.PasswordHash != password && Passwords.Verify(password, user.PasswordHash), "salted password hash");
        Check(Accounts.Login(db, email, password)?.Id == user.Id, "correct login");
        Check(Accounts.Login(db, email, "wrong") == null, "wrong password rejected");
        Check(Accounts.Login(db, "unknown@example.test", password) == null, "unknown user rejected");
        Reject(() => Accounts.Register(db, "Дидык", "Глеб", email, password, password, "слово", q, "ответ"), "duplicate e-mail");
        Reject(() => Accounts.Register(db, "", "Глеб", "bad", "123", "321", "", q, ""), "invalid registration");
        var buyer = db.Buyers.First();
        var products = db.Products.OrderBy(p => p.Id).Take(2).ToArray();
        var sale = Sales.Save(db, buyer, new DateTime(2026, 11, 25), SaleMode.Wholesale, products.Select(p => (p, 10)).ToArray());
        Check(sale.Total == 9900m && sale.Lines.Count == 2, "two-product sale total 9900");
        using var verify = new TradeContext();
        var restored = verify.Sales.Include(s => s.Lines).Single(s => s.Id == sale.Id);
        Check(restored.Lines.Count == 2 && restored.Total == 9900m, "sale and lines persisted together");
        int before = db.Sales.Count();
        Reject(() => Sales.Save(db, buyer, DateTime.Today, SaleMode.Wholesale, new[] {(products[0], 0)}), "zero quantity");
        Check(db.Sales.Count() == before, "invalid sale not saved");
        Console.WriteLine($"USER id={user.Id}; email={user.Email}; name={user.LastName} {user.FirstName}; question={user.SecretQuestionId}");
        Console.WriteLine($"SALE id={sale.Id}; subtotal={sale.Subtotal}; rate={sale.DiscountRate}; discount={sale.DiscountAmount}; total={sale.Total}");
        Console.WriteLine("PASS: 11 integration checks.");
    }
    static void Check(bool valid, string name) { if (!valid) throw new Exception("FAIL: " + name); Console.WriteLine("PASS: " + name); }
    static void Reject(Action action, string name) { try { action(); } catch (ArgumentException) { Console.WriteLine("PASS: " + name); return; } throw new Exception("FAIL: " + name); }
}
