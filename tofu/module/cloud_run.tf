locals {
  processor_image = var.use_ghcr ? "${var.region}-docker.pkg.dev/${data.google_project.project.project_id}/${google_artifact_registry_repository.artifact_registry.name}/drakon64/amaurot-processor@${data.docker_registry_image.amaurot["processor"].sha256_digest}" : data.google_artifact_registry_docker_image.amaurot["processor"].self_link
  receiver_image  = var.use_ghcr ? "${var.region}-docker.pkg.dev/${data.google_project.project.project_id}/${google_artifact_registry_repository.artifact_registry.name}/drakon64/amaurot-receiver@${data.docker_registry_image.amaurot["receiver"].sha256_digest}" : data.google_artifact_registry_docker_image.amaurot["receiver"].self_link
}

resource "google_project_service" "cloud_run" {
  service = "run.googleapis.com"
}

data "docker_registry_image" "amaurot" {
  for_each = toset(var.use_ghcr ? [
    "processor",
    "receiver",
  ] : [])

  name = "ghcr.io/drakon64/amaurot-${each.value}:latest"
}

data "google_artifact_registry_docker_image" "amaurot" {
  for_each = toset(var.use_ghcr ? [] : [
    "processor",
    "receiver",
  ])

  image_name    = "amaurot-${each.value}:latest"
  location      = var.region
  repository_id = google_artifact_registry_repository.artifact_registry.repository_id
}
