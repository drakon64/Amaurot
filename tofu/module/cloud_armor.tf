data "http" "github_meta" {
  url = "https://api.github.com/meta"

  request_headers = {
    Accept               = "application/vnd.github+json"
    X-GitHub-Api-Version = "2022-11-28"
  }
}

module "cloud_armor" {
  source  = "GoogleCloudPlatform/cloud-armor/google"
  version = "~> 6"

  name       = "amaurot"
  project_id = data.google_project.project.project_id

  default_rule_action = "deny(403)"

  security_rules = {
    "github_webhooks" = {
      action        = "allow"
      priority      = 0
      src_ip_ranges = jsondecode(data.http.github_meta.response_body)["hooks"]
    }
  }

  depends_on = [google_project_service.compute]
}
