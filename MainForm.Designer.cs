namespace GUI
{
    partial class MainForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if(disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.CheckBoxLoot = new System.Windows.Forms.CheckBox();
            this.CheckBoxSkin = new System.Windows.Forms.CheckBox();
            this.ComboBoxProtectedItem = new System.Windows.Forms.ComboBox();
            this.ListBoxProtectedItems = new System.Windows.Forms.ListBox();
            this.ButtonAddProtectedItem = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.ComboBoxFood = new System.Windows.Forms.ComboBox();
            this.ComboBoxDrink = new System.Windows.Forms.ComboBox();
            this.ComboBoxPetFood = new System.Windows.Forms.ComboBox();
            this.TextBoxFoodPercentage = new System.Windows.Forms.TextBox();
            this.TextBoxDrinkPercentage = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.TextBoxMobSearchRadius = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // CheckBoxLoot
            // 
            this.CheckBoxLoot.AutoSize = true;
            this.CheckBoxLoot.Location = new System.Drawing.Point(291, 11);
            this.CheckBoxLoot.Name = "CheckBoxLoot";
            this.CheckBoxLoot.Size = new System.Drawing.Size(47, 17);
            this.CheckBoxLoot.TabIndex = 0;
            this.CheckBoxLoot.Text = "Loot";
            this.CheckBoxLoot.UseVisualStyleBackColor = true;
            this.CheckBoxLoot.CheckedChanged += new System.EventHandler(this.Event_GUIHandler);
            // 
            // CheckBoxSkin
            // 
            this.CheckBoxSkin.AutoSize = true;
            this.CheckBoxSkin.Location = new System.Drawing.Point(291, 34);
            this.CheckBoxSkin.Name = "CheckBoxSkin";
            this.CheckBoxSkin.Size = new System.Drawing.Size(47, 17);
            this.CheckBoxSkin.TabIndex = 1;
            this.CheckBoxSkin.Text = "Skin";
            this.CheckBoxSkin.UseVisualStyleBackColor = true;
            this.CheckBoxSkin.CheckedChanged += new System.EventHandler(this.Event_GUIHandler);
            // 
            // ComboBoxProtectedItem
            // 
            this.ComboBoxProtectedItem.FormattingEnabled = true;
            this.ComboBoxProtectedItem.Location = new System.Drawing.Point(6, 30);
            this.ComboBoxProtectedItem.Name = "ComboBoxProtectedItem";
            this.ComboBoxProtectedItem.Size = new System.Drawing.Size(164, 21);
            this.ComboBoxProtectedItem.TabIndex = 2;
            // 
            // ListBoxProtectedItems
            // 
            this.ListBoxProtectedItems.FormattingEnabled = true;
            this.ListBoxProtectedItems.Location = new System.Drawing.Point(7, 57);
            this.ListBoxProtectedItems.Name = "ListBoxProtectedItems";
            this.ListBoxProtectedItems.ScrollAlwaysVisible = true;
            this.ListBoxProtectedItems.Size = new System.Drawing.Size(190, 147);
            this.ListBoxProtectedItems.TabIndex = 3;
            // 
            // ButtonAddProtectedItem
            // 
            this.ButtonAddProtectedItem.Location = new System.Drawing.Point(176, 31);
            this.ButtonAddProtectedItem.Name = "ButtonAddProtectedItem";
            this.ButtonAddProtectedItem.Size = new System.Drawing.Size(20, 20);
            this.ButtonAddProtectedItem.TabIndex = 4;
            this.ButtonAddProtectedItem.Text = "+";
            this.ButtonAddProtectedItem.UseVisualStyleBackColor = true;
            this.ButtonAddProtectedItem.Click += new System.EventHandler(this.Event_GUIHandler);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.ListBoxProtectedItems);
            this.groupBox1.Controls.Add(this.ComboBoxProtectedItem);
            this.groupBox1.Controls.Add(this.ButtonAddProtectedItem);
            this.groupBox1.Location = new System.Drawing.Point(5, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(217, 212);
            this.groupBox1.TabIndex = 5;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Protected items";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(254, 64);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(31, 13);
            this.label1.TabIndex = 6;
            this.label1.Text = "Food";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(253, 94);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(32, 13);
            this.label2.TabIndex = 7;
            this.label2.Text = "Drink";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(235, 133);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(50, 13);
            this.label3.TabIndex = 8;
            this.label3.Text = "Pet Food";
            // 
            // ComboBoxFood
            // 
            this.ComboBoxFood.FormattingEnabled = true;
            this.ComboBoxFood.Location = new System.Drawing.Point(291, 61);
            this.ComboBoxFood.Name = "ComboBoxFood";
            this.ComboBoxFood.Size = new System.Drawing.Size(164, 21);
            this.ComboBoxFood.TabIndex = 9;
            // 
            // ComboBoxDrink
            // 
            this.ComboBoxDrink.FormattingEnabled = true;
            this.ComboBoxDrink.Location = new System.Drawing.Point(291, 94);
            this.ComboBoxDrink.Name = "ComboBoxDrink";
            this.ComboBoxDrink.Size = new System.Drawing.Size(164, 21);
            this.ComboBoxDrink.TabIndex = 10;
            // 
            // ComboBoxPetFood
            // 
            this.ComboBoxPetFood.FormattingEnabled = true;
            this.ComboBoxPetFood.Location = new System.Drawing.Point(291, 130);
            this.ComboBoxPetFood.Name = "ComboBoxPetFood";
            this.ComboBoxPetFood.Size = new System.Drawing.Size(164, 21);
            this.ComboBoxPetFood.TabIndex = 11;
            // 
            // TextBoxFoodPercentage
            // 
            this.TextBoxFoodPercentage.Location = new System.Drawing.Point(461, 62);
            this.TextBoxFoodPercentage.Name = "TextBoxFoodPercentage";
            this.TextBoxFoodPercentage.Size = new System.Drawing.Size(34, 20);
            this.TextBoxFoodPercentage.TabIndex = 12;
            this.TextBoxFoodPercentage.TextChanged += new System.EventHandler(this.Event_GUIHandler);
            this.TextBoxFoodPercentage.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.Event_KeyPress);
            // 
            // TextBoxDrinkPercentage
            // 
            this.TextBoxDrinkPercentage.Location = new System.Drawing.Point(461, 94);
            this.TextBoxDrinkPercentage.Name = "TextBoxDrinkPercentage";
            this.TextBoxDrinkPercentage.Size = new System.Drawing.Size(34, 20);
            this.TextBoxDrinkPercentage.TabIndex = 13;
            this.TextBoxDrinkPercentage.TextChanged += new System.EventHandler(this.Event_GUIHandler);
            this.TextBoxDrinkPercentage.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.Event_KeyPress);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(495, 97);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(15, 13);
            this.label4.TabIndex = 14;
            this.label4.Text = "%";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(495, 65);
            this.label5.Margin = new System.Windows.Forms.Padding(0);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(15, 13);
            this.label5.TabIndex = 15;
            this.label5.Text = "%";
            // 
            // TextBoxMobSearchRadius
            // 
            this.TextBoxMobSearchRadius.Location = new System.Drawing.Point(342, 196);
            this.TextBoxMobSearchRadius.Name = "TextBoxMobSearchRadius";
            this.TextBoxMobSearchRadius.Size = new System.Drawing.Size(33, 20);
            this.TextBoxMobSearchRadius.TabIndex = 16;
            this.TextBoxMobSearchRadius.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.Event_KeyPress);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(235, 199);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(101, 13);
            this.label6.TabIndex = 17;
            this.label6.Text = "Mob Search Radius";
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(558, 261);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.TextBoxMobSearchRadius);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.TextBoxDrinkPercentage);
            this.Controls.Add(this.TextBoxFoodPercentage);
            this.Controls.Add(this.ComboBoxPetFood);
            this.Controls.Add(this.ComboBoxDrink);
            this.Controls.Add(this.ComboBoxFood);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.CheckBoxSkin);
            this.Controls.Add(this.CheckBoxLoot);
            this.Name = "MainForm";
            this.Text = "SimpleGrinder";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Event_FormClosing);
            this.groupBox1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox TextBoxDrinkPercentage;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;
        public System.Windows.Forms.Button ButtonAddProtectedItem;
        public System.Windows.Forms.ComboBox ComboBoxProtectedItem;
        public System.Windows.Forms.ListBox ListBoxProtectedItems;
        public System.Windows.Forms.CheckBox CheckBoxLoot;
        public System.Windows.Forms.CheckBox CheckBoxSkin;
        public System.Windows.Forms.ComboBox ComboBoxFood;
        public System.Windows.Forms.TextBox TextBoxFoodPercentage;
        public System.Windows.Forms.ComboBox ComboBoxDrink;
        public System.Windows.Forms.ComboBox ComboBoxPetFood;
        public System.Windows.Forms.TextBox TextBoxMobSearchRadius;
    }
}

