module "lb-http" {
  source  = "terraform-google-modules/lb-http/google//modules/serverless_negs"
  version = "~> 14"

  name    = "amaurot"
  project = data.google_project.project.project_id

  backends = {
    default = {
      enable_cdn = false
      groups     = []

      log_config = {
        enable = false
      }

      security_policy = module.cloud_armor.policy.id

      serverless_neg_backends = [{
        region = var.region

        service = {
          name = google_cloud_run_v2_service.receiver.name
        }

        type = "cloud-run"
      }]
    }
  }

  create_ipv6_address             = true
  enable_ipv6                     = true
  http_forward                    = false
  load_balancing_scheme           = "EXTERNAL_MANAGED"
  managed_ssl_certificate_domains = [var.domain]
  ssl                             = true

  depends_on = [google_project_service.compute]
}
