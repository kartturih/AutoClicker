using System;
using System.Threading.Tasks;
using System.Windows.Forms;
using AutoClicker.Services;
using AutoClicker.Utils;

namespace AutoClicker
{
    public partial class MainForm : Form
    {
        // Services
        private MacroService macroService;
        private ClickService clickService;

        // Private fields
        private System.Windows.Forms.Timer clickTimer;
        private bool isRunning = false;
        private int baseIntervalMs = 100; // Base interval in milliseconds
        private MouseButtons currentMouseButton = MouseButtons.Left;
        private POINT clickPosition;
        private bool useFixedPosition = false;
        
        // Random interval fields
        private bool useRandomInterval = false;
        private double randomMinSeconds = 5.0;
        private double randomMaxSeconds = 10.0;

        // Random delay
        private Random random = new Random();

        public MainForm()
        {
            InitializeComponent();
            InitializeServices();
            InitializeTimer();
            RegisterGlobalHotkeys();
            WinAPI.GetCursorPos(out clickPosition);
            
            // Update position label
            lblPosition.Text = $"Sijainti: {clickPosition.X}, {clickPosition.Y}";
        }

        private void InitializeServices()
        {
            macroService = new MacroService(this);
            
            // Subscribe to macro service events
            macroService.MacroActionAdded += OnMacroActionAdded;
            macroService.StatusChanged += OnMacroStatusChanged;
            macroService.PlaybackStateChanged += OnPlaybackStateChanged;
        }

        private void InitializeTimer()
        {
            clickTimer = new System.Windows.Forms.Timer();
            clickTimer.Tick += ClickTimer_Tick;
        }

        private void RegisterGlobalHotkeys()
        {
            WinAPI.RegisterHotKey(this.Handle, WinAPIConstants.HOTKEY_ID_START_STOP, WinAPIConstants.MOD_NONE, WinAPIConstants.VK_F1);
            WinAPI.RegisterHotKey(this.Handle, WinAPIConstants.HOTKEY_ID_RECORD, WinAPIConstants.MOD_NONE, WinAPIConstants.VK_F2);
            WinAPI.RegisterHotKey(this.Handle, WinAPIConstants.HOTKEY_ID_PLAY_MACRO, WinAPIConstants.MOD_NONE, WinAPIConstants.VK_F3);
        }

