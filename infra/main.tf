resource "kubernetes_namespace" "soat" {
  metadata {
    name = "soat-tech"
  }
}