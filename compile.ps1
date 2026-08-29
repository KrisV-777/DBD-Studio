$Project = "DBDStudio/DBDStudio.csproj"
$Config  = "Release"
$Out     = "dist"

$targets = @(
    @{ Rid = "win-x64";     Name = "windows-x64" }
    @{ Rid = "win-arm64";   Name = "windows-arm64" }
    @{ Rid = "linux-x64";   Name = "linux-x64" }
    @{ Rid = "linux-arm64"; Name = "linux-arm64" }
)

if (Test-Path $Out) { Remove-Item $Out -Recurse -Force }
New-Item $Out -ItemType Directory | Out-Null

Write-Host "Building..."

foreach ($target in $targets) {
    Write-Host "  $($target.Name)"
    dotnet publish $Project `
        -c $Config `
        -r $target.Rid `
        --self-contained false `
        -o "$Out\$($target.Name)"

    if ($LASTEXITCODE -ne 0) { exit 1 }
}

Write-Host "Zipping..."

foreach ($target in $targets) {
    Write-Host "  $($target.Name)"
    tar -a -c `
        -f "$Out\DBDStudio-$($target.Name).zip" `
        -C "$Out\$($target.Name)" .

    if ($LASTEXITCODE -ne 0) { exit 1 }
}

Write-Host "Done."

