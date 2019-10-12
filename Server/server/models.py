from server import db
from flask_login import UserMixin

class User(db.Model, UserMixin):
    id = db.Column(db.Integer, primary_key=True)
    username = db.Column(db.String(80), unique=True, nullable=False)
    money = db.Column(db.Float)
    password = db.Column(db.String(80), unique=False, nullable=False)

    def __repr__(self):
        return f"<User {self.username} - £{self.money} - {self.password[0:16]}..."
