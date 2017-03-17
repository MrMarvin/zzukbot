using System;
using System.Windows.Forms;

namespace GUI
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
            
            CheckBoxLoot.Checked = Settings.Looting;
            CheckBoxSkin.Checked = Settings.Skinning;
            TextBoxFoodPercentage.Text = Settings.EatAt.ToString();
            TextBoxDrinkPercentage.Text = Settings.DrinkAt.ToString();
            TextBoxMobSearchRadius.Text = Settings.SearchMobRange.ToString();
        }

        void Event_KeyPress(object Sender, KeyPressEventArgs Args)
        {
            if(Args.KeyChar == '\r')
            {
                if(Sender == ComboBoxFood || Sender == ComboBoxDrink || Sender == ComboBoxPetFood || Sender == ComboBoxProtectedItem)
                {

                }
            }
            else
            {
                Args.Handled = !char.IsDigit(Args.KeyChar);
            }
        }

        void Event_GUIHandler(object Sender, EventArgs Args)
        {
            if(Sender == CheckBoxLoot)
            {
                Settings.Looting = CheckBoxLoot.Checked;
            }
            else if(Sender == CheckBoxSkin)
            {
                Settings.Skinning = CheckBoxSkin.Checked;
            }
            else if(Sender == TextBoxFoodPercentage)
            {
                Settings.EatAt = int.Parse(TextBoxFoodPercentage.Text);
            }
            else if(Sender == TextBoxDrinkPercentage)
            {
                Settings.DrinkAt = int.Parse(TextBoxDrinkPercentage.Text);
            }
            else if(Sender == TextBoxMobSearchRadius)
            {
                Settings.SearchMobRange = int.Parse(TextBoxMobSearchRadius.Text);
            }
            else if(Sender == ButtonAddProtectedItem)
            {
                ComboBoxProtectedItem.Text = "";
            }
            else if(Sender == ButtonLoadProfile)
            {
                OpenFileDialog Dialog = new OpenFileDialog();
                Dialog.Filter = "V1 profile (*.xml)|*.xml";
                Dialog.Title = "Select a profile";

                if(Dialog.ShowDialog(this) == DialogResult.OK)
                {
                    Settings.ProfileFilePath = Dialog.FileName;
                    ProfileNameLabel.Text = System.IO.Path.GetFileName(Dialog.FileName);;
                }
            }
        }

        private void Event_FormClosing(object Sender, FormClosingEventArgs Args)
        {
            Args.Cancel = true;
            this.Hide();
        }
    }
}
