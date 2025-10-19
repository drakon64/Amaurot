resource "google_service_account" "amaurot" {
  for_each = local.services

  account_id = "amaurot-${each.key}"
}

resource "google_service_account_iam_member" "iam" {
  member             = google_service_account.amaurot["receiver"].member
  role               = "roles/iam.serviceAccountUser"
  service_account_id = google_service_account.amaurot["receiver"].id
}
