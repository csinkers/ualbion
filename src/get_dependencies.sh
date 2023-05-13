#!/bin/sh
cd ../deps
[ -d AdlMidi.NET ]   || git clone https://github.com/csinkers/AdlMidi.NET
[ -d SerdesNet ]     || git clone https://github.com/csinkers/SerdesNet
[ -d veldrid ]       || git clone https://github.com/csinkers/veldrid
[ -d veldrid-spriv ] || git clone https://github.com/mellinoe/veldrid-spirv
[ -d superpower ]    || git clone https://github.com/datalust/superpower
[ -d VeldridGen ] || git clone https://github.com/csinkers/VeldridGen
[ -d ImGuiColorTextEditNet ] || git clone https://github.com/csinkers/ImGuiColorTextEditNet
printf "\nUpdating AdlMidi.NET\n"
cd AdlMidi.NET;   git pull --rebase; cd ..
printf "\nUpdating SerdesNet\n"
cd SerdesNet;     git pull --rebase; cd ..
printf "\nUpdating veldrid\n"
cd veldrid;       git pull --rebase; cd ..
printf "\nUpdating veldrid-spirv\n"
cd veldrid-spirv; git pull --rebase; cd ..
printf "\nUpdating superpower\n"
cd superpower;    git pull --rebase; cd ..
printf "\nUpdating VeldridGen\n"
cd VeldridGen; git pull --rebase; cd ..
printf "\nUpdating ImGuiColorTextEditNet\n"
cd ImGuiColorTextEditNet; git pull --rebase; cd ..
cd ..
