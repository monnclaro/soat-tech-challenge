output "service_name" {
  value = kubernetes_service.app.metadata[0].name
}