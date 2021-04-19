@echo off

rem Simple script to install the contents of a directory to a destination

if "%1"=="" goto usage
if not exist %1 goto usage
if "%2"=="" goto usage
if %2=="" goto usage

if exist %2 rd /s /q %2
xcopy /i %1 %2
goto :eof

:usage
echo Usage: install SOURCEDIR DESTDIR
exit /b 1