        #region Event Handlers

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == WinAPIConstants.WM_HOTKEY)
            {
                switch (m.WParam.ToInt32())
                {
                    case WinAPIConstants.HOTKEY_ID_START_STOP:
                        ToggleClicking();
                        break;
                    case WinAPIConstants.HOTKEY_ID_RECORD:
                        if (!macroService.IsPlayingMacro)
                            macroService.ToggleRecording();
                        break;
                    case WinAPIConstants.HOTKEY_ID_PLAY_MACRO:
                        macroService.TogglePlayback();
                        break;
                }
            }
            base.WndProc(ref m);
        }

        private void BtnStartStop_Click(object sender, EventArgs e)
        {
            ToggleClicking();
        }

        private void BtnRecordMacro_Click(object sender, EventArgs e)
        {
            if (!btnRecordMacro.Enabled) return;

            if (macroService.IsPlayingMacro)
            {
                lblMacroStatus.Text = "ESTETTY: Ei voi tallentaa makron toiston aikana!";
                lblMacroStatus.ForeColor = System.Drawing.Color.Red;
                return;
            }

            macroService.ToggleRecording();
            UpdateRecordingUI();
        }

        private void BtnPlayMacro_Click(object sender, EventArgs e)
        {
            macroService.TogglePlayback();
        }

        private void BtnClearMacro_Click(object sender, EventArgs e)
        {
            macroService.ClearMacro();
            lbMacroActions.Items.Clear();
            btnPlayMacro.Enabled = false;
        }

        private async void BtnSetPosition_Click(object sender, EventArgs e)
        {
            btnSetPosition.Enabled = false;

            for (int i = 3; i >= 1; i--)
            {
                btnSetPosition.Text = $"Siirry paikkaan ({i})";
                await Task.Delay(1000);
            }

            WinAPI.GetCursorPos(out clickPosition);
            lblPosition.Text = $"Sijainti: {clickPosition.X}, {clickPosition.Y}";

            btnSetPosition.Text = "Aseta sijainti";
            btnSetPosition.Enabled = true;
        }

        private void ClickTimer_Tick(object sender, EventArgs e)
        {
            // Perform click using ClickService
            POINT? targetPosition = useFixedPosition ? clickPosition : null;
            ClickService.PerformClick(currentMouseButton, targetPosition, useFixedPosition);

            // Set next interval based on mode
            if (useRandomInterval)
            {
                // Calculate random interval in seconds, then convert to milliseconds
                double randomSeconds = randomMinSeconds + (random.NextDouble() * (randomMaxSeconds - randomMinSeconds));
                clickTimer.Interval = Math.Max(10, (int)(randomSeconds * 1000));
            }
            else
            {
                // Use fixed interval
                clickTimer.Interval = Math.Max(1, baseIntervalMs);
            }
        }

        private void UpdateInterval(object sender, EventArgs e)
        {
            int seconds = (int)nudSeconds.Value;
            int milliseconds = (int)nudMilliseconds.Value;
            baseIntervalMs = (seconds * 1000) + milliseconds;
            
            // Ensure minimum interval of 1ms
            if (baseIntervalMs < 1) baseIntervalMs = 1;
            
            // Update timer if running and using fixed interval
            if (isRunning && !useRandomInterval && clickTimer != null)
            {
                clickTimer.Interval = baseIntervalMs;
            }
        }

        private void UpdateRandomInterval(object sender, EventArgs e)
        {
            randomMinSeconds = (double)nudRandomMin.Value;
            randomMaxSeconds = (double)nudRandomMax.Value;
            
            // Ensure min <= max
            if (randomMinSeconds > randomMaxSeconds)
            {
                if (sender == nudRandomMin)
                {
                    nudRandomMax.Value = (decimal)randomMinSeconds;
                    randomMaxSeconds = randomMinSeconds;
                }
                else
                {
                    nudRandomMin.Value = (decimal)randomMaxSeconds;
                    randomMinSeconds = randomMaxSeconds;
                }
            }
        }

        private void UpdateIntervalMode()
        {
            // Update UI to show which mode is active
            if (useRandomInterval)
            {
                // Random mode active - dark theme
                gbRandomInterval.BackColor = Color.FromArgb(45, 45, 48);
                gbRandomInterval.ForeColor = Color.White;
                cbRandomInterval.ForeColor = Color.White;
                lblRandomMinUnit.ForeColor = Color.LightGray;
                lblRandomMaxUnit.ForeColor = Color.LightGray;
                
                // Disable fixed interval
                gbClickInterval.Enabled = false;
                gbClickInterval.ForeColor = Color.Gray;
            }
            else
            {
                // Fixed mode active - light theme
                gbRandomInterval.BackColor = Color.FromArgb(240, 240, 240);
                gbRandomInterval.ForeColor = Color.Black;
                cbRandomInterval.ForeColor = Color.Black;
                lblRandomMinUnit.ForeColor = Color.Black;
                lblRandomMaxUnit.ForeColor = Color.Black;
                
                // Enable fixed interval
                gbClickInterval.Enabled = true;
                gbClickInterval.ForeColor = Color.Black;
            }
        }

        #endregion

        #region Macro Service Event Handlers

        private void OnMacroActionAdded(MacroAction action)
        {
            try
            {
                this.Invoke(new Action(() =>
                {
                    var displayText = $"{macroService.MacroActionCount}. {action}";
                    lbMacroActions.Items.Add(displayText);
                    lbMacroActions.TopIndex = Math.Max(0, lbMacroActions.Items.Count - 1);

                    lblMacroStatus.Text = $"Tallentaa... {macroService.MacroActionCount} toimintoa tallennettu";
                    LogDebug($"UI UPDATED: '{displayText}', ListCount={lbMacroActions.Items.Count}");
                }));
            }
            catch (Exception ex)
            {
                LogDebug($"UI ERROR: {ex.Message}");
            }
        }

        private void OnMacroStatusChanged(string status)
        {
            try
            {
                this.Invoke(new Action(() =>
                {
                    lblMacroStatus.Text = status;
                    lblMacroStatus.ForeColor = macroService.IsRecording ? System.Drawing.Color.Red : 
                                             macroService.IsPlayingMacro ? System.Drawing.Color.Orange : 
                                             System.Drawing.Color.Blue;
                }));
            }
            catch (Exception ex)
            {
                LogDebug($"Status update error: {ex.Message}");
            }
        }

        private void OnPlaybackStateChanged(bool isPlaying)
        {
            try
            {
                this.Invoke(new Action(() =>
                {
                    // Update UI based on playback state
                    btnRecordMacro.Enabled = !isPlaying;
                    btnStartStop.Enabled = !isPlaying;
                    btnSetPosition.Enabled = !isPlaying;
                    btnClearMacro.Enabled = !isPlaying;
                    cbRandomInterval.Enabled = !isPlaying;
                    nudRandomMin.Enabled = !isPlaying && useRandomInterval;
                    nudRandomMax.Enabled = !isPlaying && useRandomInterval;

                    // Update macro buttons
                    if (isPlaying)
                    {
                        btnPlayMacro.Text = "Pysäytä makro (F3)";
                        btnPlayMacro.BackColor = System.Drawing.Color.Orange;
                        this.Text = "AutoClicker Pro - TOISTAA MAKROA";
                    }
                    else
                    {
                        btnPlayMacro.Text = "Toista makro (F3)";
                        btnPlayMacro.BackColor = System.Drawing.Color.LightYellow;
                        this.Text = "AutoClicker Pro";
                        btnPlayMacro.Enabled = macroService.MacroActionCount > 0;
                    }
                }));
            }
            catch (Exception ex)
            {
                LogDebug($"Playback state update error: {ex.Message}");
            }
        }

        #endregion

        #region Clicking Logic

        private void ToggleClicking()
        {
            if (isRunning)
            {
                StopClicking();
            }
            else
            {
                StartClicking();
            }
        }

        private void StartClicking()
        {
            isRunning = true;
            
            // Set initial interval based on mode
            if (useRandomInterval)
            {
                double randomSeconds = randomMinSeconds + (random.NextDouble() * (randomMaxSeconds - randomMinSeconds));
                clickTimer.Interval = Math.Max(10, (int)(randomSeconds * 1000));
            }
            else
            {
                clickTimer.Interval = Math.Max(1, baseIntervalMs);
            }
            
            clickTimer.Start();

            btnStartStop.Text = "Pysäytä (F1)";
            btnStartStop.BackColor = System.Drawing.Color.LightCoral;

            // Update status based on mode
            if (useRandomInterval)
            {
                lblStatus.Text = $"Käynnissä - Satunnainen väli {randomMinSeconds:F1}-{randomMaxSeconds:F1}s";
            }
            else
            {
                double clicksPerSecond = 1000.0 / baseIntervalMs;
                lblStatus.Text = $"Käynnissä - {clicksPerSecond:F1} CPS, {baseIntervalMs}ms väli";
            }
            lblStatus.ForeColor = System.Drawing.Color.Green;
        }

        private void StopClicking()
        {
            isRunning = false;
            clickTimer.Stop();

            btnStartStop.Text = "Käynnistä (F1)";
            btnStartStop.BackColor = System.Drawing.Color.LightGreen;
            lblStatus.Text = "Pysäytetty - Paina F1 käynnistääksesi";
            lblStatus.ForeColor = System.Drawing.Color.Red;
        }

        #endregion

        #region UI Updates

        private void UpdateRecordingUI()
        {
            if (macroService.IsRecording)
            {
                btnRecordMacro.Text = "Lopeta tallennus (F2)";
                btnRecordMacro.BackColor = System.Drawing.Color.Red;
                this.Text = "AutoClicker Pro - TALLENTAA";
                txtDebug?.Clear();
            }
            else
            {
                btnRecordMacro.Text = "Aloita tallennus (F2)";
                btnRecordMacro.BackColor = System.Drawing.Color.LightBlue;
                this.Text = "AutoClicker Pro";
                btnPlayMacro.Enabled = macroService.MacroActionCount > 0;
            }
        }

        private void LogDebug(string message)
        {
            var timestamp = DateTime.Now.ToString("HH:mm:ss.fff");
            var logEntry = $"[{timestamp}] {message}";

            try
            {
                if (txtDebug?.InvokeRequired == true)
                {
                    txtDebug.Invoke(new Action(() =>
                    {
                        txtDebug.AppendText(logEntry + Environment.NewLine);
                        txtDebug.SelectionStart = txtDebug.Text.Length;
                        txtDebug.ScrollToCaret();
                    }));
                }
                else if (txtDebug != null)
                {
                    txtDebug.AppendText(logEntry + Environment.NewLine);
                    txtDebug.SelectionStart = txtDebug.Text.Length;
                    txtDebug.ScrollToCaret();
                }
            }
            catch
            {
                // Ignore UI errors during debug logging
            }
        }

        #endregion

        #region Cleanup

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            StopClicking();
            macroService?.Dispose();

            WinAPI.UnregisterHotKey(this.Handle, WinAPIConstants.HOTKEY_ID_START_STOP);
            WinAPI.UnregisterHotKey(this.Handle, WinAPIConstants.HOTKEY_ID_RECORD);
            WinAPI.UnregisterHotKey(this.Handle, WinAPIConstants.HOTKEY_ID_PLAY_MACRO);
            
            base.OnFormClosing(e);
        }

        #endregion
    }

    // Program entry point
    public class Program
    {
        [STAThread]
        public static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }
    }
}