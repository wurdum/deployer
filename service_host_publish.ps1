Get-ExecutionPolicy

$source = 'E:\Deployer\Deployer.Service.Host\bin\Debug\'
$dest = 'E:\Virtual Machine\Share\DeployService'
$exclude = @('*.pdb', 'Deployer.Service.Host.exe.config', 'DepSettings.xml')
Remove-Item '$dest\*' -Recurse
Get-ChildItem $source -Recurse -Exclude $exclude | Copy-Item -Destination {Join-Path $dest $_.FullName.Substring($source.length)}

New-Item -path $dest -name 'Backup' -itemType 'directory'
New-Item -path $dest -name 'Packages' -itemType 'directory'