using System;
using System.Drawing;
using System.Windows.Forms;

namespace FinancialLiteracyGame
{
    public partial class EventForm : Form
    {
        private GameEvent _event;
        private Player _player;
        public bool IsConfirmed { get; private set; }

        public EventForm(GameEvent ev, Player player)
        {
            InitializeComponent();
            _event = ev;
            _player = player;
            IsConfirmed = false;

            ConfigureUI();
        }

        private void ConfigureUI()
        {
            this.Text = "Игровое событие";
            lblTitle.Text = _event.Title;
            txtDescription.Text = _event.Description;
            lblEduNote.Text = _event.EducationalNote;

            // Сброс состояния элементов управления
            btnAction.Enabled = true;
            btnAction.Visible = true;
            btnCancel.Visible = false;
            txtDescription.ForeColor = SystemColors.WindowText;

            // 1. Рыночные возможности (MarketEvent)
            if (_event is MarketEvent market)
            {
                lblTitle.ForeColor = Color.RoyalBlue;
                btnAction.Text = "Инвестировать";
                btnCancel.Visible = true;
                btnCancel.Text = "Пропустить";

                if (_player.Cash < market.Price)
                {
                    btnAction.Enabled = false;
                    txtDescription.ForeColor = Color.Red;
                    // Выводим информацию о нехватке средств и возможности взять кредит
                    txtDescription.Text += $"\n\n(Недостаточно средств! Нужно: {market.Price:N0} руб. " +
                                           "Вы можете закрыть окно, взять кредит на главной панели и вернуться.)";
                }
            }
            // 2. Благотворительность (CharityEvent)
            else if (_event is CharityEvent charity)
            {
                lblTitle.ForeColor = Color.MediumSeaGreen;
                btnAction.Text = "Пожертвовать";
                btnCancel.Visible = true;
                btnCancel.Text = "Отказаться";

                if (_player.Cash < Math.Abs(charity.BalanceEffect))
                {
                    btnAction.Enabled = false;
                    txtDescription.ForeColor = Color.Red;
                    txtDescription.Text += "\n\n(У вас недостаточно средств для пожертвования)";
                }
            }
            // 3. Обязательные события (Расходы, Инфляция, Риски)
            else if (_event is RiskEvent || _event is ExpenseEvent || _event is InflationEvent)
            {
                lblTitle.ForeColor = Color.Firebrick;
                btnAction.Text = "Принять";
                btnCancel.Visible = false; // От этих событий нельзя отказаться по правилам игры
            }
            // 4. Прочие события
            else
            {
                btnAction.Text = "ОК";
                btnCancel.Visible = false;
            }
        }

        private void btnAction_Click_1(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void btnCancel_Click_1(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
}