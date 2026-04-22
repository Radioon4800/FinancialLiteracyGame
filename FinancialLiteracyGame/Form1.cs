using System;
using System.Drawing;
using System.Windows.Forms;
using System.Windows.Forms.Integration;

namespace FinancialLiteracyGame
{
    public partial class Form1 : Form
    {
        private int stepsToPayDay = 30;
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

            Init3D();
            CreateCustomLabels();

            // Настройка таймера анимации
            diceTimer = new System.Windows.Forms.Timer();
            diceTimer.Interval = 50;
            diceTimer.Tick += DiceTimer_Tick;

            var sessionTable = DatabaseManager.GetLastSession(1);

            if (sessionTable != null && sessionTable.Rows.Count > 0)
            {
                var row = sessionTable.Rows[0];
                currentPlayer = new Player
                {
                    Name = "Алексей",
                    Profession = "Программист",
                    Salary = 50000,
                    Expenses = 30000,
                    Cash = Convert.ToDecimal(row["cash_balance"]),
                    PassiveIncome = Convert.ToDecimal(row["passive_income"])

                };
                gameEngine = new GameEngine(currentPlayer);
                int savedCell = Convert.ToInt32(row["current_cell"]);
                gameEngine.CurrentPosition = savedCell;

                // Визуально перемещаем фишку в 3D сцене
                game3D.MovePlayer(savedCell);
            }
            else
            {
                currentPlayer = new Player
                {
                    Name = "Алексей",
                    Profession = "Программист",
                    Salary = 50000,
                    Expenses = 30000,
                    Cash = 10000,
                    PassiveIncome = 0
                };
            }

            financeManager = new FinanceManager(currentPlayer);
            gameEngine = new GameEngine(currentPlayer);

            UpdateUI();
            this.KeyDown += Form1_KeyDown;
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

            // Результат: вспышка цвета
            lblDiceResult.Text = $"ВЫПАЛО: {diceValue}";
            lblDiceResult.BackColor = Color.Gold; // Золотистый фон для результата
            lblDiceResult.ForeColor = Color.DarkRed;

            game3D.MovePlayer(cellIndex);
            HandlePlayerMove(cellIndex);

            button1.Enabled = true;
        }

        private void HandlePlayerMove(int cellIndex)
        {
            int sessionId = DatabaseManager.GetCurrentSessionId(1);
            int month = 1;

            // --- ЛОГИКА СЧЕТЧИКА ЗАРПЛАТЫ ---
            stepsToPayDay--; // Уменьшаем счетчик при каждом шаге

            if (stepsToPayDay <= 0)
            {
                financeManager.ProcessPayDay();
                DatabaseManager.LogRandomEvent(sessionId, month, "Income", "Зарплата", financeManager.GetMonthlyCashflow(), 0);

                MessageBox.Show($"ДЕНЬ ЗАРПЛАТЫ!\nЧистый доход: {financeManager.GetMonthlyCashflow()} руб.\n" +
                                $"Следующая зарплата через 30 ходов.", "Фин. отчет");

                stepsToPayDay = 30; // Сбрасываем счетчик обратно
            }
            // --------------------------------

            if (sessionId == 0)
            {
                DatabaseManager.SaveSession(1, currentPlayer.Cash, currentPlayer.PassiveIncome, month, cellIndex);
                sessionId = DatabaseManager.GetCurrentSessionId(1);
            }

            // Теперь события генерируются ВСЕГДА, независимо от зарплаты
            GameEvent ev = gameEngine.GenerateEvent(cellIndex);

            if (ev != null)
            {
                using (EventForm eventWindow = new EventForm(ev))
                {
                    if (eventWindow.ShowDialog() == DialogResult.OK || ev is RiskEvent)
                    {
                        ev.Execute(currentPlayer, financeManager);
                        ev.SaveToHistory(sessionId, month);
                    }
                }
            }

            // ВАЖНО: Удаляем блок else, который был здесь раньше, 
            // так как зарплата теперь привязана к шагам, а не к пустой клетке.

            UpdateUI();

            // Проверка на банкротство и сохранение
            if (currentPlayer.Cash < -10000)
            {
                button1.Enabled = false;
                DatabaseManager.SaveSession(1, currentPlayer.Cash, currentPlayer.PassiveIncome, month, cellIndex);
                return;
            }

            DatabaseManager.SaveSession(1, currentPlayer.Cash, currentPlayer.PassiveIncome, month, cellIndex);
        }

