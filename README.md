# Model Unlocker

This tool is used to unlocking GTA5 Yft, Ydr and ydd models, the unlocked models can be imported into ZModeler3 for editing.

## Note
Currently, unlocking models with cloth mesh (Yld) is not supported. If some models cannot be imported into ZModeler3 after unlocking, please try to export them to OpenFormat using OpenIV, and then import them back to the model file.

If your computer has not installed .Net Core 3.1, please install it before running this software.

## Usage

Download the latest release from [Releases Page](https://github.com/kasuganosoras/ModelUnlocker/releases).

```
Usage: ModelUnlocker.exe [options]

Options:
  -i|--input <input>     Input file or directory.
  -o|--output <output>   Output file or directory, default is the same as input file or directory.
  -r|--override			 Override the exists file.
  -l|--loglevel <level>  Log level, default is Info, supported values: 0. Debug, 1. Info, 2. Warning, 3. Error
  -h|--help              Show help information.
```

## Example

Unlock all models in the directory and subdirectories, and output to the same directory (Override files).

```
ModelUnlocker.exe -r -i "Path/To/Models/"
```

Unlock a single model and output to another directory.
```
ModelUnlocker.exe -i "Path/To/Models/vehicle.yft" -o "Path/To/Output/"
```

## License

ModelUnlocker - A GTA V game model unlock tool.

Copyright (C) 2023 Akkariin

This program Is free software: you can redistribute it And/Or modify it under the terms Of the GNU General Public License As published by the Free Software Foundation, either version 3 Of the License, Or (at your option) any later version.

This program Is distributed In the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty Of MERCHANTABILITY Or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License For more details.

You should have received a copy Of the GNU General Public License along with this program. If Not, see http://www.gnu.org/licenses/.
