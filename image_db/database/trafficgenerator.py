from image_db.database.db import Db
from os import listdir
from os.path import isfile, join
import time

database = Db()

# Your file path here
local_data = "C:/Users/preis/Pictures/CLC_images"

onlyfiles = [f for f in listdir(local_data) if isfile(join(local_data, f))]

start = time.time()
for file in onlyfiles:
    blob_client = Db.getImageContainer().get_blob_client(blob=file)
    if not blob_client.exists():
        with open(local_data + "/" + file, "rb") as data:
            blob_client.upload_blob(data)


end = time.time()
print("Time elapsed: " + str(end - start))