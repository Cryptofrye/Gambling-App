from flask_login import login_user, current_user, logout_user, login_required
from flask import request, jsonify, Blueprint, abort
from server import app, db, bcrypt, login_manager
import server.models as models

routes = Blueprint('routes', __name__)

@login_manager.user_loader
def load_user(user_id):
    """User loader used for flask-login"""
    return models.User.query.get(user_id)

@routes.route("/ping")
def ping():
    return "Pong"

@routes.route("/register", methods=["POST"])
def register():
    if request.method == 'POST':
        # Get username and password from the POST request.
        username = request.form['username']
        password = request.form['password']
        hashed_password = bcrypt.generate_password_hash(password).decode("UTF-8")
        # Instantiate and add a user object.
        user = models.User(username=username, money=5.00, password=hashed_password)
        db.session.add(user)
        db.session.commit()
        return f"User Inserted - {username} : {password}"
    return abort(403, "Method not allowed for this endpoint")

@routes.route("/login", methods=["POST"])
def login():
    if request.method == 'POST':
        # Check if the user is already logged in.
        if current_user.is_authenticated:
            return "You're already logged in"
        username = request.form['username']
        password = request.form['password']
        user = models.User.query.filter_by(username=username).first()
        # If a user exists with the given credentials and if the password
        # matches with the one stored in the application database.
        if user and bcrypt.check_password_hash(user.password, password):
            login_user(user)
            return f"Successful Login as {username}"
        else:
            return abort(403, "Invalid Credentials")
    return abort(403, "Method not allowed for this endpoint")

@routes.route("/logout", methods=["POST"])
def logout():
    if request.method == 'POST':
        # Check to see if the user is currently logged in.
        if current_user.is_authenticated:
            logout_user()
            return f"User logged out successfully"
        else:
            return abort(403, "You're not logged in")
    return abort(403, "Method not allowed for this endpoint")

@routes.route("/user/<username>/", methods=["GET", "POST"])
def user(username):
    # POST will be to update user information
    # GET will be to retrieve information about said user.
    user = models.User.query.filter_by(username=username).first()
    if request.method == "GET":
        return jsonify(
            username = user.username,
            money = user.money,
            date_registered = user.date_registered
        )
    elif request.method == "POST":
        if current_user == user:
            old_name = user.username
            # If the user passes a username in the request
            if request.form["username"]:
                if len(request.form["username"]) > 4:
                    user.username = request.form["username"]
                    db.session.commit()
            if request.form["password"]:
                if len(request.form["password"]) > 6:
                    user.password = bcrypt.generate_password_hash(request.form["password"]).decode("UTF-8")
                    db.session.commit()
            return f"Credentials Updated for {old_name}. Username changed to {request.form['username']}"
        else:
            return abort(403, "You must log in to change your credentials")


@routes.route("/testy", methods=["POST"])
@login_required
def testy():
    return "Aloha logged in user!"
