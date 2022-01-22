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

---

## Functions

TBD

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