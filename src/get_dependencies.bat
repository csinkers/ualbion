@echo off
pushd ..\deps
if exist AdlMidi.NET goto l1
git clone https://github.com/CSinkers/AdlMidi.NET
:l1
if exist SerdesNet goto l2
git clone https://github.com/CSinkers/SerdesNet
:l2
if exist veldrid goto l3
git clone https://github.com/mellinoe/veldrid
:l3
if exist veldrid-spirv goto l4
git clone https://github.com/mellinoe/veldrid-spirv
:l4
pushd AdlMidi.NET
git pull --rebase
popd
pushd SerdesNet
git pull --rebase
popd
pushd veldrid
git pull --rebase
popd
pushd veldrid-spirv
git pull --rebase
popd
popd
