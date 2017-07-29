# Sprung Window Switcher

[![Build status](https://ci.appveyor.com/api/projects/status/nbolylxaxwco3hbb/branch/master?svg=true)](https://ci.appveyor.com/project/guija/sprung/branch/master)

A windows application to rapidly switch between multiple windows.  
Windows are searched by it's window title. The search algorithm is inspired by the command palette from the text editor [Sublime Text](https://www.sublimetext.com/).  
Sprung is intended to be used only with the keyboard. Therefore the UI is deliberately simple.  

## Download
You can download the Sprung installer from http://sprung.netzauftrag.com/releases/  

## Installation
Download the installer of the most recent version from http://sprung.netzauftrag.com/releases/  
Run the installer. The installer will create a shorcut on your desktop. Start Sprung by clicking on that shortcut.
Enjoy by using the default keyboard shortcut `Alt+Space`  
You can configure the shortcut yourself (See Settings below)  

If you want Sprung to be launched on every system startup just put a copy of the shorcut in your startup folder. (You can open your startup folder by typing `Win+R` -> `shell:startup` -> `Enter`)

## Demo
Here is a simple demo of how Sprung works:  
![alt Sprung Windows Switcher Demo](http://sprung.netzauftrag.com/demos/SprungDemoShort.gif)

## Shortcuts
There are only two shortcuts available:  
`Alt+Space`: Open Sprung Window Switcher  
`Alt+Shift+Space`: Open Sprung Window Switcher and list tabs for each browser as own windows  

## Tabs support
Please note that the tabs support is currently in alpha stadium and is available for test purposes only. Currently supported browsers are:
* Firefox
* Internet Explorer

## Settings
The settings can be modified by changing the `settings.json` file.  
Example settings.json file:

    {
        "excluded": [
            "^$",
            "^Sprung$",
            "^Program Manager - explorer$",
            "^Start - explorer$",
            "^AMD:CCC-AEMCapturingWindow$",
            "^Windows Shell Experience Host - ShellExperienceHost$"
        ],
        "list_tabs_as_windows": false,
        "open_window_list": "Alt+Space",
        "open_window_list_with_tabs" : "Alt+Shift+Space"
    }

Options:

* `excluded`: Contains a list of regular expressions. Windows with titles that matches any of the regular expression will not be listed.
* `list_tabs_as_windows`: Flag if by default with the normal `open_window_list` shortcut each browser tab should be listed as a window each (See tab support section for more info)
* `open_window_list`: Shortcut for opening Sprung Window Switcher
* `open_window_list_with_tabs`: Shortcut for opening Sprung Window Switcher with listing each tab as an own window. (See tab support section for more info)

## Contributing
Any contributions and ideas are welcome. For questions please contact me at gj@netzauftrag.com
