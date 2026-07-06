# Kubernetes 下一步學習路線

## 你現在已經有的基礎

這個專案已經包含幾個很好的 Kubernetes 練習元素：

- 多個服務：React UI、WMS API、SAP API、SAP Consumer
- 服務間通訊：Ingress、Service、HTTP API
- 非同步流程：RabbitMQ publish/consume
- 部署腳本：build image、load 到 minikube、更新 Deployment

## 第一階段：把服務穩定跑起來

目標：理解 Pod、Deployment、Service、Ingress 的日常操作。

練習：

```powershell
kubectl get pods
kubectl get svc
kubectl get ingress
kubectl describe pod <pod-name>
kubectl logs <pod-name>
```

你要能回答：

- 哪些 Pod 正在跑？
- 哪個 Service 對應哪個 Deployment？
- Ingress 如何把 `/api/wms` 導到 `wms-api`？
- Pod 如果啟動失敗，要先看 logs 還是 describe？

## 第二階段：健康檢查與自我修復

目標：理解 livenessProbe 和 readinessProbe。

這個專案的 Deployment 已經加入：

- readinessProbe：決定 Pod 什麼時候可以接流量
- livenessProbe：決定 Pod 是否需要重啟

練習：

```powershell
kubectl describe deployment wms-api
kubectl describe pod <wms-api-pod>
```

你要觀察：

- probe 成功時 Pod 會變成 Ready
- probe 失敗時 Kubernetes 會怎麼反應
- Service 只會把流量送給 Ready 的 Pod

## 第三階段：滾動更新與回復

目標：理解 image tag、rollout、rollback。

練習：

```powershell
kubectl rollout status deployment/wms-api
kubectl rollout history deployment/wms-api
kubectl rollout undo deployment/wms-api
```

你可以故意改一個 API 回傳文字，重新部署，再觀察新版如何替換舊版。

## 第四階段：設定管理

目標：把硬編碼設定移到 Kubernetes。

建議下一個實作：

- 建立 ConfigMap：放 `RabbitMQ__Host`
- 建立 Secret：放 RabbitMQ 帳號密碼
- Deployment 用 `envFrom` 或 `valueFrom` 注入設定

這會讓你開始接近真實環境的做法。

## 第五階段：擴縮與資源

目標：理解 requests、limits、replicas、HPA。

練習：

```powershell
kubectl scale deployment wms-api --replicas=3
kubectl get pods -l app=wms-api
kubectl top pods
```

下一步可以加：

- metrics-server
- HorizontalPodAutoscaler
- 壓力測試工具

## 第六階段：可觀測性

目標：不只是服務有沒有跑，而是知道它為什麼這樣跑。

建議方向：

- 結構化 logging
- OpenTelemetry
- Prometheus
- Grafana dashboard

對這個專案來說，最有價值的指標會是：

- WMS API 請求數
- RabbitMQ queue length
- SAP Consumer 成功/失敗處理數
- DLQ message count

## 我建議你的下一個實作

最適合的下一步是：把 RabbitMQ 設定改成 ConfigMap + Secret。

原因是它剛好連到 Kubernetes 很核心的觀念：

- 設定和程式分離
- Secret 不寫死在程式
- Deployment 注入環境變數
- 本機和叢集環境可以用不同設定

完成後，再接著練 HPA 和 Prometheus，會很順。

## ConfigMap + Secret 練習

這個專案已經加入 `Deploy/rabbitmq-config.yaml`，你可以用它練習：

```powershell
kubectl apply -f .\Deploy\rabbitmq-config.yaml
kubectl get configmap rabbitmq-config
kubectl get secret rabbitmq-secret
kubectl describe configmap rabbitmq-config
kubectl describe secret rabbitmq-secret
```

觀察重點：

- ConfigMap 可以直接看到內容
- Secret 預設不會在 describe 裡顯示明文
- Deployment 用 `valueFrom` 把設定注入 container

再重新部署 WMS API 和 SAP Consumer：

```powershell
.\Deploy\deploy-all.ps1 -Wms -Consumer
```

確認環境變數有進 Pod：

```powershell
kubectl exec deploy/wms-api -- printenv | findstr RabbitMQ
kubectl exec deploy/sap-consumer -- printenv | findstr RabbitMQ
```
