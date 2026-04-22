using System;
using System.Collections.Generic;
using System.Numerics;

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
            // Внутри этого класса методы и игрока (через _player) видно
            decimal income = GetMonthlyCashflow();
            _player.Cash += income;
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
        // Проверь, чтобы параметры в скобках назывались именно так
        public bool BuyAsset(decimal cost, decimal bonus)
        {
            // Проверяем, хватает ли наличных (Cash)
            if (_player.Cash >= cost)
            {
                _player.Cash -= cost;           // Вычитаем стоимость
                _player.PassiveIncome += bonus; // Увеличиваем пассивный доход!
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
        public string GetBudgetAnalysis(Player p)
        {
            decimal totalIncome = p.Salary + p.PassiveIncome;
            decimal needsPercent = (p.Expenses / totalIncome) * 100;

            if (needsPercent > 50)
                return "Внимание: Расходы на нужды превышают 50% дохода! Сократите траты.";
            return "Бюджет в норме. Вы следуете правилу 50/30/20.";
        }
    }
}