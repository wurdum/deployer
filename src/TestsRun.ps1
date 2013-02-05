Param(
	[string]$Url,
	[string]$UserName,
	[string]$Passwd,
	[string]$ConfigFile
)

$pass = convertto-securestring $Passwd -asplaintext -force
$user = New-Object Management.Automation.PSCredential($UserName, $pass)
invoke-command -ConnectionUri $Url -Credential $user -ScriptBlock `
{
	Param([string]$Url, [string]$ConfigFile)

	Write-Host "Connection to $Url is established"
	$root = "\\VBOXSVR\Share.Deployer"
	$runner = "TestRunner"
	$deployer = "Deployer.Tests"

	$runnerDir = Join-Path -path $root -childpath $runner
	$deployerDir = Join-Path -path $root -childpath $deployer

	$temp = [System.IO.Path]::GetTempPath()
	$testDir = Join-Path -path $temp -childpath "Deployer"
	$runnerTestDir = Join-Path -path $testDir -childpath $runner
	$deployerTestDir = Join-Path -path $testDir -childpath $deployer

	if (Test-Path $testDir) {
		Remove-Item -recurse -force $testDir
	}
	New-Item -itemtype directory -path $testDir | out-null

	Copy-Item $runnerDir $testDir -recurse
	Copy-Item $deployerDir $testDir -recurse

	cd $deployerTestDir
	(Get-Content $ConfigFile) | Foreach-Object { $_ -replace "\{root\}", $deployerTestDir } | Set-Content $ConfigFile

	cd $testDir
	.\TestRunner\NUnit-2.6.2\bin\nunit-console.exe /nologo /noxml .\Deployer.Tests\Deployer.Tests.Service.dll
	Write-Host "Connection is closed"
} -argumentList $Url, $ConfigFile