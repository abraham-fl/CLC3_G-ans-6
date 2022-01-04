import configparser
from azure.storage.blob import BlobServiceClient

config = configparser.ConfigParser()
config.read("imagedb.ini")


class Db():
    def __init__(self):
        Db.__BlobConnectionString = config["BlobStorage"]["ConnectionString"]
        Db.__BlobThumbnailContainer = config["BlobStorage"]["ThumbnailContainer"]
        Db.__BlobImageContainer = config["BlobStorage"]["ImageContainer"]

        Db.__BlobConnection = None
        Db.__ThumbnailConnection = None
        Db.__ImageConnection = None

        Db.__createContainerConnections()

    def __createBlobConnection():
        if not Db.__BlobConnection:
            Db.__BlobConnection = BlobServiceClient.from_connection_string(conn_str = Db.__BlobConnectionString)

    def __createContainerConnections():
        Db.__createBlobConnection()

        try:
            Db.__ThumbnailConnection = Db.__BlobConnection.get_container_client(container = Db.__BlobThumbnailContainer)
            Db.__ThumbnailConnection.get_container_properties()
        except:
            Db.__ThumbnailConnection = Db.__BlobConnection.create_container(Db.__BlobThumbnailContainer)

        try:
            Db.__ImageConnection = Db.__BlobConnection.get_container_client(container = Db.__BlobImageContainer)
            Db.__ImageConnection.get_container_properties()
        except:
            Db.__ImageConnection = Db.__BlobConnection.create_container(Db.__BlobImageContainer)

    def getThumbnailContainer():
        return Db.__ThumbnailConnection

    def getImageContainer():
        return Db.__ImageConnection