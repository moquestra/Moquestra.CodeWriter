@echo off
setlocal

if "%~1"=="" (
    echo usage: copy-source.bat ^<destination^> 1>&2
    exit /b 1
)

if not [%2]==[] (
    echo usage: copy-source.bat ^<destination^> 1>&2
    exit /b 1
)

set "root=%~dp0"
set "destination=%~1"

if exist "%destination%\*" goto copy

if exist "%destination%" (
    echo destination is an existing file. 1>&2
    exit /b 1
)

mkdir "%destination%" || exit /b 1

:copy
copy /y "%root%src\Moquestra.CodeWriter\*.cs" "%destination%" >nul || exit /b 1

endlocal
