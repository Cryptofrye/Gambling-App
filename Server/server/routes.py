import random
from flask_login import login_user, current_user, logout_user, login_required
from flask import request, jsonify, Blueprint, abort, render_template, url_for, redirect
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
        username = request.form.get('username')
        password = request.form.get('password')
        if len(username) < 4 or len(password) < 6:
            return abort(403, "Username must be at least 4 characters, and password must be at least 6.")
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

@routes.route("/login", methods=["POST"])
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

@routes.route("/logout", methods=["GET", "POST"])
def logout():
    # Check to see if the user is currently logged in.
    if current_user.is_authenticated:
        logout_user()
        return f"User logged out successfully"
    else:
        return abort(403, "You're not logged in")

@routes.route("/user/<username>/", methods=["GET", "POST"])
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
            old_name = user.username
            returnMessage = ""
            # request.form.get returns None if no parameter is found, rather than raising an error.
            if not request.form.get("username") and not request.form.get("password"):
                return abort(500, "Expected a value to change")
            if request.form.get("username"):
                # After requst.form.get() I know the username exists, so i can use request.form["username"]
                if len(request.form["username"]) > 4 and request.form["username"] != user.username:
                    user.username = request.form["username"]
                    db.session.commit()
                    returnMessage += f"Credentials Updated for {old_name}. Username changed to {request.form['username']}\n"
            if request.form.get("password"):
                if len(request.form["password"]) > 6:
                    user.password = bcrypt.generate_password_hash(request.form["password"]).decode("UTF-8")
                    db.session.commit()
                    returnMessage += f"Credentials Updated for {old_name}. Password Changed"
            return returnMessage
        else:
            return abort(403, "You must log in to change your credentials")

@routes.route("/game/play", methods=["GET", "POST"])
@login_required
def play():
    user = models.User.query.filter_by(username=current_user.username).first()
    amount = request.form.get("amount")
    if not amount:
        return abort(500, "Missing parameter - amount")
    if not amount.replace(".", "", 1).isdigit() or float(amount) < 0.2 or len(amount.split(" ")) != 1:
        return abort(500, "Amount parameter must only contain a positive float and must be at least £0.20")
    if not user.money >= float(amount):
        return abort(403, "You don't have enough money to perform this action")
    return game(user, float(amount))


@routes.route("/howtoplay", methods=["GET"])
def howtoplay():
    return jsonify(
        instructions = """
        You start with £5 in your bank account\n
        You can choose how much you bet, the minimum amount you can bet is £0.20\n
        You roll 3 dice\n 
        If you get 2 the same, you win your bet with a x5 multiplier\n
        If you get 3 the same, you win your bet with a x10 multiplier\n
        Play for as long as you like, or until you go broke!
        """
    )

def game(user, amount):
    # Roll 3 dice, if 2 are the same, you get £1, if they're all the same, you get £2
    dice = []
    for x in range(3):
        dice.append(random.randint(1,6))
    print(dice)
    for die in dice:
        if dice.count(die) == 3:
            user.money += (amount * 10)
            db.session.commit()
            return createGameJsonResponse(dice, True, amount, (amount*10), round(user.money, 1))
        elif dice.count(die) == 2:
            user.money += (amount * 5)
            db.session.commit()
            return createGameJsonResponse(dice, True, amount, (amount*5), round(user.money, 1))
    user.money -= amount
    db.session.commit()
    return createGameJsonResponse(dice, False, amount, (-amount), round(user.money, 1))

def createGameJsonResponse(dice, wonGame, amount, amountWon, newBalance):
    return jsonify(
        dice1 = dice[0],
        dice2 = dice[1],
        dice3 = dice[2],
        wonGame = wonGame,
        amountBet = amount,
        amountWon = amountWon,
        newBalance = newBalance
    )