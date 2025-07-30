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
    $hashFile = "$PSScriptRoot\Cosmere$mod\AssetBundles\.lastassetbuildhash"
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

        Write-Host "    Testing $currentHash vs $previousHash"
        if ($currentHash -eq $previousHash)
        {
            Write-Host "    No changes detected in Cosmere$mod. Skipping build."
            continue
        }

        Write-Host "    Changes detected. Continuing with build."

        $unityArgs = @(
            "-batchmode",
            "-quit",
            '-projectPath "..\AssetBuilder"',
            "-executeMethod ModAssetBundleBuilder.BuildBundles",
            "-buildTarget=$buildTarget",
            "-source=$PSScriptRoot\Cosmere$mod"
        )

        Write-Host "    Building asset bundle: Cosmere.$mod"
        $process = Start-Process $unityPath -ArgumentList $unityArgs -Wait -PassThru

        if ($process.ExitCode -ne 0)
        {
            Write-Host "    Unity failed for $mod (exit code $( $process.ExitCode )). Crashing build."
            exit
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
