# K8sDemo

A Kubernetes microservices demo built with **.NET 10**, **RabbitMQ**, **React**, **Docker**, and **Kubernetes**.

This project simulates a simplified WMS (Warehouse Management System) event flow using an event-driven architecture.

---

## Architecture

```
React UI
    │
    ▼
WMS API
    │ Publish Event
    ▼
RabbitMQ Exchange
    │
    ▼
SAP Consumer
    │ HTTP
    ▼
SAP API
```

---

## Features

- ✅ React Dashboard
- ✅ RabbitMQ Message Queue
- ✅ Event Driven Architecture
- ✅ Retry Mechanism
- ✅ Dead Letter Queue (DLQ)
- ✅ Manual Requeue
- ✅ Dashboard Statistics
- ✅ Event Trend Chart
- ✅ Health Check
- ✅ Kubernetes Deployment
- ✅ Rolling Update
- ✅ Automated Deploy Script

---

## Project Structure

```
Backend
│
├── K8sDemo.WmsApi
├── K8sDemo.SapApi
├── K8sDemo.SapConsumer
└── K8sDemo.Shared

Frontend
└── k8sdemo-react-ui

Deploy
├── deploy-all.ps1
├── rabbitmq-config.yaml
├── wms-api.yaml
├── sap-api.yaml
├── sap-consumer.yaml
└── react-ui.yaml
```

---

## Event Flow

```
Material Picked

        │
        ▼

   RabbitMQ

        │
        ▼

 SAP Consumer

        │
        ├───────────────┐
        │               │
        ▼               ▼

    Success          Retry

                        │

              Retry < 3 ?

                │      │

              Yes      No

                │      │

                ▼      ▼

             Publish   DLQ

                       │

                 Manual Requeue
```

---

## Quick Start

Start Minikube

```powershell
minikube start
```

Deploy all services

```powershell
.\Deploy\deploy-all.ps1 `
    -Wms `
    -SapApi `
    -Consumer `
    -React
```

The script will:

- Build Docker images
- Load images into Minikube
- Apply Kubernetes YAML
- Update Deployment image
- Wait for rollout
- Open Dashboard

---

## Deployment Components

| Component | Description |
|------------|-------------|
| React UI | Dashboard |
| WMS API | Publish Material Event |
| RabbitMQ | Message Broker |
| SAP Consumer | Consume & Retry |
| SAP API | Simulated SAP Service |

---

## Learning Objectives

This project is designed for practicing:

- Kubernetes Deployment
- Service
- Ingress
- ConfigMap
- Secret
- RabbitMQ
- Retry Pattern
- Dead Letter Queue
- Rolling Update
- Health Check
- Docker Image Management
- Troubleshooting

More learning notes can be found in **K8S_LEARNING.md**.

---

## RabbitMQ Configuration

RabbitMQ configuration is managed by Kubernetes.

ConfigMap

```
Deploy/rabbitmq-config.yaml
```

Secret

```
Deploy/rabbitmq-config.yaml
```

Apply configuration

```powershell
kubectl apply -f .\Deploy\rabbitmq-config.yaml
```

---

## Roadmap

### v1

- ✅ RabbitMQ
- ✅ Retry
- ✅ DLQ
- ✅ Requeue
- ✅ Dashboard
- ✅ Statistics
- ✅ Kubernetes Deployment

### v2

- ⏳ Prometheus
- ⏳ Grafana
- ⏳ OpenTelemetry

### v3

- ⏳ GitHub Actions
- ⏳ Argo CD
- ⏳ CI/CD Pipeline