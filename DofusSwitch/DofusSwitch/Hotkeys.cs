using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace DofusSwitch
{
    public static class Constants
    {
        //modifiers
        public const int NOMOD = 0x0000;
        public const int ALT = 0x0001;
        public const int CTRL = 0x0002;
        public const int SHIFT = 0x0004;
        public const int WIN = 0x0008;

        //windows message id for hotkey
        public const int WM_HOTKEY_MSG_ID = 0x0312;
    }

    public class Hotkey
    {
        private int modifier;
        private int key;
        private IntPtr hWnd;
        private int id;

        public delegate void OutputCb(string text);

        public Hotkey(int modifier, int key, IntPtr hWnd)
        {
            this.modifier = modifier;
            this.key = key;
            this.hWnd = hWnd;
            id = this.GetHashCode();
        }

        public Hotkey(int id, int modifier, int key, IntPtr hWnd)
        {
            this.modifier = modifier;
            this.key = key;
            this.hWnd = hWnd;
            id = this.GetHashCode();
        }

        public bool Register()
        {
            return RegisterHotKey(hWnd, id, modifier, key);
        }

        public bool Unregister()
        {
            return UnregisterHotKey(hWnd, id);
        }

        public override int GetHashCode()
        {
            return modifier ^ key ^ hWnd.ToInt32();
        }

        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, int fsModifiers, int vk);

        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);
    }

    /*
    public class Hotkeys
    {
        private List<Hotkey> hks;

        public List<Hotkey> Hks { get => hks; set => hks = value; }

        public Hotkeys()
        {
            Hks = new List<Hotkey>();
        }

        public void AddElement(int modifier, int key, int pid)
        {
            Hotkey hk = new Hotkey(modifier, key, pid);

            Trace.WriteLine("New element with pid: " + pid);

            Hks.Add(hk);
        }

        public bool RemoveElement(int index)
        {
            try
            {
                Hks.RemoveAt(index);

                Trace.WriteLine("Remove element at index: " + index);
            }
            catch (IndexOutOfRangeException e)
            {
                Trace.WriteLine("Failed to remove element: " + index + " with message: " + e.Message);

                return false;
            }

            return true;
        }


        public bool RegisterElement(int index)
        {
            try
            {
                if (Hks[index].Register())
                {
                    Trace.WriteLine("Register element at index: " + index);

                    return true;
                }
            }
            catch (IndexOutOfRangeException e)
            {
                Trace.WriteLine("Failed to register element: " + index + " with message: " + e.Message);
            }

            return false;
        }

        public bool RegisterAll()
        {
            for (int i = 0; i < Hks.Count; ++i)
            {
                if (!RegisterElement(i))
                {
                    return false;
                }
            }

            return true;
        }


        public bool UnregisterElement(int index)
        {
            try
            {
                if (Hks[index].Unregiser())
                {
                    Trace.WriteLine("Unregister element at index: " + index);

                    return true;
                }
            }
            catch (IndexOutOfRangeException e)
            {
                Trace.WriteLine("Failed to unregister element: " + index + " with message: " + e.Message);

                // Set IndexOutOfRangeException to the new exception's InnerException.
                // throw new System.ArgumentOutOfRangeException("index parameter is out of range.", e);
            }

            return false;
        }

        public bool UnregisterAll()
        {
            for (int i = 0; i < Hks.Count; ++i)
            {
                if (!UnregisterElement(i))
                {
                    return false;
                }
            }

            return true;
        }
    }
    */
}
