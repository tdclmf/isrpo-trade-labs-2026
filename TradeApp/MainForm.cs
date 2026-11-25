using TradeMath;

namespace TradeApp;

public sealed class MainForm : Form
{
    readonly TradeContext db;
    readonly TabControl tabs = new() { Dock = DockStyle.Fill };
    readonly TextBox login = new(), password = new() { UseSystemPasswordChar = true };
    readonly Label status = new() { AutoSize = true };
    readonly DataGridView grid = new() { Width = 720, Height = 170, AllowUserToAddRows = false, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill };
    readonly Label result = new() { AutoSize = true };
    readonly ComboBox buyer = new() { Width = 300, DropDownStyle = ComboBoxStyle.DropDownList };
    readonly ComboBox mode = new() { Width = 180, DropDownStyle = ComboBoxStyle.DropDownList };
    readonly List<Product> products;
    bool authenticated;

    public MainForm(TradeContext context, bool demo, bool capture)
    {
        db = context;
        products = db.Products.OrderBy(p => p.Id).ToList();
        Text = "ИС оптово-розничной торговли — Дидык Глеб";
        ClientSize = new Size(800, 530);
        StartPosition = FormStartPosition.CenterScreen;
        Font = new Font("Segoe UI", 10);
        Controls.Add(tabs);
        var auth = Panel("Авторизация");
        Field(auth, "E-mail", login); Field(auth, "Пароль", password);
        var signIn = new Button { Text = "Войти", Width = 180 };
        signIn.Click += (_, _) => Run(() => {
            var user = Accounts.Login(db, login.Text, password.Text);
            authenticated = user != null;
            status.Text = user == null ? "Неверный e-mail или пароль" : $"Вход выполнен: {user.LastName} {user.FirstName}";
        });
        auth.Controls.Add(signIn); auth.Controls.Add(status);
        var reg = Panel("Регистрация");
        var last = new TextBox(); var first = new TextBox(); var mail = new TextBox();
        var pass = new TextBox { UseSystemPasswordChar = true }; var repeat = new TextBox { UseSystemPasswordChar = true };
        var code = new TextBox(); var answer = new TextBox();
        Field(reg,"Фамилия",last); Field(reg,"Имя",first); Field(reg,"E-mail",mail);
        Field(reg,"Пароль",pass); Field(reg,"Повтор пароля",repeat); Field(reg,"Кодовое слово",code);
        var question = new ComboBox { Width = 430, DropDownStyle = ComboBoxStyle.DropDownList, DataSource = db.SecretQuestions.ToList() };
        Field(reg,"Секретный вопрос",question); Field(reg,"Ответ на вопрос",answer);
        var register = new Button { Text = "Зарегистрировать", Width = 220 };
        var registered = new Label { AutoSize = true };
        register.Click += (_, _) => Run(() => {
            var user = Accounts.Register(db,last.Text,first.Text,mail.Text,pass.Text,repeat.Text,code.Text,((SecretQuestion)question.SelectedItem!).Id,answer.Text);
            registered.Text = $"Пользователь сохранён. Код: {user.Id}";
        });
        reg.Controls.Add(register); reg.Controls.Add(registered);
        var sale = Panel("Продажа");
        buyer.DataSource = db.Buyers.ToList();
        mode.Items.AddRange(new object[] { "Оптовая", "Розничная" }); mode.SelectedIndex = 0;
        Field(sale,"Покупатель",buyer); Field(sale,"Режим продажи",mode);
        grid.Columns.Add("name","Товар"); grid.Columns.Add("quantity","Количество");
        grid.Columns.Add("wholesale","Оптовая цена"); grid.Columns.Add("retail","Розничная цена");
        grid.Columns[0].ReadOnly=true; grid.Columns[2].ReadOnly=true; grid.Columns[3].ReadOnly=true;
        foreach(var p in products) grid.Rows.Add(p.Name,10,p.WholesalePrice,p.RetailPrice);
        sale.Controls.Add(grid);
        var calculate = new Button { Text = "Рассчитать", Width = 180 };
        calculate.Click += (_,_) => Run(Calculate);
        var save = new Button { Text = "Сохранить сделку", Width = 220 };
        save.Click += (_,_) => Run(() => {
            RequireLogin();
            var saved = Sales.Save(db,(Buyer)buyer.SelectedItem!,DateTime.Today,CurrentMode(),Selected());
            result.Text = $"Документ № {saved.Id}. Сумма: {saved.Subtotal:N2}; скидка: {saved.DiscountAmount:N2}; итог: {saved.Total:N2} руб.";
        });
        sale.Controls.Add(calculate); sale.Controls.Add(save); sale.Controls.Add(result);
        var report=Panel("Отчёт о продажах");
        var reportText=new TextBox { Multiline=true,ReadOnly=true,Width=720,Height=300 };
        var refresh=new Button { Text="Сформировать отчёт",Width=220 };
        refresh.Click += (_,_) => Run(() => {
            RequireLogin();
            var sales=db.Sales.Where(s=>s.PurchaseDate>=DateTime.Today.AddDays(-30)).ToList();
            reportText.Text=$"Продаж за 30 дней: {sales.Count}\r\nВыручка: {sales.Sum(s=>s.Total):N2} руб.\r\nСкидки: {sales.Sum(s=>s.DiscountAmount):N2} руб.\r\n\r\n"+
                string.Join("\r\n",sales.Select(s=>$"№ {s.Id}  {s.PurchaseDate:dd.MM.yyyy}  {s.BuyerName}  {s.Total:N2} руб."));
        });
        report.Controls.Add(refresh); report.Controls.Add(reportText);
        Shown += (_,_) => {
            if(demo) { login.Text="didyk@example.test";password.Text="TestTrade2026!"; signIn.PerformClick(); }
            if(capture) BeginInvoke(() => {
                var dir=Path.Combine(AppContext.BaseDirectory,"captures");Directory.CreateDirectory(dir);
                RenderWindow(Path.Combine(dir,"login.png"));
                tabs.SelectedIndex=1; RenderWindow(Path.Combine(dir,"registration.png"));
                tabs.SelectedIndex=2; Calculate(); RenderWindow(Path.Combine(dir,"sale.png"));
                tabs.SelectedIndex=3; refresh.PerformClick(); RenderWindow(Path.Combine(dir,"report.png"));
                Close();
            });
        };
    }
    FlowLayoutPanel Panel(string title)
    {
        var page=new TabPage(title);tabs.TabPages.Add(page);
        var panel=new FlowLayoutPanel { Dock=DockStyle.Fill,FlowDirection=FlowDirection.TopDown,WrapContents=false,Padding=new Padding(20),AutoScroll=true };
        page.Controls.Add(panel);return panel;
    }
    static void Field(FlowLayoutPanel panel,string name,Control input)
    {
        var row=new FlowLayoutPanel { Width=720,Height=36 };
        row.Controls.Add(new Label { Text=name,Width=180,Height=28,TextAlign=ContentAlignment.MiddleLeft });
        if(input is TextBox) input.Width=430;
        row.Controls.Add(input);panel.Controls.Add(row);
    }
    SaleMode CurrentMode()=>mode.SelectedIndex==0?SaleMode.Wholesale:SaleMode.Retail;
    List<(Product Product,int Quantity)> Selected()
    {
        var rows=new List<(Product,int)>();
        for(int i=0;i<products.Count;i++)
        {
            if(!int.TryParse(grid.Rows[i].Cells[1].Value?.ToString(),out int quantity)) throw new ArgumentException("Количество должно быть целым числом.");
            rows.Add((products[i],quantity));
        }
        return rows;
    }
    void Calculate()
    {
        RequireLogin();
        var totals=SalesCalculator.Calculate(Selected().Select(l=>new SaleLine(l.Product.Name,l.Quantity,l.Product.WholesalePrice,l.Product.RetailPrice)).ToArray(),CurrentMode(),Sales.Rules);
        result.Text=$"Количество: {totals.Quantity}. Сумма: {totals.Subtotal:N2}; скидка {totals.Rate}%: {totals.Discount:N2}; итог: {totals.Total:N2} руб.";
    }
    void RequireLogin() { if(!authenticated) throw new ArgumentException("Сначала выполните вход."); }
    void Run(Action action) { try{action();} catch(Exception e){ MessageBox.Show(e.Message,"Ошибка",MessageBoxButtons.OK,MessageBoxIcon.Warning); } }
    void RenderWindow(string file) { Refresh();using var image=new Bitmap(Width,Height);DrawToBitmap(image,new Rectangle(0,0,Width,Height));image.Save(file); }
}
