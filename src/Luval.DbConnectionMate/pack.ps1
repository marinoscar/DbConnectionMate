# Get the first .csproj file in the current directory
$csprojFile = Get-ChildItem -Path . -Filter *.csproj | Select-Object -First 1

if (-not $csprojFile) {
    Write-Error "No .csproj file found in the current directory."
    exit 1
}

# Load the project file as XML
[xml]$projectFile = Get-Content $csprojFile.FullName

# Ensure the <Project> root element exists
if (-not $projectFile.Project) {
    Write-Error "Invalid .csproj file structure. Could not find the <Project> root element."
    exit 1
}

# Locate or create the <PropertyGroup> node
$propertyGroup = $projectFile.Project.PropertyGroup
if (-not $propertyGroup) {
    Write-Host "No <PropertyGroup> element found. Creating a new one..."
    $propertyGroup = $projectFile.CreateElement("PropertyGroup")
    $projectFile.Project.AppendChild($propertyGroup) | Out-Null
}

# Check for namespaces and handle accordingly
$namespaceManager = $null
if ($projectFile.DocumentElement.NamespaceURI -ne "") {
    $namespaceManager = New-Object System.Xml.XmlNamespaceManager($projectFile.NameTable)
    $namespaceManager.AddNamespace("ns", $projectFile.DocumentElement.NamespaceURI)
}

# Locate or create the <Version> node
if ($namespaceManager) {
    $versionNode = $propertyGroup.SelectSingleNode("ns:Version", $namespaceManager)
} else {
    $versionNode = $propertyGroup.SelectSingleNode("Version")
}

if (-not $versionNode) {
    Write-Host "No <Version> element found. Adding a default version..."
    $versionNode = $projectFile.CreateElement("Version", $projectFile.DocumentElement.NamespaceURI)
    $versionNode.InnerText = "1.0.0"  # Default version
    $propertyGroup.AppendChild($versionNode) | Out-Null
}

# Increment version
try {
    $currentVersion = [Version]$versionNode.InnerText
} catch {
    Write-Warning "Invalid version format detected. Resetting to 1.0.0."
    $currentVersion = [Version]"1.0.0"
}

$newVersion = "$($currentVersion.Major).$($currentVersion.Minor).$($currentVersion.Build + 1)"
$versionNode.InnerText = $newVersion

# Save the updated .csproj file
$projectFile.Save($csprojFile.FullName)
Write-Host "Updated version to $newVersion in $($csprojFile.FullName)."

# Build and pack the project
Write-Host "Building and packing the project..."
dotnet pack -c Release
if ($LASTEXITCODE -eq 0) {
    Write-Host "Build and pack completed successfully."
} else {
    Write-Error "Build or pack failed. Please check the logs for details."
}
