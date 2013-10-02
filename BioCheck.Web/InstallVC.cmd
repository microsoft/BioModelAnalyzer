REM see ServiceDefinition.csdef for the definition of the environment variable
if "%ComputeEmulatorRunning%" == "false" (
	REM Only run this in the cloud
	"%~dp0vcredist_x64.exe" /q /norestart
) ELSE (
	REM Running locally
)