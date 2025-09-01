# Get-ProjectTree.ps1 (Version 2.0 - Robuste)
#
# Objectif: Lister les fichiers et dossiers pertinents du projet en utilisant
# les commandes natives de PowerShell pour une fiabilité maximale.

# --- Configuration ---
$outputFile = "ProjectStructure.txt"
$excludedDirs = @("bin", "obj", ".vs", "packages", ".git")
$excludedFiles = @("*.csproj.user", "*.suo", "*.lock.json", "ProjectStructure.txt", "Get-ProjectTree.ps1", "Get-ProjectTree.bat")

# --- Exécution ---
Write-Host "Génération de l'arborescence du projet (version fiable)..."

# Récupère tous les éléments (fichiers et dossiers) de manière récursive
# en excluant les dossiers et fichiers spécifiés dès le départ.
$items = Get-ChildItem -Path . -Recurse -Exclude $excludedFiles | Where-Object {
    # On vérifie si le chemin complet de l'élément contient un des dossiers à exclure
    $shoudExclude = $false
    foreach ($dir in $excludedDirs) {
        if ($_.FullName -match "\\$dir\\|\\$dir$") {
            $shoudExclude = $true
            break
        }
    }
    -not $shoudExclude
}

# Formatte la sortie pour qu'elle soit lisible
$formattedOutput = $items | ForEach-Object {
    # Calcule la profondeur pour l'indentation
    $depth = ($_.FullName.Split('\').Count - (Get-Location).Path.Split('\').Count)
    $indentation = "    " * $depth
    
    # Ajoute un marqueur si c'est un dossier
    if ($_.PSIsContainer) {
        "$indentation+--- $($_.Name)"
    } else {
        "$indentation    $($_.Name)"
    }
}

# Crée le fichier de sortie
$header = "Structure du Projet - Généré le $(Get-Date)"
$header | Out-File -FilePath $outputFile -Encoding utf8
"D:.`n" | Add-Content -Path $outputFile -Encoding utf8 # Simule la racine
$formattedOutput | Add-Content -Path $outputFile -Encoding utf8

Write-Host "Terminé ! La structure a été sauvegardée dans '$outputFile'."