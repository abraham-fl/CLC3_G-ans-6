# Setup Azure Resources
We assume that you have a basic understanding of how to navigate the Azure Portal.

---

## Resource Group 
Create a new Resource Group with the following settings:
### Basics
1. Choose your subscription
2. Name your Resource Group
3. Host it in the North Europe region

Hosting it in North Europe is important. Make sure to select the correct region.
We were not able to create our project while hosting it in other regions (e.g. West Europe).

Click on the `Review + Create` Button to start the validation process and click `Create` as soon as the validation was passed.

Click on the Resource Group you just created.
From now on, unless specified otherwise, every major headline requires you to return to this page in order to create a new resource.

---

## Storage Account
Click `+ Create` on the upper left corner and search for the `Storage account` resource. Select the search result and click `Create`.
### Basics
1. Choose your subscription
2. Select your created resource group
3. Select a name for your storage account
4. Set the `Region` to `North Europe`
5. Keep the `Performance` at `Standard`
6. You can lower the `Redundancy` to `Locally-redundant storage (LRS)`

Leave other settings at default and click on `Review + create`. Once validation is passed click `Create`.

As soon as deployment is complete click `Go to resource`.

### Create blob containers
Click on `Blob service` in the overview.<br>
Click on `+ Container` and create three containers in total. Make sure to name them `images`, `processed` and `thumbnails`.

### Retrieve the connection string
As mentioned in the [WebApp Documentation](webapp.md), we require a connection string to connect with our storage account.
* Navigate to the overview page of the storage account. 
* Select `Access keys` in the navigation pane on the left hand side.
* Click on `Show keys`
* Copy the `Connection string` found und `key1` and replace the `<Enter your connection string here>` part in the `imagedb.ini` file.

---

## WebApp
Click `+ Create` on the upper left corner and search for the `WebApp` resource. Select the search result and click `Create`.

### Basics
1. Select your subscription
2. Select your resource group
3. Name your WebApp
4. Set `Publish` to `Code`
5. Set the `Runtime stack` to `Python 3.9`
6. Leave the `Operating System` as `Linux`
7. Set the `Region` to `North Europe`
8. Set the `Sku and size` for your `App Service Plan` to `Basic B1`

Leave other settings at default and click on `Review + create`. Once validation is passed click `Create`.

As soon as deployment is complete click `Go to resource`.

### Configure the WebApp
To make sure that every deployment triggers a build (and thus installs packages from the `requirements.txt`) file you need to add a configuration setting to your WebApp:
1. Click in `Configuration` in the `Settings` section on the navigation pane to the left.
2. Click on `+ New application setting`.
3. Enter `SCM_DO_BUILD_DURING_DEPLOYMENT` as `Name` and `true` as `Value`
4. Click `OK`
5. Click `Save`

---

## Functions

Functions are created with the help of Microsoft Visual Studio Community 2022 (v17.0.5).

A new Azure Function App has to be created in the Visual Studio (VS) by creating a new project and selecting the 
`Azure Functions` template for `C#`. If this template is not available, check your VS installer for installing the
necessary Azure extension in VS.  

Publish your Function App by right clicking and selecting `Publish`. Use the `Azure` option an press `Next`. Select
your `Azure Function App (Windows/Linux)` depending on what IOS System you want to use in your project. Choose a valid
subscription and either use a already created Function App from your azure portal where you want to deploy or create a
new one by using the green plus in the right-hand top corner of the Function Apps window. 
After selection, press `Finish` to complete your first publishing. The publishing profile will stay the same for your
Function App until you change or select a new one.  

Setting up your environment variables can be done local for testing in your `local.settings.json` file, but for deploying
on Azure, you also have to set them online.  
To set your variables online, you have to go to `portal.azure.com` and select your Function App in your Resource group. 
In `Settings` you can choose `Configuration`. Here you can set new variables by pressing `New application setting` 
and filling out your `Name` and `Value` before pressing `Ok`. Don´t forget to press `Save` before continuing and make sure
that the names match with your local variables.  
In your function you can access the environment variables with the function call 
`Environment.GetEnvironmentVariable("<VariableName>")`. 

