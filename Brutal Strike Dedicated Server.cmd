@echo off
REM change dir to location of script
SET mypath=%~dp0/photon-server-sdk_v4-0-29-11263\deploy\bin_Win32/
CD %mypath%

REM check if photon is already running:
tasklist /fi "Imagename eq PhotonSocketServer.exe" > tasks.txt
::echo _tasklist - %errorlevel%
find "PhotonSocketServer.exe" tasks.txt
::echo _find - %errorlevel%
if %errorlevel% NEQ 1 goto ERROR

echo.
echo Starting Photon as application.
start PhotonSocketServer.exe /debug LoadBalancing
::echo _start - %ERRORLEVEL%
goto END

:ERROR
echo.
echo Server already running

:END
del tasks.txt

for /f "delims=[] tokens=2" %%a in ('ping -4 -n 1 %ComputerName% ^| findstr [') do set NetworkIP=%%a
echo .
echo                 To join this server from game 
echo 1. open settings in custom server 
echo 2. write %NetworkIP%
echo 3. restart game now when you host game, game will be hosted on this server
echo 4. your friends should do the same
pause
