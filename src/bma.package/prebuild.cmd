set path=%1\Extensions\Microsoft\Web Tools\External\;%PATH%
cd %~p0\
echo calling npm in %CD%
call npm install
echo end of prebuild script