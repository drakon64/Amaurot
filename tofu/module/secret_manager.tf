locals {
  receiver_secrets = toset([
    "github-webhook-secret",
  ])

  processor_secrets = toset([
    "github-client-id",
    "github-private-key",
  ])
}

resource "google_project_service" "secret_manager" {
  service = "secretmanager.googleapis.com"
}

resource "google_secret_manager_secret" "secret" {
  for_each = toset(concat(
    tolist(local.receiver_secrets), tolist(local.processor_secrets)
  ))

  secret_id = each.key

  replication {
    user_managed {
      dynamic "replicas" {
        for_each = coalesce(var.secret_replica_regions, toset([var.region]))

        content {
          location = replicas.key
        }
      }
    }
  }

  depends_on = [google_project_service.secret_manager]
}

resource "google_secret_manager_secret_iam_member" "receiver_secret" {
  for_each = local.receiver_secrets

  member    = google_service_account.amaurot["receiver"].member
  role      = "roles/secretmanager.secretAccessor"
  secret_id = google_secret_manager_secret.secret[each.key].secret_id
}

resource "google_secret_manager_secret_iam_member" "processor_secret" {
  for_each = local.processor_secrets

  member    = google_service_account.amaurot["processor"].member
  role      = "roles/secretmanager.secretAccessor"
  secret_id = google_secret_manager_secret.secret[each.key].secret_id
}
