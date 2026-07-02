# Recursos criados:
#   - Namespace "soat"
#   - PostgreSQL: Secret + PVC + StatefulSet + Service
#   - App: ConfigMap + Secret + Deployment + Service + HPA

terraform {
  required_version = ">= 1.6.0"
  required_providers {
    kubernetes = {
      source  = "hashicorp/kubernetes"
      version = "~> 2.30"
    }
  }
}

provider "kubernetes" {
  config_path    = "~/.kube/config"
  config_context = "minikube"
}

resource "kubernetes_namespace" "soat" {
  metadata {
    name = "soat"
  }
}

module "postgres" {
  source      = "./modules/postgres"
  namespace   = kubernetes_namespace.soat.metadata[0].name
  db_name     = var.db_name
  db_user     = var.db_user
  db_password = var.db_password
}

module "app" {
  source          = "./modules/app"
  namespace       = kubernetes_namespace.soat.metadata[0].name
  db_user         = var.db_user
  db_password     = var.db_password
  jwt_secret      = var.jwt_secret
  restart_trigger = var.restart_trigger
  depends_on      = [module.postgres]
}