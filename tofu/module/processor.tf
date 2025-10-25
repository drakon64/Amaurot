resource "google_cloud_run_v2_service" "processor" {
  location = var.region
  name     = "amaurot-processor"

  ingress = "INGRESS_TRAFFIC_INTERNAL_ONLY"

  template {
    containers {
      image = local.image

      args = ["processor"]

      dynamic "env" {
        for_each = var.allowed_repositories != null ? var.allowed_repositories : []

        content {
          name = "AMAUROT_GITHUB_REPOSITORIES"

          value = join(" ", var.allowed_repositories)
        }
      }

      resources {
        cpu_idle = true

        limits = {
          cpu    = "1000m"
          memory = "614Mi"
        }
      }

      startup_probe {
        failure_threshold = 10
        period_seconds    = 1

        tcp_socket {
          port = 8080
        }
      }
    }

    max_instance_request_concurrency = 100

    scaling {
      max_instance_count = 1
      min_instance_count = 0
    }

    service_account = google_service_account.amaurot["processor"].email

    timeout = "10s"
  }

  depends_on = [
    google_project_service.cloud_run,
    google_secret_manager_secret_iam_member.processor_secret,
  ]
}

resource "google_cloud_run_v2_service_iam_member" "processor" {
  member = google_service_account.amaurot["receiver"].member
  name   = google_cloud_run_v2_service.processor.name
  role   = "roles/run.invoker"
}
