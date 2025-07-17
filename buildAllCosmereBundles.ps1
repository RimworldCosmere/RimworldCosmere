if ($IsWindows) {
    $unityPath = "C:\Program Files\Unity\Hub\Editor\2022.3.35f1\Editor\Unity.exe"
} elseif ($IsMacOS) {
    $unityPath = "/Applications/Unity/Hub/Editor/2022.3.35f1/Unity.app/Contents/MacOS/Unity"
} else {
    throw "Unsupported platform. Only Windows and macOS are supported."
}

$cleanPath = "..\AssetBuilder\Assets\Data"
$assetOutput = "..\AssetBuilder\Assets\AssetBundles"
Write-Host "Cleaning $cleanPath"
Remove-Item -Recurse -Force -ErrorAction SilentlyContinue "$cleanPath\*" | Out-Null

# List of modules to process
$mods = @("Core", "Resources", "Framework", "Scadrial")

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


foreach ($mod in $mods)
{
    Write-Host "--------------------------------------"
    Write-Host "Processing Cosmere$mod..."

    $modLower = $mod.ToLower()
    $srcAssets = "..\RimworldCosmere\Cosmere$mod\Assets"
    $bundleName = "CryptikLemur.Cosmere.$mod"
    $destPath = "..\AssetBuilder\Assets\Data\$bundleName"
    $bundleOutput = "resource_cosmere_$modLower"
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
            "--assetBundleName=$bundleName"
        )

        Write-Host "    Building asset bundle: $bundleName"
        $process = Start-Process -FilePath $unityPath -ArgumentList $unityArgs -Wait -PassThru

        if ($process.ExitCode -ne 0)
        {
            Write-Host "    Unity failed for $mod (exit code $( $process.ExitCode )). Skipping bundle move."
            exit
            Remove-Item -Recurse -Force -ErrorAction SilentlyContinue $destPath | Out-Null
            continue
        }

        if (Test-Path $finalOutput)
        {
            Write-Host "    Cleaning $finalOutput"
            Remove-Item -Recurse -Force -ErrorAction SilentlyContinue "$finalOutput\*" | Out-Null
        }
        else
        {
            New-Item -ItemType Directory -Path $finalOutput | Out-Null
        }

        $platforms = @("windows", "mac", "linux")
        $foundAny = $false
        foreach ($platform in $platforms) {
            $platformOutput = "$assetOutput\$platform\$bundleName_$platform"
            $platformManifest = "$platformOutput.manifest"

            if (Test-Path $platformOutput) {
                Move-Item $platformOutput "$finalOutput/${bundleOutput}_$platform" -Force
                $foundAny = $true
            } else {
                Write-Host "    Warning: $platformOutput not found."
            }

            if (Test-Path $platformManifest) {
                Move-Item $platformManifest "$finalOutput/${bundleOutput}_$platform.manifest" -Force
            } else {
                Write-Host "    Warning: $platformManifest not found."
            }
        }

        if (-not $foundAny) {
            Write-Host "    Warning: No asset bundles found for any platform."
        }

        $currentHash | Out-File -Encoding ASCII -FilePath $hashFile
        Write-Host "    Done with Cosmere$mod."
        Remove-Item -Recurse -Force -ErrorAction SilentlyContinue $destPath | Out-Null
    }
    else
    {
        Write-Host "    Skipping Cosmere$mod - no Assets folder found."
    }
}

Write-Host "All bundles built."
