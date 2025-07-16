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

        // Interval controls
        private GroupBox gbClickInterval;
        private NumericUpDown nudSeconds;
        private NumericUpDown nudMilliseconds;
        private Label lblSeconds;
        private Label lblMilliseconds;

        // Random interval controls
        private GroupBox gbRandomInterval;
        private CheckBox cbRandomInterval;
        private NumericUpDown nudRandomMin;
        private NumericUpDown nudRandomMax;
        private Label lblRandomMinUnit;
        private Label lblRandomMaxUnit;

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
            this.Size = new Size(580, 720);
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

            // Fixed Click Interval GroupBox
            gbClickInterval = new GroupBox
            {
                Text = "Kiinteä klikkausväli",
                Location = new Point(20, 80),
                Size = new Size(240, 100),
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

            gbClickInterval.Controls.AddRange(new Control[]
            {
                lblSecondsLabel, nudSeconds, lblSeconds,
                lblMillisecondsLabel, nudMilliseconds, lblMilliseconds
            });

            // Random Click Interval GroupBox
            gbRandomInterval = new GroupBox
            {
                Text = "Random Click Interval",
                Location = new Point(280, 80),
                Size = new Size(270, 100),
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                BackColor = Color.FromArgb(240, 240, 240),
                ForeColor = Color.Black
            };

            cbRandomInterval = new CheckBox
            {
                Text = "Käytä satunnaista väliä",
                Location = new Point(15, 25),
                Size = new Size(150, 20),
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.Black
            };
            cbRandomInterval.CheckedChanged += (s, e) => 
            {
                useRandomInterval = cbRandomInterval.Checked;
                nudRandomMin.Enabled = useRandomInterval;
                nudRandomMax.Enabled = useRandomInterval;
                
                // Update interval mode
                UpdateIntervalMode();
            };

            nudRandomMin = new NumericUpDown
            {
                Minimum = 0,
                Maximum = 9999,
                Value = 5,
                DecimalPlaces = 1,
                Increment = 0.1m,
                Location = new Point(15, 55),
                Size = new Size(70, 25),
                Font = new Font("Segoe UI", 9),
                Enabled = false
            };
            nudRandomMin.ValueChanged += UpdateRandomInterval;

            lblRandomMinUnit = new Label
            {
                Text = "Secs.",
                Location = new Point(90, 57),
                Size = new Size(35, 20),
                Font = new Font("Segoe UI", 8),
                ForeColor = Color.Black
            };

            nudRandomMax = new NumericUpDown
            {
                Minimum = 0,
                Maximum = 9999,
                Value = 10,
                DecimalPlaces = 1,
                Increment = 0.1m,
                Location = new Point(140, 55),
                Size = new Size(70, 25),
                Font = new Font("Segoe UI", 9),
                Enabled = false
            };
            nudRandomMax.ValueChanged += UpdateRandomInterval;

            lblRandomMaxUnit = new Label
            {
                Text = "Secs.",
                Location = new Point(215, 57),
                Size = new Size(35, 20),
                Font = new Font("Segoe UI", 8),
                ForeColor = Color.Black
            };

            gbRandomInterval.Controls.AddRange(new Control[]
            {
                cbRandomInterval, nudRandomMin, lblRandomMinUnit, nudRandomMax, lblRandomMaxUnit
            });

            // Mouse button selection
            GroupBox gbMouseButton = new GroupBox
            {
                Text = "Hiiren painike",
                Location = new Point(20, 190),
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
                Location = new Point(20, 280),
                Size = new Size(120, 20),
                Font = new Font("Segoe UI", 9)
            };
            cbFixedPosition.CheckedChanged += (s, e) => useFixedPosition = cbFixedPosition.Checked;

            btnSetPosition = new Button
            {
                Text = "Aseta sijainti",
                Location = new Point(150, 278),
                Size = new Size(100, 25),
                Font = new Font("Segoe UI", 9)
            };
            btnSetPosition.Click += BtnSetPosition_Click;

            lblPosition = new Label
            {
                Text = $"Sijainti: {clickPosition.X}, {clickPosition.Y}",
                Location = new Point(20, 310),
                Size = new Size(200, 20),
                Font = new Font("Segoe UI", 9)
            };

            // Macro controls
            gbMacro = new GroupBox
            {
                Text = "Makro-tallennin",
                Location = new Point(20, 340),
                Size = new Size(530, 220),
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
                Size = new Size(500, 115),
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
                Location = new Point(20, 570),
                Size = new Size(530, 80),
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
                Location = new Point(20, 660),
                Size = new Size(500, 25),
                ForeColor = Color.Red,
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };

            // Add all controls to form
            this.Controls.AddRange(new Control[]
            {
                btnStartStop, gbClickInterval, gbRandomInterval, gbMouseButton,
                cbFixedPosition, btnSetPosition, lblPosition,
                gbMacro, txtDebug, lblStatus
            });

            // Initialize interval calculation
            UpdateInterval(null, null);
        }

        #endregion
    }
}