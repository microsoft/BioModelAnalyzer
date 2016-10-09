set path=%1\Extensions\Microsoft\Web Tools\External\;%PATH%
cd %~p0\
echo calling grunt 
call node node_modules\grunt-cli\bin\grunt default --no-color
echo end of postbuild script