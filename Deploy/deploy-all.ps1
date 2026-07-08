param(
    [switch]$Wms,
    [switch]$SapApi,
    [switch]$Consumer,
    [switch]$React
)

$ErrorActionPreference = "Stop"

$Version = Get-Date -Format "yyyyMMddHHmmss"

Write-Host ""
Write-Host "Deploy Version: $Version"
Write-Host ""

if (-not ($Wms -or $SapApi -or $Consumer -or $React))
{
    Write-Host "No service selected."
    Write-Host "Usage:"
    Write-Host ".\Deploy\deploy-all.ps1 -Wms"
    Write-Host ".\Deploy\deploy-all.ps1 -SapApi"
    Write-Host ".\Deploy\deploy-all.ps1 -Consumer"
    Write-Host ".\Deploy\deploy-all.ps1 -React"
    Write-Host ".\Deploy\deploy-all.ps1 -Wms -SapApi -Consumer -React"
    exit 0
}

$root = Split-Path -Parent $PSScriptRoot

Set-Location $root

function Assert-LastCommandSucceeded
{
    param(
        [string]$Action
    )

    if ($LASTEXITCODE -ne 0)
    {
        throw "$Action failed with exit code $LASTEXITCODE"
    }
}

function Show-DockerTroubleshooting
{
    Write-Host ""
    Write-Host "Docker / Minikube troubleshooting:" -ForegroundColor Yellow
    Write-Host "1. Check Docker Desktop is fully started."
    Write-Host "2. Run:"
    Write-Host "   wsl --shutdown"
    Write-Host "3. Restart Docker Desktop manually."
    Write-Host "4. Then run:"
    Write-Host "   minikube start"
    Write-Host "5. If minikube container is stuck:"
    Write-Host "   docker ps -a | findstr minikube"
    Write-Host "   docker rm -f minikube"
    Write-Host "   minikube start"
    Write-Host ""
}

# ==========================
# Check Docker
# ==========================

Write-Host "Checking Docker..."

try
{
    docker version | Out-Null

    if ($LASTEXITCODE -ne 0)
    {
        throw
    }

    Write-Host "Docker OK"
}
catch
{
    Write-Host ""
    Write-Host "Docker Desktop is not running or not responding." -ForegroundColor Red
    Write-Host "Please start or restart Docker Desktop."

    Show-DockerTroubleshooting

    exit 1
}

# ==========================
# Check Minikube
# ==========================

Write-Host ""
Write-Host "Checking Minikube..."

$status = minikube status

if ($status -match "host:\s+Stopped")
{
    Write-Host "Minikube is stopped."
    Write-Host "Starting Minikube..."

    minikube start

    if ($LASTEXITCODE -ne 0)
    {
        Write-Host ""
        Write-Host "Failed to start Minikube." -ForegroundColor Red
        exit 1
    }
}

Write-Host "Minikube OK"

# ==========================
# Check Kubernetes Cluster
# ==========================

Write-Host ""
Write-Host "Checking Kubernetes..."

try
{
    kubectl get nodes | Out-Null

    if ($LASTEXITCODE -ne 0)
    {
        Write-Host ""
        Write-Host "Failed to start Minikube." -ForegroundColor Red

        Show-DockerTroubleshooting

        exit 1
    }

    Write-Host "Kubernetes OK"
}
catch
{
    Write-Host ""
    Write-Host "Kubernetes cluster is unavailable." -ForegroundColor Red
    Write-Host ""

    minikube status

    Write-Host ""
    Write-Host "Please run:"
    Write-Host "    minikube start"
    Write-Host ""

    exit 1
}

kubectl apply -f .\Deploy\rabbitmq-config.yaml

# ==========================
# WMS API
# ==========================

if ($Wms)
{
    Write-Host "Build WMS API: wms-api:$Version"

    docker build `
        -t wms-api:$Version `
        -f .\Backend\K8sDemo.WmsApi\dockerfile .
    Assert-LastCommandSucceeded "Build WMS API image"

    Write-Host "Load WMS API image"

    minikube image load wms-api:$Version
    Assert-LastCommandSucceeded "Load WMS API image"

    kubectl apply -f .\Deploy\wms-api.yaml
    Assert-LastCommandSucceeded "Apply WMS API manifest"

    kubectl set image `
        deployment/wms-api `
        wms-api=wms-api:$Version
    Assert-LastCommandSucceeded "Update WMS API image"

    kubectl rollout status deployment/wms-api
    Assert-LastCommandSucceeded "Roll out WMS API"
}

