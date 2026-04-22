using System;
using System.Drawing;
using System.Windows.Forms;
using System.Windows.Forms.Integration;

namespace FinancialLiteracyGame
{
    public partial class Form1 : Form
    {
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

            if (sessionId == 0)
            {
                DatabaseManager.SaveSession(1, currentPlayer.Cash, currentPlayer.PassiveIncome, month);
                sessionId = DatabaseManager.GetCurrentSessionId(1);
            }

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
            else
            {
                financeManager.ProcessPayDay();
                DatabaseManager.LogRandomEvent(sessionId, month, "Доход", "Зарплата", financeManager.GetMonthlyCashflow(), 0);
                MessageBox.Show($"ДЕНЬ ЗАРПЛАТЫ!\nЧистый доход: {financeManager.GetMonthlyCashflow()} руб.", "Фин. отчет");
            }

            DatabaseManager.SaveSession(1, currentPlayer.Cash, currentPlayer.PassiveIncome, month);
            UpdateUI();
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
            if (currentPlayer != null)
            {
                lblCash.Text = $"Наличные: {currentPlayer.Cash:N0} руб.";
                lblPassiveIncome.Text = $"Пасс. доход: {currentPlayer.PassiveIncome:N0} руб.";
                lblExpenses.Text = $"Расходы: {currentPlayer.Expenses:N0} руб.";
                lblSalary.Text = $"Зарплата: {currentPlayer.Salary:N0} руб.";
                gbProfession.Text = $"Профессия: {currentPlayer.Profession}";
                lblPassiveIncome.ForeColor = currentPlayer.PassiveIncome > 0 ? Color.DarkGreen : Color.Black;
            }
            if (currentPlayer.Cash < -10000)
            {
                MessageBox.Show("К сожалению, вы объявили себя банкротом. Попробуйте пересмотреть свою финансовую стратегию!", "Игра окончена");
                // Логика сброса игры или выхода
            }
            double progress = (double)(currentPlayer.PassiveIncome / currentPlayer.Expenses) * 100;
            // Можно вывести это в заголовок GroupBox или отдельную метку
            gbStats.Text = $"Статистика (Цель: {progress:F0}%)";

            if (financeManager.IsFinancialIndependent())
            {
                MessageBox.Show("Победа! Ваши доходы превысили расходы!", "ПОБЕДА");
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
    }
}