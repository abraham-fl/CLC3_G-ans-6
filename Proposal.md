# Project Proposal

## Team members
* Florian Abraham
* Tobias Alexander Baumgartner
* Philipp Preiser

## General Idea
Users can upload one or more images. For each image tags are added and both
image and the list of tags are saved in a database. A webapp will let the user search
the image database by filtering for certain tags.

## Technical implementation
A flask webapp will allow users to upload one or more images into a Azure BlobStorage.
The webapp will be hosted using Azure Webapps. Using a Storage Queue the images will 
be sent to the blob storage and annotated using Azure Cognitive Services, to be precise we will be using the annotation functionality of the Computer Vision ressource. The resulting tags will be stored in a Azure SQL Database. As soon as the queue is empty a Logic App willtrigger an email notification to inform you about your finished jobs.

Connection strings, API endpoints, etc. will be kept in a Azure KeyVault.

## Cloud technologies used
* Azure Storage Account (blob storage + storage queues + sql database)
* Azure App Services (web app)
* Azure Computer Vision (annotation feature)
* Azure Logic Apps (mail notifications)
* Github

## Milestones
* Setup and test Blob Storage for image upload
* Setup and test Cognitive Service for tagging images
* Setup and test Azure SQL DB for generated tags
* Combine previous steps with a Logic App for email notification
* Build flask web app for image visualization and integrated tag search

## Responsibilities
TBD - after discussing shared Azure service usage options


