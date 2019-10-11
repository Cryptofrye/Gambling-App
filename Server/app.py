from flask_sqlalchemy import SQLAlchemy
from flask_bcrypt import Bcrypt
from flask_login import LoginManager, login_user, current_user, logout_user, login_required
from flask import Flask, request

app = Flask(__name__)
app.config['SQLALCHEMY_DATABASE_URI'] = 'sqlite:///GamblingApp.db'
# Change this - needs to be secret.
app.secret_key = b'_5#y2L"F4Q8z\n\xec]/'
db = SQLAlchemy(app)
login_manager = LoginManager(app)
bcrypt = Bcrypt(app)

@login_manager.user_loader
def load_user(user_id):
    """User loader used for flask-login"""
    return models.User.query.get(user_id)

@app.route("/ping")
def ping():
    return "Pong"

@app.route("/register", methods=["POST"])
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
    return "403 - Method not allowed"

@app.route("/login", methods=["POST"])
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
            return f"Invalid Credentials for {username}"
    return "403 Method Not Allowed"

@app.route("/logout", methods=["POST"])
def logout():
    if request.method == 'POST':
        # Check to see if the user is currently logged in.
        if current_user.is_authenticated:
            logout_user()
            return f"User logged out successfully"
        else:
            return f"You're not logged in"
    return "403 Method Not Allowed"  

@app.route("/testy", methods=["POST"])
@login_required
def testy():
    return "Aloha logged in user!"

# Imported at the end to avoid circular imports.        
import models

if __name__ == '__main__':
    app.run(debug=True, host="0.0.0.0", port=1337)

