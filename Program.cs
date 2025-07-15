using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;

namespace AutoClicker
{
    // Structs need to be defined first
    [StructLayout(LayoutKind.Sequential)]
    public struct POINT
    {
        public int X;
        public int Y;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct MSLLHOOKSTRUCT
    {
        public POINT pt;
        public uint mouseData;
        public uint flags;
        public uint time;
        public IntPtr dwExtraInfo;
    }

    // Macro action structure
    public enum MacroActionType
    {
        MouseMove,
        LeftClick,
        RightClick,
        Wait
    }

    public struct MacroAction
    {
        public MacroActionType Type;
        public int X, Y;
        public int DelayMs;
        
        public override string ToString()
        {
            return Type switch
            {
                MacroActionType.MouseMove => $"Siirrä hiirtä ({X}, {Y})",
                MacroActionType.LeftClick => $"Vasen klikkaus ({X}, {Y})",
                MacroActionType.RightClick => $"Oikea klikkaus ({X}, {Y})",
                MacroActionType.Wait => $"Odota {DelayMs}ms",
                _ => "Tuntematon toiminto"
            };
        }
    }

    namespace AutoClicker
    {
        public partial class MainForm : Form
        {
            // Windows API imports
            [DllImport("user32.dll", SetLastError = true)]
            private static extern uint SendInput(uint nInputs, INPUT[] pInputs, int cbSize);

            [DllImport("user32.dll")]
            private static extern IntPtr GetMessageExtraInfo();

            [DllImport("user32.dll")]
            private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

            [DllImport("user32.dll")]
            private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

            [DllImport("user32.dll")]
            private static extern bool GetCursorPos(out POINT lpPoint);

            [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
            private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelMouseProc lpfn,
                IntPtr hMod, uint dwThreadId);

