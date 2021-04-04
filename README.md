# fsriev - a file watch utility
[![GitHub release (latest SemVer)](https://img.shields.io/github/v/release/TehGM/fsriev?include_prereleases)](https://github.com/TehGM/fsriev/releases) [![GitHub top language](https://img.shields.io/github/languages/top/TehGM/fsriev)](https://github.com/TehGM/fsriev) [![GitHub](https://img.shields.io/github/license/TehGM/fsriev)](LICENSE) [![GitHub Workflow Status](https://img.shields.io/github/workflow/status/TehGM/fsriev/.NET%20Build)](https://github.com/TehGM/fsriev/actions) [![GitHub issues](https://img.shields.io/github/issues/TehGM/fsriev)](https://github.com/TehGM/fsriev/issues)

fsriev is a simple but highly customizable file watcher for Windows and Linux.

fsriev will watch a folder and its subfolders for file changes, and it'll trigger your set of commands when the change is detected. Multiple directories can be watched at once, with each directory having its own set of rules and commands to execute.  
fsriev uses [ASP.NET Core's Configuration system](https://docs.microsoft.com/en-gb/aspnet/core/fundamentals/configuration/?view=aspnetcore-5.0) which allows for a large variety of configuration approaches without using command arguments - although these are possible to use, too! By default, fsriev will use a settings file.

### Download
See [releases](https://github.com/TehGM/fsriev/releases/) to download the latest version for your machine.  
Alternatively, [build](#Building) the project yourself.

### Configuration
The primary means of configuring fsriev is through [appsettings.json](fsriev/appsettings.json) file.
#### Workers Config
Each directory to watch needs to be added as a new JSON object to `Watchers` array.

Property Name | Type   | Required? | Default Value   | Description
:------------:|:------:|:---------:|:---------------:|------------
Name | string | No | Unnamed Watcher | The name of the watcher that will appear in log messages. Useful to recognize watcher when running multiple.
Enabled | bool | No | true | Whether the watcher is enabled. Allows disabling the watcher without removing it from the configuration.
FolderPath | string | Yes | | Path of the folder to watch.
FileFilters | array of strings | No | `*` | File name filters that need to match in order for commands to be executed. Pro-tip: this can be a file name to watch a specific file only.
Recursive | bool | No | true | Whether watcher should watch for file changes in subfolders.
SkipWhenBusy | bool | No | true | Watcher might receive multiple events at once. This switch controls if watcher should ignore them while already processing one.
NotifyFilters | string/int | No | LastWrite,FileName | Flags that will be checked to determine if the file has changed. See [NotifyFilters](https://docs.microsoft.com/en-gb/dotnet/api/system.io.notifyfilters?view=net-5.0) for a list of valid values.
Exclusions | array of strings | No | | Filters of ignored files. Useful for example when you want to ignore VS temporary files (`*~*.tmp`) or minified JS outputs (`*.min.*`).
WorkingDirectory | string | No | Value of `FolderPath` | Working directory that will be used when executing the commands.
ShowCommandOutput | bool | No | true | Whether output of ran commands should be displayed.
Commands | array of strings | No | | Commands to execute when a file change has been detected. Commands are executed in order, regardless if previous command executed correctly or not. *Note: if no command is added, a warning will be output to logs.*

#### Other Config
There are a few properties that can be configured ***outside*** of `Watchers` array.

Property Name | Type   | Required? | Default Value   | Description
:------------:|:------:|:---------:|:---------------:|------------
CommandOutputMode | string/int | No | Console | How command output should be displayed. Supported values are `Console`, `Log`. If you run fsriev manually, in terminal etc, "Console" is recommended. If you depend on log files to see the output, use "Log". *Note: you can technically use both at once (`Console,Log`), but it is not recommended. This might be useful if logging to console is disabled in [Logging configuration](#logging).*
CommandOutputLevel | string/int | No | Information | *Only applicable when `CommandOutputMode` is set to "Log".* Sets the level that command output will be logged as. Valid values are "Verbose", "Debug", "Information", "Warning", "Error" and "Fatal". *Note: this applies only to STDOUT. STDERR will always be logged as Error.*

#### Logging
By default the application will log to terminal window and to `%PROGRAMDATA%/TehGM/fsriev/logs`.

Logging configuration is done via [logsettings.json](fsriev/logsettings.json). See [Serilog.Settings.Configuration](https://github.com/serilog/serilog-settings-configuration) for more info.

Errors that occur when application is loading the configuration will be logged to `%PROGRAMDATA%/TehGM/fsriev/logs`.

> Note: On Windows `%PROGRAMDATA%` will most likely be `C:\ProgramData`.
> Currently `%PROGRAMDATA%` appears to be broken when used in [logsettings.json](fsriev/logsettings.json) on Linux, due to a likely bug in Serilog configuration library. Refer to [this issue](https://github.com/serilog/serilog-settings-configuration/issues/257) for more info.

### Important Notes
- Do **NOT** close fsriev by pressing X if any of the commands is still running. Due to terminal limitations, fsriev will not have any chance to kill command process.  
Instead, send shut down signal to fsriev - for example by pressing `Ctrl+C`. Doing so will notify fsriev to kill commands before exiting.
- Currently, the default application config contains example configuration. It will most likely log an error due to directory not existing. Simply update your configuration to solve this.

## Building
1. Install [.NET 5 SDK](https://dotnet.microsoft.com/download/dotnet/5.0).
2. Clone the repository.
3. Build the project.

## Contributing
If you spot a bug or want to suggest a feature or improvement, feel free to open a new [Issue](https://github.com/TehGM/fsriev/issues).

## License
Copyright (c) 2021 TehGM

Licensed under [Mozilla Public License 2.0](LICENSE) (MPL-2.0).
