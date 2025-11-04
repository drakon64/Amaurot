resource "google_cloud_run_v2_job" "processor" {
  location = var.region
  name     = "amaurot-processor"

  template {
    template {
      containers {
        image = local.processor_image

        resources {
          limits = {
            cpu    = "1000m"
            memory = "614Mi"
          }
        }
      }

      service_account = google_service_account.amaurot["processor"].email
    }
  }
}

resource "google_cloud_run_v2_job_iam_member" "processor" {
  member = google_service_account.amaurot["receiver"].member
  name   = google_cloud_run_v2_job.processor.name
  role   = "roles/run.jobsExecutorWithOverrides"
}
