# Define the path to your AssemblyInfo.cs file
$assemblyInfoPath = "C:\Users\hhtbb\source\repos\HDDT\HDDT.App\Properties\AssemblyInfo.cs"

# Define the regular expression patterns for the version lines
$assemblyVersionPattern = '^\[assembly: AssemblyVersion\("(\d+)\.(\d+)\.(\d+)\.(\d+)"\)\]$'
$fileVersionPattern = '^\[assembly: AssemblyFileVersion\("(\d+)\.(\d+)\.(\d+)\.(\d+)"\)\]$'

# Read the AssemblyInfo.cs file content
$assemblyInfoContent = Get-Content $assemblyInfoPath

# Function to increment version number
function Increment-Version {
    param (
        [string]$line,
        [string]$pattern,
        [string]$label
    )
    if ($line -match $pattern) {
        # Extract version numbers
        $major = [int]$matches[1]
        $minor = [int]$matches[2]
        $build = [int]$matches[3]
        $revision = [int]$matches[4]

        # Increment the build number (or any other version part as needed)
        $build++

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

Write-Host "Version numbers updated successfully!"