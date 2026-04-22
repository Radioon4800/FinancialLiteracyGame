using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using System.Windows.Forms.Integration;

namespace FinancialLiteracyGame
{
    public partial class Form1 : Form
    {
        private int totalCellsPassed = 0; // Общий счетчик пройденных клеток
        private Label lblTotalSteps;
        private int stepsToPayDay = 30;
        private int currentMonth = 1;
        private int currentCell = 0;
        private System.Windows.Forms.Timer diceTimer;
        private int animationTicks = 0;
        private Random rnd = new Random(); // Добавили генератор для анимации

        private GameBoard3D game3D;
        private ElementHost host;

        private Player currentPlayer;
        private FinanceManager financeManager;
        private GameEngine gameEngine;

        private Label lblCash;
        private Label lblPassiveIncome;
        private Label lblExpenses;
        private Label lblSalary;

        public Form1()
        {
            DatabaseManager.InitializeDatabase();
            InitializeComponent();
            this.KeyPreview = true;

            // ВАЖНО: Убираем отсюда CreateCustomLabels();

            Init3D();

            diceTimer = new System.Windows.Forms.Timer();
            diceTimer.Interval = 50;
            diceTimer.Tick += DiceTimer_Tick;

            currentPlayer = new Player();
            financeManager = new FinanceManager(currentPlayer);
            gameEngine = new GameEngine(currentPlayer);

            this.KeyDown += Form1_KeyDown;

            this.Load += (s, e) => {
                // Создаем метки только когда форма готова
                CreateCustomLabels();

                DataTable dt = DatabaseManager.GetLastSession(1);
                if (dt == null || dt.Rows.Count == 0)
                {
                    ResetGame();
                }
                else
                {
                    LoadProgressFromDb();
                }
            };
        }
        // Внутри класса Form1
        private void LoadProgressFromDb()
        {
            DataTable dtSession = DatabaseManager.GetLastSession(1);

            if (dtSession != null && dtSession.Rows.Count > 0)
            {
                DataRow sessionRow = dtSession.Rows[0];

                // ВЫЗОВ МЕТОДА ИЗ КЛАССА DatabaseManager
                DataTable dtPlayer = DatabaseManager.GetPlayer(1);

                if (dtPlayer != null && dtPlayer.Rows.Count > 0)
                {
                    DataRow pRow = dtPlayer.Rows[0];
                    currentPlayer.Name = pRow["name"].ToString();
                    currentPlayer.Profession = pRow["profession"].ToString();
                    currentPlayer.Salary = Convert.ToDecimal(pRow["monthly_salary"]);
                    currentPlayer.Expenses = Convert.ToDecimal(pRow["monthly_expenses"]);
                }

                // Загрузка остальных данных...
                currentPlayer.Cash = Convert.ToDecimal(sessionRow["cash_balance"]);
                currentPlayer.PassiveIncome = Convert.ToDecimal(sessionRow["passive_income"]);

                // Переинициализация логики
                this.financeManager = new FinanceManager(this.currentPlayer);
                this.gameEngine = new GameEngine(this.currentPlayer);

                UpdateUI();
            }
        }

        // 1. Кнопка теперь только запускает анимацию
        private void button1_Click(object sender, EventArgs e)
        {
            if (diceTimer.Enabled) return;

            lblDiceResult.Visible = true; // Показываем в центре
            lblDiceResult.BringToFront(); // Убеждаемся, что она поверх 3D
            CenterDiceLabel(); // На всякий случай обновляем центр

            button1.Enabled = false;
            animationTicks = 0;
            diceTimer.Start();
        }

        // 2. Эффект "вращения" кубика
        private void DiceTimer_Tick(object sender, EventArgs e)
        {
            animationTicks++;

            // Эффект вращения: случайные цифры и нейтральный цвет
            lblDiceResult.Text = $"🎲 {rnd.Next(1, 7)}";
            lblDiceResult.BackColor = Color.LightGray;
            lblDiceResult.ForeColor = Color.DimGray;

            if (animationTicks > 15)
            {
                diceTimer.Stop();
                FinalizeMove();
            }
        }

        // 3. Реальный расчет после анимации
        private void FinalizeMove()
        {
            int cellIndex = gameEngine.RollDice();
            int diceValue = gameEngine.GetLastRoll();
            totalCellsPassed += diceValue;
            // Результат: вспышка цвета
            lblDiceResult.Text = $"ВЫПАЛО: {diceValue}";
            lblDiceResult.BackColor = Color.Gold; // Золотистый фон для результата
            lblDiceResult.ForeColor = Color.DarkRed;

            game3D.MovePlayer(cellIndex);
            HandlePlayerMove(cellIndex);

            button1.Enabled = true;
            UpdateUI();
        }

