﻿namespace TurkishDraughts
{
    partial class GameBoardVsRobot
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
            player1TextBox = new TextBox();
            player2TextBox = new TextBox();
            currentPlayerTextBox = new TextBox();
            SuspendLayout();
            // 
            // player1TextBox
            // 
            player1TextBox.BackColor = Color.DarkGoldenrod;
            player1TextBox.BorderStyle = BorderStyle.None;
            player1TextBox.Font = new Font("Segoe UI", 23F, FontStyle.Regular, GraphicsUnit.Point);
            player1TextBox.ForeColor = Color.Black;
            player1TextBox.Location = new Point(21, 722);
            player1TextBox.Name = "player1TextBox";
            player1TextBox.ReadOnly = true;
            player1TextBox.Size = new Size(634, 41);
            player1TextBox.TabIndex = 1;
            player1TextBox.TabStop = false;
            player1TextBox.Text = "00";
            player1TextBox.TextAlign = HorizontalAlignment.Center;
            // 
            // player2TextBox
            // 
            player2TextBox.BackColor = Color.FromArgb(49, 46, 43);
            player2TextBox.BorderStyle = BorderStyle.None;
            player2TextBox.Font = new Font("Segoe UI", 23F, FontStyle.Regular, GraphicsUnit.Point);
            player2TextBox.ForeColor = Color.Black;
            player2TextBox.Location = new Point(21, 12);
            player2TextBox.Name = "player2TextBox";
            player2TextBox.ReadOnly = true;
            player2TextBox.Size = new Size(634, 41);
            player2TextBox.TabIndex = 2;
            player2TextBox.TabStop = false;
            player2TextBox.Text = "00";
            player2TextBox.TextAlign = HorizontalAlignment.Center;
            // 
            // currentPlayerTextBox
            // 
            currentPlayerTextBox.BackColor = Color.PeachPuff;
            currentPlayerTextBox.Font = new Font("Segoe UI", 15F, FontStyle.Regular, GraphicsUnit.Point);
            currentPlayerTextBox.Location = new Point(663, 344);
            currentPlayerTextBox.Multiline = true;
            currentPlayerTextBox.Name = "currentPlayerTextBox";
            currentPlayerTextBox.ReadOnly = true;
            currentPlayerTextBox.Size = new Size(105, 87);
            currentPlayerTextBox.TabIndex = 3;
            currentPlayerTextBox.TabStop = false;
            currentPlayerTextBox.Text = "Text";
            currentPlayerTextBox.TextAlign = HorizontalAlignment.Center;
            // 
            // GameBoardVsRobot
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackgroundImage = Properties.Resources.Board;
            BackgroundImageLayout = ImageLayout.Stretch;
            ClientSize = new Size(780, 793);
            Controls.Add(currentPlayerTextBox);
            Controls.Add(player2TextBox);
            Controls.Add(player1TextBox);
            DoubleBuffered = true;
            Name = "GameBoardVsRobot";
            Text = "GameBoardVsRobot";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private TextBox player1TextBox;
        private TextBox player2TextBox;
        private TextBox currentPlayerTextBox;
    }
}