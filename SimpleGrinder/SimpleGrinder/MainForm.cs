using System;
using System.Windows.Forms;

namespace GUI
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
            
            CheckBoxLoot.Checked = Settings.Instance.Looting;
            CheckBoxSkin.Checked = Settings.Instance.Skinning;
            TextBoxFoodPercentage.Text = Settings.Instance.EatAt.ToString();
            TextBoxDrinkPercentage.Text = Settings.Instance.DrinkAt.ToString();
            TextBoxMobSearchRadius.Text = Settings.Instance.SearchMobRange.ToString();
            
            if(Settings.Instance.ProfileFilePath != null)
            {
                ProfileNameLabel.Text = System.IO.Path.GetFileName(Settings.Instance.ProfileFilePath);
            }
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
                Settings.Instance.Looting = CheckBoxLoot.Checked;
                Settings.SaveSettings();
            }
            else if(Sender == CheckBoxSkin)
            {
                Settings.Instance.Skinning = CheckBoxSkin.Checked;
                Settings.SaveSettings();
            }
            else if(Sender == TextBoxFoodPercentage)
            {
                Settings.Instance.EatAt = int.Parse(TextBoxFoodPercentage.Text);
                Settings.SaveSettings();
            }
            else if(Sender == TextBoxDrinkPercentage)
            {
                Settings.Instance.DrinkAt = int.Parse(TextBoxDrinkPercentage.Text);
                Settings.SaveSettings();
            }
            else if(Sender == TextBoxMobSearchRadius)
            {
                Settings.Instance.SearchMobRange = int.Parse(TextBoxMobSearchRadius.Text);
                Settings.SaveSettings();
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
                    Settings.Instance.ProfileFilePath = Dialog.FileName;
                    ProfileNameLabel.Text = System.IO.Path.GetFileName(Dialog.FileName);
                    Settings.SaveSettings();
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
