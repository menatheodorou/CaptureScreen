using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Drawing;
using System.Drawing.Imaging;
using System.Text.RegularExpressions;
using ScreenShotDemo;

namespace CaptureApplication
{
    class Capture
    {
        public const string DIR_PATH = "screens";

        private static ScreenCapture sCapture;

        public static void Init()
        {
            sCapture = new ScreenCapture();
        }

        public static void CaptureScreen()
        {
            // Safe to create the directory
            // Determine whether the directory exists.
            if (!Directory.Exists(DIR_PATH))
            {
                // Try to create the directory.
                DirectoryInfo di = Directory.CreateDirectory(DIR_PATH);
            }

            // Retrieve Active Window Name
            string title = GetActiveWindowTitle();

            // Take Image
            Image img = sCapture.CaptureScreen();

            // Naming
            Int32 unixTimestamp = (Int32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;

            string fileName = unixTimestamp.ToString() + "_" + title + ".png";
            fileName = MakeValidFilename(fileName);

            // Save Image
            sCapture.CaptureWindowToFile(DIR_PATH + "/" + fileName, ImageFormat.Png);
        }

        /*
        static string MakeValidFilename(string text)
        {
            text = text.Replace('\'', '’'); // U+2019 right single quotation mark
            text = text.Replace('"', '”'); // U+201D right double quotation mark
            text = text.Replace('/', '⁄');  // U+2044 fraction slash
            text = text.Replace('\\', '⁄');  // U+2044 fraction slash
            foreach (char c in System.IO.Path.GetInvalidFileNameChars())
            {
                text = text.Replace(c, '_');
            }
            return text;
        }
         */

        static string MakeValidFilename(string arbitraryString)
        {
            var invalidChars = System.IO.Path.GetInvalidFileNameChars();
            var replaceIndex = arbitraryString.IndexOfAny(invalidChars, 0);
            if (replaceIndex == -1) return arbitraryString;

            var r = new StringBuilder();
            var i = 0;

            do
            {
                r.Append(arbitraryString, i, replaceIndex - i);

                switch (arbitraryString[replaceIndex])
                {
                    case '"':
                        r.Append("''");
                        break;
                    case '<':
                        r.Append('\u02c2'); // '˂' (modifier letter left arrowhead)
                        break;
                    case '>':
                        r.Append('\u02c3'); // '˃' (modifier letter right arrowhead)
                        break;
                    case '|':
                        r.Append('\u2223'); // '∣' (divides)
                        break;
                    case ':':
                        r.Append('-');
                        break;
                    case '*':
                        r.Append('\u2217'); // '∗' (asterisk operator)
                        break;
                    case '\\':
                    case '/':
                        r.Append('\u2044'); // '⁄' (fraction slash)
                        break;
                    case '\0':
                    case '\f':
                    case '?':
                        break;
                    case '\t':
                    case '\n':
                    case '\r':
                    case '\v':
                        r.Append(' ');
                        break;
                    default:
                        r.Append('_');
                        break;
                }

                i = replaceIndex + 1;
                replaceIndex = arbitraryString.IndexOfAny(invalidChars, i);
            } while (replaceIndex != -1);

            r.Append(arbitraryString, i, arbitraryString.Length - i);

            return r.ToString();
        }

        private static string GetActiveWindowTitle()
        {
            const int nChars = 256;
            StringBuilder Buff = new StringBuilder(nChars);
            IntPtr handle = GetForegroundWindow();

            if (GetWindowText(handle, Buff, nChars) > 0)
            {
                return Buff.ToString();
            }
            return null;
        }

        [DllImport("user32.dll")]
        static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int count);
    }
}
