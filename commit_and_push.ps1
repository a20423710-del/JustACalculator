# Commit and push script for JustACalculator
# Usage: Open PowerShell and run from any location.

Set-StrictMode -Version Latest

$RepoPath = "C:\Users\Alex\source\repos\JustACalculator"
$Message = "Save project"

if (-not (Test-Path $RepoPath)) {
	Write-Error "Repository path not found: $RepoPath"
	exit 1
}

Push-Location $RepoPath
try {
	git status
	git add -A
	git commit -m "$Message" || Write-Host "Nothing to commit or commit failed."
	git push origin master
} finally {
	Pop-Location
}
