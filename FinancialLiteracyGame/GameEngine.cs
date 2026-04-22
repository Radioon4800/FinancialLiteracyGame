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
                "Совет: Наличие резервного фонда (3-6 окладов) позволяет оплачивать такие расходы без долгов."));

            _riskDeck.Add(new RiskEvent(
                "Визит к стоматологу",
                "Внезапная зубная боль требует лечения.",
                3500m, "Медицина",
                "Совет: Добровольное медицинское страхование (ДМС) часто покрывает такие расходы."));

            // Наполняем колоду РЫНКА
            _marketDeck.Add(new MarketEvent(
                "Акции IT-компании",
                "Цена за лот сегодня: 10 000 руб. Ожидаемый доход: 1 000 руб/мес.",
                10000m, 1000m,
                "Совет: Инвестиции в акции — это способ создать пассивный доход, но помните о рисках изменения цены."));

            _marketDeck.Add(new MarketEvent(
                "Банковский депозит",
                "Открытие вклада под высокий процент. Вложение: 5 000 руб. Доход: 400 руб/мес.",
                5000m, 400m,
                "Совет: Депозит — самый консервативный и надежный инструмент для начинающего инвестора."));
        }

        public GameEvent GenerateEvent(int cellIndex)
        {
            if (cellIndex % 6 == 0) return null; // Клетка зарплаты

            if (cellIndex % 2 != 0) // Белые клетки — Риски
            {
                return _riskDeck[_rnd.Next(_riskDeck.Count)];
            }
            else // Синие клетки — Рынок
            {
                return _marketDeck[_rnd.Next(_marketDeck.Count)];
            }
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
    }
}