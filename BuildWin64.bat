call app_info_setup.bat

SET FILENAME=%APP_NAME%

:Actually do the unity build
rmdir build\win /S /Q
mkdir build\win
call GenerateBuildDate.bat
echo Building project...

:So let's delete these things from the shared stuff that we don't need in this kind of build? (remove the : in front to delete)
:del /Q Assets\RT\MySQL\RTSqlManager.cs
:del /Q Assets\RT\RTNetworkServer.cs

:%UNITY_EXE% -quit -batchmode -logFile log.txt -buildWindows64Player build/win/%APP_NAME%.exe -projectPath %cd%
%UNITY_EXE% -quit -batchmode -logFile log.txt -executeMethod Win64Builder.BuildRelease -projectPath %cd%
echo Finished building.
if not exist build/win/%APP_NAME%.exe (
echo Error with build!
start notepad.exe log.txt
%PROTON_DIR%\shared\win\utils\beeper.exe /p
pause
)


if "%NO_PAUSE%"=="" pause
