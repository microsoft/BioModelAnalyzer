cd %~p0\
echo calling npm in %CD%
call ..\..\packages\Npm.js\tools\npm install
cd %~p0\
echo calling grunt prebuild
call ..\..\packages\Node.js\node node_modules\grunt-cli\bin\grunt prebuild --no-color
echo end of prebuild script