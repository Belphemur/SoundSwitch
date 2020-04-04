using System;
using System.Collections;
using System.Collections.Generic;
using System.Windows.Forms;

namespace SoundSwitch.Common.WinApi.Keyboard
{
    //A class to keep share procedures
    public static class HotKeyShared
    {
        /// <summary>Checks if a string is a valid Hotkey name.
        /// </summary>
        /// <param name="text">The string to check</param>
        /// <returns>true if the name is valid.</returns>
        public static bool IsValidHotkeyName(string text)
        {
            //If the name starts with a number, contains space or is null, return false.
            if (string.IsNullOrEmpty(text)) return false;

            if (text.Contains(" ") || char.IsDigit((char) text.ToCharArray().GetValue(0)))
                return false;

            return true;
        }

        /// <summary>Parses a shortcut string like 'Control + Alt + Shift + V' and returns the key and modifiers.
        /// </summary>
        /// <param name="text">The shortcut string to parse.</param>
        /// <returns>The Modifier in the lower bound and the key in the upper bound.</returns>
        public static object[] ParseShortcut(string text)
        {
            var hasAlt     = false;
            var hasControl = false;
            var hasShift   = false;
            var hasWin     = false;

            var  modifier = HotKey.ModifierKeys.None; //Variable to contain modifier.
            Keys key      = 0;                        //The key to register.
            int  current  = 0;

            string[] result;
            var      separators = new[] {" + "};
            result = text.Split(separators, StringSplitOptions.RemoveEmptyEntries);

            //Iterate through the keys and find the modifier.
            foreach (string entry in result)
            {
                //Find the Control Key.
                if (entry.Trim() == Keys.Control.ToString())
                {
                    hasControl = true;
                }

                //Find the Alt key.
                if (entry.Trim() == Keys.Alt.ToString())
                {
                    hasAlt = true;
                }

                //Find the Shift key.
                if (entry.Trim() == Keys.Shift.ToString())
                {
                    hasShift = true;
                }

                //Find the Window key.
                if (entry.Trim() == Keys.LWin.ToString() && current != result.Length - 1)
                {
                    hasWin = true;
                }

                current++;
            }

            if (hasControl)
            {
                modifier |= HotKey.ModifierKeys.Control;
            }

            if (hasAlt)
            {
                modifier |= HotKey.ModifierKeys.Alt;
            }

            if (hasShift)
            {
                modifier |= HotKey.ModifierKeys.Shift;
            }

            if (hasWin)
            {
                modifier |= HotKey.ModifierKeys.Win;
            }

            var keysConverter = new KeysConverter();
            key = (Keys) keysConverter.ConvertFrom(result.GetValue(result.Length - 1));

            return new object[] {modifier, key};
        }

        /// <summary>Parses a shortcut string like 'Control + Alt + Shift + V' and returns the key and modifiers.
        /// </summary>
        /// <param name="text">The shortcut string to parse.</param>
        /// <param name="separator">The delimiter for the shortcut.</param>
        /// <returns>The Modifier in the lower bound and the key in the upper bound.</returns>
        public static object[] ParseShortcut(string text, string separator)
        {
            var hasAlt     = false;
            var hasControl = false;
            var hasShift   = false;
            var hasWin     = false;

            var  modifier = HotKey.ModifierKeys.None; //Variable to contain modifier.
            Keys key      = 0;                        //The key to register.
            int  current  = 0;


            string[] result;
            string[] separators = {separator};
            result = text.Split(separators, StringSplitOptions.RemoveEmptyEntries);

            //Iterate through the keys and find the modifier.
            foreach (string entry in result)
            {
                //Find the Control Key.
                if (entry.Trim() == Keys.Control.ToString())
                {
                    hasControl = true;
                }

                //Find the Alt key.
                if (entry.Trim() == Keys.Alt.ToString())
                {
                    hasAlt = true;
                }

                //Find the Shift key.
                if (entry.Trim() == Keys.Shift.ToString())
                {
                    hasShift = true;
                }

                //Find the Window key.
                if (entry.Trim() == Keys.LWin.ToString() && current != result.Length - 1)
                {
                    hasWin = true;
                }

                current++;
            }

            if (hasControl)
            {
                modifier |= HotKey.ModifierKeys.Control;
            }

            if (hasAlt)
            {
                modifier |= HotKey.ModifierKeys.Alt;
            }

            if (hasShift)
            {
                modifier |= HotKey.ModifierKeys.Shift;
            }

            if (hasWin)
            {
                modifier |= HotKey.ModifierKeys.Win;
            }

            KeysConverter keyconverter = new KeysConverter();
            key = (Keys) keyconverter.ConvertFrom(result.GetValue(result.Length - 1));

            return new object[] {modifier, key};
        }

        /// <summary>Combines the modifier and key to a shortcut.
        /// Changes Control;Shift;Alt;T to Control + Shift + Alt + T
        /// </summary>
        /// <param name="mod">The modifier.</param>
        /// <param name="key">The key.</param>
        /// <returns>A string representation of the modifier and key.</returns>
        public static string CombineShortcut(HotKey.ModifierKeys mod, Keys key)
        {
            string hotkey = "";
            foreach (HotKey.ModifierKeys a in new ParseModifier((int) mod))
            {
                hotkey += a + " + ";
            }

            if (hotkey.Contains(HotKey.ModifierKeys.None.ToString())) hotkey = "";
            hotkey += key.ToString();
            return hotkey;
        }

