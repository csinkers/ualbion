@echo off

if not exist "C:\ualbion.sln" goto c_dir_err
goto c_dir_ok

:c_dir_err
echo Error: C drive must be mounted to point at the ualbion main directory.
echo E.g. mount C "C:\Git\ualbion"
goto end

:c_dir_ok

if not exist "D:\game.ins" goto d_dir_err
if not exist "D:\game.gog" goto d_dir_err
goto d_dir_ok

:d_dir_err
echo Error: D drive must be mounted to point at the Albion GOG installation.
echo E.g. mount D "C:\GOG Games\Albion"
goto end

:d_dir_ok

imgmount E D:\game.ins -t iso -fs iso
if errorlevel 1 goto mount_err
if not exist E:\ALBION goto mount_err
goto mount_ok

:mount_err
echo Error: Could not mount Albion CD image, please verify your game installation.
goto end

:mount_ok

set DST=C:\ALBION
mkdir %DST%
mkdir %DST%\DRIVERS
mkdir %DST%\SAVES
mkdir %DST%\CD
mkdir %DST%\CD\XLDLIBS
mkdir %DST%\CD\XLDLIBS\INITIAL
mkdir %DST%\CD\XLDLIBS\ENGLISH
mkdir %DST%\CD\XLDLIBS\GERMAN
mkdir %DST%\CD\XLDLIBS\FRENCH

set SRC=E:\ALBION
copy %SRC%\ROOT\MAIN.EXE %DST%
copy %SRC%\DRIVERS\ALBISND.OPL %DST%\DRIVERS
copy %SRC%\XLDLIBS\*.* %DST%\CD\XLDLIBS
copy %SRC%\XLDLIBS\INITIAL\*.* %DST%\CD\XLDLIBS\INITIAL
if exist %SRC%\XLDLIBS\ENGLISH copy %SRC%\XLDLIBS\ENGLISH\*.* %DST%\CD\XLDLIBS\ENGLISH
if exist %SRC%\XLDLIBS\GERMAN copy %SRC%\XLDLIBS\GERMAN\*.* %DST%\CD\XLDLIBS\GERMAN
if exist %SRC%\XLDLIBS\FRENCH copy %SRC%\XLDLIBS\FRENCH\*.* %DST%\CD\XLDLIBS\FRENCH

if not exist %SRC%\XLDLIBS\ENGLISH echo Note: No English language files found.
if not exist %SRC%\XLDLIBS\GERMAN echo Note: No German language files found.
if not exist %SRC%\XLDLIBS\FRENCH echo Note: No French language files found.

echo Successfully copied game file to ualbion.

:end
