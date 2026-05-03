@echo off
dotnet publish Editor/Editor.csproj -c Release -r win-x64 ^
    -p:IlcOptimizationPreference=Speed ^
    -p:IlcFoldIdenticalMethodBodies=true ^
    -p:IlcInstructionSetExtensions=native
pause