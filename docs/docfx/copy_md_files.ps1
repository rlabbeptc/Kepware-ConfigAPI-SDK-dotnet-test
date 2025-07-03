# This script is used to migrate markdown files from the repository to support the docfx build actions
# The migration ensures that the embedded links are correct when the website is built.
# 

# Define source and destination directories
$sourceDir = ".\"
$destinationDir = ".\"

# Define the subfolders in the repository that md files will be migrated to the website
$folderList = @("Kepware.Api\", "Kepware.Api.Sample\", "KepwareSync.Service\")

$mdFiles = @()

# Get all .md files recursively from the folder lists
foreach ($folder in $folderList)
{
    $joinedPath = $sourceDir + $folder
    # Get all .md files recursively from the source directory
    $list = Get-ChildItem -Path $joinedPath -Filter *.md -Recurse
    $mdFiles += $list 
}


foreach ($file in $mdFiles) {
    # Get the relative path of the file from the source directory
    $relativePath = Resolve-Path -Path $file.FullName -RelativeBasePath $sourceDir -Relative

    # Create the destination path by combining the destination directory with the relative path
    $destPath = Join-Path $destinationDir $relativePath.Substring(2)
    # Create the destination directory if it doesn't exist
    $destDir = Split-Path $destPath -Parent
    if (-not (Test-Path $destDir)) {
        New-Item -Path $destDir -ItemType Directory -Force
    }

    # Copy the file to the destination path
    Copy-Item -Path $file.FullName -Destination $destPath -Force
}

Write-Output "All .md files have been copied successfully!"
