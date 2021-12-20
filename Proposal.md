# Project Proposal

## Team members
* Florian Abraham
* Tobias Alexander Baumgartner
* Philipp Preiser

## General Idea
Users can upload one or more images. For each image tags are added and both
image and the list of tags are saved in a database. A webapp will let the user search the image database by filtering for certain tags.

## Technical implementation
A generator function will upload X images/second to a blob storage. The upload will trigger a image preprocessing pipeline which, among other functions, will reduce image resolution. The created thumbnail is then stored in the blob storage again to be shown on a webapp. The webapp will be hosted using Azure Webapps. Using a Storage Queue the images will be sent to Azure Cognitive Services to be annotated using the Computer Vision ressource. The resulting tags will be stored in a Azure SQL Database. At the end of the day a Logic App will trigger an email notification to give you today's recap. Communication between the services will mostly be handled by queue systems.

Connection strings, API endpoints, etc. will be kept in a Azure KeyVault.

## Cloud technologies used
* Azure Storage Account (blob storage + storage queues + sql database)
* Azure Functions (pipeline)
* Application Insights to monitor Functions scaling
* Azure App Services (web app)
* Azure Computer Vision (annotation feature)
* Azure Logic Apps (mail notifications)
* Github

## Milestones
1. Setup and test generator function + Blob Storage for image upload
1. Setup and test Azure SQL DB for generated tags
1. Setup and test image preprocessing pipeline
1.  Setup and test Cognitive Service for tagging images
    * also implement "dummy" Cognitive service to circumvent expensive Cloud Vision API calls 
1. Combine previous steps with a Logic App for email notification
1. Build flask web app for image visualization and integrated tag search

## Responsibilities
* Milestones 1 + 2: Philipp Preiser
<br>
* Milestones 3 + 4: Tobias Baumgartner
<br>
* Milestones 5 + 6: Florian Abraham

