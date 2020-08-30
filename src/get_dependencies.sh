#!/bin/sh
cd ../deps
[ -d AdlMidi.NET ]   || git clone https://github.com/csinkers/AdlMidi.NET
[ -d SerdesNet ]     || git clone https://github.com/csinkers/SerdesNet
[ -d veldrid ]       || git clone https://github.com/csinkers/veldrid && git checkout -b docking origin/docking
[ -d veldrid-spriv ] || git clone https://github.com/mellinoe/veldrid-spirv
printf "\nUpdating AdlMidi.NET\n"
cd AdlMidi.NET;   git pull --rebase; cd ..
printf "\nUpdating SerdesNet\n"
cd SerdesNet;     git pull --rebase; cd ..
printf "\nUpdating veldrid\n"
cd veldrid;       git pull --rebase; cd ..
printf "\nUpdating veldrid-spirv\n"
cd veldrid-spirv; git pull --rebase; cd ..
cd ..
