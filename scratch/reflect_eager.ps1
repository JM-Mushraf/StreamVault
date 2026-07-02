try {
    $dllPath = Get-ChildItem -Path "c:\Users\mushr\OneDrive\Desktop\StreamVault" -Filter "CloudinaryDotNet.dll" -Recurse | Select-Object -First 1 -ExpandProperty FullName;
    [System.Reflection.Assembly]::LoadFrom($dllPath) | Out-Null;

    Write-Host "--- Checking VideoUploadParams members containing 'eager' ---";
    [CloudinaryDotNet.Actions.VideoUploadParams].GetProperties() | Where-Object { $_.Name -like "*eager*" } | Select-Object Name, PropertyType | Out-String | Write-Host;

    Write-Host "--- Checking VideoUploadResult members containing 'eager' ---";
    [CloudinaryDotNet.Actions.VideoUploadResult].GetProperties() | Where-Object { $_.Name -like "*eager*" } | Select-Object Name, PropertyType | Out-String | Write-Host;
} catch {
    $_.Exception.ToString() | Write-Host;
}
