resource "google_cloud_run_v2_service" "receiver" {
  location = var.region
  name     = "amaurot-receiver"

  ingress = "INGRESS_TRAFFIC_INTERNAL_LOAD_BALANCER"

  template {
    containers {
      image = local.image

      args = ["receiver"]

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

    service_account = google_service_account.amaurot["receiver"].email

    timeout = "10s"
  }

  depends_on = [
    google_project_service.cloud_run,
    google_secret_manager_secret_iam_member.receiver_secret,
  ]
}

resource "google_cloud_run_v2_service_iam_member" "receiver" {
  member = "allUsers"
  name   = google_cloud_run_v2_service.receiver.name
  role   = "roles/run.invoker"
}
