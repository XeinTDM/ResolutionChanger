# ResolutionChanger
ResolutionChanger is a simple WPF application for changing the display resolution of your monitors. It utilizes Windows API functions to enumerate display devices and settings, allowing users to select and apply different resolutions to their displays.

### Features
- Enumerates and lists all connected display devices.
- Retrieves and displays all available resolutions for each display.
- Allows users to change the resolution of a selected display.
- Saves the selected resolution as the default setting for the display.

### Usage
1. **Launch the Application:** Open the `ResolutionChanger` application.
2. **Select a Display:** Choose the display you want to change the resolution for from the `DisplaysComboBox`.
3. **Select a Resolution:** Pick the desired resolution from the `ResolutionsComboBox`.
4. **Apply Changes:** Click the `Save` button to apply the selected resolution to the display.

## Code Overview
### WinAPI Integration
The application uses several WinAPI functions to interact with display settings:

- `ChangeDisplaySettingsEx:` Changes the settings of the specified display device to the specified graphics mode.
- `EnumDisplaySettings:` Retrieves information about one of the graphics modes for a display device.
- `EnumDisplayDevices:` Obtains information about the display devices in the current session.

### Structs and Enums
The application defines several structures and enumerations to hold display settings information:

- `DISPLAY_DEVICE:` Contains information about the display device.
- `DEVMODE:` Contains information about the initialization and environment of a printer or a display device.
- `DisplayDeviceStateFlags:` Flags that define the state of a display device.

### Main Logic
- **Initialization:** The main window initializes and loads displays and their resolutions.
- **Button Click:** Handles the event when the user clicks the Save button to change the display resolution.
- **Load Displays:** Enumerates and populates the DisplaysComboBox with connected display devices.
- **Load Resolutions:** Enumerates and populates the ResolutionsComboBox with available resolutions for the selected display.
- **GetDisplayDevices:** Helper function to list all connected display devices.

### Requirements
- .NET 8.0
- Windows OS

### Installation
1. Clone the repository:
```bash
git clone https://github.com/XeinTDM/ResolutionChanger.git
```
2. Open the solution in Visual Studio.
3. Build and run the project.

### License
[![License: Unlicense](https://img.shields.io/badge/license-Unlicense-blue.svg)](http://unlicense.org/)

Feel free to use, modify, and distribute the code as you wish.
