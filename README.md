# Amaurot

Amaurot is an experimental GitHub App for running OpenTofu workflows within Google Cloud Run.

## Workflow

Amaurot is made up of two Google Cloud Run services, a *receiver* and a *processor*.

When a pull request is raised or updated in a repository where Amaurot is installed, GitHub sends a webhook containing information about the pull request to the receiver. The receiver then creates a Task in Google Cloud Tasks and responds to the webhook.

Google Cloud Tasks will send a request to the processor. The processor will use the information from the webhook to download the repository contents with the pull request applied, and will run OpenTofu plans for each workspace defined in the repositories `amaurot.json` file that had changes to its configuration files. The results of these plans are posted as a comment to the pull request and saved in Google Cloud Firestore.

When the pull request is merged, the process is repeated except that the processor will instead run an OpenTofu apply for each workspace defined in the repositories `amaurot.json` file that had changes to its configuration files, and the prior plan output will be pulled from Firestore. The results of these applies are posted as a comment to the pull request, and the plan output is deleted from Firestore.

## License

EUPL v. 1.2 only.
