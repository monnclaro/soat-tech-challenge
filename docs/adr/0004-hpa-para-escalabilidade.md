# ADR 0004 — HPA (Horizontal Pod Autoscaler) para Escalabilidade da API

**Status:** Aceito (herdado da Fase 2, mantido na Fase 3)

## Contexto

A oficina tem picos de demanda (horários de abertura, campanhas), e o requisito pede "Cluster Kubernetes com escalabilidade".

## Decisão

`HorizontalPodAutoscaler` (`k8s/hpa.yaml`) escalando o `Deployment soat-api` entre 2 e 10 réplicas, por utilização de CPU (70%) e memória (80%). O cluster (node group do EKS, `infra-k8s/eks.tf`) tem `min_size=2`/`max_size=6`, permitindo que o cluster também cresça em número de nodes conforme os pods aumentam.

## Alternativas descartadas

- **Vertical Pod Autoscaler (VPA)**: ajusta requests/limits de um pod já em execução, mas não aumenta o número de réplicas — não resolve pico de requisições concorrentes, só uso ineficiente de recursos por pod.
- **Scaling manual (réplicas fixas dimensionadas para o pico)**: desperdiça recursos (e custo) fora do horário de pico; contraria o requisito explícito de "escalabilidade dinâmica".
- **KEDA (event-driven autoscaling)**: adequado para filas/eventos; a API é request-response HTTP puro, então CPU/memória já são um proxy direto e suficiente de carga.

## Consequências

- Métricas de CPU/memória dependem do `metrics-server` estar instalado no cluster (via addon do EKS, `infra-k8s/eks.tf` → `cluster_addons`).
- O tempo de scale-up (alguns minutos até novos pods/nodes ficarem prontos) não é instantâneo — picos muito abruptos podem gerar latência elevada momentânea, mitigado em parte por `minReplicas: 2` já operando acima do mínimo absoluto.
