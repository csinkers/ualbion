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
git checkout -b master origin/master
echo.

:l3
if exist veldrid-spirv goto l4
echo Cloning veldrid-spirv
git clone https://github.com/mellinoe/veldrid-spirv
echo.

:l4
if exist superpower goto l5
echo Cloning superpower
git clone https://github.com/datalust/superpower
echo.

:l5
if exist ImGuiColorTextEditNet goto l6
echo Cloning ImGuiColorTextEditNet
git clone https://github.com/csinkers/ImGuiColorTextEditNet
echo.

:l6
echo Updating AdlMidi.NET
pushd AdlMidi.NET
git stash && git pull --rebase && git stash pop
popd
echo.

echo Updating SerdesNet
pushd SerdesNet
git stash && git pull --rebase && git stash pop
popd
echo.

echo Updating veldrid
pushd veldrid
git checkout master
git stash && git pull --rebase && git stash pop
popd
echo.

echo Updating veldrid-spirv
pushd veldrid-spirv
git stash && git pull --rebase && git stash pop
popd

echo Updating superpower
pushd superpower
git stash && git pull --rebase && git stash pop
popd

echo Updating ImGuiColorTextEditNet
pushd ImGuiColorTextEditNet
git stash && git pull --rebase && git stash pop
popd

popd
