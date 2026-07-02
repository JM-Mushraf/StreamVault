try {
    $dllPath = Get-ChildItem -Path "c:\Users\mushr\OneDrive\Desktop\StreamVault" -Filter "CloudinaryDotNet.dll" -Recurse | Select-Object -First 1 -ExpandProperty FullName;
    if (-not $dllPath) {
        Write-Host "CloudinaryDotNet.dll not found";
        return;
    }
    Write-Host "Found DLL: $dllPath";
    [System.Reflection.Assembly]::LoadFrom($dllPath) | Out-Null;

    Write-Host "`n--- VideoUploadParams properties ---";
    [CloudinaryDotNet.Actions.VideoUploadParams].GetProperties() | Select-Object Name, PropertyType | Out-String | Write-Host;

    Write-Host "`n--- VideoUploadResult properties ---";
    [CloudinaryDotNet.Actions.VideoUploadResult].GetProperties() | Select-Object Name, PropertyType | Out-String | Write-Host;

    Write-Host "`n--- Transformation methods ---";
    [CloudinaryDotNet.Transformation].GetMethods() | Where-Object { $_.IsPublic -and $_.ReturnType -eq [CloudinaryDotNet.Transformation] } | Select-Object Name | Sort-Object Name -Unique | Out-String | Write-Host;
} catch {
    $_.Exception.ToString() | Write-Host;
}
