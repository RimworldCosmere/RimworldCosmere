$abilityFiles = Get-ChildItem -Path "CosmereScadrial\Defs\Allomancy" -Recurse -Filter "Abilities.xml"

foreach ($file in $abilityFiles)
{
    try
    {
        [xml]$xml = Get-Content $file.FullName
        foreach ($def in $xml.SelectNodes("//CosmereScadrial.Defs.AllomanticAbilityDef"))
        {
            if ($def.Abstract -ne "True")
            {
                $metal = $def.metal
                $label = if ($def.label)
                {
                    $def.label
                }
                elseif ($metal)
                {
                    $metal.ToLower()
                }
                else
                {
                    "unknown"
                }
                $description = $def.description
                Write-Output "$label [$metal] - $description"
                Write-Output ""
                Write-Output "---"
                Write-Output ""
            }
        }
    }
    catch
    {
        Write-Warning "Failed to process $( $file.FullName ): $_"
    }
}
