# K8sDemo Learning Notes

這份筆記整理這個專案可以練習的 Kubernetes 與可觀測性主題。

## 1. 基本資源觀察

練習目標：理解 Pod、Deployment、Service、Ingress 的關係。

```powershell
kubectl get pods
kubectl get svc
kubectl get ingress
kubectl describe pod <pod-name>
kubectl logs <pod-name>
```

你應該能回答：

- 哪些 Pod 正在 Running？
- Service selector 對應哪些 Pod labels？
- Ingress 如何把 `/api/wms` 導到 `wms-api`？
- Pod 不 Ready 時要先看 `describe` 還是 `logs`？

## 2. ConfigMap 與 Secret

練習目標：把設定從程式碼移到 Kubernetes。

設定檔：

```text
Deploy/rabbitmq-config.yaml
```

查看設定：

```powershell
kubectl get configmap rabbitmq-config -o yaml
kubectl describe secret rabbitmq-secret
```

確認環境變數有注入：

```powershell
kubectl exec deploy/wms-api -- printenv | findstr RabbitMQ
kubectl exec deploy/sap-consumer -- printenv | findstr RabbitMQ
```

重點：

- ConfigMap 適合一般設定
- Secret 適合帳號、密碼
- .NET 使用 `__` 對應巢狀設定，例如 `RabbitMQ__Host`

## 3. Health Checks 與 Probes

練習目標：理解 livenessProbe 與 readinessProbe 的差異。

服務端點：

```text
/health/live
/health/ready
/healthz
```

Kubernetes probes：

- liveness：服務本身是否還活著
- readiness：服務是否已準備好接流量

查看：

```powershell
kubectl describe pod -l app=wms-api
kubectl describe pod -l app=sap-consumer
kubectl describe pod -l app=sap-api
```

手動驗證：

```powershell
Invoke-RestMethod http://127.0.0.1:<ingress-port>/health/live
Invoke-RestMethod http://127.0.0.1:<ingress-port>/health/ready
```

## 4. RabbitMQ Retry / DLQ Flow

練習目標：理解事件流、重試佇列與死信佇列。

目前流程：

```text
sap-events exchange
  routing key: material
  -> sap-material

SAP Consumer startup
  -> 宣告 exchange
  -> 宣告 sap-material
  -> 宣告 sap-material-retry
  -> 宣告 sap-material-dlq

retry count < 3
  -> sap-material-retry
  -> 等待 TTL
  -> dead-letter 回 sap-events/material
  -> sap-material

retry count >= 3
  -> sap-material-dlq
```

送出事件：

```powershell
Invoke-RestMethod -Method Post http://127.0.0.1:<ingress-port>/api/wms/material-picked
Invoke-RestMethod -Method Post http://127.0.0.1:<ingress-port>/api/wms/material-retry
Invoke-RestMethod -Method Post http://127.0.0.1:<ingress-port>/api/wms/material-fail
```

看 dashboard：

```powershell
Invoke-RestMethod http://127.0.0.1:<ingress-port>/api/dashboard/status
Invoke-RestMethod http://127.0.0.1:<ingress-port>/api/dashboard/dlq
```

看 RabbitMQ queues：

```powershell
kubectl exec deploy/rabbitmq -- rabbitmqctl list_queues name messages
```

看 Consumer logs：

```powershell
kubectl logs deployment/sap-consumer --tail=120
```

## 5. Rolling Update 與 Rollback

練習目標：理解 image tag、rollout、rollback。

```powershell
kubectl rollout status deployment/wms-api
kubectl rollout history deployment/wms-api
kubectl rollout undo deployment/wms-api
```

可以故意改一段回應文字、重新部署，觀察新舊 Pod 如何替換。

## 6. Prometheus Metrics

練習目標：讓 Prometheus scrape Pod metrics。

部署 Prometheus：

```powershell
.\Deploy\deploy-all.ps1 -Monitoring
```

開 Prometheus UI：

```powershell
kubectl port-forward svc/prometheus 19090:9090
```

瀏覽：

```text
http://127.0.0.1:19090
```

查 targets：

```promql
up
```

查服務指標：

```promql
k8sdemo_wms_published_total
k8sdemo_sap_api_requests_total
k8sdemo_sap_consumer_success_total
k8sdemo_sap_consumer_retry_total
k8sdemo_sap_consumer_dlq_total
k8sdemo_sap_consumer_dlq_messages
```

查最近 5 分鐘變化：

```promql
increase(k8sdemo_wms_published_total[5m])
increase(k8sdemo_sap_consumer_retry_total[5m])
increase(k8sdemo_sap_consumer_dlq_total[5m])
```

## 7. Grafana Dashboard

練習目標：把 Prometheus 指標視覺化。

部署 Grafana：

```powershell
.\Deploy\deploy-all.ps1 -Grafana
```

開 Grafana：

```powershell
kubectl port-forward svc/grafana 13000:3000
```

瀏覽：

```text
http://127.0.0.1:13000
```

登入：

```text
admin / admin
```

已預先建立：

- Prometheus datasource
- `K8sDemo Overview` dashboard

## 8. 資源與擴展練習

練習目標：理解 requests、limits、replicas。

```powershell
kubectl scale deployment wms-api --replicas=3
kubectl get pods -l app=wms-api
kubectl top pods
```

後續可以練：

- metrics-server
- HorizontalPodAutoscaler
- 壓力測試

## 9. 常見 Build 問題

如果 Windows 上遇到 `obj` 或 `bin` 檔案 `Access denied`：

```powershell
dotnet build-server shutdown

Get-ChildItem C:\Workspace\Project\K8sDemo -Recurse -Directory -Include bin,obj |
  Remove-Item -Recurse -Force

dotnet restore C:\Workspace\Project\K8sDemo\K8sDemo.slnx
dotnet build C:\Workspace\Project\K8sDemo\K8sDemo.slnx -m:1 -nr:false
```

這通常是 build server 或輸出檔被鎖住，不是程式碼錯誤。
