Param(
  [Parameter(Mandatory=$True,Position=1)]
  [string]$InputSolution,
  [Parameter(Mandatory=$False,Position=2)]
  [string]$OutputSolution,
  [Parameter(Mandatory=$False,Position=3)]
  [string]$TestPattern
)

if (-not $OutputSolution) { $OutputSolution = [IO.Path]::ChangeExtension($InputSolution, ".Tests.sln") }

if (Test-Path $OutputSolution) { Remove-Item $OutputSolution }

if (-not $TestPattern) { $TestPattern = '.+?\.Tests' }

$TestPattern = "^\s*Project\("".+?""\)\s*=\s*""$TestPattern"",.+$"

$State = 0

foreach ($SlnLine in Get-Content $InputSolution) {

  if ($SlnLine -match '^\s*GlobalSection\(NestedProjects\)') {

    $State = 4

  } elseif ($State -eq 4 -and $SlnLine -match '^\s*EndGlobalSection') {

    $State = 3
    continue;

  } elseif ($SlnLine -match '^\s*Global$') {

    $State = 3
    
  }

  if ($State -eq 3) {
    # Copy build configuration

    Add-Content $OutputSolution $SlnLine

  } elseif ($SlnLine -match $TestPattern) {
    # Copy test projects

    $State = 2
    Add-Content $OutputSolution $SlnLine

  } elseif ($SlnLine -match '^\s*Project\(') {
    #Ignore any preceding non-test projects

    $State = 1

  } elseif ($State -eq 2) {
    # Copy project metadata

    Add-Content $OutputSolution $SlnLine
    if ($SlnLine -match '^\s*EndProject$') { $State = 1 }

  } elseif ($State -eq 0) {
    # Copy header

    Add-Content $OutputSolution $SlnLine

  }

}

Write-Host "Created $OutputSolution."
