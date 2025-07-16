using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using AutoClicker.Utils;

namespace AutoClicker.Services
{
    /// <summary>
    /// Service for handling macro recording and playback
    /// </summary>
    public class MacroService
    {
        private List<MacroAction> recordedMacro = new List<MacroAction>();
        private bool isRecording = false;
        private bool isPlayingMacro = false;
        private DateTime recordingStartTime;
        private int macroPlaybackIndex = 0;
        private System.Windows.Forms.Timer macroTimer;
        private IntPtr hookID = IntPtr.Zero;
        private WinAPI.LowLevelMouseProc hookProc;
        private MainForm parentForm;

        public event Action<MacroAction> MacroActionAdded;
        public event Action<string> StatusChanged;
        public event Action<bool> PlaybackStateChanged;

        public bool IsRecording => isRecording;
        public bool IsPlayingMacro => isPlayingMacro;
        public int MacroActionCount => recordedMacro.Count;
        public IReadOnlyList<MacroAction> MacroActions => recordedMacro.AsReadOnly();

        public MacroService(MainForm parent)
        {
            parentForm = parent;
            hookProc = HookCallback;
            
            macroTimer = new System.Windows.Forms.Timer();
            macroTimer.Tick += MacroTimer_Tick;
        }

        /// <summary>
        /// Starts macro recording
        /// </summary>
        public void StartRecording()
        {
            if (isPlayingMacro || isRecording) return;

            recordedMacro.Clear();
            isRecording = true;
            recordingStartTime = DateTime.Now;

            StatusChanged?.Invoke("Tallentaa... Klikkaa muissa sovelluksissa (ei tässä ikkunassa)");
            
            // Install mouse hook
            hookID = SetHook(hookProc);
        }

        /// <summary>
        /// Stops macro recording
        /// </summary>
        public void StopRecording()
        {
            if (!isRecording) return;

            isRecording = false;

            // Remove mouse hook
            if (hookID != IntPtr.Zero)
            {
                WinAPI.UnhookWindowsHookEx(hookID);
                hookID = IntPtr.Zero;
            }

            StatusChanged?.Invoke($"Tallennus valmis - {recordedMacro.Count} toimintoa");
        }

        /// <summary>
        /// Toggles macro recording state
        /// </summary>
        public void ToggleRecording()
        {
            if (isRecording)
                StopRecording();
            else
                StartRecording();
        }

        /// <summary>
        /// Starts macro playback
        /// </summary>
        public void StartPlayback()
        {
            if (recordedMacro.Count == 0 || isRecording || isPlayingMacro) return;

            isPlayingMacro = true;
            macroPlaybackIndex = 0;

            StatusChanged?.Invoke("Toistaa makroa... (KAIKKI NAPIT LUKITTU)");
            PlaybackStateChanged?.Invoke(true);

            PlayNextMacroAction();
        }

        /// <summary>
        /// Stops macro playback
        /// </summary>
        public void StopPlayback()
        {
            if (!isPlayingMacro) return;

            isPlayingMacro = false;
            macroTimer.Stop();

            StatusChanged?.Invoke("Makro valmis - Napit palautettu");
            PlaybackStateChanged?.Invoke(false);
        }

        /// <summary>
        /// Toggles macro playback
        /// </summary>
        public void TogglePlayback()
        {
            if (isPlayingMacro)
                StopPlayback();
            else
                StartPlayback();
        }

        /// <summary>
        /// Clears recorded macro
        /// </summary>
        public void ClearMacro()
        {
            recordedMacro.Clear();
            StatusChanged?.Invoke("Makro tyhjennetty");
        }

        /// <summary>
        /// Disposes the macro service and cleans up resources
        /// </summary>
        public void Dispose()
        {
            StopRecording();
            StopPlayback();
            macroTimer?.Dispose();
        }

        private IntPtr SetHook(WinAPI.LowLevelMouseProc proc)
        {
            using (System.Diagnostics.Process curProcess = System.Diagnostics.Process.GetCurrentProcess())
            using (System.Diagnostics.ProcessModule curModule = curProcess.MainModule)
            {
                return WinAPI.SetWindowsHookEx(WinAPIConstants.WH_MOUSE_LL, proc,
                    WinAPI.GetModuleHandle(curModule.ModuleName), 0);
            }
        }

