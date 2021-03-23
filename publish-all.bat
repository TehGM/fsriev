@echo off

set outputdir=fsriev\bin\Output\
set publishdir=fsriev\bin\Release\net5.0\publish\
if not exist %outputdir% mkdir %outputdir%

set name="Portable (Framework Dependent)"
echo Publishing: %name%
dotnet publish -p:PublishProfile=%name%
del /s %publishdir%portable\*.Development.json
tar -a -c -f %outputdir%fsriev.Portable.zip -C %publishdir%portable *

set name="Win x86"
echo Publishing: %name%
dotnet publish -p:PublishProfile=%name%
del /s %publishdir%win-x86\*.Development.json
tar -a -c -f %outputdir%fsriev.Win-x86.zip -C %publishdir%win-x86 *

set name="Win x64"
echo Publishing: %name%
dotnet publish -p:PublishProfile=%name%
del /s %publishdir%win-x64\*.Development.json
tar -a -c -f %outputdir%fsriev.Win-x64.zip -C %publishdir%win-x64 *

set name="Win ARM"
echo Publishing: %name%
dotnet publish -p:PublishProfile=%name%
del /s %publishdir%win-arm\*.Development.json
tar -a -c -f %outputdir%fsriev.Win-ARM.zip -C %publishdir%win-arm *

set name="Linux x64"
echo Publishing: %name%
dotnet publish -p:PublishProfile=%name%
del /s %publishdir%linux-x64\*.Development.json
tar -a -c -f %outputdir%fsriev.Linux-x64.zip -C %publishdir%linux-x64 *

set name="Linux ARM"
echo Publishing: %name%
dotnet publish -p:PublishProfile=%name%
del /s %publishdir%linux-arm\*.Development.json
tar -a -c -f %outputdir%fsriev.Linux-ARM.zip -C %publishdir%linux-arm *