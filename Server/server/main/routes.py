import random
from flask_login import login_user, current_user, logout_user, login_required
from flask import request, jsonify, Blueprint, abort, render_template, url_for, redirect
from server import app, db, bcrypt, login_manager
import server.models as models

main = Blueprint('main', __name__)

@main.route("/ping/")
def ping():
    return "Pong"