set path=%1\Extensions\Microsoft\Web Tools\External\;%PATH%
cd %~p0\
echo calling npm in %CD%
call npm install
cd %~p0\
echo calling grunt prebuild
call node node_modules\grunt-cli\bin\grunt prebuild --no-color
echo end of prebuild script