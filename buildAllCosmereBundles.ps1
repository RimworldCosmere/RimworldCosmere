if ($IsWindows)
{
    $unityPath = "C:\Program Files\Unity\Hub\Editor\2022.3.35f1\Editor\Unity.exe"
    $buildTarget = "windows"
}
elseif ($IsMacOS)
{
    $unityPath = "/Applications/Unity/Hub/Editor/2022.3.35f1/Unity.app/Contents/MacOS/Unity"
    $buildTarget = "mac"
}

if (($null -eq $unityPath) -or (-not (Test-Path -Path $unityPath -PathType Leaf)))
{
    $unityPath = $Env:UNITY_PATH
}

if ($null -eq $buildTarget)
{
    $buildTarget = $Env:UNITY_BUILD_TARGET;
}

if ( [string]::IsNullOrEmpty($unityPath))
{
    Write-Host "Could not find unityPath. If you are on Windows or Mac, make sure your powershell version is up to date (v7)"
    Write-Host "If you are not, or don't want to update, set your UNITY_PATH environment variable"
    exit
}

if ( [string]::IsNullOrEmpty($buildTarget))
{
    Write-Host "Could not find buildTarget. If you are on Windows or Mac, make sure your powershell version is up to date (v7)"
    Write-Host "If you are not, or don't want to update, set your UNITY_BUILD_TARGET environment variable (windows, mac, linux)"
    exit
}


$cleanPath = "$PSScriptRoot\..\AssetBuilder\Assets\Data"
$assetOutput = "$PSScriptRoot\..\AssetBuilder\Assets\AssetBundles"
Write-Host "Cleaning $cleanPath"
Remove-Item -Recurse -Force -ErrorAction SilentlyContinue "$cleanPath\*" | Out-Null

function Get-FolderHash($folderPath)
{
    $hashString = (Get-ChildItem -Path $folderPath -Recurse -File | Get-FileHash -Algorithm SHA256).Hash | Out-String
    return (Get-FileHash -Algorithm SHA256 -InputStream ([IO.MemoryStream]::new([System.Text.Encoding]::UTF8.GetBytes($hashString)))).Hash
}


# List of modules to process
$mods = Get-ChildItem -Directory -Name |
        Where-Object { $_ -like 'Cosmere*' } |
        ForEach-Object { $_ -replace '^Cosmere', '' }
foreach ($mod in $mods)
{
    Write-Host "--------------------------------------"
    Write-Host "Processing Cosmere$mod..."

    $srcAssets = "$PSScriptRoot\Cosmere$mod\Assets"
    $bundleName = "Cosmere.$mod"
    $destPath = "$PSScriptRoot\..\AssetBuilder\Assets\Data\$bundleName"
    $finalOutput = "$PSScriptRoot\Cosmere$mod\AssetBundles"
    $hashFile = "$PSScriptRoot\Cosmere$mod\.lastassetbuildhash"

    if (Test-Path $srcAssets)
    {
        Write-Host "    Fetching FolderHash for $srcAssets"
        $currentHash = Get-FolderHash $srcAssets
        $previousHash = if (Test-Path $hashFile)
        {
            (Get-Content $hashFile -Raw).Trim()
        }
        else
        {
            ""
        }

        Write-Host "    Testing $currentHash vs $previousHash"
        if ($currentHash -eq $previousHash)
        {
            Write-Host "    No changes detected in Cosmere$mod. Skipping build."
            continue
        }

        Write-Host "    Changes detected. Continuing with build."

        Write-Host "    Copying $srcAssets\* -> $destPath"
        Remove-Item -Recurse -Force -ErrorAction SilentlyContinue "$destPath" | Out-Null
        New-Item -ItemType Directory -Force -Path $destPath | Out-Null
        Copy-Item "$srcAssets\*" -Destination $destPath -Recurse -Force

        $unityArgs = @(
            "-batchmode",
            "-quit",
            '-projectPath="..\AssetBuilder"',
            "-executeMethod=ModAssetBundleBuilder.BuildBundles",
            "--assetBundleName=$bundleName",
            "--buildTarget=$buildTarget",
            "--outputLocation=$finalOutput"
        )

        Write-Host "    Building asset bundle: $bundleName"
        $process = Start-Process $unityPath -ArgumentList $unityArgs -Wait -PassThru

        if ($process.ExitCode -ne 0)
        {
            Write-Host "    Unity failed for $mod (exit code $( $process.ExitCode )). Crashing build."
            exit
        }
        Remove-Item -Recurse -Force -ErrorAction SilentlyContinue "$destPath" | Out-Null

        $currentHash | Out-File -Encoding ASCII -FilePath $hashFile
        Write-Host "    Done with Cosmere$mod."
    }
    else
    {
        Write-Host "    Skipping Cosmere$mod - no Assets folder found."
    }
}

Write-Host "All bundles built."
