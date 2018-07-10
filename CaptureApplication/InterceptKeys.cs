using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace CaptureApplication
{
    // Capture: Ctrl + Shift + C
    // Count: Ctrl + Shift + D
    // Delete Last: Ctrl + Shift + Backspace
    // Delete First: Ctrl + Shift + Home
    // Delete All: Ctrl + Shift + Delete
    // Help: Ctrl + H
    // Quit: Ctrl + Shift + Q
    class InterceptKeys
    {

        private const int WH_KEYBOARD_LL = 13;

        private const int WM_KEYDOWN = 0x0100;
        private const uint WM_SYSKEYDOWN = 0x104;

        private static LowLevelKeyboardProc _proc = HookCallback;

        private static IntPtr _hookID = IntPtr.Zero;


        public static void Init()
        {
            _hookID = SetHook(_proc);
            

        }

        public static void Destroy()
        {
            UnhookWindowsHookEx(_hookID);
        }

        private static void deleteLatestOne()
        {
            // Process the list of files found in the directory.
            DirectoryInfo info = new DirectoryInfo(Capture.DIR_PATH);
            // Getting files by creation date
            FileInfo[] files = info.GetFiles().OrderBy(p => p.CreationTime).ToArray();
            if (files.Length > 0)
            {
                files[files.Length - 1].Delete();
            }
        }

        private static void deleteFirstOne()
        {
            // Process the list of files found in the directory.
            DirectoryInfo info = new DirectoryInfo(Capture.DIR_PATH);
            // Getting files by creation date
            FileInfo[] files = info.GetFiles().OrderBy(p => p.CreationTime).ToArray();
            if (files.Length > 0)
            {
                files[0].Delete();
            }
        }

        private static void deleteAll()
        {
            DirectoryInfo info = new DirectoryInfo(Capture.DIR_PATH);
            if (info.Exists)
            {
                info.Delete(true);
            }
        }

        private static void capture()
        {
            Capture.CaptureScreen();
        }

        private static void showCount()
        {
            DirectoryInfo info = new DirectoryInfo(Capture.DIR_PATH);
            int length = info.GetFiles().Length;
            System.Windows.Forms.MessageBox.Show("Count: " + length);
        }

        private static void showHelp()
        {
            string help = "Capture: Ctrl + Alt + Shift + C\nCount: Ctrl + Alt + Shift + D\n" +
                "Delete Last: Ctrl + Alt + Shift + Backspace\nDelete First: Ctrl + Alt + Shift + Home\nDelete All: Ctrl + Alt + Shift + Delete\nQuit: Ctrl + Alt + Shift + Q";
            System.Windows.Forms.MessageBox.Show(help);
        }

        private static void exitApp()
        {
            Application.Exit();
        }
        private static IntPtr SetHook(LowLevelKeyboardProc proc)
        {
            using (Process curProcess = Process.GetCurrentProcess())

            using (ProcessModule curModule = curProcess.MainModule)
            {
                return SetWindowsHookEx(WH_KEYBOARD_LL, proc,
                    GetModuleHandle(curModule.ModuleName), 0);
            }

        }


        private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

        private static IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0 && wParam == (IntPtr)WM_KEYDOWN)
            {
                if ((Control.ModifierKeys & Keys.Control) == Keys.Control && (Control.ModifierKeys & Keys.Alt) == Keys.Alt && (Control.ModifierKeys & Keys.Shift) == Keys.Shift)
                {
                    int vkCode = Marshal.ReadInt32(lParam);
                    Console.WriteLine((Keys)vkCode);

                    // Capture: Ctrl + Alt + Shift + C
                    // Count: Ctrl + Alt + Shift + D
                    // Delete Last: Ctrl + Alt + Shift + Backspace
                    // Delete First: Ctrl + Alt + Shift + Home
                    // Delete All: Ctrl + Alt + Shift + Delete
                    // Help: Ctrl + Alt + Shift + H
                    // Quit: Ctrl + Alt + Shift + Q

                    switch (vkCode)
                    {
                        case 'C':
                            // Capture
                            capture();
                            break;
                        case 'D':
                            // Show counts
                            showCount();
                            break;
                        case (int)Keys.Back:
                            // Delete latest one
                            deleteLatestOne();
                            break;
                        case (int)Keys.Home:
                            // Delete first one
                            deleteFirstOne();
                            break;
                        case (int)Keys.Delete:
                            // Delete all
                            deleteAll();
                            break;
                        case 'H':
                            showHelp();
                            break;
                        case 'Q':
                            // Exit
                            exitApp();
                            break;
                    }
                }
            }

            return CallNextHookEx(_hookID, nCode, wParam, lParam);
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]

        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]

        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]

        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]

        private static extern IntPtr GetModuleHandle(string lpModuleName);
    }
}
