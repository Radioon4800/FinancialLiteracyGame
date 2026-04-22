namespace FinancialLiteracyGame
{
    public class Player
    {
        public string Name { get; set; }
        public string Profession { get; set; }

        // Финансовые показатели
        public decimal Cash { get; set; }          // Наличные (капитал)
        public decimal Salary { get; set; }        // Активный доход
        public decimal PassiveIncome { get; set; } // Доход от активов
        public decimal Expenses { get; set; }      // Ежемесячные траты

        // В будущем сюда добавим List<Asset> и List<Loan>
    }
}