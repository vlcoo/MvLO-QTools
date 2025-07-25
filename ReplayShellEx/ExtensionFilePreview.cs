﻿using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using ReplayFile;
using SharpShell.Attributes;
using SharpShell.SharpPreviewHandler;
using Label = System.Windows.Forms.Label;

namespace ReplayShellEx;

[ComVisible(true)]
[DisplayName("MvLO Replay Preview Handler")]
[COMServerAssociation(AssociationType.ClassOfExtension, ".mvlreplay")]
[PreviewHandler(DisableLowILProcessIsolation = false)]
public class ExtensionFilePreview : SharpPreviewHandler
{
    protected override PreviewHandlerControl DoPreview()
    {
        var handler = new ExtensionFilePreviewControl();
        if (!string.IsNullOrEmpty(SelectedFilePath))
        {
            handler.DoPreview(SelectedFilePath);
        }

        return handler;
    }
    
    public class ExtensionFilePreviewControl: PreviewHandlerControl
    {
        public void DoPreview(string filePath)
        {
            var replay = new BinaryReplayFile(File.OpenRead(filePath));
            
            Controls.Add(new Label
            {
                Dock = DockStyle.Bottom,
                Text = $"Generated by MvLO-QTools v{BinaryReplayFile.SolutionVersion}",
                Font = new Font(DefaultFont.FontFamily, DefaultFont.Size * 0.8f, FontStyle.Italic),
                ForeColor = SystemColors.GrayText,
            });
            
            if (!replay.Valid)
            {
                Controls.Add(new Label
                {
                    Dock = DockStyle.Fill,
                    TextAlign = ContentAlignment.MiddleCenter,
                    Text = "This file can't be previewed because it's incompatible or corrupted.",
                    ForeColor = SystemColors.ControlText
                });
                return;
            }
            
            // Auto-generated by WinForms Designer
            var tableRoot = new TableLayoutPanel();
            var panelHeader = new Panel();
            var pictureMap = new PictureBox();
            var labelTitle = new Label();
            var tableProperties = new TableLayoutPanel();
            var labelTitleDate = new Label();
            var labelDate = new Label();
            var labelTitleStage = new Label();
            var labelStage = new Label();
            var labelTitleCoins = new Label();
            var labelCoins = new Label();
            var labelTitleStars = new Label();
            var labelStars = new Label();
            var labelTitleLives = new Label();
            var labelLives = new Label();
            var labelTitleDuration = new Label();
            var labelDuration = new Label();
            var labelHeaderParticipants = new Label();
            var panelParticipants = new Panel();
            tableRoot.SuspendLayout();
            panelHeader.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pictureMap).BeginInit();
            tableProperties.SuspendLayout();
            panelParticipants.SuspendLayout();
            SuspendLayout();
            // 
            // tableRoot
            // 
            tableRoot.ColumnCount = 1;
            tableRoot.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tableRoot.Controls.Add(panelHeader, 0, 0);
            tableRoot.Controls.Add(tableProperties, 0, 1);
            tableRoot.Controls.Add(labelHeaderParticipants, 0, 2);
            tableRoot.Controls.Add(panelParticipants, 0, 3);
            tableRoot.Dock = DockStyle.Fill;
            tableRoot.Location = new Point(0, 0);
            tableRoot.Name = "tableRoot";
            tableRoot.Padding = new Padding(8);
            tableRoot.RowCount = 4;
            tableRoot.RowStyles.Add(new RowStyle());
            tableRoot.RowStyles.Add(new RowStyle());
            tableRoot.RowStyles.Add(new RowStyle());
            tableRoot.RowStyles.Add(new RowStyle());
            tableRoot.Size = new Size(582, 635);
            tableRoot.TabIndex = 0;
            // 
            // panelHeader
            // 
            panelHeader.Controls.Add(pictureMap);
            panelHeader.Controls.Add(labelTitle);
            panelHeader.Dock = DockStyle.Top;
            panelHeader.Location = new Point(11, 11);
            panelHeader.Name = "panelHeader";
            panelHeader.Size = new Size(560, 32);
            panelHeader.TabIndex = 0;
            // 
            // pictureMap
            // 
            pictureMap.Dock = DockStyle.Right;
            pictureMap.Location = new Point(528, 0);
            pictureMap.Name = "pictureMap";
            pictureMap.Size = new Size(32, 32);
            pictureMap.SizeMode = PictureBoxSizeMode.Zoom;
            pictureMap.TabIndex = 1;
            pictureMap.TabStop = false;
            pictureMap.Image = StageIconGetter.GetIconBitmap(replay.Rules.StageName);
            // 
            // labelTitle
            // 
            labelTitle.Dock = DockStyle.Left;
            labelTitle.Font = new Font(DefaultFont.FontFamily, DefaultFont.Size * 1.5f, FontStyle.Bold);
            labelTitle.Location = new Point(0, 0);
            labelTitle.Name = "labelTitle";
            labelTitle.Size = new Size(300, 32);
            labelTitle.TabIndex = 0;
            labelTitle.Text = "MvLO Match Replay";
            labelTitle.ForeColor = SystemColors.ControlText;
            // 
            // tableProperties
            // 
            tableProperties.AutoSize = true;
            tableProperties.ColumnCount = 2;
            tableProperties.ColumnStyles.Add(new ColumnStyle());
            tableProperties.ColumnStyles.Add(new ColumnStyle());
            tableProperties.Controls.Add(labelTitleDate, 0, 0);
            tableProperties.Controls.Add(labelDate, 1, 0);
            tableProperties.Controls.Add(labelTitleStage, 0, 2);
            tableProperties.Controls.Add(labelStage, 1, 2);
            tableProperties.Controls.Add(labelTitleCoins, 0, 4);
            tableProperties.Controls.Add(labelCoins, 1, 4);
            tableProperties.Controls.Add(labelTitleStars, 0, 3);
            tableProperties.Controls.Add(labelStars, 1, 3);
            tableProperties.Controls.Add(labelTitleLives, 0, 5);
            tableProperties.Controls.Add(labelLives, 1, 5);
            tableProperties.Controls.Add(labelTitleDuration, 0, 1);
            tableProperties.Controls.Add(labelDuration, 1, 1);
            tableProperties.Dock = DockStyle.Top;
            tableProperties.Location = new Point(11, 49);
            tableProperties.Name = "tableProperties";
            tableProperties.RowCount = 6;
            tableProperties.RowStyles.Add(new RowStyle(SizeType.Absolute, 24F));
            tableProperties.RowStyles.Add(new RowStyle(SizeType.Absolute, 24F));
            tableProperties.RowStyles.Add(new RowStyle(SizeType.Absolute, 24F));
            tableProperties.RowStyles.Add(new RowStyle(SizeType.Absolute, 24F));
            tableProperties.RowStyles.Add(new RowStyle(SizeType.Absolute, 24F));
            tableProperties.RowStyles.Add(new RowStyle(SizeType.Absolute, 24F));
            tableProperties.Size = new Size(560, 120);
            tableProperties.TabIndex = 1;
            // 
            // labelTitleDate
            // 
            labelTitleDate.AutoSize = true;
            labelTitleDate.Font = new Font(DefaultFont.FontFamily, DefaultFont.Size, FontStyle.Bold);
            labelTitleDate.Location = new Point(3, 0);
            labelTitleDate.Name = "labelTitleDate";
            labelTitleDate.Size = new Size(42, 20);
            labelTitleDate.TabIndex = 6;
            labelTitleDate.Text = "Date";
            labelTitleDate.ForeColor = SystemColors.ControlText;
            // 
            // labelDate
            // 
            labelDate.AutoSize = true;
            labelDate.Location = new Point(97, 0);
            labelDate.Name = "labelDate";
            labelDate.Size = new Size(120, 20);
            labelDate.TabIndex = 7;
            labelDate.Text = replay.ReplayDate;
            labelDate.ForeColor = SystemColors.ControlText;
            // 
            // labelTitleStage
            // 
            labelTitleStage.AutoSize = true;
            labelTitleStage.Font = new Font(DefaultFont.FontFamily, DefaultFont.Size, FontStyle.Bold);
            labelTitleStage.Location = new Point(3, 40);
            labelTitleStage.Name = "labelTitleStage";
            labelTitleStage.Size = new Size(50, 20);
            labelTitleStage.TabIndex = 0;
            labelTitleStage.Text = "Stage";
            labelTitleStage.ForeColor = SystemColors.ControlText;
            // 
            // labelStage
            // 
            labelStage.AutoSize = true;
            labelStage.Location = new Point(97, 40);
            labelStage.Name = "labelStage";
            labelStage.Size = new Size(33, 20);
            labelStage.TabIndex = 1;
            labelStage.Text = replay.Rules.StageName;
            labelStage.ForeColor = SystemColors.ControlText;
            // 
            // labelTitleCoins
            // 
            labelTitleCoins.AutoSize = true;
            labelTitleCoins.Font = new Font(DefaultFont.FontFamily, DefaultFont.Size, FontStyle.Bold);
            labelTitleCoins.Location = new Point(3, 80);
            labelTitleCoins.Name = "labelTitleCoins";
            labelTitleCoins.Size = new Size(88, 20);
            labelTitleCoins.TabIndex = 2;
            labelTitleCoins.Text = "Coins/p.up";
            labelTitleCoins.ForeColor = SystemColors.ControlText;
            // 
            // labelCoins
            // 
            labelCoins.AutoSize = true;
            labelCoins.Location = new Point(97, 80);
            labelCoins.Name = "labelCoins";
            labelCoins.Size = new Size(17, 20);
            labelCoins.TabIndex = 3;
            labelCoins.Text = GameRules.PropertyToString(replay.Rules.CoinsForPowerup);
            labelCoins.ForeColor = SystemColors.ControlText;
            // 
            // labelTitleStars
            // 
            labelTitleStars.AutoSize = true;
            labelTitleStars.Font = new Font(DefaultFont.FontFamily, DefaultFont.Size, FontStyle.Bold);
            labelTitleStars.Location = new Point(3, 60);
            labelTitleStars.Name = "labelTitleStars";
            labelTitleStars.Size = new Size(45, 20);
            labelTitleStars.TabIndex = 4;
            labelTitleStars.Text = "Stars";
            labelTitleStars.ForeColor = SystemColors.ControlText;
            // 
            // labelStars
            // 
            labelStars.AutoSize = true;
            labelStars.Location = new Point(97, 60);
            labelStars.Name = "labelStars";
            labelStars.Size = new Size(21, 20);
            labelStars.TabIndex = 5;
            labelStars.Text = GameRules.PropertyToString(replay.Rules.StarsToWin);
            labelStars.ForeColor = SystemColors.ControlText;
            // 
            // labelTitleLives
            // 
            labelTitleLives.AutoSize = true;
            labelTitleLives.Font = new Font(DefaultFont.FontFamily, DefaultFont.Size, FontStyle.Bold);
            labelTitleLives.Location = new Point(3, 100);
            labelTitleLives.Name = "labelTitleLives";
            labelTitleLives.Size = new Size(44, 20);
            labelTitleLives.TabIndex = 8;
            labelTitleLives.Text = "Lives";
            labelTitleLives.ForeColor = SystemColors.ControlText;
            // 
            // labelLives
            // 
            labelLives.AutoSize = true;
            labelLives.Location = new Point(97, 100);
            labelLives.Name = "labelLives";
            labelLives.Size = new Size(17, 20);
            labelLives.TabIndex = 9;
            labelLives.Text = GameRules.PropertyToString(replay.Rules.Lives);
            labelLives.ForeColor = SystemColors.ControlText;
            // 
            // labelTitleDuration
            // 
            labelTitleDuration.AutoSize = true;
            labelTitleDuration.Font = new Font(DefaultFont.FontFamily, DefaultFont.Size, FontStyle.Bold);
            labelTitleDuration.Location = new Point(3, 20);
            labelTitleDuration.Name = "labelTitleDuration";
            labelTitleDuration.Size = new Size(71, 20);
            labelTitleDuration.TabIndex = 10;
            labelTitleDuration.Text = "Duration";
            labelTitleDuration.ForeColor = SystemColors.ControlText;
            // 
            // labelDuration
            // 
            labelDuration.AutoSize = true;
            labelDuration.Location = new Point(97, 20);
            labelDuration.Name = "labelDuration";
            labelDuration.Size = new Size(57, 20);
            labelDuration.TabIndex = 11;
            labelDuration.Text = replay.ReplayDuration;
            labelDuration.ForeColor = SystemColors.ControlText;
            // 
            // labelHeaderParticipants
            // 
            labelHeaderParticipants.AutoSize = true;
            labelHeaderParticipants.Dock = DockStyle.Top;
            labelHeaderParticipants.Font = new Font(DefaultFont.FontFamily, DefaultFont.Size, FontStyle.Bold);
            labelHeaderParticipants.Location = new Point(8, 184);
            labelHeaderParticipants.Margin = new Padding(0, 12, 0, 0);
            labelHeaderParticipants.Name = "labelHeaderParticipants";
            labelHeaderParticipants.Size = new Size(566, 20);
            labelHeaderParticipants.TabIndex = 2;
            labelHeaderParticipants.Text = "Participants";
            labelHeaderParticipants.ForeColor = SystemColors.ControlText;
            // 
            // panelParticipants
            // 
            panelParticipants.AutoScroll = true;
            panelParticipants.AutoSize = true;
            panelParticipants.Dock = DockStyle.Fill;
            panelParticipants.Location = new Point(11, 211);
            panelParticipants.Name = "panelParticipants";
            panelParticipants.Padding = new Padding(4);
            panelParticipants.Size = new Size(560, 40);
            panelParticipants.TabIndex = 3;

