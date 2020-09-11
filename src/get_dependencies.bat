@echo off
pushd ..\deps

if exist AdlMidi.NET goto l1
echo Cloning AdlMidi.NET
git clone https://github.com/csinkers/AdlMidi.NET
echo.
:l1
if exist SerdesNet goto l2
echo Cloning SerdesNet
git clone https://github.com/csinkers/SerdesNet
echo.
:l2
if exist veldrid goto l3
echo Cloning veldrid
git clone https://github.com/csinkers/veldrid
git checkout -b docking origin/docking
echo.
:l3
if exist veldrid-spirv goto l4
echo Cloning veldrid-spirv
git clone https://github.com/mellinoe/veldrid-spirv
echo.
:l4
echo Updating AdlMidi.NET
pushd AdlMidi.NET
git pull --rebase
popd
echo.

echo Updating SerdesNet
pushd SerdesNet
git pull --rebase
popd
echo.

echo Updating veldrid
pushd veldrid
git checkout docking
git pull --rebase
popd
echo.

echo Updating veldrid-spirv
pushd veldrid-spirv
git pull --rebase
popd

popd
