using System;
using System.Windows.Forms;

namespace FinancialLiteracyGame
{
    // 1. Бытовые расходы (учат планировать бюджет)
    public class ExpenseEvent : GameEvent
    {
        public override string EventType => "Expense";

        public ExpenseEvent(string title, string desc, decimal amount, string note)
        {
            Title = title;
            Description = desc;
            BalanceEffect = -amount;
            IncomeEffect = 0;
            EducationalNote = note;
        }

        public override void Execute(Player player, FinanceManager finance)
        {
            finance.ApplyExpense(Math.Abs(BalanceEffect));
        }
    }

    // 2. Глобальное событие: Инфляция (учит защищать капитал)
    public class InflationEvent : GameEvent
    {
        public override string EventType => "Inflation";

        public InflationEvent(decimal percent, string note)
        {
            Title = "Инфляция!";
            Description = $"Цены в стране выросли на {percent * 100:F0}%. Ваши ежемесячные расходы увеличились.";
            EducationalNote = note;
        }

        public override void Execute(Player player, FinanceManager finance)
        {
            decimal increase = player.Expenses * 0.05m;
            player.Expenses += increase;
        }
    }

    // 3. Благотворительность (учит социальной ответственности)
    public class CharityEvent : GameEvent
    {
        public override string EventType => "Charity";

        public CharityEvent(string title, string desc, decimal amount, string note)
        {
            Title = title;
            Description = desc;
            BalanceEffect = -amount;
            EducationalNote = note;
        }

        public override void Execute(Player player, FinanceManager finance)
        {
            if (player.Cash >= Math.Abs(BalanceEffect))
            {
                player.Cash -= Math.Abs(BalanceEffect);
            }
        }
    }
    // 4. Рыночные возможности (учат инвестировать)

        public class MarketEvent : GameEvent
        {
            public override string EventType => "Market";
            public decimal Price { get; set; }
            public decimal MonthlyIncome { get; set; }

            public MarketEvent(string title, string desc, decimal price, decimal monthlyIncome, string note)
            {
                Title = title;
                Description = desc;
                Price = price;
                MonthlyIncome = monthlyIncome;
                EducationalNote = note;
            }

            public override void Execute(Player player, FinanceManager finance)
            {
            player.Cash -= Price;
            player.PassiveIncome += MonthlyIncome;
        }
        }
    }