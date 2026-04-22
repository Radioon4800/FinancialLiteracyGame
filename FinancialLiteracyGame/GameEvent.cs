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
        public override string EventType => "Risk";
        public string Category { get; private set; } // Для хранения "Медицина", "Имущество" и т.д.
        public bool IsInsurable { get; private set; }

        // Обновленный конструктор
        public RiskEvent(string title, string desc, decimal amount, string category, string note, bool isInsurable = true)
        {
            Title = title;
            Description = desc;
            BalanceEffect = -amount;
            Category = category; // Теперь строка сохраняется здесь
            EducationalNote = note;
            IsInsurable = isInsurable; // А здесь получаем bool
        }

        public RiskEvent(string title, string desc, decimal amount, string note, bool isInsurable = true)
        {
            Title = title;
            Description = desc;
            BalanceEffect = -amount;
            EducationalNote = note;
            IsInsurable = isInsurable;
        }

        public override void Execute(Player player, FinanceManager finance)
        {
            if (player.IsInsured && IsInsurable)
            {
                // Если застрахован, игрок теряет, например, только 10% от суммы (франшиза)
                decimal reducedLoss = BalanceEffect * 0.1m;
                player.Cash += reducedLoss;

                MessageBox.Show(
                    $"Вас спасла страховка!\nВместо {-BalanceEffect:N0} руб. вы потеряли всего {-reducedLoss:N0} руб.",
                    "Работает страховой полис",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );
            }
            else
            {
                // Полная потеря
                player.Cash += BalanceEffect;

                if (IsInsurable)
                {
                    MessageBox.Show(
                        "У вас не было страховки, поэтому пришлось оплатить ущерб полностью.",
                        "Урок финансовой грамотности",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning
                    );
                }
            }
        }
    }
}