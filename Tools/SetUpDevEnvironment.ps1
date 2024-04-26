param(
    [Parameter()]
    [string] $password,
    [Parameter()]
    [switch]$localgRPC
)

if ($password -eq "") {
    $password = "Vi-CELL#01";
}

function Find-InTextFile {
    <#
    .SYNOPSIS
        Performs a find (or replace) on a string in a text file or files.
    .EXAMPLE
        PS> Find-InTextFile -FilePath 'C:\MyFile.txt' -Find 'water' -Replace 'wine'
    
        Replaces all instances of the string 'water' into the string 'wine' in
        'C:\MyFile.txt'.
    .EXAMPLE
        PS> Find-InTextFile -FilePath 'C:\MyFile.txt' -Find 'water'
    
        Finds all instances of the string 'water' in the file 'C:\MyFile.txt'.
    .PARAMETER FilePath
        The file path of the text file you'd like to perform a find/replace on.
    .PARAMETER Find
        The string you'd like to replace.
    .PARAMETER Replace
        The string you'd like to replace your 'Find' string with.
    .PARAMETER NewFilePath
        If a new file with the replaced the string needs to be created instead of replacing
        the contents of the existing file use this param to create a new file.
    .PARAMETER Force
        If the NewFilePath param is used using this param will overwrite any file that
        exists in NewFilePath.
    #>
    [CmdletBinding(DefaultParameterSetName = 'NewFile')]
    [OutputType()]
    param (
        [Parameter(Mandatory = $true)]
        [ValidateScript({Test-Path -Path $_ -PathType 'Leaf'})]
        [string[]]$FilePath,
        [Parameter(Mandatory = $true)]
        [string]$Find,
        [Parameter()]
        [string]$Replace,
        [Parameter(ParameterSetName = 'NewFile')]
        [ValidateScript({ Test-Path -Path ($_ | Split-Path -Parent) -PathType 'Container' })]
        [string]$NewFilePath,
        [Parameter(ParameterSetName = 'NewFile')]
        [switch]$Force
    )
    begin {
        $Find = [regex]::Escape($Find)
    }
    process {
        try {
            foreach ($File in $FilePath) {
                if ($Replace) {
                    if ($NewFilePath) {
                        if ((Test-Path -Path $NewFilePath -PathType 'Leaf') -and $Force.IsPresent) {
                            Remove-Item -Path $NewFilePath -Force
                            (Get-Content $File) -replace $Find, $Replace | Add-Content -Path $NewFilePath -Force
                        } elseif ((Test-Path -Path $NewFilePath -PathType 'Leaf') -and !$Force.IsPresent) {
                            Write-Warning "The file at '$NewFilePath' already exists and the -Force param was not used"
                        } else {
                            (Get-Content $File) -replace $Find, $Replace | Add-Content -Path $NewFilePath -Force
                        }
                    } else {
                        (Get-Content $File) -replace $Find, $Replace | Add-Content -Path "$File.tmp" -Force
                        Remove-Item -Path $File
                        Move-Item -Path "$File.tmp" -Destination $File
                    }
                } else {
                    Select-String -Path $File -Pattern $Find
                }
            }
        } catch {
            Write-Error $_.Exception.Message
        }
    }
}

$scriptDir = Split-Path $script:MyInvocation.MyCommand.Path

Write-Host "Updating the SignInViewModel.cs password..."
$path = @(Resolve-Path "$scriptDir\..\ScoutViewModels\ViewModels\Misc\SignInViewModel.cs")
Find-InTextFile -FilePath $path -Find "var password = Environment.GetEnvironmentVariable(`"ScoutX_Password`") ?? `"`";" -Replace "var password = Environment.GetEnvironmentVariable(`"ScoutX_Password`") ?? `"$password`";"

Write-Host "Updating the environment.config IsFromHardware..."
$path = @(Resolve-Path "$scriptDir\..\ScoutUtilities\UIConfiguration\environment.config";)
Find-InTextFile -FilePath $path -Find "isFromHardware=`"True`"" -Replace "isFromHardware=`"False`""

Write-Host "Updating the ui.config useWindowedMode..."
$path = @(Resolve-Path "$scriptDir\..\ScoutUtilities\UIConfiguration\ui.config";)
Find-InTextFile -FilePath $path -Find "useWindowedMode=`"False`"" -Replace "useWindowedMode=`"True`""

Write-Host "Updating the WatchdogConfiguration.json OpcServerPath..."
$path = @(Resolve-Path "$scriptDir\..\ScoutServices\Watchdog\WatchdogConfiguration.json";)
Find-InTextFile -FilePath $path -Find "`"servers`": [ `"\\Instrument\\OPCUaServer\\ViCellOpcUaServer.exe`" ]" -Replace "`"servers`": [ ]"

# update the gRPC Client, Server, Protos dll paths
if ($localgRPC) {
    write-host "updating the csproj files to use local gRPC dlls..."

    if (Test-Path "$scriptDir\..\..\Hawkeye_gRpc\GrpcClient\bin\Debug\net48\Protos.dll") {
        
        # the csproj files that will be updated:
        $path = @(
            (Resolve-Path "$scriptDir\..\ScoutOpcUa\ScoutOpcUa.csproj"),
            (Resolve-Path "$scriptDir\..\ScoutOpcUaTests\ScoutOpcUaTests.csproj"),
            (Resolve-Path "$scriptDir\..\ScoutServices\ScoutServices.csproj"),
            (Resolve-Path "$scriptDir\..\ScoutServicesTests\ScoutServicesTests.csproj"),
            (Resolve-Path "$scriptDir\..\ScoutViewModels\ScoutViewModels.csproj")
        )

        $clientStrOld = "..\..\target\dependencies\lib\GrpcClient.dll"
        $serverStrOld = "..\..\target\dependencies\lib\GrpcServer.dll"
        $protosStrOld = "..\..\target\dependencies\lib\Protos.dll"

        $clientStrNew = "..\..\..\gRpc\GrpcClient\bin\Debug\net48\GrpcClient.dll"
        $serverStrNew = "..\..\..\gRpc\GrpcServer\bin\Debug\net48\GrpcServer.dll"
        $protosStrNew = "..\..\..\gRpc\GrpcClient\bin\Debug\net48\Protos.dll"

        Find-InTextFile -FilePath $path -Find $clientStrOld -Replace $clientStrNew
        Find-InTextFile -FilePath $path -Find $serverStrOld -Replace $serverStrNew
        Find-InTextFile -FilePath $path -Find $protosStrOld -Replace $protosStrNew

        $clientStrOld = "..\target\dependencies\lib\GrpcClient.dll"
        $serverStrOld = "..\target\dependencies\lib\GrpcServer.dll"
        $protosStrOld = "..\target\dependencies\lib\Protos.dll"

        $clientStrNew = "..\..\gRpc\GrpcClient\bin\Debug\net48\GrpcClient.dll"
        $serverStrNew = "..\..\gRpc\GrpcServer\bin\Debug\net48\GrpcServer.dll"
        $protosStrNew = "..\..\gRpc\GrpcClient\bin\Debug\net48\Protos.dll"

        Find-InTextFile -FilePath $path -Find $clientStrOld -Replace $clientStrNew
        Find-InTextFile -FilePath $path -Find $serverStrOld -Replace $serverStrNew
        Find-InTextFile -FilePath $path -Find $protosStrOld -Replace $protosStrNew
    } else {
        Write-Host "`tWARNING: Cannot find Protos.dll with expected relative path. Csproj files not updated" -ForegroundColor Yellow
    }
}