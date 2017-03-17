using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        }

        private void Event_FormClosing(object Sender, FormClosingEventArgs Args)
        {
            Args.Cancel = true;
            this.Hide();
        }

        private void btnLoadProfile_Click(object sender, EventArgs e)
        {
            OpenFileDialog fileDialog = new OpenFileDialog();

            if (fileDialog.ShowDialog() == DialogResult.OK)
            {
                SimpleGrinder.CurrentProfile = Profile.ParseV1Profile(fileDialog.FileName);
            }
        }
    }
}
