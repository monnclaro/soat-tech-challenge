resource "kubernetes_service" "postgres" {
  metadata {
    name = "postgres-service"
  }

  spec {
    selector = {
      app = "postgres"
    }

    port {
      port        = 5432
      target_port = 5432
    }
  }
}