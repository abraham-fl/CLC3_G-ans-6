# Create your WebApp
Either create your own WebApp to display the images stored in the Azure BlobStorage or simply copy the code from this [repository](../web_app).
## Create an .ini file
If you copied the provided WebApp you will have to create a .ini filed called `imagedb.ini` . Enter the following information:
```
[BlobStorage]
ConnectionString = <Enter your connection string here>
ThumbnailContainer = thumbnails
```
We will come back to this file later to add a connection string.