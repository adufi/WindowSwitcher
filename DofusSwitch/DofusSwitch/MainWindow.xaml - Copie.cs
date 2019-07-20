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
        private int MAX_ITEM        = 8;

        private int TB_ID           = 0;
        private int TB_TITLE        = 1;
        private int TB_PID          = 2;
        private int TB_PID_NB       = 3;
        private int BTN_RESTORE     = 4;
        private int BTN_REGISTER    = 5;
        private int BTN_RESET       = 5;

        private int KEY             = 49;
        private int MODIFIER        = Constants.ALT + Constants.SHIFT;

        private int current_id = -1;

        public MainWindow()
        {
            InitializeComponent();

            for (int i = 0; i < 8; ++i)
            {
                MyGrid.RowDefinitions.Add(new RowDefinition());

                MyDockPanel d = new MyDockPanel(i);
                d.PreviewMouseRightButtonUp += Dock_MouseRightButtonUp;

                Grid.SetRow(d, i);
                MyGrid.Children.Add(d);
            }

            WriteLine("Children: " + MyGrid.Children[7]);
        }

        private void Dock_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            MyDockPanel dp = sender as MyDockPanel;
            dp.ContextMenu = new ContextMenu();

            WriteLine("DockPanel_MouseRightButtonUp with ID: " + dp.Identifier());

            Process[] processes = Process.GetProcessesByName("Dofus");

            if (processes.Length == 0)
            {
                this.current_id = -1;

                MyMenuItem mi = new MyMenuItem(0, "No such process");

                mi.Click += MenuItem_Click;

                dp.ContextMenu.Items.Add(mi);
            }
            else
            {
                this.current_id = dp.Identifier();

                for (int i = 0; i < processes.Length; ++i)
                {
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
            DockPanel dp = MyGrid.Children[identifier] as DockPanel;
            Grid g = dp.Children[0] as Grid;

            MyTextBlock tb = g.Children[1] as MyTextBlock;
            tb.Value(processId.ToString());

            tb = g.Children[2] as MyTextBlock;
            tb.Value(processTitle);
        }





        // Init
        private void Init_StackPanel()
        {
            UIElementCollection children = MainSP.Children;

            foreach (var child in children)
            {
                StackPanel sp = child as StackPanel;

                Button btn = null;
                TextBlock tb = null;
                
                tb = sp.Children[TB_ID] as TextBlock;
                tb.Name = sp.Name + "_tb" + TB_ID;

                tb = sp.Children[TB_TITLE] as TextBlock;
                tb.Name = sp.Name + "_tb" + TB_TITLE;

                tb = sp.Children[TB_PID] as TextBlock;
                tb.Name = sp.Name + "_tb" + TB_PID;

                tb = sp.Children[TB_PID_NB] as TextBlock;
                tb.Name = sp.Name + "_tb" + TB_PID_NB;

                btn = sp.Children[BTN_RESTORE] as Button;
                btn.Name = sp.Name + "_btn" + BTN_RESTORE;
                btn.Click += Restore_Click;

                btn = sp.Children[BTN_REGISTER] as Button;
                btn.Name = sp.Name + "_btn" + BTN_REGISTER;
                btn.Click += Register_Click;

                btn = sp.Children[BTN_RESET] as Button;
                btn.Name = sp.Name + "_btn" + BTN_RESET;
                btn.Click += Reset_Click;
            }
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);

            _windowHandle = new WindowInteropHelper(this).Handle;
            _source = HwndSource.FromHwnd(_windowHandle);
            _source.AddHook(HwndHook);
            
            for (int i = 0; i < 8; ++i)
            {
                if (RegisterHotKey(_windowHandle, i, MOD_CONTROL, (uint)(KEY + i)))
                {
                    WriteLine("InitHotkeys => Success to register Hotkey with id: " + i + " and key: " + (KEY + i));
                }
                else
                {
                    WriteLine("InitHotkeys => Failed to register Hotkey with id: " + i + " and key: " + (KEY + i));
                }
            }
        }

        // LOGICS
        // Call when a MenuIten is selected
        private void MenuItemSelected2(int spId, string processTitle, int processId)
        {
            StackPanel sp = MainSP.Children[spId - 1] as StackPanel;

            TextBlock tb = sp.Children[TB_TITLE] as TextBlock;
            tb.Text = "Title: " + processTitle;

            tb = sp.Children[TB_PID_NB] as TextBlock;
            tb.Text = processId.ToString();
        }

        // Output Function
        public void WriteLine(string text)
        {
            tb_Output.Text += text + Environment.NewLine;
        }

        // Find Id's sender
        private int SenderId(object sender, string intro)
        {
            Button btn = sender as Button;

            WriteLine(intro + " => event recieved with Name: " + btn.Name);

            string[] nameSplit = btn.Name.Split('_');

            if (nameSplit.Length == 0)
            {
                WriteLine(intro + " => Length null for Button: " + btn.Name);
                return -1;
            }
            // Get StackPanel ID
            string s1 = Utils.GetAlphaString(nameSplit[0]);
            string s2 = Utils.GetAlphaString(nameSplit[1]);

            int spId = -1;

            if (!Int32.TryParse(s1, out spId))
            {
                WriteLine(intro + " => Failed to parse values");
                return -1;
            }
            
            return spId;
        }


        private const uint VK_CAPITAL = 0x14;
        private const uint MOD_CONTROL = 0x0002; //CTRL
        private const int HOTKEY_ID = 9000;
        private IntPtr _windowHandle;
        private HwndSource _source;
        

        // EVENTS
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

        // Build a Context Menu
        private void StackPanel_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            WriteLine("StackPanel_MouseRightButtonUp");

            StackPanel sp = sender as StackPanel;
            sp.ContextMenu = new ContextMenu();

            Process[] processes = Process.GetProcessesByName("Dofus");

            if (processes.Length == 0)
            {
                MenuItem mi = new MenuItem();
                
                mi.Click += MenuItem_Click2;
                mi.Header = "No such process";
            }
            else
            {
                for (int i = 0; i < processes.Length; ++i)
                {
                    // Name = StackPanel Name + Process PID
                    // Header = Process Title
                    MenuItem mi = new MenuItem();

                    mi.Name = sp.Name + "_mi" + processes[i].Id;
                    mi.Click += MenuItem_Click2;
                    mi.Header = processes[i].MainWindowTitle;

                    sp.ContextMenu.Items.Add(mi);
                }
            }
        }

        // 
        private void MenuItem_Click2(object sender, RoutedEventArgs e)
        {
            MenuItem mi = sender as MenuItem;

            WriteLine("MenuItem_Click => event recieved with Name: " + mi.Name + "and Header: " + mi.Header);

            // Test if Context Menu is filled
            if ((string)mi.Header == "No such process")
            {
                return;
            }
            
            // Parse StackPanel Name
            string[] nameSplit = mi.Name.Split('_');
            foreach (var ns in nameSplit)
            {
               WriteLine($"<{ns}>");
            }

            if (nameSplit.Length == 0)
            {
                WriteLine("MenuItem_Click => Length null for MenuItem: " + mi.Name);
                return;
            }
            // Get StackPanel ID
            string s1 = Utils.GetAlphaString(nameSplit[0]);
            string s2 = Utils.GetAlphaString(nameSplit[1]);

            int senderId = 0;
            int processId = 0;
            string processTitle = mi.Header as String;

            if (!Int32.TryParse(s1, out senderId) || !Int32.TryParse(s2, out processId))
            {
                WriteLine("MenuItem_Click => Failed to parse values");
                return;
            }

            MenuItemSelected2(senderId, processTitle, processId);
        }

        // Buttons Events
        private void Restore_Click(object sender, RoutedEventArgs e)
        {
            int spId = 0;

            if ((spId = SenderId(sender, "Restore_Click")) == -1)
            {
                return;
            }

            StackPanel sp = MainSP.Children[spId - 1] as StackPanel;
            TextBlock tb = sp.Children[TB_PID_NB] as TextBlock;

            if (tb.Text == String.Empty)
            {
                WriteLine("Restore_Click => No PID is given");
                return;
            }

            Utils.MinProcess(Int32.Parse(tb.Text));
            Utils.MaxProcess(Int32.Parse(tb.Text));
        }


        private void Register_Click(object sender, RoutedEventArgs e)
        {
            /*
            int spId = 0;

            if ((spId = SenderId(sender, "Register_Click")) == -1)
            {
                return;
            }
            WriteLine("Register_Click => DEBUG: " + (HKs.Count));


            Button btn = sender as Button;
            
            if ((string)btn.Content == "Register")
            {
                btn.Content = "Unregister";

                // Register Hotkey
                // And check failure
                
                if (!HKs[spId - 1].Register())
                {
                    WriteLine("Register_Click => Failed to register Hotkey: " + spId);
                    return;
                }
                else
                {
                    WriteLine("Register_Click => Success to register Hotkey: " + spId);
                }
            }
            else
            {
                btn.Content = "Register";

                // Unregister Hotkey
                // And check failure
                if (!HKs[spId - 1].Unregister())
                {
                    WriteLine("Register_Click => Failed to unregister Hotkey: " + spId);
                    return;
                }
                else
                {
                    WriteLine("Register_Click => Success to unregister Hotkey: " + spId);
                }
            }
            */
        }


        private void Reset_Click(object sender, RoutedEventArgs e)
        {

        }

        private IntPtr HwndHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            const int WM_HOTKEY = 0x0312;
            switch (msg)
            {
                case WM_HOTKEY:
                    int id = wParam.ToInt32();

                    WriteLine("HwndHook => WndProc Triggered with id: " + id);

                    StackPanel sp = MainSP.Children[id] as StackPanel;

                    TextBlock tb = sp.Children[TB_PID_NB] as TextBlock;

                    if (tb.Text != String.Empty)
                    {
                        Utils.MinProcess(Int32.Parse(tb.Text));
                        Utils.MaxProcess(Int32.Parse(tb.Text));
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
