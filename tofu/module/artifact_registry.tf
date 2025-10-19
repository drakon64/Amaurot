resource "google_project_service" "artifact_registry" {
  service = "artifactregistry.googleapis.com"
}

resource "google_artifact_registry_repository" "artifact_registry" {
  format        = "DOCKER"
  repository_id = var.use_ghcr ? "ghcr" : "amaurot"

  cleanup_policies {
    id = "Untagged"

    action = "DELETE"

    condition {
      tag_state = "UNTAGGED"
    }
  }

  mode = var.use_ghcr ? "REMOTE_REPOSITORY" : "STANDARD_REPOSITORY"

  dynamic "remote_repository_config" {
    for_each = var.use_ghcr ? [true] : []

    content {
      docker_repository {
        custom_repository {
          uri = "https://ghcr.io"
        }
      }
    }
  }

  vulnerability_scanning_config {
    enablement_config = "DISABLED"
  }

  depends_on = [google_project_service.artifact_registry]
}
