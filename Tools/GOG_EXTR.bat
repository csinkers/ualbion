@echo off
if not exist ualbion.sln goto badpath

imgmount d game.ins -t iso -fs iso
if errorlevel 0 goto mountok
imgmount d ..\game.ins -t iso -fs iso
:mountok

D:
cd ALBION
mkdir C:\ALBION
mkdir C:\ALBION\SAVES
mkdir C:\ALBION\CD
mkdir C:\ALBION\CD\XLDLIBS
mkdir C:\ALBION\CD\XLDLIBS\INITIAL
mkdir C:\ALBION\CD\XLDLIBS\ENGLISH
mkdir C:\ALBION\CD\XLDLIBS\GERMAN
mkdir C:\ALBION\CD\XLDLIBS\FRENCH

copy ROOT\MAIN.EXE C:\ALBION
copy XLDLIBS\*.* C:\ALBION\CD\XLDLIBS
copy XLDLIBS\INITIAL\*.* C:\ALBION\CD\XLDLIBS\INITIAL
copy XLDLIBS\ENGLISH\*.* C:\ALBION\CD\XLDLIBS\ENGLISH
copy XLDLIBS\GERMAN\*.* C:\ALBION\CD\XLDLIBS\GERMAN
copy XLDLIBS\FRENCH\*.* C:\ALBION\CD\XLDLIBS\FRENCH
goto end

:badpath
echo C drive must be mounted to point at the ualbion depot directory.

:end