        private void HandlePlayerMove(int cellIndex)
        {
            int sessionId = DatabaseManager.GetCurrentSessionId(1);

            if (sessionId == 0)
            {
                DatabaseManager.CreateNewSession(1, currentPlayer.Name, currentPlayer.Profession,
                                                 currentPlayer.Salary, currentPlayer.Expenses, currentPlayer.Cash);
                sessionId = DatabaseManager.GetCurrentSessionId(1);
            }

            // --- ИСПРАВЛЕННАЯ ЛОГИКА ЗАРПЛАТЫ ---

            // 1. Узнаем, сколько клеток мы прошли на самом деле
            int diceRoll = gameEngine.GetLastRoll();

            // 2. Вычитаем реальный путь из дистанции до зарплаты
            stepsToPayDay -= diceRoll;

            if (stepsToPayDay <= 0)
            {
                // ОШИБКА БЫЛА ТУТ: нужно вызывать метод У ОБЪЕКТА financeManager
                financeManager.ProcessPayDay();

                // И ТУТ: доход тоже берем из financeManager
                decimal income = financeManager.GetMonthlyCashflow();

                DatabaseManager.LogRandomEvent(sessionId, currentMonth, "Income", "Зарплата", income, 0);

                MessageBox.Show($"ДЕНЬ ЗАРПЛАТЫ!\nЧистый доход: {income:N0} руб.", "Фин. отчет");

                // Сбрасываем счетчик, учитывая "перелет"
                stepsToPayDay = 30 + stepsToPayDay;

                currentMonth++;
            }

            // Сохраняем прогресс (включая пройденные клетки, если добавили их)
            DatabaseManager.SaveSession(1, currentPlayer.Cash, currentPlayer.PassiveIncome, currentMonth, cellIndex, stepsToPayDay);

            // Обработка события на клетке
            GameEvent ev = gameEngine.GenerateEvent(cellIndex);
            if (ev != null)
            {
                using (EventForm eventWindow = new EventForm(ev, currentPlayer))
                {
                    if (eventWindow.ShowDialog() == DialogResult.OK)
                    {
                        ev.Execute(currentPlayer, financeManager);
                    }
                }
            }

            UpdateUI();
        }

        private void CreateCustomLabels()
        {
            // Если метки уже созданы, не дублируем их
            if (lblCash != null) return;

            // 1. Инициализируем метки с дефолтным текстом
            lblCash = new Label { Location = new Point(10, 30), AutoSize = true, Text = "Наличные: 0" };
            lblPassiveIncome = new Label { Location = new Point(10, 60), AutoSize = true, Text = "Пассив: 0" };
            lblExpenses = new Label { Location = new Point(10, 90), AutoSize = true, ForeColor = Color.Red, Text = "Расходы: 0" };
            lblSalary = new Label { Location = new Point(10, 120), AutoSize = true, ForeColor = Color.Green, Text = "Зарплата: 0" };

            lblDiceResult = new Label
            {
                AutoSize = false,
                Size = new Size(180, 60),
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Arial", 16, FontStyle.Bold),
                ForeColor = Color.OrangeRed,
                BackColor = Color.WhiteSmoke,
                BorderStyle = BorderStyle.FixedSingle,
                Text = "Кубик: -"
            };
            lblTotalSteps = new Label
            {
                Location = new Point(10, 150), // Разместим в самом верху блока статистики
                AutoSize = true,
                Font = new Font("Arial", 9, FontStyle.Bold),
                ForeColor = Color.Blue,
                Text = "Пройдено клеток: 0"
            };

            if (gbStats != null)
            {
                // Добавляем новую метку в контейнер
                gbStats.Controls.Add(lblTotalSteps);
                // Не забудьте сдвинуть остальные метки чуть ниже, если они перекрываются
                lblCash.Top = 30;
                lblPassiveIncome.Top = 55;
            }
            // 2. Добавляем финансовые метки в контейнер статистики
            if (gbStats != null)
            {
                gbStats.Controls.AddRange(new Control[] { lblCash, lblPassiveIncome, lblExpenses, lblSalary });
            }

            // 3. Добавляем кубик ТОЛЬКО В ОДНО МЕСТО
            if (LeftSide != null)
            {
                LeftSide.Controls.Add(lblDiceResult);
                lblDiceResult.Dock = DockStyle.Bottom;
            }
            else if (gbStats != null)
            {
                gbStats.Controls.Add(lblDiceResult);
                lblDiceResult.Location = new Point(10, 160); // Чуть выше, чтобы не перекрывать
            }

            lblDiceResult.BringToFront();
            CenterDiceLabel();
        }
        private void CenterDiceLabel()
        {
            if (lblDiceResult != null)
            {
                // Если метка внутри панели, используем размеры панели
                int parentWidth = lblDiceResult.Parent.ClientSize.Width;
                int parentHeight = lblDiceResult.Parent.ClientSize.Height;

                lblDiceResult.Left = (parentWidth - lblDiceResult.Width) / 2;
                lblDiceResult.Top = (parentHeight - lblDiceResult.Height) / 2;

                // Временный яркий цвет для теста, чтобы увидеть её
                lblDiceResult.BackColor = Color.Yellow;
            }
        }

