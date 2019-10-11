from flask_sqlalchemy import SQLAlchemy
from flask_bcrypt import Bcrypt
from flask import Flask, request

app = Flask(__name__)
app.config['SQLALCHEMY_DATABASE_URI'] = 'sqlite:///GamblingApp.db'
db = SQLAlchemy(app)
bcrypt = Bcrypt(app)

@app.route("/ping")
def ping():
    return "Pong"

@app.route("/register", methods=["POST"])
def register():
    if request.method == 'POST':
        username = request.form['username']
        password = request.form['password']
        user = models.User(username=username, money=5.00, password=password)
        db.session.add(user)
        db.session.commit()
        return f"User Inserted - {username} : {password}"
    return "403 - Method not allowed"

import models

if __name__ == '__main__':
    app.run(debug=True, host="0.0.0.0", port=1337)

