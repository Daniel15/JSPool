@echo off
"%ProgramFiles(x86)%\MSBuild\12.0\Bin\MSBuild.exe" build.proj /t:Package;Push /p:BuildType=Release
pause