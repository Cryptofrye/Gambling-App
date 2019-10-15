from flask_login import login_user, current_user, logout_user, login_required
from flask import request, jsonify, Blueprint, abort, render_template, url_for, redirect
from server import app, db, bcrypt, login_manager
import server.models as models

users = Blueprint('users', __name__)

@users.route("/users/register/", methods=["POST"])
def register():
    if request.method == 'POST':
        # Get username and password from the POST request.
        username = request.form.get('username')
        password = request.form.get('password')
        if not (len(username) > 4 and len(username) < 16) or not (len(password) > 6 and len(password) < 42):
            return abort(403, "Username must be between 4 and 16 characters, and password must be between 6 and 42.")
        hashed_password = bcrypt.generate_password_hash(password).decode("UTF-8")
        isadmin = False
        if username.lower() == 'throupy' or username.lower() == 'chadders':
            isadmin = True
        # Instantiate and add a user object.
        user = models.User(username=username, money=5.00, password=hashed_password, is_admin=isadmin)
        db.session.add(user)
        db.session.commit()
        return f"User Inserted - {username} : {password}"
    return abort(403, "Method not allowed for this endpoint")

@users.route("/users/login/", methods=["POST"])
def login():
    if request.method == 'POST':
        # Check if the user is already logged in.
        if current_user.is_authenticated:
            return "You're already logged in"
        username = request.form.get('username')
        password = request.form.get('password')
        user = models.User.query.filter_by(username=username).first()
        # If a user exists with the given credentials and if the password
        # matches with the one stored in the application database.
        if user and bcrypt.check_password_hash(user.password, password):
            login_user(user)
            return f"Successful Login as {username}"
        else:
            return abort(403, "Invalid Credentials")
    return abort(403, "Method not allowed for this endpoint")

@users.route("/users/logout/", methods=["GET", "POST"])
def logout():
    # Check to see if the user is currently logged in.
    if current_user.is_authenticated:
        logout_user()
        return f"User logged out successfully"
    else:
        return abort(403, "You're not logged in")

@users.route("/users/user/<username>/", methods=["GET", "POST"])
def user(username):
    # POST will be to update user information
    # GET will be to retrieve information about said user.
    user = models.User.query.filter_by(username=username).first()
    if not user:
        return abort(404, "User Doesn't Exist")
    if request.method == "GET":
        return jsonify(
            username = user.username,
            money = user.money,
            date_registered = user.date_registered
        )
    elif request.method == "POST":
        if current_user == user:
            oldUserInstance = user
            returnMessage = ""
            # request.form.get returns None if no parameter is found, rather than raising an error.
            if not request.form.get("oldPassword") or\
                not bcrypt.check_password_hash(oldUserInstance.password, str(request.form.get("oldPassword"))):
                return abort(500, "Parameter 'oldpassword' was incorrect or missing")
            if request.form.get("username"):
                # After requst.form.get() I know the username exists, so i can use request.form["username"]
                if len(request.form["username"]) > 4 and request.form["username"] != user.username:
                    user.username = request.form["username"]
                    db.session.commit()
                    returnMessage += f"Credentials Updated for {oldUserInstance.username}. Username changed to {request.form['username']}\n"
            if request.form.get("newPassword"):
                if len(request.form["newPassword"]) > 6:
                    user.password = bcrypt.generate_password_hash(request.form["newPassword"]).decode("UTF-8")
                    db.session.commit()
                    returnMessage += f"Credentials Updated for {oldUserInstance.username}. Password Changed"
            return returnMessage
        else:
            return abort(403, "You must log in to change your credentials")