        /// <summary>Combines the modifier and key to a shortcut.
        /// Changes Control;Shift;Alt; to Control + Shift + Alt
        /// </summary>
        /// <param name="mod">The modifier.</param>
        /// <returns>A string representation of the modifier</returns>
        public static string CombineShortcut(HotKey.ModifierKeys mod)
        {
            string hotkey = "";
            foreach (HotKey.ModifierKeys a in new ParseModifier((int) mod))
            {
                hotkey += a + " + ";
            }

            if (hotkey.Contains(HotKey.ModifierKeys.None.ToString())) hotkey = "";
            if (hotkey.Trim().EndsWith("+")) hotkey                          = hotkey.Trim().Substring(0, hotkey.Length - 1);

            return hotkey;
        }

        /// <summary>Allows the conversion of an integer to its modifier representation.
        /// </summary>
        public struct ParseModifier : IEnumerable
        {
            private List<HotKey.ModifierKeys> Enumeration;
            public  bool                      HasAlt;
            public  bool                      HasControl;
            public  bool                      HasShift;
            public  bool                      HasWin;

            /// <summary>Initializes this class.
            /// </summary>
            /// <param name="Modifier">The integer representation of the modifier to parse.</param>
            public ParseModifier(int Modifier)
            {
                Enumeration = new List<HotKey.ModifierKeys>();
                HasAlt      = false;
                HasWin      = false;
                HasShift    = false;
                HasControl  = false;
                switch (Modifier)
                {
                    case 0:
                        Enumeration.Add(HotKey.ModifierKeys.None);
                        break;
                    case 1:
                        HasAlt = true;
                        Enumeration.Add(HotKey.ModifierKeys.Alt);
                        break;
                    case 2:
                        HasControl = true;
                        Enumeration.Add(HotKey.ModifierKeys.Control);
                        break;
                    case 3:
                        HasAlt     = true;
                        HasControl = true;
                        Enumeration.Add(HotKey.ModifierKeys.Control);
                        Enumeration.Add(HotKey.ModifierKeys.Alt);
                        break;
                    case 4:
                        HasShift = true;
                        Enumeration.Add(HotKey.ModifierKeys.Shift);
                        break;
                    case 5:
                        HasShift = true;
                        HasAlt   = true;
                        Enumeration.Add(HotKey.ModifierKeys.Shift);
                        Enumeration.Add(HotKey.ModifierKeys.Alt);
                        break;
                    case 6:
                        HasShift   = true;
                        HasControl = true;
                        Enumeration.Add(HotKey.ModifierKeys.Shift);
                        Enumeration.Add(HotKey.ModifierKeys.Control);
                        break;
                    case 7:
                        HasControl = true;
                        HasShift   = true;
                        HasAlt     = true;
                        Enumeration.Add(HotKey.ModifierKeys.Shift);
                        Enumeration.Add(HotKey.ModifierKeys.Control);
                        Enumeration.Add(HotKey.ModifierKeys.Alt);
                        break;
                    case 8:
                        HasWin = true;
                        Enumeration.Add(HotKey.ModifierKeys.Win);
                        break;
                    case 9:
                        HasAlt = true;
                        HasWin = true;
                        Enumeration.Add(HotKey.ModifierKeys.Alt);
                        Enumeration.Add(HotKey.ModifierKeys.Win);
                        break;
                    case 10:
                        HasControl = true;
                        HasWin     = true;
                        Enumeration.Add(HotKey.ModifierKeys.Control);
                        Enumeration.Add(HotKey.ModifierKeys.Win);
                        break;
                    case 11:
                        HasControl = true;
                        HasAlt     = true;
                        HasWin     = true;
                        Enumeration.Add(HotKey.ModifierKeys.Control);
                        Enumeration.Add(HotKey.ModifierKeys.Alt);
                        Enumeration.Add(HotKey.ModifierKeys.Win);
                        break;
                    case 12:
                        HasShift = true;
                        HasWin   = true;
                        Enumeration.Add(HotKey.ModifierKeys.Shift);
                        Enumeration.Add(HotKey.ModifierKeys.Win);
                        break;
                    case 13:
                        HasShift = true;
                        HasAlt   = true;
                        HasWin   = true;
                        Enumeration.Add(HotKey.ModifierKeys.Shift);
                        Enumeration.Add(HotKey.ModifierKeys.Alt);
                        Enumeration.Add(HotKey.ModifierKeys.Win);
                        break;
                    case 14:
                        HasShift   = true;
                        HasControl = true;
                        HasWin     = true;
                        Enumeration.Add(HotKey.ModifierKeys.Shift);
                        Enumeration.Add(HotKey.ModifierKeys.Control);
                        Enumeration.Add(HotKey.ModifierKeys.Win);
                        break;
                    case 15:
                        HasShift   = true;
                        HasControl = true;
                        HasAlt     = true;
                        HasWin     = true;
                        Enumeration.Add(HotKey.ModifierKeys.Shift);
                        Enumeration.Add(HotKey.ModifierKeys.Control);
                        Enumeration.Add(HotKey.ModifierKeys.Alt);
                        Enumeration.Add(HotKey.ModifierKeys.Win);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException("The argument is parsed is more than the expected range", "Modifier");
                }
            }

            /// <summary>Initializes this class.
            /// </summary>
            /// <param name="mod">the modifier to parse.</param>
            public ParseModifier(HotKey.ModifierKeys mod) : this((int) mod)
            {
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return Enumeration.GetEnumerator();
            }
        }
    }
}