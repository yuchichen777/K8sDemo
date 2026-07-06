# K8sDemo

這是一個用來練習 Kubernetes 的小型微服務範例：

- React UI
- WMS API
- SAP API
- SAP Consumer
- RabbitMQ

目前專案適合練習服務部署、Ingress 路由、訊息佇列、健康檢查、滾動更新和故障排查。

## 快速部署

在專案根目錄執行：

```powershell
.\Deploy\deploy-all.ps1 -Wms -SapApi -Consumer -React
```

腳本會建立新的 image tag、載入 minikube、套用 YAML、更新 Deployment image，並等待 rollout 完成。

## 建議練習順序

1. 觀察 Pod 與 Service
2. 走一次完整事件流程
3. 故意讓服務失敗，練習查看 logs 與 events
4. 調整 replicas，觀察 Service 如何分流
5. 修改程式後重新部署，觀察 rolling update
6. 把 RabbitMQ 設定移到 ConfigMap 和 Secret
7. 加上 HPA，練習自動擴縮

更完整的學習路線在 [K8S_LEARNING.md](K8S_LEARNING.md)。

## RabbitMQ 設定

RabbitMQ 連線資訊放在 Kubernetes 設定檔：

- `Deploy/rabbitmq-config.yaml` 的 ConfigMap：RabbitMQ host 與 management port
- `Deploy/rabbitmq-config.yaml` 的 Secret：RabbitMQ username 與 password

套用設定：

```powershell
kubectl apply -f .\Deploy\rabbitmq-config.yaml
```
