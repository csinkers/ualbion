@echo off
copy /Y C:\Depot\Main\bitbucket\ghidrasym\Debug\ghidrasym.dll C:\Depot\Main\bitbucket\pathtools\installed\x86
copy /Y C:\Depot\Main\bitbucket\ghidrasym\Debug\ghidrasym.pdb C:\Depot\Main\bitbucket\pathtools\installed\x86
pushd albion_sr
runjob ..\..\pathtools\installed\x86\windbg.exe SR-Main.exe
popd
