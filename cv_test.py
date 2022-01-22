from azure.cognitiveservices.vision.computervision import ComputerVisionClient
from msrest.authentication import CognitiveServicesCredentials
from image_db.database.db import Db

import configparser

def set_image_tags(image):
    config = configparser.ConfigParser()
    config.read("imagedb.ini")

    subscription_key = config["ComputerVision"]["SubscriptionKey"]
    endpoint = config["ComputerVision"]["Endpoint"]

    computervision_client = ComputerVisionClient(endpoint, CognitiveServicesCredentials(subscription_key))
    
    tag_results = computervision_client.tag_image(image.url)

    tags = ", ".join([t.name for t in tag_results.tags])

    blob_client = Db.getImageContainer().get_blob_client(blob=image.name)
    blob_client.set_blob_metadata({"tags": tags})





if __name__ == "__main__":
    set_image_tags(image)