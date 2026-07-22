@echo off
rem Builds StockBrakeToggle.dll against KSP's own managed assemblies (no SDK
rem or NuGet package required - uses the C# compiler bundled with the .NET
rem Framework). Assumes this script lives at
rem GameData/StockBrakeToggle/Source/build.bat inside a KSP install; override
rem KSP below if that's not the case.

set "KSP=%~dp0..\..\.."
set "MANAGED=%KSP%\KSP_x64_Data\Managed"
set "CSC=C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe"

"%CSC%" /noconfig /nostdlib+ /target:library /optimize+ /nologo ^
 /out:"%KSP%\GameData\StockBrakeToggle\StockBrakeToggle.dll" ^
 /r:"%MANAGED%\mscorlib.dll" ^
 /r:"%MANAGED%\System.dll" ^
 /r:"%MANAGED%\System.Core.dll" ^
 /r:"%MANAGED%\UnityEngine.dll" ^
 /r:"%MANAGED%\UnityEngine.CoreModule.dll" ^
 /r:"%MANAGED%\UnityEngine.InputLegacyModule.dll" ^
 /r:"%MANAGED%\Assembly-CSharp.dll" ^
 "%~dp0ToggleStockBrake.cs"

if %errorlevel%==0 (echo OK: StockBrakeToggle.dll built) else (echo BUILD FAILED)
