$p = Start-Process -FilePath "dotnet" -ArgumentList "run --project backend --no-build" -PassThru
Start-Sleep -Seconds 6
Stop-Process -Id $p.Id -Force -ErrorAction SilentlyContinue
Stop-Process -Name "project_PRN232.Backend" -Force -ErrorAction SilentlyContinue
Write-Host "Completed executing DatabaseFontFixer!"

