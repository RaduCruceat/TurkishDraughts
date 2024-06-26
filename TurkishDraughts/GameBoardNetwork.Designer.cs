﻿namespace TurkishDraughts
{
    partial class GameBoardNetwork
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
            if (disposing && (components != null))
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(GameBoardNetwork));
            serverStartButton = new Button();
            clientButton = new Button();
            clientIPTextBox = new TextBox();
            currentPlayerTextBox = new TextBox();
            player2TextBox = new TextBox();
            player1TextBox = new TextBox();
            SuspendLayout();
            // 
            // serverStartButton
            // 
            serverStartButton.BackColor = Color.FromArgb(181, 136, 99);
            serverStartButton.FlatStyle = FlatStyle.Flat;
            serverStartButton.Location = new Point(662, 79);
            serverStartButton.Name = "serverStartButton";
            serverStartButton.Size = new Size(109, 23);
            serverStartButton.TabIndex = 0;
            serverStartButton.TabStop = false;
            serverStartButton.Text = "Start Server";
            serverStartButton.UseVisualStyleBackColor = false;
            serverStartButton.Click += serverStartButton_Click;
            // 
            // clientButton
            // 
            clientButton.BackColor = Color.FromArgb(181, 136, 99);
            clientButton.FlatStyle = FlatStyle.Flat;
            clientButton.Location = new Point(662, 106);
            clientButton.Name = "clientButton";
            clientButton.Size = new Size(109, 23);
            clientButton.TabIndex = 1;
            clientButton.TabStop = false;
            clientButton.Text = "Connect to Host";
            clientButton.UseVisualStyleBackColor = false;
            clientButton.Click += clientButton_Click;
            // 
            // clientIPTextBox
            // 
            clientIPTextBox.BackColor = Color.FromArgb(241, 217, 181);
            clientIPTextBox.Location = new Point(662, 134);
            clientIPTextBox.MaxLength = 15;
            clientIPTextBox.Name = "clientIPTextBox";
            clientIPTextBox.Size = new Size(109, 23);
            clientIPTextBox.TabIndex = 2;
            clientIPTextBox.TabStop = false;
            clientIPTextBox.Text = "127.0.0.1";
            clientIPTextBox.TextAlign = HorizontalAlignment.Center;
            // 
            // currentPlayerTextBox
            // 
            currentPlayerTextBox.BackColor = Color.FromArgb(241, 217, 181);
            currentPlayerTextBox.BorderStyle = BorderStyle.None;
            currentPlayerTextBox.Font = new Font("Segoe UI", 16F, FontStyle.Regular, GraphicsUnit.Point);
            currentPlayerTextBox.Location = new Point(668, 350);
            currentPlayerTextBox.Multiline = true;
            currentPlayerTextBox.Name = "currentPlayerTextBox";
            currentPlayerTextBox.ReadOnly = true;
            currentPlayerTextBox.Size = new Size(100, 75);
            currentPlayerTextBox.TabIndex = 3;
            currentPlayerTextBox.TabStop = false;
            currentPlayerTextBox.TextAlign = HorizontalAlignment.Center;
            // 
            // player2TextBox
            // 
            player2TextBox.BackColor = Color.FromArgb(49, 46, 43);
            player2TextBox.BorderStyle = BorderStyle.None;
            player2TextBox.Font = new Font("Segoe UI", 23F, FontStyle.Regular, GraphicsUnit.Point);
            player2TextBox.Location = new Point(22, 16);
            player2TextBox.Name = "player2TextBox";
            player2TextBox.ReadOnly = true;
            player2TextBox.Size = new Size(635, 41);
            player2TextBox.TabIndex = 4;
            player2TextBox.TabStop = false;
            player2TextBox.Text = "Negru";
            player2TextBox.TextAlign = HorizontalAlignment.Center;
            player2TextBox.TextChanged += player2TextBox_TextChanged;
            // 
            // player1TextBox
            // 
            player1TextBox.BackColor = Color.FromArgb(181, 136, 99);
            player1TextBox.BorderStyle = BorderStyle.None;
            player1TextBox.Font = new Font("Segoe UI", 23F, FontStyle.Regular, GraphicsUnit.Point);
            player1TextBox.Location = new Point(22, 716);
            player1TextBox.Name = "player1TextBox";
            player1TextBox.ReadOnly = true;
            player1TextBox.Size = new Size(635, 41);
            player1TextBox.TabIndex = 5;
            player1TextBox.TabStop = false;
            player1TextBox.Text = "Rosu";
            player1TextBox.TextAlign = HorizontalAlignment.Center;
            player1TextBox.TextChanged += player1TextBox_TextChanged;
            // 
            // GameBoardNetwork
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackgroundImage = Properties.Resources.Board;
            BackgroundImageLayout = ImageLayout.Stretch;
            ClientSize = new Size(780, 793);
            Controls.Add(player1TextBox);
            Controls.Add(player2TextBox);
            Controls.Add(currentPlayerTextBox);
            Controls.Add(clientIPTextBox);
            Controls.Add(clientButton);
            Controls.Add(serverStartButton);
            DoubleBuffered = true;
            Icon = (Icon)resources.GetObject("$this.Icon");
            MaximumSize = new Size(796, 832);
            MinimumSize = new Size(796, 832);
            Name = "GameBoardNetwork";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Turkish Draughts";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Button serverStartButton;
        private Button clientButton;
        private TextBox clientIPTextBox;
        private TextBox currentPlayerTextBox;
        private TextBox player2TextBox;
        private TextBox player1TextBox;
    }
}