# ==========================
# SAP API
# ==========================

if ($SapApi)
{
    Write-Host "Build SAP API: sap-api:$Version"

    docker build `
        -t sap-api:$Version `
        -f .\Backend\K8sDemo.SapApi\dockerfile .
    Assert-LastCommandSucceeded "Build SAP API image"

    Write-Host "Load SAP API image"

    minikube image load sap-api:$Version
    Assert-LastCommandSucceeded "Load SAP API image"

    kubectl apply -f .\Deploy\sap-api.yaml
    Assert-LastCommandSucceeded "Apply SAP API manifest"

    kubectl set image `
        deployment/sap-api `
        sap-api=sap-api:$Version
    Assert-LastCommandSucceeded "Update SAP API image"

    kubectl rollout status deployment/sap-api
    Assert-LastCommandSucceeded "Roll out SAP API"
}

# ==========================
# SAP Consumer
# ==========================

if ($Consumer)
{
    Write-Host "Build SAP Consumer: sap-consumer:$Version"

    docker build `
        -t sap-consumer:$Version `
        -f .\Backend\K8sDemo.SapConsumer\dockerfile .
    Assert-LastCommandSucceeded "Build SAP Consumer image"

    Write-Host "Load SAP Consumer image"

    minikube image load sap-consumer:$Version
    Assert-LastCommandSucceeded "Load SAP Consumer image"

    kubectl apply -f .\Deploy\sap-consumer.yaml
    Assert-LastCommandSucceeded "Apply SAP Consumer manifest"

    kubectl set image `
        deployment/sap-consumer `
        sap-consumer=sap-consumer:$Version
    Assert-LastCommandSucceeded "Update SAP Consumer image"

    kubectl rollout status deployment/sap-consumer
    Assert-LastCommandSucceeded "Roll out SAP Consumer"
}

# ==========================
# React UI
# ==========================

if ($React)
{
    Write-Host "Build React UI: react-ui:$Version"

    docker build `
        -t react-ui:$Version `
        -f .\Frontend\k8sdemo-react-ui\dockerfile `
        .\Frontend\k8sdemo-react-ui
    Assert-LastCommandSucceeded "Build React UI image"

    Write-Host "Load React UI image"

    minikube image load react-ui:$Version
    Assert-LastCommandSucceeded "Load React UI image"

    kubectl apply -f .\Deploy\react-ui.yaml
    Assert-LastCommandSucceeded "Apply React UI manifest"

    kubectl set image `
        deployment/react-ui `
        react-ui=react-ui:$Version
    Assert-LastCommandSucceeded "Update React UI image"

    kubectl rollout status deployment/react-ui
    Assert-LastCommandSucceeded "Roll out React UI"
}

Write-Host ""
Write-Host "Current Pods"

kubectl get pods

Write-Host ""
Write-Host "Current Services"

kubectl get svc

Write-Host ""
Write-Host "Deploy finished"

Write-Host ""
Write-Host "Starting ingress tunnel..."

$job = Start-Job -ScriptBlock {
    minikube service ingress-nginx-controller `
        -n ingress-nginx `
        --url 2>$null
}

$IngressUrl = $null

for ($i = 1; $i -le 10; $i++)
{
    Start-Sleep -Seconds 1

    $output = Receive-Job $job -Keep -ErrorAction SilentlyContinue

    $IngressUrl =
        $output |
        Where-Object { $_ -match "^http://127\.0\.0\.1:\d+" } |
        Select-Object -First 1

    if ($IngressUrl)
    {
        break
    }
}

Write-Host ""
Write-Host "Dashboard:"
Write-Host $IngressUrl

if ($IngressUrl)
{
    Start-Process $IngressUrl
}
else
{
    Write-Host "Cannot get ingress url."
}
