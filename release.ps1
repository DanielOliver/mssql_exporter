$hasTag = $env:APPVEYOR_REPO_TAG
$AUTH="Authorization: token $github_api_token";
$repoUrl="https://api.github.com/repos/DanielOliver/mssql_exporter";
$uploadUrl="https://uploads.github.com/repos/DanielOliver/mssql_exporter";
$githubToken=$env:githubToken

Remove-Item src/server/bin -Force -Recurse -ErrorAction SilentlyContinue;
Remove-Item zip -Force -Recurse -ErrorAction SilentlyContinue;
mkdir zip;

dotnet publish src/server -c Release -r win-x64 -o "./bin/win_x64" --self-contained true /p:PublishTrimmed=true
dotnet publish src/server -c Release -r win-x64 -o "./bin/linux_x64" --self-contained true /p:PublishTrimmed=true


function Create-Tar($tarFile, $dest) {
    Compress-7Zip $dest $tarFile TAR
}
$winx64zip = "mssql_exporter_win_x64.zip";
$winx64zipPath = "zip/$winx64zip";
# $linuxx64zip = "mssql_exporter_linux_x64.zip";
$linuxx64zip = "mssql_exporter_linux_x64.tar";
$linuxx64zipPath = "zip/$linuxx64zip";
Compress-Archive -Path src/server/bin/win_x64/* -DestinationPath $winx64zipPath -Force;
# Compress-Archive -Path src/server/bin/linux_x64/* -DestinationPath $linuxx64zipPath -Force;
$originalPath = Get-Location
try {
    if (-not (Get-Command Compress-7Zip -ErrorAction Ignore)) {
        Save-Module -Name 7Zip4Powershell -Path . -RequiredVersion 1.9.0 -Force;
        $pathToModule = ".\7Zip4Powershell\1.9.0\7Zip4PowerShell.psd1";
        $pathToModule = Resolve-Path $(Join-Path $(Get-Location) $pathToModule);
        Import-Module -Name $pathToModule;
    }
    Compress-7Zip -ArchiveFileName $linuxx64zipPath -Path "./src/server/bin/linux_x64/" -Format TAR
    # Move-Item $linuxx64zip "zip/"

}
finally {
    Set-Location $originalPath;
}

if($hasTag -eq "true") {
    Write-Host "Finding release.";
    [Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12;

    $tag = $env:APPVEYOR_REPO_TAG_NAME;
    $tagUrl="$repoUrl/releases/tags/$tag";
    $releaseResponse = Invoke-RestMethod -Uri $tagUrl -Headers @{"Authorization"="token $githubToken"}
    $uploadsUrl = $releaseResponse.upload_url

    $winx64AssetUrl = $uploadsUrl -replace "\{\?name,label}", "?name=$winx64zip"
    $linuxx64AssetUrl = $uploadsUrl -replace "\{\?name,label}", "?name=$linuxx64zip"
    Write-Host "Uploading win_x64 release Asset";
    Invoke-WebRequest -Uri $winx64AssetUrl -Method Post -InFile $winx64zipPath -ContentType "application/octet-stream" -Headers @{"Authorization"="token $githubToken"}
    Write-Host "Uploading linux_x64 release Asset";
    Invoke-WebRequest -Uri $linuxx64AssetUrl -Method Post -InFile $linuxx64zipPath -ContentType "application/octet-stream" -Headers @{"Authorization"="token $githubToken"}
    Write-Host "Uploaded release assets";

}


