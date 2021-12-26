import os
from flask import Blueprint
from flask import render_template, request

main = Blueprint("main", __name__)

@main.route("/")
@main.route("/home")
def home():
    page = request.args.get("page", 1, type=int)
    images = os.listdir(r"C:\Users\flori\OneDrive\Dokumente\FH_Hagenberg\Studium\DSE\3_Semester\CLC\Projekt\CLC3_G-ans-6\image_db\static\images")
    return render_template("home.html", images=images)
