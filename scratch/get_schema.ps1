try {
    $conn = New-Object System.Data.SqlClient.SqlConnection("Server=mushrafm24\SQLEXPRESS;Database=StreamVault_DB;Trusted_Connection=True;TrustServerCertificate=True;");
    $conn.Open();
    $cmd = $conn.CreateCommand();
    $cmd.CommandText = "
    SELECT TABLE_NAME, COLUMN_NAME, DATA_TYPE, CHARACTER_MAXIMUM_LENGTH, IS_NULLABLE 
    FROM INFORMATION_SCHEMA.COLUMNS 
    ORDER BY TABLE_NAME, ORDINAL_POSITION
    ";
    $adapter = New-Object System.Data.SqlClient.SqlDataAdapter($cmd);
    $dataset = New-Object System.Data.DataSet;
    $adapter.Fill($dataset) | Out-Null;
    $dataset.Tables[0] | Select-Object TABLE_NAME, COLUMN_NAME, DATA_TYPE, CHARACTER_MAXIMUM_LENGTH, IS_NULLABLE | ConvertTo-Json | Out-File -FilePath "c:\Users\mushr\OneDrive\Desktop\StreamVault\scratch\all_columns.json" -Encoding utf8;
    $conn.Close();
    Write-Host "Success";
} catch {
    $_.Exception.ToString() | Out-File -FilePath "c:\Users\mushr\OneDrive\Desktop\StreamVault\scratch\error.txt" -Encoding utf8;
    Write-Host "Error occurred";
}
