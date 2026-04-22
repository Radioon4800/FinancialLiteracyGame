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

    public partial class ProfessionForm : Form
    {
        public Player SelectedPlayer { get; private set; }

        // Список профессий (в реальности можно вынести в JSON или БД)
        private List<ProfessionData> professions = new List<ProfessionData>
    {
    new ProfessionData {
        Title = "Программист",
        Description = "Высокий старт, но и высокие запросы. Легко покупать пассивы, но трудно сокращать расходы.",
        Salary = 75000, Expenses = 45000, StartingCash = 25000, ImagePath = "dev"
    },
    new ProfessionData {
        Title = "Врач",
        Description = "Стабильный средний класс. Хороший баланс для тех, кто хочет играть в долгую.",
        Salary = 55000, Expenses = 30000, StartingCash = 15000, ImagePath = "doc"
    },
    new ProfessionData {
        Title = "Учитель",
        Description = "Сложный уровень. Маленькая зарплата заставляет очень тщательно выбирать каждую инвестицию.",
        Salary = 35000, Expenses = 22000, StartingCash = 8000, ImagePath = "teacher"
    },
    new ProfessionData {
        Title = "Студент",
        Description = "Экстремальный уровень. Денег почти нет, но за счет мизерных расходов путь к свободе может быть коротким.",
        Salary = 18000, Expenses = 12000, StartingCash = 4000, ImagePath = "student"
    },
    new ProfessionData {
        Title = "Менеджер",
        Description = "Золотая середина. Много возможностей для роста и средние стартовые накопления.",
        Salary = 45000, Expenses = 25000, StartingCash = 12000, ImagePath = "manager"
    },
    new ProfessionData {
        Title = "Бизнесмен",
        Description = "Читерский уровень? Денег много, но если бизнес прогорит (события риск), падать будет больно.",
        Salary = 120000, Expenses = 85000, StartingCash = 50000, ImagePath = "boss"
    },
    new ProfessionData {
        Title = "Таксист",
        Description = "Работа на себя. Весь доход зависит от ваших действий, расходы минимальны, но и накоплений нет.",
        Salary = 40000, Expenses = 20000, StartingCash = 5000, ImagePath = "taxi"
    }
};

        public ProfessionForm()
        {
            InitializeComponent();
            listProfessions.DataSource = professions;
            listProfessions.DisplayMember = "Title";
            listProfessions.SelectedIndexChanged += ListProfessions_SelectedIndexChanged;
        }

        private void ListProfessions_SelectedIndexChanged(object sender, EventArgs e)
        {
            var selected = (ProfessionData)listProfessions.SelectedItem;
            if (selected != null)
            {
                lblDescription.Text = selected.Description;
                lblStartSalary.Text = $"Зарплата: {selected.Salary:N0} руб.";
                lblStartExpenses.Text = $"Расходы: {selected.Expenses:N0} руб.";
                lblStartCash.Text = $"На старте: {selected.StartingCash:N0} руб.";

                // Загрузка картинки (если файл есть в папке bin/Images)
                // picProfession.Image = Image.FromFile("Images/" + selected.ImagePath);
            }
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                MessageBox.Show("Пожалуйста, введите ваше имя!");
                return;
            }

            var selected = (ProfessionData)listProfessions.SelectedItem;

            SelectedPlayer = new Player
            {
                Name = txtName.Text,
                Profession = selected.Title,
                Salary = selected.Salary,
                Expenses = selected.Expenses,
                Cash = selected.StartingCash,
                PassiveIncome = 0
            };

            this.DialogResult = DialogResult.OK;
            this.Close();
        }
    }    
    public class ProfessionData
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public decimal Salary { get; set; }
        public decimal Expenses { get; set; }
        public decimal StartingCash { get; set; }
        public string ImagePath { get; set; } // Имя файла из ресурсов или папки
    }
}
