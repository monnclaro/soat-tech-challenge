variable "db_name" {
  type    = string
  default = "soattechchallenge"
}

variable "db_user" {
  type    = string
  default = "postgres"
}

variable "db_password" {
  type      = string
  sensitive = true
  default   = "postgres"
}

variable "jwt_secret" {
  type      = string
  sensitive = true
  default   = "SuaChaveSuperSecretaComMinimo32Caracteres"
}

variable "restart_trigger" {
  type    = string
  default = "manual"
}

variable "kubeconfig_path" {
  type    = string
  default = "~/.kube/config"
}