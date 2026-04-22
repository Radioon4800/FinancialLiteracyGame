namespace FinancialLiteracyGame
{
    public enum GameStage { RatRace, FastTrack }
    public class Player
    {
        public string Name { get; set; }
        public string Profession { get; set; }
        public decimal Cash { get; set; }
        public decimal Salary { get; set; }
        public decimal Expenses { get; set; }
        public decimal PassiveIncome { get; set; }
        public decimal TotalDebt { get; set; } = 0;
        public decimal LoanInterestPayment => TotalDebt * 0.1m;
        public bool IsInsured { get; set; } = false;
        public decimal InsurancePremium { get; set; } = 1500;
        public GameStage CurrentStage { get; set; } = GameStage.RatRace;

        // Свойство для проверки условия из твоей работы: 
        // Пассивный доход > Расходы
        public bool IsReadyForFastTrack => PassiveIncome > Expenses;

        // Полезный расчет: сколько осталось до перехода (в процентах)
        public double ExitProgress
        {
            get
            {
                if (Expenses <= 0) return 100;
                double progress = (double)(PassiveIncome / Expenses) * 100;
                return progress > 100 ? 100 : progress;
            }
        }
    }

}