            [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            private static extern bool UnhookWindowsHookEx(IntPtr hhk);

            [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
            private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode,
                IntPtr wParam, IntPtr lParam);

            [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
            private static extern IntPtr GetModuleHandle(string lpModuleName);

            [DllImport("user32.dll")]
            private static extern IntPtr WindowFromPoint(POINT Point);

            [DllImport("user32.dll")]
            private static extern IntPtr GetForegroundWindow();

            // Mouse hook constants
            private const int WH_MOUSE_LL = 14;
            private const int WM_LBUTTONDOWN = 0x0201;
            private const int WM_RBUTTONDOWN = 0x0204;

            private delegate IntPtr LowLevelMouseProc(int nCode, IntPtr wParam, IntPtr lParam);
            private LowLevelMouseProc _proc = HookCallback;
            private static IntPtr _hookID = IntPtr.Zero;
            private static MainForm _instance;

            // Structs
            [StructLayout(LayoutKind.Sequential)]
            public struct INPUT
            {
                public uint type;
                public InputUnion union;
            }

            [StructLayout(LayoutKind.Explicit)]
            public struct InputUnion
            {
                [FieldOffset(0)]
                public MOUSEINPUT mi;
                [FieldOffset(0)]
                public KEYBDINPUT ki;
            }

            [StructLayout(LayoutKind.Sequential)]
            public struct MOUSEINPUT
            {
                public int dx;
                public int dy;
                public uint mouseData;
                public uint dwFlags;
                public uint time;
                public IntPtr dwExtraInfo;
            }

            [StructLayout(LayoutKind.Sequential)]
            public struct KEYBDINPUT
            {
                public ushort wVk;
                public ushort wScan;
                public uint dwFlags;
                public uint time;
                public IntPtr dwExtraInfo;
            }

            // Constants
            private const uint INPUT_MOUSE = 0;
            private const uint INPUT_KEYBOARD = 1;
            private const uint MOUSEEVENTF_LEFTDOWN = 0x0002;
            private const uint MOUSEEVENTF_LEFTUP = 0x0004;
            private const uint MOUSEEVENTF_RIGHTDOWN = 0x0008;
            private const uint MOUSEEVENTF_RIGHTUP = 0x0010;
            private const uint KEYEVENTF_KEYUP = 0x0002;

            // Hotkey constants
            private const int HOTKEY_ID_START_STOP = 1;
            private const int HOTKEY_ID_RECORD = 2;
            private const int HOTKEY_ID_PLAY_MACRO = 3;
            private const uint MOD_NONE = 0x0000;
            private const uint VK_F1 = 0x70;
            private const uint VK_F2 = 0x71;
            private const uint VK_F3 = 0x72;
            private const int WM_HOTKEY = 0x0312;

            // Private fields
            private System.Windows.Forms.Timer clickTimer;
            private bool isRunning = false;
            private int baseIntervalMs = 100; // Base interval in milliseconds
            private MouseButtons currentMouseButton = MouseButtons.Left;
            private POINT clickPosition;
            private bool useFixedPosition = false;
            private bool useRandomInterval = false;
            private int randomPercentage = 20;

            // Macro recording fields
            private List<MacroAction> recordedMacro = new List<MacroAction>();
            private bool isRecording = false;
            private bool isPlayingMacro = false;
            private DateTime recordingStartTime;
            private int macroPlaybackIndex = 0;
            private System.Windows.Forms.Timer macroTimer;

            // Random delay
            private Random random = new Random();

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
            private TextBox? txtDebug;

            public MainForm()
            {
                _instance = this;
                InitializeComponent();
                InitializeTimer();
                RegisterGlobalHotkeys();
                GetCursorPos(out clickPosition);
            }

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

            private void UpdateInterval(object sender, EventArgs e)
            {
                int seconds = (int)nudSeconds.Value;
                int milliseconds = (int)nudMilliseconds.Value;
                baseIntervalMs = (seconds * 1000) + milliseconds;
                
                // Ensure minimum interval of 1ms
                if (baseIntervalMs < 1) baseIntervalMs = 1;
                
                // Update timer if running
                if (isRunning && clickTimer != null)
                {
                    clickTimer.Interval = GetRandomizedInterval(baseIntervalMs);
                }
            }

            private void InitializeTimer()
            {
                clickTimer = new System.Windows.Forms.Timer();
                clickTimer.Tick += ClickTimer_Tick;

                macroTimer = new System.Windows.Forms.Timer();
                macroTimer.Tick += MacroTimer_Tick;
            }

            private void RegisterGlobalHotkeys()
            {
                RegisterHotKey(this.Handle, HOTKEY_ID_START_STOP, MOD_NONE, VK_F1);
                RegisterHotKey(this.Handle, HOTKEY_ID_RECORD, MOD_NONE, VK_F2);
                RegisterHotKey(this.Handle, HOTKEY_ID_PLAY_MACRO, MOD_NONE, VK_F3);
            }

            protected override void WndProc(ref Message m)
            {
                if (m.Msg == WM_HOTKEY)
                {
                    switch (m.WParam.ToInt32())
                    {
                        case HOTKEY_ID_START_STOP:
                            ToggleClicking();
                            break;
                        case HOTKEY_ID_RECORD:
                            if (!isPlayingMacro)
                                ToggleRecording();
                            break;
                        case HOTKEY_ID_PLAY_MACRO:
                            PlayMacro();
                            break;
                    }
                }
                base.WndProc(ref m);
            }

            private void BtnStartStop_Click(object sender, EventArgs e)
            {
                ToggleClicking();
            }

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
                
                clickTimer.Interval = GetRandomizedInterval(baseIntervalMs);
                clickTimer.Start();

                btnStartStop.Text = "Pysäytä (F1)";
                btnStartStop.BackColor = Color.LightCoral;

                double clicksPerSecond = 1000.0 / baseIntervalMs;
                string intervalInfo = useRandomInterval ? $" (±{randomPercentage}%)" : "";
                lblStatus.Text = $"Käynnissä - {clicksPerSecond:F1} CPS, {baseIntervalMs}ms väli{intervalInfo}";
                lblStatus.ForeColor = Color.Green;
            }

            private void StopClicking()
            {
                isRunning = false;
                clickTimer.Stop();

                btnStartStop.Text = "Käynnistä (F1)";
                btnStartStop.BackColor = Color.LightGreen;
                lblStatus.Text = "Pysäytetty - Paina F1 käynnistääksesi";
                lblStatus.ForeColor = Color.Red;
            }

            private void ClickTimer_Tick(object sender, EventArgs e)
            {
                PerformClick();

                // Randomize next interval if enabled
                if (useRandomInterval && isRunning)
                {
                    clickTimer.Interval = GetRandomizedInterval(baseIntervalMs);
                }
            }

            private int GetRandomizedInterval(int baseInterval)
            {
                if (!useRandomInterval)
                    return baseInterval;

                // Calculate random variation based on percentage
                double variation = baseInterval * (randomPercentage / 100.0);
                int minInterval = Math.Max(1, (int)(baseInterval - variation));
                int maxInterval = (int)(baseInterval + variation);
                
                return random.Next(minInterval, maxInterval + 1);
            }

            private void PerformClick()
            {
                INPUT[] inputs = new INPUT[2];

                // Move mouse to position if using fixed position
                if (useFixedPosition)
                {
                    INPUT moveInput = new INPUT
                    {
                        type = INPUT_MOUSE,
                        union = new InputUnion
                        {
                            mi = new MOUSEINPUT
                            {
                                dx = clickPosition.X * 65536 / Screen.PrimaryScreen.Bounds.Width,
                                dy = clickPosition.Y * 65536 / Screen.PrimaryScreen.Bounds.Height,
                                mouseData = 0,
                                dwFlags = 0x0001 | 0x8000,
                                time = 0,
                                dwExtraInfo = GetMessageExtraInfo()
                            }
                        }
                    };

                    SendInput(1, new INPUT[] { moveInput }, Marshal.SizeOf(typeof(INPUT)));
                    Thread.Sleep(1);
                }

                if (currentMouseButton == MouseButtons.Left)
                {
                    inputs[0] = new INPUT
                    {
                        type = INPUT_MOUSE,
                        union = new InputUnion
                        {
                            mi = new MOUSEINPUT
                            {
                                dx = 0, dy = 0, mouseData = 0,
                                dwFlags = MOUSEEVENTF_LEFTDOWN,
                                time = 0, dwExtraInfo = GetMessageExtraInfo()
                            }
                        }
                    };

                    inputs[1] = new INPUT
                    {
                        type = INPUT_MOUSE,
                        union = new InputUnion
                        {
                            mi = new MOUSEINPUT
                            {
                                dx = 0, dy = 0, mouseData = 0,
                                dwFlags = MOUSEEVENTF_LEFTUP,
                                time = 0, dwExtraInfo = GetMessageExtraInfo()
                            }
                        }
                    };
                }
                else
                {
                    inputs[0] = new INPUT
                    {
                        type = INPUT_MOUSE,
                        union = new InputUnion
                        {
                            mi = new MOUSEINPUT
                            {
                                dx = 0, dy = 0, mouseData = 0,
                                dwFlags = MOUSEEVENTF_RIGHTDOWN,
                                time = 0, dwExtraInfo = GetMessageExtraInfo()
                            }
                        }
                    };

                    inputs[1] = new INPUT
                    {
                        type = INPUT_MOUSE,
                        union = new InputUnion
                        {
                            mi = new MOUSEINPUT
                            {
                                dx = 0, dy = 0, mouseData = 0,
                                dwFlags = MOUSEEVENTF_RIGHTUP,
                                time = 0, dwExtraInfo = GetMessageExtraInfo()
                            }
                        }
                    };
                }

                SendInput(2, inputs, Marshal.SizeOf(typeof(INPUT)));
            }

            private async void BtnSetPosition_Click(object sender, EventArgs e)
            {
                btnSetPosition.Enabled = false;

                for (int i = 3; i >= 1; i--)
                {
                    btnSetPosition.Text = $"Siirry paikkaan ({i})";
                    await Task.Delay(1000);
                }

                GetCursorPos(out clickPosition);
                lblPosition.Text = $"Sijainti: {clickPosition.X}, {clickPosition.Y}";

                btnSetPosition.Text = "Aseta sijainti";
                btnSetPosition.Enabled = true;
            }

            protected override void OnFormClosing(FormClosingEventArgs e)
            {
                StopClicking();
                StopRecording();

                if (_hookID != IntPtr.Zero)
                {
                    UnhookWindowsHookEx(_hookID);
                    _hookID = IntPtr.Zero;
                }

                UnregisterHotKey(this.Handle, HOTKEY_ID_START_STOP);
                UnregisterHotKey(this.Handle, HOTKEY_ID_RECORD);
                UnregisterHotKey(this.Handle, HOTKEY_ID_PLAY_MACRO);
                base.OnFormClosing(e);
            }

            // Macro functionality
            private void BtnRecordMacro_Click(object sender, EventArgs e)
            {
                var debugInfo = $"NAPPI KLIKATTU! isPlaying: {isPlayingMacro}, isRecording: {isRecording}, Enabled: {btnRecordMacro.Enabled}";
                lblMacroStatus.Text = debugInfo;
                System.Diagnostics.Debug.WriteLine(debugInfo);

                if (!btnRecordMacro.Enabled)
                {
                    lblMacroStatus.Text = "VIRHE: Nappi reagoi vaikka se on pois käytöstä!";
                    lblMacroStatus.ForeColor = Color.Red;
                    return;
                }

                this.Text = $"AutoClicker Pro - Recording: {isRecording}, Playing: {isPlayingMacro}";

                if (isPlayingMacro)
                {
                    lblMacroStatus.Text = "ESTETTY: Ei voi tallentaa makron toiston aikana!";
                    lblMacroStatus.ForeColor = Color.Red;
                    return;
                }

                ToggleRecording();
            }

            private void BtnPlayMacro_Click(object sender, EventArgs e)
            {
                PlayMacro();
            }

            private void BtnClearMacro_Click(object sender, EventArgs e)
            {
                recordedMacro.Clear();
                lbMacroActions.Items.Clear();
                btnPlayMacro.Enabled = false;
                lblMacroStatus.Text = "Makro tyhjennetty";
                lblMacroStatus.ForeColor = Color.Blue;
            }

            private void ToggleRecording()
            {
                if (isPlayingMacro) return;

                if (isRecording)
                {
                    StopRecording();
                }
                else
                {
                    StartRecording();
                }
            }

            private void StartRecording()
            {
                if (isPlayingMacro || isRunning)
                {
                    lblMacroStatus.Text = "EI VOI TALLENTAA: Makro toistuu tai clickeri käynnissä!";
                    lblMacroStatus.ForeColor = Color.Red;
                    return;
                }

                recordedMacro.Clear();
                lbMacroActions.Items.Clear();
                txtDebug?.Clear();
                isRecording = true;
                recordingStartTime = DateTime.Now;

                LogDebug($"RECORDING STARTED: Time={recordingStartTime:HH:mm:ss.fff}");

                btnRecordMacro.Text = "Lopeta tallennus (F2)";
                btnRecordMacro.BackColor = Color.Red;
                lblMacroStatus.Text = "Tallentaa... Klikkaa muissa sovelluksissa (ei tässä ikkunassa)";
                lblMacroStatus.ForeColor = Color.Red;

                this.Text = $"AutoClicker Pro - TALLENTAA";

                if (!isPlayingMacro)
                {
                    _hookID = SetHook(_proc);
                    LogDebug($"MOUSE HOOK INSTALLED: hookID={_hookID}");
                }
                else
                {
                    LogDebug("MOUSE HOOK NOT INSTALLED: Macro is playing");
                }
            }

            private void StopRecording()
            {
                if (!isRecording) return;

                isRecording = false;

                if (_hookID != IntPtr.Zero)
                {
                    UnhookWindowsHookEx(_hookID);
                    _hookID = IntPtr.Zero;
                }

                btnRecordMacro.Text = "Aloita tallennus (F2)";
                btnRecordMacro.BackColor = Color.LightBlue;
                lblMacroStatus.Text = $"Tallennus valmis - {recordedMacro.Count} toimintoa";
                lblMacroStatus.ForeColor = Color.Green;

                this.Text = $"AutoClicker Pro - VALMIS (Recording: {isRecording})";

                btnPlayMacro.Enabled = recordedMacro.Count > 0;
            }

            private static IntPtr SetHook(LowLevelMouseProc proc)
            {
                using (System.Diagnostics.Process curProcess = System.Diagnostics.Process.GetCurrentProcess())
                using (System.Diagnostics.ProcessModule curModule = curProcess.MainModule)
                {
                    return SetWindowsHookEx(WH_MOUSE_LL, proc,
                        GetModuleHandle(curModule.ModuleName), 0);
                }
            }

            private static IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
            {
                if (nCode >= 0 && _instance != null)
                {
                    _instance.LogDebug($"HOOK: nCode={nCode}, wParam={wParam}, isRec={_instance.isRecording}, isPlay={_instance.isPlayingMacro}");

                    if (_instance.isRecording && !_instance.isPlayingMacro)
                    {
                        if (wParam == (IntPtr)WM_LBUTTONDOWN || wParam == (IntPtr)WM_RBUTTONDOWN)
                        {
                            MSLLHOOKSTRUCT hookStruct = (MSLLHOOKSTRUCT)Marshal.PtrToStructure(lParam, typeof(MSLLHOOKSTRUCT));

                            _instance.LogDebug($"CLICK DETECTED: Button={wParam}, Pos=({hookStruct.pt.X},{hookStruct.pt.Y})");

                            IntPtr clickedWindow = WindowFromPoint(hookStruct.pt);
                            IntPtr topWindow = GetForegroundWindow();

                            _instance.LogDebug($"Window check: clicked={clickedWindow}, top={topWindow}, our={_instance.Handle}");

                            if (clickedWindow != IntPtr.Zero && topWindow == _instance.Handle &&
                                clickedWindow == _instance.Handle)
                            {
                                _instance.LogDebug("CLICK IGNORED: Definitely on autoclicker window");
                                return CallNextHookEx(_hookID, nCode, wParam, lParam);
                            }

                            MacroActionType actionType = wParam == (IntPtr)WM_LBUTTONDOWN ?
                                MacroActionType.LeftClick : MacroActionType.RightClick;

                            _instance.LogDebug($"ADDING TO MACRO: {actionType} at ({hookStruct.pt.X},{hookStruct.pt.Y})");
                            _instance.AddMacroAction(actionType, hookStruct.pt.X, hookStruct.pt.Y);
                        }
                    }
                }
                return CallNextHookEx(_hookID, nCode, wParam, lParam);
            }

            private void AddMacroAction(MacroActionType actionType, int x, int y)
            {
                var debugMsg = $"AddMacroAction: isRec={isRecording}, type={actionType}, pos=({x},{y})";
                LogDebug(debugMsg);

                if (!isRecording)
                {
                    LogDebug("SKIPPED: Not recording");
                    return;
                }

                var timeSinceStart = DateTime.Now - recordingStartTime;
                var action = new MacroAction
                {
                    Type = actionType,
                    X = x,
                    Y = y,
                    DelayMs = (int)timeSinceStart.TotalMilliseconds
                };

                recordedMacro.Add(action);
                LogDebug($"ADDED: Count={recordedMacro.Count}, Action={action}");

                try
                {
                    this.Invoke(new Action(() =>
                    {
                        var displayText = $"{recordedMacro.Count}. {action}";
                        lbMacroActions.Items.Add(displayText);
                        lbMacroActions.TopIndex = Math.Max(0, lbMacroActions.Items.Count - 1);

                        lblMacroStatus.Text = $"Tallentaa... {recordedMacro.Count} toimintoa tallennettu";
                        LogDebug($"UI UPDATED: '{displayText}', ListCount={lbMacroActions.Items.Count}");
                    }));
                }
                catch (Exception ex)
                {
                    LogDebug($"UI ERROR: {ex.Message}");
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

            private void PlayMacro()
            {
                if (recordedMacro.Count == 0 || isRecording || isRunning) return;

                if (isPlayingMacro)
                {
                    StopMacroPlayback();
                    return;
                }

                StartMacroPlayback();
            }

            private void StartMacroPlayback()
            {
                isPlayingMacro = true;
                macroPlaybackIndex = 0;

                btnPlayMacro.Text = "Pysäytä makro (F3)";
                btnPlayMacro.BackColor = Color.Orange;
                lblMacroStatus.Text = "Toistaa makroa... (KAIKKI NAPIT LUKITTU)";
                lblMacroStatus.ForeColor = Color.Orange;

                this.Text = $"AutoClicker Pro - TOISTAA MAKROA";

                btnRecordMacro.Enabled = false;
                btnStartStop.Enabled = false;
                btnSetPosition.Enabled = false;
                btnClearMacro.Enabled = false;
                cbRandomInterval.Enabled = false;
                nudRandomPercent.Enabled = false;

                System.Diagnostics.Debug.WriteLine($"MAKRO ALKAA: btnRecordMacro.Enabled = {btnRecordMacro.Enabled}");

                PlayNextMacroAction();
            }

            private void StopMacroPlayback()
            {
                isPlayingMacro = false;
                macroTimer.Stop();

                btnPlayMacro.Text = "Toista makro (F3)";
                btnPlayMacro.BackColor = Color.LightYellow;
                lblMacroStatus.Text = "Makro valmis - Napit palautettu";
                lblMacroStatus.ForeColor = Color.Blue;

                this.Text = "AutoClicker Pro";

                btnRecordMacro.Enabled = true;
                btnStartStop.Enabled = true;
                btnSetPosition.Enabled = true;
                btnClearMacro.Enabled = true;

                System.Diagnostics.Debug.WriteLine($"NAPIT PALAUTETTU: btnRecordMacro.Enabled = {btnRecordMacro.Enabled}");
            }

            private void PlayNextMacroAction()
            {
                if (!isPlayingMacro || macroPlaybackIndex >= recordedMacro.Count)
                {
                    StopMacroPlayback();
                    lblMacroStatus.Text = "Makro toistettu loppuun (Tallennus VAPAA)";
                    return;
                }

                var action = recordedMacro[macroPlaybackIndex];

                int delay = 0;
                if (macroPlaybackIndex > 0)
                {
                    delay = action.DelayMs - recordedMacro[macroPlaybackIndex - 1].DelayMs;
                }
                else
                {
                    delay = 50;
                }

                macroTimer.Interval = Math.Max(10, delay);
                macroTimer.Start();
            }

            private void MacroTimer_Tick(object sender, EventArgs e)
            {
                macroTimer.Stop();

                if (!isPlayingMacro || macroPlaybackIndex >= recordedMacro.Count) return;

                var action = recordedMacro[macroPlaybackIndex];
                ExecuteMacroAction(action);

                macroPlaybackIndex++;

                if (macroPlaybackIndex >= recordedMacro.Count)
                {
                    var endTimer = new System.Windows.Forms.Timer();
                    endTimer.Interval = 100;
                    endTimer.Tick += (s, ev) =>
                    {
                        endTimer.Stop();
                        endTimer.Dispose();
                        if (isPlayingMacro)
                        {
                            StopMacroPlayback();
                        }
                    };
                    endTimer.Start();
                }
                else
                {
                    PlayNextMacroAction();
                }
            }

            private void ExecuteMacroAction(MacroAction action)
            {
                switch (action.Type)
                {
                    case MacroActionType.MouseMove:
                        MoveMouseTo(action.X, action.Y);
                        break;
                    case MacroActionType.LeftClick:
                        MoveMouseTo(action.X, action.Y);
                        Thread.Sleep(10);
                        PerformMouseClick(MouseButtons.Left);
                        break;
                    case MacroActionType.RightClick:
                        MoveMouseTo(action.X, action.Y);
                        Thread.Sleep(10);
                        PerformMouseClick(MouseButtons.Right);
                        break;
                }
            }

            private void MoveMouseTo(int x, int y)
            {
                INPUT moveInput = new INPUT
                {
                    type = INPUT_MOUSE,
                    union = new InputUnion
                    {
                        mi = new MOUSEINPUT
                        {
                            dx = x * 65536 / Screen.PrimaryScreen.Bounds.Width,
                            dy = y * 65536 / Screen.PrimaryScreen.Bounds.Height,
                            mouseData = 0,
                            dwFlags = 0x0001 | 0x8000,
                            time = 0,
                            dwExtraInfo = GetMessageExtraInfo()
                        }
                    }
                };

                SendInput(1, new INPUT[] { moveInput }, Marshal.SizeOf(typeof(INPUT)));
            }

            private void PerformMouseClick(MouseButtons button)
            {
                INPUT[] inputs = new INPUT[2];

                if (button == MouseButtons.Left)
                {
                    inputs[0] = new INPUT
                    {
                        type = INPUT_MOUSE,
                        union = new InputUnion
                        {
                            mi = new MOUSEINPUT
                            {
                                dwFlags = MOUSEEVENTF_LEFTDOWN,
                                dwExtraInfo = GetMessageExtraInfo()
                            }
                        }
                    };

                    inputs[1] = new INPUT
                    {
                        type = INPUT_MOUSE,
                        union = new InputUnion
                        {
                            mi = new MOUSEINPUT
                            {
                                dwFlags = MOUSEEVENTF_LEFTUP,
                                dwExtraInfo = GetMessageExtraInfo()
                            }
                        }
                    };
                }
                else
                {
                    inputs[0] = new INPUT
                    {
                        type = INPUT_MOUSE,
                        union = new InputUnion
                        {
                            mi = new MOUSEINPUT
                            {
                                dwFlags = MOUSEEVENTF_RIGHTDOWN,
                                dwExtraInfo = GetMessageExtraInfo()
                            }
                        }
                    };

                    inputs[1] = new INPUT
                    {
                        type = INPUT_MOUSE,
                        union = new InputUnion
                        {
                            mi = new MOUSEINPUT
                            {
                                dwFlags = MOUSEEVENTF_RIGHTUP,
                                dwExtraInfo = GetMessageExtraInfo()
                            }
                        }
                    };
                }

                SendInput(2, inputs, Marshal.SizeOf(typeof(INPUT)));
            }
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
}