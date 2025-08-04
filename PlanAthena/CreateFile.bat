@echo off
powershell -NoProfile -ExecutionPolicy Bypass -Command "& {
    $excludedDirs = @('\bin', '\obj', '\Properties');
    $rootPath = 'D:\Users\eldam\Source\Repos\PlanAthena\PlanAthena'; # Remplacez par le chemin de votre dossier racine

    function Get-Tree {
        param([string]$path, [int]$level = 0)

        $indent = '    ' * $level;
        Get-ChildItem $path | ForEach-Object {
            $excluded = $false;
            foreach ($dir in $excludedDirs) {
                if ($_.FullName -like "*$dir") {
                    $excluded = $true;
                    break;
                }
            }
            if (-not $excluded) {
                Write-Output ('{0}{1}' -f $indent, $_.Name);
                if ($_.PSIsContainer) {
                    Get-Tree -path $_.FullName -level ($level + 1);
                }
            }
        }
    }

    Get-Tree -path $rootPath | Out-File 'file.txt';
}"
