using System;
using System.Drawing;
using System.Windows.Forms;

namespace FinancialLiteracyGame
{
    public partial class EventForm : Form
    {
        private GameEvent _event;
        public bool IsConfirmed { get; private set; }

        public EventForm(GameEvent ev)
        {
            InitializeComponent();
            _event = ev;
            IsConfirmed = false;

            // Настройка внешнего вида
            this.Text = "Игровое событие";
            lblTitle.Text = ev.Title;
            txtDescription.Text = ev.Description;
            lblEduNote.Text = ev.EducationalNote;

            // Стилизация в зависимости от типа (для наглядности в дипломе)
            if (ev is RiskEvent)
            {
                lblTitle.ForeColor = Color.Firebrick;
                btnAction.Text = "Оплатить";
                btnCancel.Visible = false; // Нельзя отказаться от непредвиденного расхода
            }
            else if (ev is MarketEvent)
            {
                lblTitle.ForeColor = Color.RoyalBlue;
                btnAction.Text = "Инвестировать";
                btnCancel.Visible = true; // Добавьте кнопку "Пропустить" для рынка
            }
        }

        private void btnCancel_Click_1(object sender, EventArgs e)
        {
            IsConfirmed = false;
            this.DialogResult = DialogResult.Cancel; // И эту для кнопки отмены
            this.Close();
        }

        private void btnAction_Click_1(object sender, EventArgs e)
        {
            IsConfirmed = false;
            this.DialogResult = DialogResult.Cancel; // И эту для кнопки отмены
            this.Close();
        }
    }
}