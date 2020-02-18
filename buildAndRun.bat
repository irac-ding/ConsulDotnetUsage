cd /d %~dp0
start /wait  build.bat
echo "build done"
start   consulRun.bat
TIMEOUT /T 5
start  runServiceA.bat
start  runServiceB.bat
TIMEOUT /T 5
start  startBrower.bat