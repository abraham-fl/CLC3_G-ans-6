import os
from flask import Blueprint
from flask import render_template, request
from flask_sqlalchemy import Pagination

main = Blueprint("main", __name__)

@main.route("/", methods = ['GET'])
@main.route("/home", methods = ['GET'])
def home():
    page = request.args.get("page", 1, type=int)
    per_page = 6
    images = os.listdir(r"C:\Users\flori\OneDrive\Dokumente\FH_Hagenberg\Studium\DSE\3_Semester\CLC\Projekt\CLC3_G-ans-6\image_db\static\images")
    # get the start and end index based on page number
    start = (page - 1) * per_page
    end = start + per_page
    # page 1 is [0:6], page 2 is [6:12]
    items = images[start:end]
    pagination = Pagination(None, page, per_page, len(images), items)
    return render_template('home.html', pagination=pagination)