        private void UpdateUI()
        {
            if (currentPlayer == null) return;

            // 1. ЛОГИКА ИГРОВЫХ КЛЕТОК И РЫНКА
            int cell = gameEngine.CurrentPosition;
            // Клетка четная, не кратна 6 (не PayDay)
            bool isMarketCell = (cell % 2 == 0) && (cell % 6 != 0);

            button2.Enabled = isMarketCell;
            button2.Text = isMarketCell ? "Открыть рынок" : "Рынок недоступен";
            button2.BackColor = isMarketCell ? Color.LightGreen : Color.LightGray;

            // 2. ОБНОВЛЕНИЕ ТЕКСТОВЫХ ПОЛЕЙ
            lblTotalSteps.Text = $"Пройдено клеток: {totalCellsPassed}";
            lblCash.Text = $"Наличные: {currentPlayer.Cash:N0} руб.";
            lblPassiveIncome.Text = $"Пассив: {currentPlayer.PassiveIncome:N0} руб.";
            lblExpenses.Text = $"Расходы: {currentPlayer.Expenses:N0} руб.";
            lblSalary.Text = $"Зарплата: {currentPlayer.Salary:N0} руб.";
            gbProfession.Text = $"Профессия: {currentPlayer.Profession}";

            // Визуальные акценты
            lblPassiveIncome.ForeColor = currentPlayer.PassiveIncome > 0 ? Color.DarkGreen : Color.Black;
            lblCash.ForeColor = currentPlayer.Cash < 0 ? Color.Red : Color.Black;

            // 3. РАСЧЕТ ПРОГРЕССА СВОБОДЫ (из вашей формулы в дипломе)
            double freedomProgress = 0;
            if (currentPlayer.Expenses > 0)
            {
                freedomProgress = (double)(currentPlayer.PassiveIncome / currentPlayer.Expenses) * 100;
            }

            // Ограничиваем для красоты (например, до 120%, чтобы показать перевыполнение)
            double displayProgress = Math.Min(120, freedomProgress);

            string stageName = currentPlayer.CurrentStage == GameStage.FastTrack ? "Скоростная дорожка" : "Крысиные бега";
            gbStats.Text = $"Статистика | Этап: {stageName} (Свобода: {displayProgress:F0}%)";

            decimal netIncome = (currentPlayer.Salary + currentPlayer.PassiveIncome)
                    - currentPlayer.Expenses
                    - currentPlayer.LoanInterestPayment;

            // 2. Устанавливаем цвет в зависимости от результата
            if (netIncome > 0)
            {
                // Прибыль — темно-зеленый (DarkGreen лучше читается на светлом фоне, чем просто Green)
                lblNetIncome.ForeColor = Color.DarkGreen;
            }
            else if (netIncome < 0)
            {
                // Дефицит бюджета — красный
                lblNetIncome.ForeColor = Color.Red;
            }
            else
            {
                // Ноль — обычный черный
                lblNetIncome.ForeColor = Color.Black;
            }

            // 3. Выводим текст (теперь он будет нужного цвета)
            lblNetIncome.Text = $"{netIncome:N0} руб./мес.";

            lblCash.Text = $"{currentPlayer.Cash:N0} руб.";
            lblPassiveIncome.Text = $"{currentPlayer.PassiveIncome:N0} руб.";
            lblNetIncome.Text = $"{netIncome:N0} руб./мес."; // Игрок увидит, как доход падает из-за кредитов
            lblDebt.Text = $"Ваш долг: {currentPlayer.TotalDebt:N0} руб.";

            // Логика банкротства (если баланс слишком отрицательный)
            if (currentPlayer.Cash < -10000)
            {
                button1.Enabled = false; // Блокируем кубик
                lblMessage.Text = "Вы банкрот! Слишком много долгов.";
                lblMessage.ForeColor = Color.Red;
            }

            // 4. ПРОВЕРКА СОСТОЯНИЙ (Банкротство -> Переход -> Победа)

            // БАНКРОТСТВО
            if (currentPlayer.Cash < -10000)
            {
                button1.Enabled = false;
                MessageBox.Show("Вы банкрот! Долги превысили 10,000 руб.", "Игра окончена", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                return;
            }

            // ПЕРЕХОД НА СКОРОСТНУЮ ДОРОЖКУ
            if (currentPlayer.CurrentStage == GameStage.RatRace && currentPlayer.PassiveIncome > currentPlayer.Expenses)
            {
                currentPlayer.CurrentStage = GameStage.FastTrack;
                HandleStageTransition();
            }

            // ПОБЕДА НА СКОРОСТНОЙ ДОРОЖКЕ
            // Условие из вашей цитаты: эффективное управление капиталом (например, накопить 1 млн пассива)
            if (currentPlayer.CurrentStage == GameStage.FastTrack && currentPlayer.PassiveIncome > 1000000)
            {
                button1.Enabled = false;
                MessageBox.Show("ФИНАЛЬНАЯ ПОБЕДА!\nВы стали финансовым магнатом на скоростной дорожке!", "Триумф");
            }
            decimal totalInc = currentPlayer.Salary + currentPlayer.PassiveIncome;
            if (totalInc > 0 && (currentPlayer.Expenses / totalInc) > 0.8m)
            {
                lblExpenses.ForeColor = Color.Red;
            }
            else
            {
                lblExpenses.ForeColor = Color.Black;
            }
        }
        private void HandleStageTransition()
        {
            // 1. Останавливаем текущие процессы (таймеры), чтобы игрок осознал момент
            diceTimer?.Stop();
            button1.Enabled = false;

            // 2. Визуальное и звуковое уведомление
            // В дипломной работе это показывает обработку смены состояний (State Machine)
            MessageBox.Show(
                "ПОЗДРАВЛЯЕМ! ВЫ ВЫШЛИ ИЗ КРЫСИНЫХ БЕГОВ!\n\n" +
                "Ваш пассивный доход превысил расходы. Вы доказали, что умеете управлять малым капиталом.\n" +
                "Добро пожаловать на Скоростную Дорожку (Fast Track).",
                "Финансовая свобода достигнута!",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information
            );

            // 3. Смена визуальной темы (UX-отклик на успех)
            // Используем "премиальные" цвета: золотистый, бежевый или глубокий синий
            this.BackColor = Color.OldLace;
            gbStats.BackColor = Color.FromArgb(255, 248, 220); // Cornsilk
            gbStats.ForeColor = Color.DarkSlateBlue;

            // 4. Масштабирование экономики (Математическая логика этапа)
            // На скоростной дорожке оперируют суммами в 10-100 раз больше
            currentPlayer.Cash *= 10;

            // В реальной жизни богатые люди тратят больше (инфляция образа жизни)
            // Но их пассивный доход здесь должен расти еще быстрее
            currentPlayer.Expenses *= 5;

            // 5. Обновление элементов управления
            // Теперь кнопка кредита может выдавать суммы по 500,000 вместо 10,000
            button3.Text = "Кредит для бизнеса (500к)";

            // Блокируем или перенастраиваем "мелкие" кнопки, если они есть
            if (button2 != null)
            {
                button2.Text = "Инвестировать в корпорации";
                button2.BackColor = Color.Gold;
            }

            // 6. Сброс игрового прогресса до зарплаты (новый цикл)
            stepsToPayDay = 30;

            // 7. Финальное обновление UI и логов
            lblMessage.Text = "Этап: Скоростная дорожка";
            lblMessage.ForeColor = Color.DarkGoldenrod;

            button1.Enabled = true;
            UpdateUI();

            // Сохраняем переход в базу данных, чтобы не потерять статус при перезагрузке
            int sessionId = DatabaseManager.GetCurrentSessionId(1);
            DatabaseManager.SaveSession(1, currentPlayer.Cash, currentPlayer.PassiveIncome, currentMonth, currentCell, stepsToPayDay);
        }
        private void CheckBudgetHealth()
        {
            decimal totalIncome = currentPlayer.Salary + currentPlayer.PassiveIncome;
            if (totalIncome == 0) return;

            decimal expenseRatio = (currentPlayer.Expenses / totalIncome) * 100;

            if (expenseRatio > 50)
            {
                MessageBox.Show($"Ваши расходы ({expenseRatio:F0}%) превышают рекомендуемые 50%.\n" +
                                "На этапе 'Крысиных бегов' это опасно. Старайтесь не покупать пассивы!",
                                "Финансовый совет");
            }
        }

        private void Init3D()
        {
            host = new ElementHost();
            host.Dock = DockStyle.Fill;
            game3D = new GameBoard3D();
            host.Child = game3D;
            if (panel != null) panel.Controls.Add(host);
            host.MouseDown += (s, e) => { host.Focus(); };
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            double step = 5.0;
            switch (e.KeyCode)
            {
                case Keys.A: game3D?.RotateBoard(-step); break;
                case Keys.D: game3D?.RotateBoard(step); break;
            }
        }

        private void btnNewGame_Click_Click(object sender, EventArgs e)
        {
            var result = MessageBox.Show("Вы уверены, что хотите начать новую игру? Текущий прогресс будет потерян.",
                                 "Новая игра", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                ResetGame();
            }
        }
        private void ResetGame()
        {
            using (ProfessionForm profForm = new ProfessionForm())
            {
                if (profForm.ShowDialog() == DialogResult.OK)
                {
                    if (profForm.SelectedPlayer == null) return;

                    // 1. Обновляем данные игрока напрямую из формы выбора
                    this.currentPlayer = profForm.SelectedPlayer;

                    // 2. Инициализируем сессию в базе данных
                    DatabaseManager.CreateNewSession(
                        1,
                        currentPlayer.Name,
                        currentPlayer.Profession,
                        currentPlayer.Salary,
                        currentPlayer.Expenses,
                        currentPlayer.Cash
                    );

                    // 3. ПЕРЕИНИЦИАЛИЗИРУЕМ менеджеры с НОВЫМ объектом игрока
                    this.financeManager = new FinanceManager(this.currentPlayer);
                    this.gameEngine = new GameEngine(this.currentPlayer);

                    // 4. Сброс визуального состояния
                    this.currentCell = 0;
                    this.stepsToPayDay = 30;
                    if (game3D != null) game3D.MovePlayer(0);

                    // 5. ВАЖНО: Принудительно вызываем обновление UI
                    UpdateUI();

                    MessageBox.Show($"Удачи, {currentPlayer.Name}! Ваша цель — пассивный доход > {currentPlayer.Expenses * 1.2m:N0} руб.");
                }
            }
        }

        private void gbStats_Enter(object sender, EventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {
            // 1. Генерируем доступный актив для текущей клетки или ситуации
            // Можно сделать, чтобы активы были доступны всегда, либо только на определенных клетках
            GameEvent marketEvent = gameEngine.GenerateMarketOpportunity();

            if (marketEvent == null)
            {
                MessageBox.Show("В данный момент нет доступных предложений для инвестирования.", "Рынок активов");
                return;
            }

            // 2. Открываем окно с предложением (используем вашу EventForm)
            using (EventForm assetWindow = new EventForm(marketEvent, currentPlayer))
            {
                if (assetWindow.ShowDialog() == DialogResult.OK)
                {
                    // 3. Выполняем покупку 
                    // (Теперь мы уверены, что денег хватает, так как кнопка в форме была бы заблокирована)
                    marketEvent.Execute(currentPlayer, financeManager);

                    // 4. Сохраняем событие в историю и обновляем базу
                    int sessionId = DatabaseManager.GetCurrentSessionId(1);
                    marketEvent.SaveToHistory(sessionId, currentMonth);

                    // Сохраняем новое состояние баланса и пассивного дохода
                    DatabaseManager.SaveSession(1, currentPlayer.Cash, currentPlayer.PassiveIncome,
                                                currentMonth, currentCell, stepsToPayDay);

                    // 5. Обновляем интерфейс (это обновит все лейблы и проверит банкротство)
                    UpdateUI();

                    MessageBox.Show($"Поздравляем с приобретением: {marketEvent.Title}!", "Успешная сделка");
                }
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            decimal loanStep = 10000; // Сумма одного кредита

            // 1. Увеличиваем наличность и долг
            currentPlayer.Cash += loanStep;
            currentPlayer.TotalDebt += loanStep;

            // 2. Обновляем базу данных (чтобы кредит не пропал при перезапуске)
            // DatabaseManager.UpdateDebt(1, currentPlayer.TotalDebt); 

            // 3. Обновляем интерфейс
            UpdateUI();

            MessageBox.Show($"Вы взяли кредит на {loanStep:N0} руб.\n" +
                            $"Теперь ваш долг: {currentPlayer.TotalDebt:N0} руб.\n" +
                            $"Ежемесячный платеж вырос на {loanStep * 0.1m:N0} руб.",
                            "Кредит оформлен");
        }
    }
}