            foreach (var player in replay.Players.Reverse())
            {
                var hasWon = false;
                if (replay.Rules.IsTeamsEnabled) hasWon = player.Team == replay.WinningTeam;
                else if (replay.WinningPlayer != null) hasWon = player == replay.WinningPlayer;
                var nameColor = SystemColors.ControlText;
                if (replay.Rules.IsTeamsEnabled)
                    nameColor = player.Team switch
                    {
                        0 => Color.Firebrick,
                        1 => Color.ForestGreen,
                        2 => Color.SteelBlue,
                        3 => Color.DarkGoldenrod,
                        4 => Color.MediumOrchid,
                        _ => SystemColors.ControlText
                    };
                var tableParticipant = new TableLayoutPanel();
                var labelCharacter = new Label();
                var labelUsername = new Label();
                var labelStarCount = new Label();
                var labelWinner = new Label();
                tableParticipant.SuspendLayout();
                panelParticipants.Controls.Add(tableParticipant);
                // 
                // tableParticipant
                // 
                tableParticipant.ColumnCount = 4;
                tableParticipant.ColumnStyles.Add(new ColumnStyle());
                tableParticipant.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
                tableParticipant.ColumnStyles.Add(new ColumnStyle());
                tableParticipant.ColumnStyles.Add(new ColumnStyle());
                tableParticipant.Controls.Add(labelCharacter, 0, 0);
                tableParticipant.Controls.Add(labelUsername, 1, 0);
                tableParticipant.Controls.Add(labelStarCount, 2, 0);
                tableParticipant.Controls.Add(labelWinner, 3, 0);
                tableParticipant.Dock = DockStyle.Top;
                tableParticipant.Location = new Point(4, 4);
                tableParticipant.Name = "tableParticipant";
                tableParticipant.RowCount = 1;
                tableParticipant.RowStyles.Add(new RowStyle());
                tableParticipant.Size = new Size(552, 32);
                tableParticipant.TabIndex = 1;
                // 
                // labelCharacter
                // 
                labelCharacter.AutoSize = true;
                labelCharacter.Dock = DockStyle.Left;
                labelCharacter.ForeColor = player.Character switch
                {
                    0 => Color.Firebrick,
                    1 => Color.ForestGreen,
                    _ => SystemColors.ControlText
                };
                labelCharacter.Location = new Point(3, 0);
                labelCharacter.Name = "labelCharacter";
                labelCharacter.MinimumSize = new Size(20, 32);
                labelCharacter.TabIndex = 3;
                labelCharacter.Text = player.Character switch
                {
                    0 => "M",
                    1 => "L",
                    _ => "-"
                };
                labelCharacter.TextAlign = ContentAlignment.MiddleCenter;
                // 
                // labelUsername
                // 
                labelUsername.AutoSize = true;
                labelUsername.Dock = DockStyle.Fill;
                labelUsername.ForeColor = nameColor;
                labelUsername.Location = new Point(25, 0);
                labelUsername.Name = "labelUsername";
                labelUsername.Size = new Size(459, 32);
                labelUsername.TabIndex = 2;
                labelUsername.Text = player.Username;
                labelUsername.TextAlign = ContentAlignment.MiddleLeft;
                // 
                // labelStarCount
                // 
                labelStarCount.AutoSize = true;
                labelStarCount.Dock = DockStyle.Right;
                labelStarCount.Location = new Point(490, 0);
                labelStarCount.Name = "labelStarCount";
                labelStarCount.MinimumSize = new Size(30, 32);
                labelStarCount.TabIndex = 1;
                labelStarCount.Text = $"{player.FinalObjectiveCount} ☆";
                labelStarCount.TextAlign = ContentAlignment.MiddleCenter;
                labelStarCount.ForeColor = SystemColors.ControlText;
                // 
                // labelWinner
                // 
                labelWinner.AutoSize = true;
                labelWinner.Dock = DockStyle.Right;
                labelWinner.Location = new Point(526, 0);
                labelWinner.Name = "labelWinner";
                labelWinner.MinimumSize = new Size(24, 32);
                labelWinner.TabIndex = 0;
                labelWinner.Text = hasWon ? "W" : " ";
                labelWinner.TextAlign = ContentAlignment.MiddleCenter;
                labelWinner.ForeColor = Color.Gold;
                labelWinner.Font = new Font(DefaultFont.FontFamily, DefaultFont.Size, FontStyle.Bold);
                tableParticipant.ResumeLayout(false);
                tableParticipant.PerformLayout();
            }
            
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(9F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = SystemColors.Window;
            Controls.Add(tableRoot);
            MaximumSize = new Size(600, 4000);
            Name = "MvLO-QTools (ReplayShellEx)";
            Text = Name;
            tableRoot.ResumeLayout(false);
            tableRoot.PerformLayout();
            panelHeader.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)pictureMap).EndInit();
            tableProperties.ResumeLayout(false);
            tableProperties.PerformLayout();
            panelParticipants.ResumeLayout(false);
            ResumeLayout(false);
        }
    }
}