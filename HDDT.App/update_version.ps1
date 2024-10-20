# Define the path to your AssemblyInfo.cs file
$assemblyInfoPath = "C:\Users\hhtbb\source\repos\HDDT\HDDT.App\Properties\AssemblyInfo.cs"

# Define the path for the version file
$versionFilePath = "C:\Users\hhtbb\source\repos\HDDT\Build\version.txt"

# Define the regular expression patterns for the version lines
$assemblyVersionPattern = '^\[assembly: AssemblyVersion\("(\d+)\.(\d+)\.(\d+)\.(\d+)"\)\]$'
$fileVersionPattern = '^\[assembly: AssemblyFileVersion\("(\d+)\.(\d+)\.(\d+)\.(\d+)"\)\]$'

# Read the AssemblyInfo.cs file content
$assemblyInfoContent = Get-Content $assemblyInfoPath

# Initialize global version variables
$global:major = 0
$global:minor = 0
$global:build = 0
$global:revision = 0

# Function to increment version number
function Increment-Version {
    param (
        [string]$line,
        [string]$pattern,
        [string]$label
    )
    if ($line -match $pattern) {
        # Extract version numbers
        $global:major = [int]$matches[1]
        $global:minor = [int]$matches[2]
        $global:build = [int]$matches[3]
        $global:revision = [int]$matches[4]

        # Increment the build number
        $global:build++

        # Return the updated version line
        return "[assembly: $label(`"$major.$minor.$build.$revision`")]"
    } else {
        return $line
    }
}

# Iterate through the content of AssemblyInfo.cs and update the version lines
$updatedContent = $assemblyInfoContent | ForEach-Object {
    if ($_ -match $assemblyVersionPattern) {
        Increment-Version $_ $assemblyVersionPattern 'AssemblyVersion'
    } elseif ($_ -match $fileVersionPattern) {
        Increment-Version $_ $fileVersionPattern 'AssemblyFileVersion'
    } else {
        $_
    }
}

# Write the updated content back to the AssemblyInfo.cs file
$updatedContent | Set-Content $assemblyInfoPath

# Write new version info to version.txt
$versionInfo = @"
Version: $major.$minor.$build.$revision
Date: $(Get-Date -Format "yyyy-MM-dd")
Author: Phuc Pham Hong
Notes: Updated version after build.
"@

# Create or overwrite the version file
$versionInfo | Set-Content $versionFilePath

Write-Host "Version numbers updated successfully!"