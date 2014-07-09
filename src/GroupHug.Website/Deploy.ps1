# IIS deployment script for api.spotlight.com

function FixBindings([string]$websiteName, $bindings, [string]$protocol, [int]$port) {
    $iisPath = "IIS:\Sites\$websiteName"
    if (!(Test-Path $iisPath)) {
        Abort "Can't find $websiteName at $iisPath"
    }

    $existingBindings = @(Get-WebBinding -Name $websiteName -Protocol $protocol)

    $verifiedBindings = @() # ones found that we need
    $extraBindings = @() # ones found that we need to delete
    $missingBindings = @() # ones not found

    foreach($binding in $existingBindings) {
        $binds = SplitWebBindingInformation($binding)
        if (($binds.IPAddress -ne '*') -or ($binds.Port -ne $port)) {
            Write-Host "Extra binding" $binding.bindingInformation;
            $extraBindings += $binding
            continue
        }
        if ($bindings -contains $binds.HostHeader) {
            $verifiedBindings += $binds.HostHeader;
            continue;
        }
        $extraBindings += $binding
    }

    foreach ($binding in $bindings) {
        if (-not ($verifiedBindings -contains $binding)) {
            $missingBindings += $binding;
        }
    }

    foreach($binding in $verifiedBindings) {
        Write-Host "Verified binding: $binding"
    }

    foreach ($binding in $missingBindings) {
        Write-Host "Created binding $protocol/$binding"
        New-WebBinding -Name $websiteName -HostHeader $binding -Protocol $protocol
    }

    foreach ($binding in $extraBindings) {
        Write-Host "Removed unknown binding $($binding.bindingInformation) for $websiteName"
        $binding | Remove-WebBinding
    }
}

$iisVersion = Get-ItemProperty "HKLM:\software\microsoft\InetStp";
if ($iisVersion.MajorVersion -eq 7) {
    if ($iisVersion.MinorVersion -ge 5) {
        try {
            Import-Module WebAdministration -ea Stop
        } catch {
            Abort "WebAdministration module not found"
        }
    } else {
        if (-not (Get-PSSnapIn | Where {$_.Name -eq "WebAdministration";})) {
            try {
                Add-PSSnapIn WebAdministration -ea Stop
            } catch {
                Abort "WebAdministration module not found"
            }
        } 
    }
} else {
    Abort "Couldn't determine IIS version - is IIS installed?"
}


function SplitWebBindingInformation($binding) {
    $ip,$port,$hh = $binding.bindingInformation.split(':')
    return New-Object PSObject -Property @{
        Port=$port
        HostHeader=$hh
        IPAddress=$ip
    }
}


$websiteName = "GroupHug"
$websitePath = "IIS:\Sites\$websiteName"

if (!(Test-Path $websitePath)) {
    Write-Host "Creating new IIS website $websiteName at $websitePath"
    New-Website $websiteName | out-null
}

Set-ItemProperty $websitePath -name applicationPool -value "ASP.NET v4.0"

$machineName = [Environment]::MachineName.ToLower()

# On dev machines, this will point IIS at your working project folder.
# On Octopus tentacles, this will point IIS at the folder where the code's been deployed
# By happy coincidence, both of these are the Right Thing to do :)
$scriptPath = split-path $MyInvocation.MyCommand.Definition -parent 
Write-Host "Setting PhysicalPath to $scriptPath"
Set-ItemProperty $websitePath -name PhysicalPath -value "$scriptPath"

$hostHeaders = @("grouphug")
$hostHeaders += "grouphug.local"
$hostHeaders += "grouphug.$machineName"

FixBindings "GroupHug" $hostHeaders "http" 80    

Write-Host "******** Successfully deployed GroupHug ********"