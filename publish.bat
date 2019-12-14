@echo off
dotnet publish -c Release ualbion.sln
rm -r ualbion\bin\Release\netcoreapp3.0\win-x64\publish
copy ualbion\bin\Release\netcoreapp3.0\win-x64\publish\native\win-x64\SDL2.dll ualbion\bin\Release\netcoreapp3.0\win-x64\publish
copy libveldrid-spirv.dll ualbion\bin\Release\netcoreapp3.0\win-x64\publish
pushd ualbion\bin\Release\netcoreapp3.0\win-x64\publish
