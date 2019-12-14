@dotnet build -c Release ualbion.sln 
copy ualbion\bin\Release\netcoreapp3.0\win-x64\native\win-x64\SDL2.dll ualbion\bin\Release\netcoreapp3.0\win-x64
copy libveldrid-spirv.dll ualbion\bin\Release\netcoreapp3.0\win-x64
cd ualbion\bin\Release\netcoreapp3.0\win-x64
