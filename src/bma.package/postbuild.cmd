cd %~p0\
echo calling grunt 
call  ..\..\packages\Node.js\node node_modules\grunt-cli\bin\grunt default --no-color
echo end of postbuild script