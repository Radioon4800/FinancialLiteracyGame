using System;
using System.Windows.Forms; // Нужно для MessageBox

namespace FinancialLiteracyGame
{
    public abstract class GameEvent
    {
        public string Title { get; protected set; }
        public string Description { get; protected set; }
        public string EducationalNote { get; protected set; }
        public abstract string EventType { get; }
        public decimal BalanceEffect { get; protected set; }
        public decimal IncomeEffect { get; protected set; }
        public int CurrentPosition { get; set; } = 0;
        public abstract void Execute(Player player, FinanceManager finance);
        public void SaveToHistory(int sessionId, int currentMonth)
        {
            DatabaseManager.LogRandomEvent(
                sessionId,
                currentMonth,
                EventType,
                Title,
                BalanceEffect,
                IncomeEffect
            );
        }

        // Вспомогательный метод для логгирования в стиле старого DatabaseManager
        public void LogToDatabase(int sessionId, int month)
        {
            SaveToHistory(sessionId, month);
        }
    }

    public class RiskEvent : GameEvent
    {
        public override string EventType => "Risk"; // Реализация абстрактного члена

        public RiskEvent(string title, string desc, decimal amount, string insurance, string note)
        {
            Title = title;
            Description = desc;
            BalanceEffect = -amount;
            IncomeEffect = 0;
            EducationalNote = note; // Теперь ошибка исчезнет
        }

        public override void Execute(Player player, FinanceManager finance)
        {
            finance.ApplyExpense(-BalanceEffect);
        }
    }

    public class MarketEvent : GameEvent
    {
        public override string EventType => "Market"; // Реализация абстрактного члена

        public MarketEvent(string title, string desc, decimal cost, decimal bonus, string note)
        {
            Title = title;
            Description = desc;
            BalanceEffect = -cost;
            IncomeEffect = bonus;
            EducationalNote = note;
        }

        public override void Execute(Player player, FinanceManager finance)
        {
            decimal cost = Math.Abs(BalanceEffect);

            bool success = finance.BuyAsset(cost, IncomeEffect);

            if (success)
            {
                MessageBox.Show($"Инвестиция оформлена!\nДоход в месяц: +{IncomeEffect} руб.", "Рынок");
            }
            else
            {
                MessageBox.Show("Не хватает наличных!", "Внимание");
            }
        }
    }
}