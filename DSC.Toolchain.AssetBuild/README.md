# AssetBuild

AssetBuild is a utility that converts PNG images to AssetData structures.

## Command structure
```
AssetBuild 
    <filename \"*.(4|8|16)bpp.png\">
    <-wram|-file> 
    <-bitmap|-tiles>    
    <out_header.h> 
    <out_asm.s> 
    [<resource_filename.roa>]
```

Arguments (in this order, do not skip any):
- `<filename \"*.(4|8|16)bpp.png\">`
    
    Path to the PNG image. Its extension must specify the color depth the converter should use (e.g. `"my_image.8bpp.png"` for
    a 256-color image).
    Auto color reducer will be implemented soon. For now, make sure the number of colors doesn't exceed the bit depth you provide.

- `<-wram|-file>` 

    Tells if the resulted asset data will be accessed from memory or from a file (FAT/NITRO).

- `<-bitmap|-tiles>`

    Tells if the image will be converted as a bitmap or as a tile set.
    *(+ TO ADD: metatiles features)*

- `<out_header.h>`

    The path of the output C header file.

- `<out_asm.s>`

    The path of the output C assembly file containing binary resource data.

- `<resource_filename.roa>` - optional

    If `-file` is specified, then this argument represents the ouput file the effective asset data is written to.

    This is be the file that will be loaded from the filesystem.

## How to compile

In Visual Studio's Developer PowerShell, type in one of the following commands depending by target platform (Windows/Linux/Max).

(Self-contained option will produce a larger size binary in order to remove any other dependency issues, we'll see later if it is necessary to keep it that way.)

```dotnet publish -c Release -r win-x64 --self-contained true -p:PublishReadyToRun=true```

```dotnet publish -c Release -r linux-x64 --self-contained true -p:PublishReadyToRun=true```

```dotnet publish -c Release -r osx-x64 --self-contained true -p:PublishReadyToRun=true```

Check the `bin/Release/net6.0/<platform>/publish/` location for the single executable file.

## Usage example 

To compile an 8-bit PNG into a bitmap asset data that resides in memory, use the command

`AssetBuild path/to/image.8bpp.png -wram -bitmap image.h image.s`

Then, in the C++ code:

```C++
    #include "image.h"

    const DSC::AssetData* my_image = &ROA_image8;

    // use my_image 
```

Project structure:

```
source/main.cpp
source/image.h
source/image.s
Makefile
```

The image compile options can also be specified in a file that has the same name and path as the image but has the extensions `.asset`.

File `path/to/image.8bpp.asset` contains text `-wram -bitmap`.

Command

`AssetBuild path/to/image.8bpp.png image.h image.s`

... and AssetBuild knows to extracts the options `-wram -bitmap` from the file.