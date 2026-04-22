using System;
using System.Collections.Generic;

namespace FinancialLiteracyGame
{
    public class GameEngine
    {
        private int _lastRoll = 0;
        private Player _player;
        private FinanceManager _finance;
        public int CurrentPosition
        {
            get => _currentPosition;
            set => _currentPosition = value;
        }
        private int _currentPosition = 0;
        private Random _rnd = new Random();

        // Списки событий (наши колоды)
        private List<GameEvent> _riskDeck = new List<GameEvent>();
        private List<GameEvent> _marketDeck = new List<GameEvent>();
        private List<GameEvent> _chanceDeck = new List<GameEvent>();

        public GameEngine(Player player)
        {
            _player = player;
            _finance = new FinanceManager(player);
            InitializeDecks(); // Наполняем колоды при старте
        }

        private void InitializeDecks()
        {
            // Наполняем колоду РИСКОВ (согласно пункту 2.1)
            _riskDeck.Add(new RiskEvent(
                "Поломка смартфона",
                "Ваш экран разбился, требуется замена.",
                5000m, "Имущество",
                "Совет: Наличие резервного фонда позволяет оплачивать такие расходы без долгов."));

            _riskDeck.Add(new ExpenseEvent(
                "Штраф ГИБДД",
                "Вы превысили скорость на пустой трассе. Камера зафиксировала нарушение.",
                2500m,
                "Совет: Соблюдение правил не только безопасно, но и экономит ваш бюджет."));

            _riskDeck.Add(new RiskEvent(
                "Визит к стоматологу",
                "Внезапная зубная боль требует лечения.",
                3500m, "Медицина",
                "Совет: ДМС часто покрывает такие расходы."));

            // --- Колода РЫНКА (твои активы) ---
            _marketDeck.Add(new MarketEvent(
                "Акции IT-компании",
                "Цена за лот: 10 000 руб. Доход: 1 000 руб/мес.",
                10000m, 1000m,
                "Совет: Инвестиции в акции создают пассивный доход, но их цена может колебаться."));

            _marketDeck.Add(new MarketEvent(
                "Курс по саморазвитию",
                "Обучение новой востребованной технологии. Вложение: 15 000 руб. Повышение зарплаты: 2 000 руб/мес.",
                15000m, 2000m,
                "Совет: Инвестиции в себя — самые высокодоходные в долгосрочной перспективе."));

            _marketDeck.Add(new MarketEvent(
                "Гараж в аренду",
                "Недвижимость. Цена: 50 000 руб. Доход: 4 000 руб/мес.",
                50000m, 4000m,
                "Совет: Недвижимость дает стабильный доход, но требует большого стартового капитала."));

            _marketDeck.Add(new MarketEvent(
                "Облигации госзайма",
                "Низкий риск. Цена: 5 000 руб. Доход: 300 руб/мес.",
                5000m, 300m,
                "Совет: Облигации надежнее акций, но доход по ним обычно ниже."));

            _chanceDeck.Add(new CharityEvent(
                "Благотворительный марафон",
                "Сбор средств на помощь детям-сиротам.",
                3000m,
                "Благотворительность помогает развивать эмпатию и социальную ответственность."));

            _chanceDeck.Add(new InflationEvent(
                0.05m,
                "Инфляция съедает ваши сбережения. Инвестируйте, чтобы деньги работали!"));
        }

        public GameEvent GenerateEvent(int cellIndex)
        {
            // 1. Клетка зарплаты (каждая 6-я)
            if (cellIndex % 6 == 0) return null;

            // 2. Нечетные клетки — всегда Риски (белые)
            if (cellIndex % 2 != 0)
            {
                if (_riskDeck.Count == 0) return null; // Защита от пустого списка
                return _riskDeck[_rnd.Next(_riskDeck.Count)];
            }

            // 3. Четные клетки (синие) — делим между Рынком и Шансом
            // Используем остаток от деления на 4 или просто рандом внутри
            else
            {
                // 50% шанс вытянуть карточку "Шанс", 50% - "Рынок"
                if (_rnd.Next(2) == 0 && _chanceDeck.Count > 0)
                {
                    return _chanceDeck[_rnd.Next(_chanceDeck.Count)];
                }
                else
                {
                    if (_marketDeck.Count == 0) return null;
                    return _marketDeck[_rnd.Next(_marketDeck.Count)];
                }
            }
        }
        public decimal GetTotalExpenses()
        {
            decimal total = _player.Expenses + (_player.TotalDebt * 0.1m);

            // Если страховка активна, добавляем её в список пассивных расходов
            if (_player.IsInsured)
            {
                total += _player.InsurancePremium;
            }

            return total;
        }

        public int RollDice()
        {
            _lastRoll = _rnd.Next(1, 7);
            _currentPosition = (_currentPosition + _lastRoll) % 24;
            return _currentPosition;
        }

        public int GetLastRoll()
        {
            return _lastRoll;
        }
        public Player GetStartProfile(string professionName)
        {
            switch (professionName)
            {
                case "Программист":
                    return new Player { Profession = "Программист", Salary = 60000, Expenses = 35000, Cash = 15000 };
                case "Врач":
                    return new Player { Profession = "Врач", Salary = 45000, Expenses = 20000, Cash = 10000 };
                case "Студент":
                    return new Player { Profession = "Студент", Salary = 15000, Expenses = 12000, Cash = 3000 };
                default:
                    return new Player { Profession = "Разнорабочий", Salary = 30000, Expenses = 20000, Cash = 5000 };
            }
        }
        public GameEvent GenerateMarketOpportunity()
        {
            if (_marketDeck.Count == 0) return null;

            // Выбираем случайный актив из списка доступных на рынке
            return _marketDeck[_rnd.Next(_marketDeck.Count)];
        }

    }

}