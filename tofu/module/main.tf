terraform {
  required_providers {
    docker = {
      source  = "kreuzwerker/docker"
      version = "~> 3"
    }

    google = {
      source = "hashicorp/google"
    }

    http = {
      source  = "hashicorp/http"
      version = "~> 3"
    }
  }
}

provider "docker" {
  disable_docker_daemon_check = true
}

locals {
  services = toset([
    "processor",
    "receiver",
  ])
}

data "google_project" "project" {}

resource "google_project_service" "compute" {
  service = "compute.googleapis.com"
}
