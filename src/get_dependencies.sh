#!/bin/sh
pushd ../deps
[ -d AdlMidi.NET ] || git clone https://github.com/CSinkers/AdlMidi.NET
[ -d SerdesNet ] || git clone https://github.com/CSinkers/SerdesNet
[ -d veldrid ] || git clone https://github.com/mellinoe/veldrid
[ -d veldrid-spriv ] || git clone https://github.com/mellinoe/veldrid-spirv
pushd AdlMidi.NET; git pull --rebase; popd
pushd SerdesNet; git pull --rebase; popd
pushd veldrid; git pull --rebase; popd
pushd veldrid-spirv; git pull --rebase; popd
popd