A new trigger can be created by right-clicking on the Function App and selecting `Add` and `New Azure Function...`.
Use the `Azure Function` template for `C#` in the menu, give it an appropriate name and press `Add`. The next step is
trigger type dependent. 

### [UploadBlobTrigger](../azure_functions/UploadBlobTrigger.cs)  
Creating a blob trigger:
1. You have to specify the path to the blob trigger in your storage account. (in this case `images`)
2. The `Connection string setting name` can be set on `AzureWebJobsStorage`. 
3. Enable the tick at`Configure Blob trigger connection` and press `Add`. 
4. Choose `Azure Storage` and press `Next`.
5. Select your `Subsription name` in the drop-down menu and select the `Storage accounts` where your blob storage is inside,
and press `Next`.
6. Specify your `Connection string name` to `AzureWebJobsStorage` and press `Finish`.

A blob trigger expects a blob, with whom it gets triggered, a name and a logger. Additionally, a queue item was added
so we can send a message into a queue when finished. The queue has to be specified, in this case it is `queue-tagging-done`. There are 2 mods available in the Trigger, by either calling the
cognitive service function to tag the image, or use a dummy image and generate random five tags for the image. The 
decision which mode is used is made by the image name. With a prefix of `dummy_<imageName>` the function calls the random
generator for creating tags, otherwise the cognitive service will create the tags by specifying the `VisualFeatureTypes`
for tag creation and not other other available features. The tags are being saved in the Metadata of the blob. To trigger the
next step in our workflow, a message with the filename is put in the specified queue.

### [ContrastQueueTrigger](../azure_functions/ContrastQueueTrigger.cs)  
Creating a queue trigger:
1. You have to specify the queue name in your storage account where the trigger should listen to. (in this case `queue-tagging-done`)
2. The `Connection string setting name` can be set on `AzureWebJobsStorage`. 
3. If you haven't set up a connection to your storage account yet, follow steps 4 to 7. Otherwise, you are done by pressing `Add`.
4. Enable the tick at`Configure Queue trigger connection` and press `Add`
5. Choose `Azure Storage` and press `Next`.
6. Select your `Subsription name` in the drop-down menu and select the `Storage accounts` where your blob storage is inside,
and press `Next`.
7. Specify your `Connection string name` to `AzureWebJobsStorage` and press `Finish`.

A queue trigger receives a queue item, in this case this is the file name which is done tagging. Additionally, a queue item was added
so we can send a message into a queue when finished. The queue has to be specified, in this case it is `queue-histogram-flattening-done`.
To access the image blob from the storage a `BlobServiceClient` will access the specific blob in the `ContainerNameImage` container (from environment variables) and contrast enhancement is done for
preprocessing purposes. Other computational operations could be applied here. After processing, the Bitmap is saved in another container
called `ContainerNameProcessed` (from environment variables) and a message is put into the next queue to trigger the next function.

### [ResizeQueueTrigger](../azure_functions/ResizeQueueTrigger.cs)  
This trigger is created the same way as the ContrastQueueTrigger with the slight difference being that this trigger is listening to
the `queue-histogram-flattening-done` queue. We retrieve the image from the container `ContainerNameProcessed` (from environment variables) with the
`BlobServiceClient`. The loaded Bitmap is being resized to a custom format `480x480`, by keeping the image ratio from width to height and filling up
the boarder with white pixels. The processed image is being saved in the `ContainerNameThumbnails` container (from environment variables) and ready
to be used in our WebApp with a preprocessed and resized image, and generated tags.

---

## Computer Vision
Click `+ Create` on the upper left corner and search for the `Computer Vision` resource. Select the search result and click `Create`.

### Basics
1. Select your subscription
2. Select your resource group
3. Set the `Region` to `North Europe`
4. Name your Computer Vision resource
5. Select `Free F0` as your `Pricing tier`
6. Read and accept the terms at the bottom of the page


### Identity
1. Set the `Status` as `On`

Leave other settings at default and click on `Review + create`. Once validation is passed click `Create`.


### Retrieve Endpoint and Key
The endpoint and key can be found in the Computer Vision Service in `Resource Management` and 
`Keys and Endpoint`. The keys refer to as subscription keys and in combination with the endpoint, both is needed in the 
[Blob Trigger Function](../azure_functions/UploadBlobTrigger.cs)   to use the cognitive service for tagging.