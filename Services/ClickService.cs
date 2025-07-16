using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using AutoClicker.Utils;

namespace AutoClicker.Services
{
    /// <summary>
    /// Service for handling mouse clicks and movements
    /// </summary>
    public class ClickService
    {
        /// <summary>
        /// Performs a mouse click at the current cursor position or specified position
        /// </summary>
        /// <param name="button">Mouse button to click</param>
        /// <param name="position">Optional position to move to before clicking</param>
        /// <param name="moveToPosition">Whether to move mouse to position first</param>
        public static void PerformClick(MouseButtons button, POINT? position = null, bool moveToPosition = false)
        {
            // Move mouse to position if specified
            if (moveToPosition && position.HasValue)
            {
                MoveMouseTo(position.Value.X, position.Value.Y);
                Thread.Sleep(1); // Small delay to ensure mouse moved
            }

            INPUT[] inputs = new INPUT[2];

            if (button == MouseButtons.Left)
            {
                // Left mouse down
                inputs[0] = new INPUT
                {
                    type = WinAPIConstants.INPUT_MOUSE,
                    union = new InputUnion
                    {
                        mi = new MOUSEINPUT
                        {
                            dx = 0,
                            dy = 0,
                            mouseData = 0,
                            dwFlags = WinAPIConstants.MOUSEEVENTF_LEFTDOWN,
                            time = 0,
                            dwExtraInfo = WinAPI.GetMessageExtraInfo()
                        }
                    }
                };

                // Left mouse up
                inputs[1] = new INPUT
                {
                    type = WinAPIConstants.INPUT_MOUSE,
                    union = new InputUnion
                    {
                        mi = new MOUSEINPUT
                        {
                            dx = 0,
                            dy = 0,
                            mouseData = 0,
                            dwFlags = WinAPIConstants.MOUSEEVENTF_LEFTUP,
                            time = 0,
                            dwExtraInfo = WinAPI.GetMessageExtraInfo()
                        }
                    }
                };
            }
            else // Right click
            {
                // Right mouse down
                inputs[0] = new INPUT
                {
                    type = WinAPIConstants.INPUT_MOUSE,
                    union = new InputUnion
                    {
                        mi = new MOUSEINPUT
                        {
                            dx = 0,
                            dy = 0,
                            mouseData = 0,
                            dwFlags = WinAPIConstants.MOUSEEVENTF_RIGHTDOWN,
                            time = 0,
                            dwExtraInfo = WinAPI.GetMessageExtraInfo()
                        }
                    }
                };

                // Right mouse up
                inputs[1] = new INPUT
                {
                    type = WinAPIConstants.INPUT_MOUSE,
                    union = new InputUnion
                    {
                        mi = new MOUSEINPUT
                        {
                            dx = 0,
                            dy = 0,
                            mouseData = 0,
                            dwFlags = WinAPIConstants.MOUSEEVENTF_RIGHTUP,
                            time = 0,
                            dwExtraInfo = WinAPI.GetMessageExtraInfo()
                        }
                    }
                };
            }

            WinAPI.SendInput(2, inputs, Marshal.SizeOf(typeof(INPUT)));
        }

        /// <summary>
        /// Moves mouse to specified coordinates
        /// </summary>
        /// <param name="x">X coordinate</param>
        /// <param name="y">Y coordinate</param>
        public static void MoveMouseTo(int x, int y)
        {
            INPUT moveInput = new INPUT
            {
                type = WinAPIConstants.INPUT_MOUSE,
                union = new InputUnion
                {
                    mi = new MOUSEINPUT
                    {
                        dx = x * 65536 / Screen.PrimaryScreen.Bounds.Width,
                        dy = y * 65536 / Screen.PrimaryScreen.Bounds.Height,
                        mouseData = 0,
                        dwFlags = WinAPIConstants.MOUSEEVENTF_MOVE | WinAPIConstants.MOUSEEVENTF_ABSOLUTE,
                        time = 0,
                        dwExtraInfo = WinAPI.GetMessageExtraInfo()
                    }
                }
            };

            WinAPI.SendInput(1, new INPUT[] { moveInput }, Marshal.SizeOf(typeof(INPUT)));
        }

        /// <summary>
        /// Gets the current cursor position
        /// </summary>
        /// <returns>Current cursor position</returns>
        public static POINT GetCurrentCursorPosition()
        {
            WinAPI.GetCursorPos(out POINT pos);
            return pos;
        }

        /// <summary>
        /// Calculates randomized interval based on base interval and percentage
        /// </summary>
        /// <param name="baseInterval">Base interval in milliseconds</param>
        /// <param name="useRandom">Whether to use randomization</param>
        /// <param name="randomPercentage">Percentage of randomization (±%)</param>
        /// <param name="random">Random number generator</param>
        /// <returns>Randomized interval</returns>
        public static int GetRandomizedInterval(int baseInterval, bool useRandom, int randomPercentage, Random random)
        {
            if (!useRandom)
                return baseInterval;

            // Calculate random variation based on percentage
            double variation = baseInterval * (randomPercentage / 100.0);
            int minInterval = Math.Max(1, (int)(baseInterval - variation));
            int maxInterval = (int)(baseInterval + variation);
            
            return random.Next(minInterval, maxInterval + 1);
        }
    }
}