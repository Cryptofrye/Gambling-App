from flask_login import login_user, current_user, logout_user, login_required
from flask import request, jsonify, Blueprint, abort, render_template, url_for, redirect
from server import app, db, bcrypt, login_manager
import server.models as models

users = Blueprint('users', __name__)

@users.route("/users/test/")
def test():
    x = models.User.query.filter_by(username='Throupy').first()
    return str(x.diceGameStats.first().totalMoneyEarned)


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
        # Dice Game Statistics Child Model
        diceGameStats = models.DiceGameStats(parentUser=user)
        blackjackGameStats = models.BlackjackGameStats(parentUser=user)
        blackJackHand = models.BlackJackHand(player=user)
        db.session.add(user);db.session.add(diceGameStats);db.session.add(blackJackHand);db.session.add(blackjackGameStats)
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

@users.route("/users/sendmoney/<username>/", methods=["GET", "POST"])
@login_required
def sendmoney(username):
    recipient = models.User.query.filter_by(username=username).first()
    if not recipient:
        return abort(404, "User Doesn't Exist")
    # current_user will be sender because of the login_required decorator
    sender = models.User.query.filter_by(username=current_user.username).first()
    amount = request.form.get("amount")
    if ableToSend(sender, amount):
        # Send the money
        recipient.money += float(amount)
        sender.money -= float(amount)
        db.session.commit()
        return f"Sender money is now {sender.money}, recipient money is now {recipient.money}"
        

def ableToSend(sender, amount):
    if not amount:
        return abort(500, "Missing parameter - amount")
    if not amount.replace(".", "", 1).isdigit() or float(amount) < 0.2 or len(amount.split(" ")) != 1:
        return abort(500, "Amount parameter must only contain a positive float and must be at least £0.20")
    if not sender.money >= float(amount):
        return abort(403, "You don't have enough money to perform this action")
    return True


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
            date_registered = user.date_registered,
            dice_game_stats = [{
                'dice_game_wins' : user.diceGameStats.diceGameWins,
                'dice_game_plays' : user.diceGameStats.diceGamePlays,
                'total_money_earned' : user.diceGameStats.totalMoneyEarned,
                'total_moeny_lost' : user.diceGameStats.totalMoneyLost
            }]
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