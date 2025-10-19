terraform {
  backend "gcs" {
    bucket = var.state_bucket

    prefix = "amaurot"
  }
}

provider "google" {
  project = var.project
  region  = var.region
}

module "amaurot" {
  source = "./module"

  allowed_repositories   = var.allowed_repositories
  domain                 = var.domain
  region                 = var.region
  secret_replica_regions = var.secret_replica_regions
  skip_graceful_shutdown = var.skip_graceful_shutdown
  use_ghcr               = var.use_ghcr
  zones                  = var.zones
}
