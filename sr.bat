@echo off
copy /Y ..\ghidrasym\Debug\ghidrasym.dll ..\pathtools\installed\x86
copy /Y ..\ghidrasym\Debug\ghidrasym.pdb ..\pathtools\installed\x86
pushd albion_sr
runjob ..\..\pathtools\installed\x86\windbg.exe SR-Main.exe
popd
