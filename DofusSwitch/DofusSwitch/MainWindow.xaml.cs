using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;


namespace DofusSwitch
{
    /// <summary>
    /// Logique d'interaction pour MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // CONSTANTS
        private int MAX_ITEM = 8;

        // DOCKPANEL WIDGETS POSITION
        private int TB_ID = 0;
        private int TB_PID = 1;
        private int TB_TITLE = 2;
        private int BTN_RESTORE = 3;
        private int BTN_RESET = 4;

        // HOTKEY MODS
        public const int NOMOD = 0x0000;
        public const int ALT = 0x0001;
        public const int CTRL = 0x0002;
        public const int SHIFT = 0x0004;
        public const int WIN = 0x0008;

        // HOTKEY SIGNAL
        public const int WM_HOTKEY_MSG_ID = 0x0312;

        // START KEY // 49 = 1
        private int KEY = 49;

        // VARS
        private int current_id = -1;

        private int[] pids = new int[8];
        private string[] titles = new string[8];

        private IntPtr _windowHandle;
        private HwndSource _source;


        public MainWindow()
        {
            InitializeComponent();

            // Variables
            for (int i = 0; i < pids.Length; ++i) { pids[i] = 0; }
            for (int i = 0; i < titles.Length; ++i) { titles[i] = String.Empty; }

            // UI
            for (int i = 0; i < MAX_ITEM; ++i)
            {
                MyGrid.RowDefinitions.Add(new RowDefinition());

                MyDockPanel d = new MyDockPanel(i);
                d.PreviewMouseRightButtonUp += Dock_MouseRightButtonUp;

                Grid g = d.Children[0] as Grid;
                // Restore Events
                MyButton b = g.Children[BTN_RESTORE] as MyButton;
                b.Click += Restore_Click;
                MyButton c = g.Children[BTN_RESET] as MyButton;
                c.Click += Reset_Click;


                Grid.SetRow(d, i);
                MyGrid.Children.Add(d);
            }

            WriteLine("Permet de restaurer des fenêtres en appuyant sur ALT et un nombre du haut {1-8}");
        }


        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);

            _windowHandle = new WindowInteropHelper(this).Handle;
            _source = HwndSource.FromHwnd(_windowHandle);
            _source.AddHook(HwndHook);
            
            for (int i = 0; i < MAX_ITEM; ++i)
            {
                // Only work on top numbers
                if (RegisterHotKey(_windowHandle, i, ALT, (uint)(KEY + i)))
                {
                    WriteLine("InitHotkeys => Success to register Hotkey with id: " + i + " and key: " + (KEY + i));
                }
                else
                {
                    WriteLine("InitHotkeys => Failed to register Hotkey with id: " + i + " and key: " + (KEY + i));
                }
            }
        }

        // Executed when window is closing
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            _source.RemoveHook(HwndHook);

            for (int i = 0; i < 8; ++i)
            {
                if (UnregisterHotKey(_windowHandle, i))
                {
                    WriteLine("InitHotkeys => Success to unregister Hotkey with id: " + i);
                }
                else
                {
                    WriteLine("InitHotkeys => Failed to unregister Hotkey with id: " + i);
                }
            }
            base.OnClosed(e);
        }


        // Output Function
        public void WriteLine(string text)
        {
            tb_Output.Text += text + Environment.NewLine;
        }


        // DockPanel and ContextMenu
        private void Dock_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            MyDockPanel dp = sender as MyDockPanel;
            dp.ContextMenu = new ContextMenu();

            WriteLine("DockPanel_MouseRightButtonUp with ID: " + dp.Identifier());

            // Process[] processes = Process.GetProcessesByName("Dofus");
            Process[] processes = Process.GetProcesses();

            if (processes.Length == 0)
            {
                this.current_id = -1;

                MyMenuItem mi = new MyMenuItem(0, "Error: Unable to find processes");
                // MyMenuItem mi = new MyMenuItem(0, "No such process");

                mi.Click += MenuItem_Click;

                dp.ContextMenu.Items.Add(mi);
            }
            else
            {
                this.current_id = dp.Identifier();

                for (int i = 0; i < processes.Length; ++i)
                {
                    if (processes[i].MainWindowTitle == "")
                        continue;

                    // Name = StackPanel Name + Process PID
                    // Header = Process Title
                    MyMenuItem mi = new MyMenuItem(processes[i].Id, processes[i].MainWindowTitle);
                    
                    mi.Click += MenuItem_Click;

                    dp.ContextMenu.Items.Add(mi);
                }
            }
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            MyMenuItem mi = sender as MyMenuItem;

            WriteLine("MenuItem_Click => event recieved with Name: " + mi.Name + "and Header: " + mi.Header);

            // Test if Context Menu is filled
            if (mi.PID() == 0 || (string)mi.Header == "No such process")
            {
                return;
            }

            MenuItemSelected(this.current_id, (string)mi.Header, mi.PID());
        }

        // Call when a MenuIten is selected
        private void MenuItemSelected(int identifier, string processTitle, int processId)
        {
            this.pids[identifier] = processId;
            this.titles[identifier] = processTitle;

            DockPanel dp = MyGrid.Children[identifier] as DockPanel;
            Grid g = dp.Children[0] as Grid;

            MyTextBlock tb = g.Children[TB_PID] as MyTextBlock;
            tb.Text = "PID: " + processId.ToString();

            tb = g.Children[TB_TITLE] as MyTextBlock;
            tb.Text = "Title: " + processTitle;
        }


        // Buttons Events
        private void Restore_Click(object sender, RoutedEventArgs e)
        {
            MyButton b = sender as MyButton;
            int id = b.Identifier();

            WriteLine("Restore_Click => Event recieved with id: " + id);
            if (this.titles[id] == String.Empty)
            {
                return;
            }
            
            Utils.MinProcess(this.pids[id]);
            Utils.MaxProcess(this.pids[id]);
        }

        private void Reset_Click(object sender, RoutedEventArgs e)
        {
            MyButton b = sender as MyButton;
            int id = b.Identifier();

            this.pids[id] = 0;
            this.titles[id] = String.Empty;

            DockPanel dp = MyGrid.Children[id] as DockPanel;
            Grid g = dp.Children[0] as Grid;

            MyTextBlock tb = g.Children[TB_PID] as MyTextBlock;
            tb.Text = "PID: ";

            tb = g.Children[TB_TITLE] as MyTextBlock;
            tb.Text = "Title: ";
        }

        private IntPtr HwndHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            const int WM_HOTKEY = 0x0312;
            switch (msg)
            {
                case WM_HOTKEY:
                    int id = wParam.ToInt32();

                    WriteLine("HwndHook => WndProc Triggered with id: " + id);
                    
                    if (this.titles[id] != String.Empty)
                    {
                        Utils.MinProcess(this.pids[id]);
                        Utils.MaxProcess(this.pids[id]);
                    }
                    break;
            }
            return IntPtr.Zero;
        }

        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);
    }

    public class MyMenuItem: MenuItem
    {
        private int pid;

        public MyMenuItem(int pid, string title)
        {
            this.pid = pid;

            Header = title;
        }

        public void PID(int pid)
        {
            this.pid = pid;
        }

        public int PID()
        {
            return this.pid;
        }
    }

    public class MyButton : Button
    {
        private int identifier;

        public int Identifier() { return this.identifier; }
        public void Identifier(int value) { this.identifier = value; }

        public MyButton(int id, string content)
        {
            this.identifier = id;

            Width = 75f;
            Height = 35f;
            // Margin = new Thickness(10f, 0f, 10f, 0f);
            Content = content;
        }
    }

    public class MyTextBlock : TextBlock
    {
        private int identifier;

        public int Identifier() { return this.identifier; }
        public void Identifier(int value) { this.identifier = value; }

        private string pre;
        private string value;
                
        public MyTextBlock(int id, string pre)
        {
            this.identifier = id;
            this.pre = pre;
            
            Margin = new Thickness(10f, 0f, 10f, 0f);
            // TextAlignment = TextAlignment.Center;
            VerticalAlignment = VerticalAlignment.Center;

            SetText();
        }

        private void SetText()
        {
            Text = pre + value;
        }

        public string Pre()
        {
            return this.pre;
        }

        public string Value()
        {
            return this.value;
        }

        public void Pre(string pre)
        {
            this.pre = pre;
            SetText();
        }

        public void Value(string value)
        {
            this.value = value;
            SetText();
        }
    }

    public class MyDockPanel : DockPanel
    {
        private int identifier;

        public int Identifier() { return this.identifier; }
        public void Identifier(int value) { this.identifier = value; }

        public MyDockPanel(int identifier)
        {
            this.identifier = identifier;

            Background = new SolidColorBrush(Colors.White);

            ColumnDefinition s0 = new ColumnDefinition();
            ColumnDefinition s1 = new ColumnDefinition();
            ColumnDefinition s2 = new ColumnDefinition();
            ColumnDefinition s3 = new ColumnDefinition();
            ColumnDefinition s4 = new ColumnDefinition();

            s0.Width = GridLength.Auto;
            s1.Width = new GridLength(1f, GridUnitType.Star);
            s2.Width = new GridLength(3f, GridUnitType.Star);
            s3.Width = new GridLength(2f, GridUnitType.Star);
            s4.Width = new GridLength(2f, GridUnitType.Star);


            Grid g = new Grid();
            g.ColumnDefinitions.Add(s0);
            g.ColumnDefinitions.Add(s1);
            g.ColumnDefinitions.Add(s2);
            g.ColumnDefinitions.Add(s3);
            g.ColumnDefinitions.Add(s4);


            MyTextBlock id = new MyTextBlock(identifier, identifier.ToString());

            MyTextBlock pid = new MyTextBlock(identifier, "PID: ");

            MyTextBlock title = new MyTextBlock(identifier, "Title: ");

            MyButton restore = new MyButton(identifier, "Restore");

            MyButton reset = new MyButton(identifier, "Reset");

            Grid.SetColumn(id, 0);
            g.Children.Add(id);

            Grid.SetColumn(pid, 1);
            g.Children.Add(pid);

            Grid.SetColumn(title, 2);
            g.Children.Add(title);

            Grid.SetColumn(restore, 3);
            g.Children.Add(restore);

            Grid.SetColumn(reset, 4);
            g.Children.Add(reset);

            Children.Add(g);
        }
    }
}
