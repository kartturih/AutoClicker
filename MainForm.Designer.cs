using System;
using System.Drawing;
using System.Windows.Forms;

namespace AutoClicker
{
    partial class MainForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        // GUI Controls
        private Button btnStartStop;
        private RadioButton rbLeftClick;
        private RadioButton rbRightClick;
        private CheckBox cbFixedPosition;
        private Button btnSetPosition;
        private Label lblPosition;
        private Label lblStatus;

        // New interval controls
        private GroupBox gbClickInterval;
        private NumericUpDown nudSeconds;
        private NumericUpDown nudMilliseconds;
        private Label lblSeconds;
        private Label lblMilliseconds;
        private CheckBox cbRandomInterval;
        private NumericUpDown nudRandomPercent;
        private Label lblRandomPercent;

        // Macro controls
        private GroupBox gbMacro;
        private Button btnRecordMacro;
        private Button btnPlayMacro;
        private Button btnClearMacro;
        private ListBox lbMacroActions;
        private Label lblMacroStatus;

        // Debug controls
        private TextBox txtDebug;

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
            this.Text = "AutoClicker Pro";
            this.Size = new Size(520, 700);
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(240, 240, 240);

            // Start/Stop button
            btnStartStop = new Button
            {
                Text = "Käynnistä (F1)",
                Size = new Size(140, 45),
                Location = new Point(20, 20),
                BackColor = Color.LightGreen,
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };
            btnStartStop.Click += BtnStartStop_Click;

            // Click Interval GroupBox
            gbClickInterval = new GroupBox
            {
                Text = "Klikkausväli",
                Location = new Point(20, 80),
                Size = new Size(240, 120),
                Font = new Font("Segoe UI", 9, FontStyle.Bold)
            };

            // Seconds input
            Label lblSecondsLabel = new Label
            {
                Text = "Sekuntia:",
                Location = new Point(15, 25),
                Size = new Size(60, 20),
                Font = new Font("Segoe UI", 9)
            };

            nudSeconds = new NumericUpDown
            {
                Minimum = 0,
                Maximum = 999,
                Value = 0,
                Location = new Point(80, 23),
                Size = new Size(60, 25),
                Font = new Font("Segoe UI", 9)
            };
            nudSeconds.ValueChanged += UpdateInterval;

            lblSeconds = new Label
            {
                Text = "s",
                Location = new Point(145, 25),
                Size = new Size(15, 20),
                Font = new Font("Segoe UI", 9)
            };

            // Milliseconds input
            Label lblMillisecondsLabel = new Label
            {
                Text = "Millisekuntia:",
                Location = new Point(15, 55),
                Size = new Size(80, 20),
                Font = new Font("Segoe UI", 9)
            };

            nudMilliseconds = new NumericUpDown
            {
                Minimum = 1,
                Maximum = 999,
                Value = 100,
                Location = new Point(100, 53),
                Size = new Size(60, 25),
                Font = new Font("Segoe UI", 9)
            };
            nudMilliseconds.ValueChanged += UpdateInterval;

            lblMilliseconds = new Label
            {
                Text = "ms",
                Location = new Point(165, 55),
                Size = new Size(25, 20),
                Font = new Font("Segoe UI", 9)
            };

            // Random interval checkbox
            cbRandomInterval = new CheckBox
            {
                Text = "Satunnainen väli:",
                Location = new Point(15, 85),
                Size = new Size(110, 20),
                Font = new Font("Segoe UI", 9)
            };
            cbRandomInterval.CheckedChanged += (s, e) => useRandomInterval = cbRandomInterval.Checked;

            nudRandomPercent = new NumericUpDown
            {
                Minimum = 1,
                Maximum = 50,
                Value = 20,
                Location = new Point(130, 83),
                Size = new Size(50, 25),
                Font = new Font("Segoe UI", 9)
            };
            nudRandomPercent.ValueChanged += (s, e) => randomPercentage = (int)nudRandomPercent.Value;

            lblRandomPercent = new Label
            {
                Text = "±%",
                Location = new Point(185, 85),
                Size = new Size(25, 20),
                Font = new Font("Segoe UI", 9)
            };

            gbClickInterval.Controls.AddRange(new Control[]
            {
                lblSecondsLabel, nudSeconds, lblSeconds,
                lblMillisecondsLabel, nudMilliseconds, lblMilliseconds,
                cbRandomInterval, nudRandomPercent, lblRandomPercent
            });

            // Mouse button selection
            GroupBox gbMouseButton = new GroupBox
            {
                Text = "Hiiren painike",
                Location = new Point(280, 80),
                Size = new Size(150, 70),
                Font = new Font("Segoe UI", 9, FontStyle.Bold)
            };