        private void CreateCustomLabels()
        {
            lblCash = new Label { Location = new Point(10, 30), AutoSize = true };
            lblPassiveIncome = new Label { Location = new Point(10, 60), AutoSize = true };
            lblExpenses = new Label { Location = new Point(10, 90), AutoSize = true, ForeColor = Color.Red };
            lblSalary = new Label { Location = new Point(10, 120), AutoSize = true, ForeColor = Color.Green };

            // Инициализация метки кубика
            lblDiceResult = new Label
            {
                AutoSize = false,
                Size = new Size(180, 60), // Ширина под размер боковой панели
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Arial", 16, FontStyle.Bold),
                ForeColor = Color.OrangeRed,
                BackColor = Color.WhiteSmoke,
                BorderStyle = BorderStyle.FixedSingle,
                Text = "Кубик: -"
            };

            // Добавляем метку в LeftSide (убедитесь, что имя панели совпадает)
            if (LeftSide != null)
            {
                LeftSide.Controls.Add(lblDiceResult);

                // Позиционируем в самый низ панели
                lblDiceResult.Dock = DockStyle.Bottom;
            }
            else if (gbStats != null) // Если LeftSide нет, используем GroupBox
            {
                gbStats.Controls.Add(lblDiceResult);
                lblDiceResult.Location = new Point(10, 180);
            }

        // Выводим на передний план
        lblDiceResult.BringToFront();

            // Центрируем
            CenterDiceLabel();

            if (gbStats != null)
            {
                gbStats.Controls.Add(lblCash);
                gbStats.Controls.Add(lblPassiveIncome);
                gbStats.Controls.Add(lblExpenses);
                gbStats.Controls.Add(lblSalary);
                gbStats.Controls.Add(lblDiceResult);
            }
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

            // 1. Обновляем основные показатели
            lblCash.Text = $"Наличные: {currentPlayer.Cash:N0} руб.";
            lblPassiveIncome.Text = $"Пассивный доход: {currentPlayer.PassiveIncome:N0} руб.";
            lblExpenses.Text = $"Расходы: {currentPlayer.Expenses:N0} руб.";
            lblSalary.Text = $"Зарплата: {currentPlayer.Salary:N0} руб.";
            gbProfession.Text = $"Профессия: {currentPlayer.Profession}";

            // Подсветка дохода (зеленый, если есть инвестиции)
            lblPassiveIncome.ForeColor = currentPlayer.PassiveIncome > 0 ? Color.DarkGreen : Color.Black;

            // 2. Расчет прогресса (исправлено деление)
            // Используем decimal для точности, прежде чем перевести в double для отображения
            decimal progressPercent = 0;
            if (currentPlayer.Expenses > 0)
            {
                progressPercent = (currentPlayer.PassiveIncome / (currentPlayer.Expenses * 1.2m)) * 100;
            }

            // Ограничиваем прогресс 100%, чтобы не пугать игрока цифрами 200%
            double displayProgress = Math.Min(100, (double)progressPercent);
            gbStats.Text = $"Статистика (Свобода: {displayProgress:F0}%)";

            // 3. Проверка на банкротство
            if (currentPlayer.Cash < -10000)
            {
                button1.Enabled = false;
                lblCash.ForeColor = Color.Red;

                MessageBox.Show(
                    "Вы банкрот! Долги превысили 10,000 руб.",
                    "Игра окончена",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Stop
                );
                return; // Дальше проверять победу нет смысла
            }

            // 4. Проверка условия победы (из твоего FinanceManager)
            if (financeManager.IsFinancialIndependent())
            {
                button1.Enabled = false; // Останавливаем игру
                MessageBox.Show(
                    "ПОЗДРАВЛЯЕМ!\nВы достигли финансовой независимости.\nВаш пассивный доход покрывает расходы с запасом 20%!",
                    "ПОБЕДА",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );
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
            // 1. Инициализируем стартовые параметры в БД
            // (Позже здесь будет вызов формы выбора профессии)
            DatabaseManager.CreateNewSession(1, "Алексей", "Программист", 50000, 30000, 10000);

            // 2. Обнуляем локальные объекты
            currentPlayer = new Player
            {
                Name = "Алексей",
                Profession = "Программист",
                Salary = 50000,
                Expenses = 30000,
                Cash = 10000,
                PassiveIncome = 0
            };

            financeManager = new FinanceManager(currentPlayer);
            gameEngine = new GameEngine(currentPlayer);
            gameEngine.CurrentPosition = 0;

            // 3. Сбрасываем UI и 3D
            game3D.MovePlayer(0);
            button1.Enabled = true;
            lblCash.ForeColor = Color.Black;

            UpdateUI();

            MessageBox.Show("Новая игра начата!", "Успех");
        }
    }
}