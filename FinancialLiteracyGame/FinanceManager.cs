using System;
using System.Collections.Generic;

namespace FinancialLiteracyGame
{
    public class FinanceManager
    {
        private Player _player;

        public FinanceManager(Player player)
        {
            _player = player;
        }

        // Расчет ежемесячного денежного потока (Cashflow)
        // Согласно диплому: Доходы - Расходы
        public decimal GetMonthlyCashflow()
        {
            return (_player.Salary + _player.PassiveIncome) - _player.Expenses;
        }

        // Логика "Дня зарплаты"
        public void ProcessPayDay()
        {
            decimal netProfit = GetMonthlyCashflow();
            _player.Cash += netProfit;
        }

        // Обработка непредвиденного расхода
        public void ApplyExpense(decimal amount)
        {
            _player.Cash -= amount;
            if (_player.Cash < 0)
            {
                // Здесь в будущем добавим логику принудительного кредита, 
                // если наличные ушли в минус
            }
        }

        // Покупка актива (например, акций)
        public bool BuyAsset(decimal cost, decimal addedPassiveIncome)
        {
            if (_player.Cash >= cost)
            {
                _player.Cash -= cost;
                _player.PassiveIncome += addedPassiveIncome;
                return true;
            }
            return false;
        }

        // Проверка условия победы (выход из Крысиных бегов)
        // Пункт 2.1: Пассивный доход > Расходы + 20%
        public bool IsFinancialIndependent()
        {
            return _player.PassiveIncome >= (_player.Expenses * 1.2m);
        }
    }
}