using TradeMath;
using System.Reflection;

var lines = new[] { new SaleLine("Товар А", 10, 500m, 650m), new SaleLine("Товар Б", 10, 600m, 750m) };
var rules = new[] { new DiscountRule(20, 0, 5), new DiscountRule(0, 10000, 7), new DiscountRule(20, 10000, 10) };
var totals = SalesCalculator.Calculate(lines, SaleMode.Wholesale, rules);
Check(totals == new SaleTotals(20, 11000, 10, 1100, 9900), "оптовая сделка");
foreach (var row in new[] { (19, 9999.99m, 0m), (20, 9999.99m, 5m), (19, 10000m, 7m), (20, 10000m, 10m) })
{
    var result = SalesCalculator.Calculate(new[] { new SaleLine("Проверка", row.Item1, row.Item2 / row.Item1, 1) }, SaleMode.Wholesale, rules);
    Check(result.Rate == row.Item3, $"граница Q={row.Item1}, S={row.Item2}");
}
var retail = SalesCalculator.Calculate(lines, SaleMode.Retail, Array.Empty<DiscountRule>());
Check(retail.Subtotal == 14000m && retail.Rate == 0, "розничная цена и отсутствие скидки");
Throws(() => SalesCalculator.Calculate(Array.Empty<SaleLine>(), SaleMode.Wholesale, rules));
Throws(() => SalesCalculator.Calculate(new[] { new SaleLine("Товар", 0, 10, 20) }, SaleMode.Wholesale, rules));
Throws(() => SalesCalculator.Calculate(lines, SaleMode.Wholesale, new[] { new DiscountRule(0, 0, 101) }));
var helper = typeof(SalesCalculator).Assembly.GetType("TradeMath.Money")!;
Check(!helper.IsPublic, "Money имеет доступ internal");
Console.WriteLine($"Q={totals.Quantity}; subtotal={totals.Subtotal}; discount={totals.Discount}; total={totals.Total}");
Console.WriteLine("PASS: 10 checks. Public calculator available; internal helper hidden.");

static void Check(bool condition, string name)
{
    if (!condition) throw new Exception("FAIL: " + name);
    Console.WriteLine("PASS: " + name);
}
static void Throws(Action action)
{
    try { action(); } catch (ArgumentException) { Console.WriteLine("PASS: invalid input rejected"); return; }
    throw new Exception("FAIL: invalid input accepted");
}
