from collections import Counter
from flask import Blueprint
from flask import render_template, request
from flask_sqlalchemy import Pagination
from image_db.database.db import Db

main = Blueprint("main", __name__)

database = Db()

@main.route("/", methods = ['GET'])
@main.route("/home", methods = ['GET'])
def home():
    blob_images = Db.getImageContainer().list_blobs()
    count = Counter()
    for image in blob_images:
        blob_client = Db.getImageContainer().get_blob_client(blob=image.name)
        count.update(blob_client.get_blob_properties().metadata["tags"].split(", "))

    count = [i[0] for i in count.most_common()]
    return render_template('home.html', pagination = None, count = count)

@main.route("/results", methods = ['GET', 'POST'])
def results():
    page = request.args.get("page", 1, type=int)
    tags = [request.args.get("tag1"), request.args.get("tag2"), request.args.get("tag3")]
    reduced_tags = set([t for t in tags if t])
    per_page = 6
    blob_images = Db.getImageContainer().list_blobs()
    images = []
    count = Counter()
    for image in blob_images:
        blob_client = Db.getImageContainer().get_blob_client(blob=image.name)
        image_tags = set(blob_client.get_blob_properties().metadata["tags"].split(", "))
        if reduced_tags.issubset(image_tags):
            images.append(blob_client)
            count.update(image_tags)

    count = [i[0] for i in count.most_common()]
    # get the start and end index based on page number
    start = (page - 1) * per_page
    end = start + per_page
    # page 1 is [0:6], page 2 is [6:12]
    items = images[start:end]
    pagination = Pagination(None, page, per_page, len(images), items)
    return render_template('results.html', pagination=pagination, count = count, tag1 = tags[0], tag2 = tags[1], tag3=tags[2])
