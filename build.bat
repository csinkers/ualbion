@dotnet build -c Release ualbion.sln 
copy src\ualbion\bin\Release\netcoreapp3.0\win-x64\native\win-x64\SDL2.dll src\ualbion\bin\Release\netcoreapp3.0\win-x64
copy libveldrid-spirv.dll src\ualbion\bin\Release\netcoreapp3.0\win-x64
cd src\ualbion\bin\Release\netcoreapp3.0\win-x64