        private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0 && isRecording && !isPlayingMacro)
            {
                if (wParam == (IntPtr)WinAPIConstants.WM_LBUTTONDOWN || 
                    wParam == (IntPtr)WinAPIConstants.WM_RBUTTONDOWN)
                {
                    MSLLHOOKSTRUCT hookStruct = (MSLLHOOKSTRUCT)Marshal.PtrToStructure(lParam, typeof(MSLLHOOKSTRUCT));

                    // Check if click is on our autoclicker window
                    IntPtr clickedWindow = WinAPI.WindowFromPoint(hookStruct.pt);
                    IntPtr topWindow = WinAPI.GetForegroundWindow();

                    // Only ignore if both the clicked window AND foreground window are ours
                    if (clickedWindow != IntPtr.Zero && topWindow == parentForm.Handle &&
                        clickedWindow == parentForm.Handle)
                    {
                        return WinAPI.CallNextHookEx(hookID, nCode, wParam, lParam);
                    }

                    MacroActionType actionType = wParam == (IntPtr)WinAPIConstants.WM_LBUTTONDOWN ?
                        MacroActionType.LeftClick : MacroActionType.RightClick;

                    AddMacroAction(actionType, hookStruct.pt.X, hookStruct.pt.Y);
                }
            }
            return WinAPI.CallNextHookEx(hookID, nCode, wParam, lParam);
        }

        private void AddMacroAction(MacroActionType actionType, int x, int y)
        {
            if (!isRecording) return;

            var timeSinceStart = DateTime.Now - recordingStartTime;
            var action = new MacroAction
            {
                Type = actionType,
                X = x,
                Y = y,
                DelayMs = (int)timeSinceStart.TotalMilliseconds
            };

            recordedMacro.Add(action);
            MacroActionAdded?.Invoke(action);
        }

        private void PlayNextMacroAction()
        {
            if (!isPlayingMacro || macroPlaybackIndex >= recordedMacro.Count)
            {
                StopPlayback();
                StatusChanged?.Invoke("Makro toistettu loppuun (Tallennus VAPAA)");
                return;
            }

            var action = recordedMacro[macroPlaybackIndex];

            // Calculate delay for this action
            int delay = 0;
            if (macroPlaybackIndex > 0)
            {
                delay = action.DelayMs - recordedMacro[macroPlaybackIndex - 1].DelayMs;
            }
            else
            {
                delay = 50; // Small delay for first action
            }

            // Set timer for the delay
            macroTimer.Interval = Math.Max(10, delay); // Minimum 10ms delay
            macroTimer.Start();
        }

        private void MacroTimer_Tick(object sender, EventArgs e)
        {
            macroTimer.Stop();

            if (!isPlayingMacro || macroPlaybackIndex >= recordedMacro.Count) return;

            var action = recordedMacro[macroPlaybackIndex];
            ExecuteMacroAction(action);

            macroPlaybackIndex++;

            // Check if this was the last action
            if (macroPlaybackIndex >= recordedMacro.Count)
            {
                // Schedule the end of macro after the last click has been processed
                var endTimer = new System.Windows.Forms.Timer();
                endTimer.Interval = 100; // Very short delay just to let the last click complete
                endTimer.Tick += (s, ev) =>
                {
                    endTimer.Stop();
                    endTimer.Dispose();
                    if (isPlayingMacro) // Double check we're still playing
                    {
                        StopPlayback();
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
                    ClickService.MoveMouseTo(action.X, action.Y);
                    break;
                case MacroActionType.LeftClick:
                    ClickService.MoveMouseTo(action.X, action.Y);
                    Thread.Sleep(10);
                    ClickService.PerformClick(MouseButtons.Left);
                    break;
                case MacroActionType.RightClick:
                    ClickService.MoveMouseTo(action.X, action.Y);
                    Thread.Sleep(10);
                    ClickService.PerformClick(MouseButtons.Right);
                    break;
            }
        }
    }
}