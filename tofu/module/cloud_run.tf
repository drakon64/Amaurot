locals {
  image = var.use_ghcr ? "${var.region}-docker.pkg.dev/${data.google_project.project.project_id}/${google_artifact_registry_repository.artifact_registry.name}/drakon64/amaurot@${data.docker_registry_image.amaurot.sha256_digest}" : data.google_artifact_registry_docker_image.amaurot.self_link
}

resource "google_project_service" "cloud_run" {
  service = "run.googleapis.com"
}

data "docker_registry_image" "amaurot" {
  name = "ghcr.io/drakon64/amaurot:latest"
}

data "google_artifact_registry_docker_image" "amaurot" {
  image_name    = "amaurot:latest"
  location      = var.region
  repository_id = google_artifact_registry_repository.artifact_registry.repository_id
}
