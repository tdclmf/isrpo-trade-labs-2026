using System.Diagnostics;

namespace TradeMath;

public enum SaleMode { Wholesale, Retail }
public record SaleLine(string Name, int Quantity, decimal WholesalePrice, decimal RetailPrice);
public record DiscountRule(int MinQuantity, decimal MinSubtotal, decimal Rate);
public record SaleTotals(int Quantity, decimal Subtotal, decimal Rate, decimal Discount, decimal Total);

public static class SalesCalculator
{
    public static SaleTotals Calculate(IReadOnlyList<SaleLine> lines, SaleMode mode,
        IReadOnlyList<DiscountRule> rules)
    {
        ArgumentNullException.ThrowIfNull(lines);
        ArgumentNullException.ThrowIfNull(rules);
        if (lines.Count == 0 || !Enum.IsDefined(mode))
            throw new ArgumentException("Выберите товары и режим продажи.");
        int quantity = 0;
        decimal subtotal = 0;
        foreach (var line in lines)
        {
            if (string.IsNullOrWhiteSpace(line.Name) || line.Quantity <= 0 ||
                line.WholesalePrice < 0 || line.RetailPrice < 0)
                throw new ArgumentException("Некорректная товарная позиция.");
            quantity = checked(quantity + line.Quantity);
            var price = mode == SaleMode.Wholesale ? line.WholesalePrice : line.RetailPrice;
            subtotal += Money.Round(price * line.Quantity);
        }
        decimal rate = 0;
        foreach (var rule in rules)
        {
            if (rule.MinQuantity < 0 || rule.MinSubtotal < 0 || rule.Rate < 0 || rule.Rate > 100)
                throw new ArgumentException("Некорректное правило скидки.");
            if (quantity >= rule.MinQuantity && subtotal >= rule.MinSubtotal)
                rate = Math.Max(rate, rule.Rate);
        }
        var discount = Money.Round(subtotal * rate / 100);
        var totals = new SaleTotals(quantity, subtotal, rate, discount, subtotal - discount);
        Debug.Assert(totals.Total >= 0 && totals.Total + totals.Discount == totals.Subtotal);
        Trace.WriteLine($"Рассчитана сделка: {totals.Total} руб.");
        DebugPrint.PrintObject(totals, "SaleTotals");
        return totals;
    }
}

internal static class Money
{
    internal static decimal Round(decimal value) => Math.Round(value, 2, MidpointRounding.AwayFromZero);
}

public static class DebugPrint
{
    [Conditional("DEBUG")]
    public static void PrintEntry(string name) => Console.WriteLine("DEBUG enter: " + name);
    [Conditional("DEBUG")]
    public static void PrintExit(string name) => Console.WriteLine("DEBUG exit: " + name);
    [Conditional("DEBUG")]
    public static void PrintObject(object value, string name) => Console.WriteLine($"DEBUG {name}: {value}");
}
