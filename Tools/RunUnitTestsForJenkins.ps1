$scriptDir = Split-Path $script:MyInvocation.MyCommand.Path
$targetDir = Resolve-Path "$scriptDir\..\target\Release"
$pwd = &pwd
$runner = "$pwd\packages\nunit.consolerunner\3.12.0\tools\nunit3-console.exe"
$exitCode = 0

$testDlls = @('ScoutUiTest.dll', 'ScoutVmTests.dll', 'ScoutServicesTests.dll', 'ScoutModelsTests.dll', 'ScoutUtilitiesTest.dll', 'ScoutOpcUaTests.dll', 'ScoutDomainsTests.dll')

if (!(Test-Path $targetDir)) {
	Write-Host "Directory does not exist: '$targetDir'" -ForegroundColor Red
	Write-Host "Error - unable to find and set the target directory. You can only run this script from the parent directory of this batch file. Current Running Dir: '$pwd'" -ForegroundColor Yellow
	exit 2;
}

if (!(Test-Path $runner)) {
	Write-Host "Nunit test running not found: '$runner'" -ForegroundColor Red
	exit 3;
}

foreach($dll in $testDlls) {
	if (!(Test-Path "$targetDir\$dll")) {
		Write-Host "--------------------------------------------"
		Write-Host "Nunit Test DLL not found: '$dll'" -ForegroundColor Yellow
		$exitCode++;
		Write-Host "--------------------------------------------"
		continue;
	}
	
	$dllPath = "$targetDir\$dll"
	if (!(Test-Path "$targetDir\..\testResults")) {
		mkdir "$targetDir\..\testResults"
	}
	$workPath = Resolve-Path "$targetDir\..\testResults"
	$workArg = "--work $workPath";
	$outputArg = "--result=$dll.xml;format=nunit3"
	
	cmd /c "$runner $dllPath $workArg $outputArg"
	
	$testExitCode = $lastExitCode
	$exitCode += $testExitCode
	
	Write-Host "--------------------------------------------"
	if ($testExitCode -ne 0) {
		Write-Host "$dll ExitCode - $testExitCode" -ForegroundColor Red
	} else {
		Write-Host "$dll ExitCode - $testExitCode"
	}
	Write-Host "--------------------------------------------"
}

Write-Host "--------------------------------------------"
if ($exitCode -ne 0) {
	Write-Host "Unit Test Summary::Overall Error Code: $exitCode" -ForegroundColor Red
} else {
	Write-Host "Unit Test Summary::Overall Error Code: $exitCode"
}
Write-Host "--------------------------------------------"
exit $exitCode
