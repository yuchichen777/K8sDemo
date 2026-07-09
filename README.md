# K8sDemo

A Kubernetes microservices demo built with .NET 10, RabbitMQ, React, Docker, Minikube, Prometheus, and Grafana.

This project simulates a simplified WMS event flow:

```text
React UI
  -> WMS API
  -> RabbitMQ
  -> SAP Consumer
  -> SAP API
```

## Features

- React dashboard
- RabbitMQ event publishing and consuming
- Retry mechanism
- Dead letter queue (DLQ)
- Centralized RabbitMQ topology initialization
- Manual DLQ requeue
- Dashboard statistics
- Kubernetes Deployments, Services, Ingress, ConfigMap, and Secret
- Health checks with liveness and readiness probes
- Prometheus metrics endpoints
- Prometheus deployment with annotation-based pod scraping
- Grafana deployment with a pre-provisioned K8sDemo dashboard
- Central NuGet package version management

## Project Structure

```text
Backend/
  K8sDemo.WmsApi/
  K8sDemo.SapApi/
  K8sDemo.SapConsumer/
  K8sDemo.Shared/

Frontend/
  k8sdemo-react-ui/

Deploy/
  deploy-all.ps1
  rabbitmq-config.yaml
  ingress.yaml
  wms-api.yaml
  sap-api.yaml
  sap-consumer.yaml
  react-ui.yaml
  prometheus.yaml
  grafana.yaml
```

## Quick Start

Start Minikube:

```powershell
minikube start
```

Deploy the application services:

```powershell
.\Deploy\deploy-all.ps1 -Wms -SapApi -Consumer -React
```

Deploy monitoring:

```powershell
.\Deploy\deploy-all.ps1 -Monitoring
.\Deploy\deploy-all.ps1 -Grafana
```

The deploy script checks Docker, Minikube, and Kubernetes, then builds images, loads them into Minikube, applies manifests, updates Deployment images, and waits for rollouts.

## Runtime Configuration

Runtime settings are managed with `appsettings.json` and Kubernetes environment variables.

Local configuration examples:

- `Backend/K8sDemo.WmsApi/appsettings.json`
- `Backend/K8sDemo.SapConsumer/appsettings.json`

Kubernetes configuration:

- `Deploy/rabbitmq-config.yaml`

Important keys:

```text
RabbitMQ__Host
RabbitMQ__ManagementPort
RabbitMQ__ExchangeName
RabbitMQ__MaterialQueueName
RabbitMQ__MaterialRetryQueueName
RabbitMQ__MaterialDlqQueueName
RabbitMQ__MaterialRoutingKey
RabbitMQ__RetryDelayMilliseconds
SapApi__BaseUrl
SapConsumer__BaseUrl
Retry__MaxRetryCount
```

## RabbitMQ Retry / DLQ Flow

Retry delay is handled by RabbitMQ, not by blocking the SAP Consumer.

```text
sap-events exchange
  routing key: material
  -> sap-material

SAP Consumer
  startup
    -> initialize exchange, material queue, retry queue, and DLQ
  success
    -> ack
  retry count < Retry__MaxRetryCount
    -> publish to sap-material-retry
    -> ack original message
  retry count >= Retry__MaxRetryCount
    -> nack original message
    -> sap-material-dlq

sap-material-retry
  x-message-ttl: RabbitMQ__RetryDelayMilliseconds
  x-dead-letter-exchange: sap-events
  x-dead-letter-routing-key: material
```

RabbitMQ credentials are stored in the `rabbitmq-secret` Secret:

```text
RabbitMQ__Username
RabbitMQ__Password
```

## Health Checks

Each backend service exposes:

```text
/health/live
/health/ready
/healthz
```

Kubernetes probes use:

- `livenessProbe`: `/health/live`
- `readinessProbe`: `/health/ready`

WMS readiness checks RabbitMQ. SAP Consumer readiness checks RabbitMQ and SAP API. SAP API readiness checks only itself.

Ingress exposes WMS health endpoints:

```powershell
Invoke-RestMethod http://127.0.0.1:<ingress-port>/health/live
Invoke-RestMethod http://127.0.0.1:<ingress-port>/health/ready
```

## Metrics

Each backend service exposes Prometheus metrics:

```text
/metrics
```

Important metrics:

```promql
k8sdemo_wms_published_total
k8sdemo_sap_api_requests_total
k8sdemo_sap_api_success_total
k8sdemo_sap_api_failure_total
k8sdemo_sap_consumer_success_total
k8sdemo_sap_consumer_fail_total
k8sdemo_sap_consumer_retry_total
k8sdemo_sap_consumer_dlq_total
k8sdemo_sap_consumer_dlq_messages
```

Prometheus discovers pods with these annotations:

```yaml
prometheus.io/scrape: "true"
prometheus.io/path: /metrics
prometheus.io/port: "8080"
```

Open Prometheus:

```powershell
kubectl port-forward svc/prometheus 19090:9090
```

Then browse:

```text
http://127.0.0.1:19090
```

Useful PromQL:

```promql
up
increase(k8sdemo_wms_published_total[5m])
increase(k8sdemo_sap_consumer_retry_total[5m])
increase(k8sdemo_sap_consumer_dlq_total[5m])
```

## Grafana

Open Grafana:

```powershell
kubectl port-forward svc/grafana 13000:3000
```

Then browse:

```text
http://127.0.0.1:13000
```

Default login:

```text
admin / admin
```

Grafana is provisioned with:

- Prometheus datasource: `http://prometheus:9090`
- Dashboard: `K8sDemo Overview`

## Smoke Test

Use the ingress dashboard URL printed by `deploy-all.ps1`.

Check service health:

```powershell
Invoke-RestMethod http://127.0.0.1:<ingress-port>/api/dashboard/status
Invoke-RestMethod http://127.0.0.1:<ingress-port>/health/ready
```

Publish demo events:

```powershell
Invoke-RestMethod -Method Post http://127.0.0.1:<ingress-port>/api/wms/material-picked
Invoke-RestMethod -Method Post http://127.0.0.1:<ingress-port>/api/wms/material-retry
Invoke-RestMethod -Method Post http://127.0.0.1:<ingress-port>/api/wms/material-fail
```

Check metrics:

```powershell
Invoke-RestMethod http://127.0.0.1:<ingress-port>/metrics
```

## Build Troubleshooting

If `dotnet build` fails with `Access denied` under `bin` or `obj`, stop build servers and clean generated output:

```powershell
dotnet build-server shutdown

Get-ChildItem C:\Workspace\Project\K8sDemo -Recurse -Directory -Include bin,obj |
  Remove-Item -Recurse -Force

dotnet restore C:\Workspace\Project\K8sDemo\K8sDemo.slnx
dotnet build C:\Workspace\Project\K8sDemo\K8sDemo.slnx -m:1 -nr:false
```

Using `-m:1 -nr:false` avoids some Windows file locking issues around the shared project output.

## Learning Notes

More Kubernetes practice notes are in [K8S_LEARNING.md](K8S_LEARNING.md).
