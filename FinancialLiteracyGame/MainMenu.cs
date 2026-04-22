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
            var result = MessageBox.Show(
                "Начать новую игру? Прогресс в текущем слоте будет перезаписан.",
                "Новая игра",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (result == DialogResult.Yes)
            {
                // Создаем новую запись в БД через транзакцию
                // В будущем параметры (имя, профессия) будут приходить из формы создания персонажа
                DatabaseManager.CreateNewSession(1, "Алексей", "Программист", 50000, 30000, 10000);
                LaunchGame();
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
