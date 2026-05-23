# Recursos criados:
#   - ConfigMap  soat-api-config  (mesmos valores dos seus YAMLs)
#   - Secret     soat-api-secret  (mesmos valores dos seus YAMLs)
#   - Deployment soat-api         (espelho do seu deployment.yaml)
#   - Service    soat-api-service (espelho do seu service.yaml)
#   - HPA        soat-api-hpa     (espelho do seu hpa.yaml)

resource "kubernetes_config_map" "app" {
  metadata {
    name      = "soat-api-config"
    namespace = var.namespace
  }
  data = {
    ASPNETCORE_ENVIRONMENT       = "Production"
    ConnectionStrings__Default   = "Host=postgres-service;Port=5432;Database=soattechchallenge;Username=${var.db_user};Password=${var.db_password}"
    JwtSettings__Secret          = var.jwt_secret
    JwtSettings__ExpirationHours = "2"
  }
}

resource "kubernetes_secret" "app" {
  metadata {
    name      = "soat-api-secret"
    namespace = var.namespace
  }
  data = {
    POSTGRES_USER     = var.db_user
    POSTGRES_PASSWORD = var.db_password
    JWT_SECRET        = var.jwt_secret
  }
}

resource "kubernetes_deployment" "app" {
  metadata {
    name      = "soat-api"
    namespace = var.namespace
    labels    = { app = "soat-api" }
  }
  spec {
    replicas = 2
    selector {
      match_labels = { app = "soat-api" }
    }
    strategy {
      type = "RollingUpdate"
      rolling_update {
        max_surge       = "1"
        max_unavailable = "0"
      }
    }
    template {
      metadata {
        labels = { app = "soat-api" }
      }
      spec {
        container {
          name              = "soat-api"
          image             = "soat-api:latest"
          image_pull_policy = "Never"
          port {
            container_port = 8080
          }
          env_from {
            config_map_ref {
              name = kubernetes_config_map.app.metadata[0].name
            }
          }
          env_from {
            secret_ref {
              name = kubernetes_secret.app.metadata[0].name
            }
          }
          resources {
            requests = { memory = "256Mi", cpu = "250m" }
            limits   = { memory = "512Mi", cpu = "500m" }
          }
        }
      }
    }
  }
}

resource "kubernetes_service" "app" {
  metadata {
    name      = "soat-api-service"
    namespace = var.namespace
  }
  spec {
    selector = { app = "soat-api" }
    type     = "NodePort"
    port {
      protocol    = "TCP"
      port        = 80
      target_port = 8080
      node_port   = 30080
    }
  }
}

resource "kubernetes_horizontal_pod_autoscaler_v2" "app" {
  metadata {
    name      = "soat-api-hpa"
    namespace = var.namespace
  }
  spec {
    min_replicas = 2
    max_replicas = 10
    scale_target_ref {
      api_version = "apps/v1"
      kind        = "Deployment"
      name        = kubernetes_deployment.app.metadata[0].name
    }
    metric {
      type = "Resource"
      resource {
        name = "cpu"
        target {
          type                = "Utilization"
          average_utilization = 70
        }
      }
    }
  }
}