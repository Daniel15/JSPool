# Checks that the last ran command returned with an exit code of 0
function Assert-LastExitCode {
  if ($LASTEXITCODE -ne 0) {
    throw 'Non-zero exit code encountered'
  }
}

# Use date in version number
$env:DNX_BUILD_VERSION = Get-Date -format yyyyMMdd-HHmm

dotnet restore; Assert-LastExitCode
# JSPool.Example.Web is a "legacy" csproj project, so NuGet packages need to be restored the old way too
nuget restore src\JSPool.sln; Assert-LastExitCode
& "${Env:ProgramFiles(x86)}\MSBuild\14.0\Bin\MSBuild.exe" src\JSPool.sln /t:rebuild /p:Configuration=Release; Assert-LastExitCode
dotnet test tests\JSPool.Tests; Assert-LastExitCode
dotnet pack src\JSPool -c Release --version-suffix "$env:DNX_BUILD_VERSION"; Assert-LastExitCode