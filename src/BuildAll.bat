@echo off

if not defined msbuildExec (
	set msbuildExec=msbuild
)

if not defined gitExec (
	set gitExec=git
)

if not defined sevenZipExec (
	set sevenZipExec=7z
)


echo Cleaning up...

call "%gitExec%" reset --hard
call "%gitExec%" pull
call "%gitExec%" clean -qfdx
if errorlevel 1 goto cleanUpFailed


echo Creating archives with sources...

rem zip itextsharp-src-core.zip
call "%sevenZipExec%" a -tzip itextsharp-src-core.zip -r .\core\* > nul
if errorlevel 1 goto archiveFailed

rem zip itextsharp-src-pdfa.zip
call "%sevenZipExec%" a -tzip itextsharp-src-pdfa.zip -r .\extras\itextsharp.pdfa\* > nul
if errorlevel 1 goto archiveFailed

rem zip itextsharp-src-xtra.zip
call "%sevenZipExec%" a -tzip itextsharp-src-xtra.zip -r .\extras\iTextSharp.xtra\* > nul
if errorlevel 1 goto archiveFailed

rem zip itextsharp-src-xmlworker.zip
call "%sevenZipExec%" a -tzip itextsharp-src-xmlworker.zip -r .\extras\itextsharp.xmlworker\* > nul
if errorlevel 1 goto archiveFailed



echo Building binaries...

rem rebuild core Release
call "%msbuildExec%" core\itextsharp.csproj /t:Rebuild /p:Configuration=Release,Platform=AnyCPU > nul
if errorlevel 1 goto buildFailed

rem rebuild core Release_woDrawing
call "%msbuildExec%" core\itextsharp.csproj /t:Rebuild /p:Configuration=Release_woDrawing,Platform=AnyCPU > nul
if errorlevel 1 goto buildFailed

rem rebuild pdfa Release
call "%msbuildExec%" extras\itextsharp.pdfa\itextsharp.pdfa.csproj /t:Rebuild /p:Configuration=Release,Platform=AnyCPU > nul
if errorlevel 1 goto buildFailed

rem rebuild xtra Release
call "%msbuildExec%" extras\iTextSharp.xtra\iTextSharp.xtra.csproj /t:Rebuild /p:Configuration=Release,Platform=AnyCPU > nul
if errorlevel 1 goto buildFailed

rem rebuild xmlworker Release
call "%msbuildExec%" extras\itextsharp.xmlworker\itextsharp.xmlworker.csproj /t:Rebuild /p:Configuration=Release,Platform=AnyCPU > nul
if errorlevel 1 goto buildFailed



echo Creating archives with binaries...

rem zip core dll Release itextsharp-dll-core.zip
call "%sevenZipExec%" a -tzip itextsharp-dll-core.zip -r .\core\bin\Release\* > nul
if errorlevel 1 goto archiveFailed

rem zip core dll Release_woDrawing itextsharp-dll-core-wo_Drawing.zip
call "%sevenZipExec%" a -tzip itextsharp-dll-core-wo_Drawing.zip -r .\core\bin\Release_woDrawing\* > nul
if errorlevel 1 goto archiveFailed

rem zip pdfa dll Release itextsharp-dll-pdfa.zip
call "%sevenZipExec%" a -tzip itextsharp-dll-pdfa.zip .\extras\itextsharp.pdfa\bin\Release\itextsharp.pdfa.dll > nul
if errorlevel 1 goto archiveFailed

rem zip xtra dll Release itextsharp-dll-xtra.zip
call "%sevenZipExec%" a -tzip itextsharp-dll-xtra.zip .\extras\iTextSharp.xtra\bin\Release\itextsharp.xtra.dll > nul
if errorlevel 1 goto archiveFailed

rem zip xmlworker dll Release itextsharp-dll-xmlworker.zip
call "%sevenZipExec%" a -tzip itextsharp-dll-xmlworker.zip .\extras\itextsharp.xmlworker\bin\Release\itextsharp.xmlworker.dll > nul
if errorlevel 1 goto archiveFailed


echo Creating resulting archives...

rem add to result notice.txt
call "%sevenZipExec%" a -tzip itextsharp-all.zip .\notice.txt > nul
if errorlevel 1 goto archiveFailed

rem add to result archives
call "%sevenZipExec%" a -tzip itextsharp-all.zip itextsharp-dll-core.zip itextsharp-dll-core-wo_Drawing.zip itextsharp-dll-pdfa.zip itextsharp-dll-xtra.zip itextsharp-src-core.zip itextsharp-src-pdfa.zip itextsharp-src-xtra.zip > nul
if errorlevel 1 goto archiveFailed

rem add to result archives
call "%sevenZipExec%" a -tzip itextsharp.xmlworker-all.zip itextsharp-src-xmlworker.zip itextsharp-dll-xmlworker.zip > nul
if errorlevel 1 goto archiveFailed

rem delete temp archives
call :deleteTempFiles



echo Success!
pause
exit



:buildFailed
echo:
echo Binaries build failed. Please, make sure that either msbuildExec path variable is defined or msbuild.exe is available to call from command line. The process will be aborted.
echo:
call :deleteTempFiles
pause
exit

:cleanUpFailed
echo:
echo Cleaning up failed. Please, make sure that either gitExec path variable is defined or git.exe is available to call from command line. The process will be aborted.
echo:
pause
exit

:archiveFailed
echo:
echo Archive creating failed. Please, make sure that either sevenZipExec path variable is defined or 7z.exe is available to call from command line. The process will be aborted.
echo:
call :deleteTempFiles
pause
exit

:deleteTempFiles
echo Deleting temporarily created zip files...
if exist itextsharp-dll-core.zip (
	del itextsharp-dll-core.zip
)
if exist itextsharp-dll-core-wo_Drawing.zip (
	del itextsharp-dll-core-wo_Drawing.zip
)
if exist itextsharp-dll-pdfa.zip (
	del itextsharp-dll-pdfa.zip
)
if exist itextsharp-dll-xtra.zip (
	del itextsharp-dll-xtra.zip
)
if exist itextsharp-src-core.zip (
	del itextsharp-src-core.zip
)
if exist itextsharp-src-pdfa.zip (
	del itextsharp-src-pdfa.zip
)
if exist itextsharp-src-xtra.zip (
	del itextsharp-src-xtra.zip
)
if exist itextsharp-src-xmlworker.zip (
	del itextsharp-src-xmlworker.zip
)
if exist itextsharp-dll-xmlworker.zip (
	del itextsharp-dll-xmlworker.zip
)
exit /b