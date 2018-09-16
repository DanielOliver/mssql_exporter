$hasTag = $env:APPVEYOR_REPO_TAG
$AUTH="Authorization: token $github_api_token";
$repoUrl="https://api.github.com/repos/DanielOliver/mssql_exporter";
$uploadUrl="https://uploads.github.com/repos/DanielOliver/mssql_exporter";
$githubToken=$env:githubToken

Remove-Item src/server/bin -Force -Recurse -ErrorAction SilentlyContinue;
Remove-Item zip -Force -Recurse -ErrorAction SilentlyContinue;
dotnet publish src/server -c Release -r win-x64 --self-contained -o "./bin/win_x64";
dotnet publish src/server -c Release -r linux-x64 --self-contained -o "./bin/linux_x64";

mkdir zip;
$winx64zip = "mssql_exporter_win_x64.zip";
$winx64zipPath = "zip/$winx64zip";
$linuxx64zip = "mssql_exporter_linux_x64.tar.gz";
$linuxx64zipPath = "zip/$linuxx64zip";
Compress-Archive -Path src/server/bin/win_x64/* -DestinationPath $winx64zipPath -Force;
tar -C "src/server/bin/linux_x64/" -zcvf $linuxx64zipPath "./*";

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


