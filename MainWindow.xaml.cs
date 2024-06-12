using System.Runtime.InteropServices;
using System.Windows;

namespace ResolutionChanger
{
    public partial class MainWindow : Window
    {
        #region WinAPI
        [DllImport("user32.dll")]
        private static extern int ChangeDisplaySettingsEx(string lpszDeviceName, ref DEVMODE lpDevMode, IntPtr hwnd, int dwflags, IntPtr lParam);

        [DllImport("user32.dll")]
        private static extern bool EnumDisplaySettings(string deviceName, int modeNum, ref DEVMODE devMode);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern bool EnumDisplayDevices(string lpDevice, uint iDevNum, ref DISPLAY_DEVICE lpDisplayDevice, uint dwFlags);

        private const int ENUM_CURRENT_SETTINGS = -1;
        private const int CDS_UPDATEREGISTRY = 0x01;
        private const int DISP_CHANGE_SUCCESSFUL = 0;
        private const uint EDD_GET_DEVICE_INTERFACE_NAME = 0x00000001;

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        private struct DISPLAY_DEVICE
        {
            [MarshalAs(UnmanagedType.U4)]
            public int cb;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
            public string DeviceName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
            public string DeviceString;
            [MarshalAs(UnmanagedType.U4)]
            public DisplayDeviceStateFlags StateFlags;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
            public string DeviceID;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
            public string DeviceKey;
        }

        [Flags]
        private enum DisplayDeviceStateFlags : int
        {
            AttachedToDesktop = 0x1,
            MultiDriver = 0x2,
            PrimaryDevice = 0x4,
            MirroringDriver = 0x8,
            VGACompatible = 0x10,
            Removable = 0x20,
            ModesPruned = 0x8000000,
            Remote = 0x4000000,
            Disconnect = 0x2000000
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct DEVMODE
        {
            private const int CCHDEVICENAME = 32;
            private const int CCHFORMNAME = 32;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = CCHDEVICENAME)]
            public string dmDeviceName;
            public ushort dmSpecVersion;
            public ushort dmDriverVersion;
            public ushort dmSize;
            public ushort dmDriverExtra;
            public uint dmFields;

            public int dmPositionX;
            public int dmPositionY;
            public uint dmDisplayOrientation;
            public uint dmDisplayFixedOutput;

            public short dmColor;
            public short dmDuplex;
            public short dmYResolution;
            public short dmTTOption;
            public short dmCollate;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = CCHFORMNAME)]
            public string dmFormName;
            public ushort dmLogPixels;
            public ushort dmBitsPerPel;
            public uint dmPelsWidth;
            public uint dmPelsHeight;

            public uint dmDisplayFlags;
            public uint dmNup;
            public uint dmDisplayFrequency;

            public uint dmICMMethod;
            public uint dmICMIntent;
            public uint dmMediaType;
            public uint dmDitherType;
            public uint dmReserved1;
            public uint dmReserved2;
            public uint dmPanningWidth;
            public uint dmPanningHeight;
        }
        #endregion

        public MainWindow()
        {
            InitializeComponent();
            LoadDisplays();
            LoadDisplayResolutions();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (DisplaysComboBox.SelectedItem != null && ResolutionsComboBox.SelectedItem != null)
            {
                string selectedDisplay = DisplaysComboBox.SelectedItem.ToString();
                string selectedResolution = ResolutionsComboBox.SelectedItem.ToString().Replace(" (Default Resolution)", "");
                string[] resolution = selectedResolution.Split('x');
                int width = int.Parse(resolution[0]);
                int height = int.Parse(resolution[1]);

                DEVMODE devMode = new()
                {
                    dmSize = (ushort)Marshal.SizeOf(typeof(DEVMODE))
                };

                if (EnumDisplaySettings(selectedDisplay, ENUM_CURRENT_SETTINGS, ref devMode))
                {
                    devMode.dmPelsWidth = (uint)width;
                    devMode.dmPelsHeight = (uint)height;

                    int result = ChangeDisplaySettingsEx(selectedDisplay, ref devMode, IntPtr.Zero, CDS_UPDATEREGISTRY, IntPtr.Zero);

                    if (result == DISP_CHANGE_SUCCESSFUL)
                    {
                        System.Windows.MessageBox.Show("Resolution changed successfully.");
                    }
                    else
                    {
                        System.Windows.MessageBox.Show("Failed to change resolution. Error code: " + result);
                        ResolutionsComboBox.Items.Remove(selectedResolution);
                    }
                }
                else
                {
                    System.Windows.MessageBox.Show("Failed to get the current display settings.");
                    DisplaysComboBox.Items.Remove(selectedDisplay);
                    DisplaysComboBox.SelectedIndex = 0;
                }
            }
            else
            {
                System.Windows.MessageBox.Show("No display or resolution selected.");
            }
        }

        private void LoadDisplays()
        {
            DisplaysComboBox.Items.Clear();
            List<string> displays = GetDisplayDevices();
            foreach (var display in displays)
            {
                DisplaysComboBox.Items.Add(display);
            }
            if (DisplaysComboBox.Items.Count > 0)
                DisplaysComboBox.SelectedIndex = 0;
        }

        private void LoadDisplayResolutions()
        {
            ResolutionsComboBox.Items.Clear();
            HashSet<string> addedResolutions = [];

            if (DisplaysComboBox.SelectedItem != null)
            {
                string selectedDisplay = DisplaysComboBox.SelectedItem.ToString();

                DEVMODE devMode = new();
                if (EnumDisplaySettings(selectedDisplay, ENUM_CURRENT_SETTINGS, ref devMode))
                {
                    string currentResolution = $"{devMode.dmPelsWidth}x{devMode.dmPelsHeight}";
                    ResolutionsComboBox.Items.Add(currentResolution + " (Default Resolution)");
                    ResolutionsComboBox.SelectedItem = currentResolution + " (Default Resolution)";
                    addedResolutions.Add(currentResolution);

                    int modeNum = 0;
                    while (EnumDisplaySettings(null, modeNum, ref devMode))
                    {
                        string resolution = $"{devMode.dmPelsWidth}x{devMode.dmPelsHeight}";
                        if (!addedResolutions.Contains(resolution))
                        {
                            ResolutionsComboBox.Items.Add(resolution);
                            addedResolutions.Add(resolution);
                        }
                        modeNum++;
                    }
                }
            }
        }

        private List<string> GetDisplayDevices()
        {
            List<string> displays = [];
            DISPLAY_DEVICE d = new();
            d.cb = Marshal.SizeOf(d);
            int devNum = 0;

            while (EnumDisplayDevices(null, (uint)devNum, ref d, EDD_GET_DEVICE_INTERFACE_NAME))
            {
                displays.Add(d.DeviceName);
                devNum++;
            }

            return displays;
        }
    }
}