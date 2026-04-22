using System;
using System.Drawing;
using System.Windows.Forms;

namespace FinancialLiteracyGame
{
    public class OpportunityEvent : GameEvent
    {
        public override string EventType => "Opportunity";
        public decimal Cost { get; private set; }

        public OpportunityEvent(string title, string desc, decimal cost, decimal cashFlow, string note)
        {
            Title = title;
            Description = desc;
            Cost = cost;
            BalanceEffect = -cost;
            IncomeEffect = cashFlow;
            EducationalNote = note;
        }

        public override void Execute(Player player, FinanceManager finance)
        {
            if (player.Cash >= Cost)
            {
                player.Cash -= Cost;
                player.PassiveIncome += IncomeEffect;
            }
            else
            {
                MessageBox.Show("Недостаточно средств для покупки этого актива!", "Ошибка");
            }
        }
    }
}
