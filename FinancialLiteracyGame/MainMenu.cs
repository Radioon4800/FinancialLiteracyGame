using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FinancialLiteracyGame
{
    public partial class MainMenuForm : Form
    {
        public MainMenuForm()
        {
            InitializeComponent();
        }

        private void MainMenuForm_Load(object sender, EventArgs e)
        {
            RefreshLoadButton();
        }
        private void RefreshLoadButton()
        {
            // Пытаемся получить последнюю сессию для игрока №1
            var session = DatabaseManager.GetLastSession(1);
            btnLoad.Enabled = (session != null && session.Rows.Count > 0);
        }

        private void btnNewGame_Click(object sender, EventArgs e)
        {
            using (ProfessionForm profForm = new ProfessionForm())
            {
                // 1. Показываем форму выбора персонажа
                if (profForm.ShowDialog() == DialogResult.OK)
                {
                    Player newPlayer = profForm.SelectedPlayer;

                    // 2. Создаем новую запись в БД (обнуляем старый прогресс для ID 1)
                    DatabaseManager.CreateNewSession(
                        1,
                        newPlayer.Name,
                        newPlayer.Profession,
                        newPlayer.Salary,
                        newPlayer.Expenses,
                        newPlayer.Cash
                    );

                    // 3. Запускаем основную форму игры
                    Form1 gameForm = new Form1();
                    gameForm.Show();

                    // Скрываем меню
                    this.Hide();

                    // Подписываемся на закрытие игры, чтобы вернуть меню или закрыть всё
                    gameForm.FormClosed += (s, args) => this.Close();
                }
            }
        }

        private void btnLoad_Click(object sender, EventArgs e)
        {
            LaunchGame();
        }
        private void LaunchGame()
        {
            // Создаем экземпляр игровой формы
            Form1 gameForm = new Form1();

            // Подписываемся на событие закрытия игровой формы
            gameForm.FormClosed += (s, args) =>
            {
                // Когда игрок выйдет из игры, снова показываем главное меню
                this.Show();
                // Обновляем состояние кнопки загрузки (вдруг появилось новое сохранение)
                RefreshLoadButton();
            };

            // Показываем игру и скрываем меню
            gameForm.Show();
            this.Hide();
        }

        private void btnSettings_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Здесь будут настройки звука и графики.", "Настройки");
        }

        private void btntutorial_Click(object sender, EventArgs e)
        {
            MessageBox.Show(
                "Как играть:\n1. Бросайте кубик (кнопка в игре).\n" +
                "2. Следите за балансом и пассивным доходом.\n" +
                "3. Остерегайтесь банкротства (баланс < -10,000)!",
                "Руководство");
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
    }
}
