variable "allowed_repositories" {
  default = null
  type    = set(string)
}

variable "domain" {
  type = string
}

variable "region" {
  type = string
}

variable "secret_replica_regions" {
  default = null
  type    = set(string)
}

variable "skip_graceful_shutdown" {
  default = false
}

variable "use_ghcr" {
  description = "Use pre-built Amaurot images from GitHub Packages"
  default     = true
}

variable "zones" {
  type = set(string)
}
