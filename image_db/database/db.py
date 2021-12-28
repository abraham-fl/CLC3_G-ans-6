import configparser
import pyodbc
from azure.storage.blob import BlobServiceClient

config = configparser.ConfigParser()
config.read("imagedb.ini")


class Db():
    def __init__(self):
        Db.__SQLDriver = config["SQLStorage"]["Driver"]
        Db.__SQLServer = config["SQLStorage"]["Server"]
        Db.__SQLDatabase = config["SQLStorage"]["Database"]
        Db.__SQLUsername = config["SQLStorage"]["Username"]
        Db.__SQLPassword = config["SQLStorage"]["Password"]

        Db.__SQLConnectionString = f"Driver={Db.__SQLDriver};Server={Db.__SQLServer};Database={Db.__SQLDatabase};Uid={Db.__SQLUsername};Pwd={Db.__SQLPassword};Encrypt=yes;TrustServerCertificate=no;Connection Timeout=30;"
        Db.__BlobConnectionString = config["BlobStorage"]["ConnectionString"]
        Db.__BlobThumbnailContainer = config["BlobStorage"]["ThumbnailContainer"]
        Db.__BlobImageContainer = config["BlobStorage"]["ImageContainer"]

        Db.__SQLConnection = None
        Db.__BlobConnection = None
        Db.__ThumbnailConnection = None
        Db.__ImageConnection = None

        Db.__createSQLConnection()
        Db.__createContainerConnections()

    def __createSQLConnection():
        if not Db.__SQLConnection:
            Db.__SQLConnection = pyodbc.connect(Db.__SQLConnectionString)

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

    def get_SQL_cursor():
        return Db.__SQLConnection.cursor()

    def close_SQL_connection():
        if Db.__SQLConnection:
            Db.__SQLConnection.close()

    def getThumbnailContainer():
        return Db.__ThumbnailConnection

    def getImageContainer():
        return Db.__ImageConnection