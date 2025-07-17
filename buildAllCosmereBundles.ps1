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
else
{
    $unityPath = $Env:UNITY_PATH
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


$cleanPath = "..\AssetBuilder\Assets\Data"
$assetOutput = "..\AssetBuilder\Assets\AssetBundles"
Write-Host "Cleaning $cleanPath"
Remove-Item -Recurse -Force -ErrorAction SilentlyContinue "$cleanPath\*" | Out-Null

function Get-FolderHash($folderPath)
{
    $hashAlgorithm = [System.Security.Cryptography.SHA256]::Create()
    $allFiles = Get-ChildItem -Recurse -File $folderPath | Sort-Object FullName

    $stream = New-Object System.IO.MemoryStream

    foreach ($file in $allFiles)
    {
        $relativePath = Resolve-Path $file.FullName -Relative | Out-String
        $bytes = [System.Text.Encoding]::UTF8.GetBytes($relativePath.Trim())
        $stream.Write($bytes, 0, $bytes.Length)

        $fileBytes = [System.IO.File]::ReadAllBytes($file.FullName)
        $stream.Write($fileBytes, 0, $fileBytes.Length)
    }

    $stream.Position = 0
    $finalHash = $hashAlgorithm.ComputeHash($stream)
    $stream.Dispose()

    return [System.BitConverter]::ToString($finalHash) -replace "-", ""
}


# List of modules to process
$mods = Get-ChildItem -Directory -Name |
        Where-Object { $_ -like 'Cosmere*' } |
        ForEach-Object { $_ -replace '^Cosmere', '' }
foreach ($mod in $mods)
{
    Write-Host "--------------------------------------"
    Write-Host "Processing Cosmere$mod..."

    $srcAssets = "..\RimworldCosmere\Cosmere$mod\Assets"
    $bundleName = "CryptikLemur.Cosmere.$mod"
    $destPath = "..\AssetBuilder\Assets\Data\$bundleName"
    $finalOutput = "..\RimworldCosmere\Cosmere$mod\AssetBundles"
    $hashFile = "..\RimworldCosmere\Cosmere$mod\.lastassetbuildhash"

    if (Test-Path $srcAssets)
    {
        $currentHash = Get-FolderHash $srcAssets
        $previousHash = if (Test-Path $hashFile)
        {
            (Get-Content $hashFile -Raw).Trim()
        }
        else
        {
            ""
        }

        if ($currentHash -eq $previousHash)
        {
            Write-Host "    No changes detected in Cosmere$mod. Skipping build."
            continue
        }

        Write-Host "    Changes detected. Continuing with build."

        Write-Host "    Copying $srcAssets\* -> $destPath"
        New-Item -ItemType Directory -Force -Path $destPath | Out-Null
        Copy-Item "$srcAssets\*" -Destination $destPath -Recurse -Force

        $unityArgs = @(
            "-batchmode",
            "-quit",
            '-projectPath="..\AssetBuilder"',
            "-executeMethod", "ModAssetBundleBuilder.BuildBundles",
            "--assetBundleName=$bundleName",
            "--buildTarget=$buildTarget",
            "--outputLocation=$finalOutput"
        )

        Write-Host "    Building asset bundle: $bundleName"
        $process = Start-Process $unityPath -ArgumentList $unityArgs -Wait -PassThru

        if ($process.ExitCode -ne 0)
        {
            Write-Host "    Unity failed for $mod (exit code $( $process.ExitCode )). Skipping bundle move."
            continue
        }

        $currentHash | Out-File -Encoding ASCII -FilePath $hashFile
        Write-Host "    Done with Cosmere$mod."
    }
    else
    {
        Write-Host "    Skipping Cosmere$mod - no Assets folder found."
    }
}

Write-Host "All bundles built."