            rbLeftClick = new RadioButton
            {
                Text = "Vasen",
                Location = new Point(15, 25),
                Size = new Size(60, 20),
                Checked = true,
                Font = new Font("Segoe UI", 9)
            };
            rbLeftClick.CheckedChanged += (s, e) =>
            {
                if (rbLeftClick.Checked) currentMouseButton = MouseButtons.Left;
            };

            rbRightClick = new RadioButton
            {
                Text = "Oikea",
                Location = new Point(80, 25),
                Size = new Size(60, 20),
                Font = new Font("Segoe UI", 9)
            };
            rbRightClick.CheckedChanged += (s, e) =>
            {
                if (rbRightClick.Checked) currentMouseButton = MouseButtons.Right;
            };

            gbMouseButton.Controls.AddRange(new Control[] { rbLeftClick, rbRightClick });

            // Fixed position settings
            cbFixedPosition = new CheckBox
            {
                Text = "Kiinteä sijainti",
                Location = new Point(20, 220),
                Size = new Size(120, 20),
                Font = new Font("Segoe UI", 9)
            };
            cbFixedPosition.CheckedChanged += (s, e) => useFixedPosition = cbFixedPosition.Checked;

            btnSetPosition = new Button
            {
                Text = "Aseta sijainti",
                Location = new Point(150, 218),
                Size = new Size(100, 25),
                Font = new Font("Segoe UI", 9)
            };
            btnSetPosition.Click += BtnSetPosition_Click;

            lblPosition = new Label
            {
                Text = $"Sijainti: {clickPosition.X}, {clickPosition.Y}",
                Location = new Point(20, 250),
                Size = new Size(200, 20),
                Font = new Font("Segoe UI", 9)
            };

            // Macro controls
            gbMacro = new GroupBox
            {
                Text = "Makro-tallennin",
                Location = new Point(20, 280),
                Size = new Size(470, 220),
                Font = new Font("Segoe UI", 9, FontStyle.Bold)
            };

            btnRecordMacro = new Button
            {
                Text = "Aloita tallennus (F2)",
                Location = new Point(15, 25),
                Size = new Size(140, 35),
                BackColor = Color.LightBlue,
                Font = new Font("Segoe UI", 9, FontStyle.Bold)
            };
            btnRecordMacro.Click += BtnRecordMacro_Click;

            btnPlayMacro = new Button
            {
                Text = "Toista makro (F3)",
                Location = new Point(165, 25),
                Size = new Size(140, 35),
                BackColor = Color.LightYellow,
                Enabled = false,
                Font = new Font("Segoe UI", 9, FontStyle.Bold)
            };
            btnPlayMacro.Click += BtnPlayMacro_Click;

            btnClearMacro = new Button
            {
                Text = "Tyhjennä",
                Location = new Point(315, 25),
                Size = new Size(80, 35),
                BackColor = Color.LightCoral,
                Font = new Font("Segoe UI", 9, FontStyle.Bold)
            };
            btnClearMacro.Click += BtnClearMacro_Click;

            lblMacroStatus = new Label
            {
                Text = "Valmis tallentamaan",
                Location = new Point(15, 70),
                Size = new Size(400, 20),
                ForeColor = Color.Blue,
                Font = new Font("Segoe UI", 9)
            };

            lbMacroActions = new ListBox
            {
                Location = new Point(15, 95),
                Size = new Size(440, 115),
                Font = new Font("Consolas", 8)
            };

            gbMacro.Controls.AddRange(new Control[]
            {
                btnRecordMacro, btnPlayMacro, btnClearMacro,
                lblMacroStatus, lbMacroActions
            });

            // Debug window
            txtDebug = new TextBox
            {
                Location = new Point(20, 510),
                Size = new Size(470, 100),
                Multiline = true,
                ScrollBars = ScrollBars.Vertical,
                ReadOnly = true,
                BackColor = Color.Black,
                ForeColor = Color.Lime,
                Font = new Font("Consolas", 8)
            };

            // Status label
            lblStatus = new Label
            {
                Text = "Pysäytetty - Paina F1 käynnistääksesi",
                Location = new Point(20, 620),
                Size = new Size(400, 25),
                ForeColor = Color.Red,
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };

            // Add all controls to form
            this.Controls.AddRange(new Control[]
            {
                btnStartStop, gbClickInterval, gbMouseButton,
                cbFixedPosition, btnSetPosition, lblPosition,
                gbMacro, txtDebug, lblStatus
            });

            // Initialize interval calculation
            UpdateInterval(null, null);
        }

        #endregion
    }
}