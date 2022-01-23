from web_app.image_db.database.db import Db
from os import listdir, rename
from os.path import isfile, join
import time

database = Db()

# Your file path here
local_data = r"C:/Users/flori/OneDrive/Dokumente/Programmieren/Python/Bilder aus Forum holen/imgs/testing_flo"

onlyfiles = [f for f in listdir(local_data) if isfile(join(local_data, f))]
[rename(join(local_data, f), join(local_data, "dummy_" + f)) for f in onlyfiles if not "dummy_" in f]
onlyfiles = [f for f in listdir(local_data) if isfile(join(local_data, f))]


start = time.time()
for file in onlyfiles:
    blob_client = Db.getImageContainer().get_blob_client(blob=file)
    print(f"Uploaded {file}.\n")
    with open(local_data + "/" + file, "rb") as data:
        blob_client.upload_blob(data, overwrite=True)

end = time.time()
print("Time elapsed: " + str(end - start))