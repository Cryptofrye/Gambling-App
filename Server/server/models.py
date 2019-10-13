from datetime import datetime
from server import db, login_manager
from flask_login import UserMixin

@login_manager.user_loader
def load_user(user_id):
    """User loader used for flask-login"""
    return User.query.get(user_id)

class User(db.Model, UserMixin):
    id = db.Column(db.Integer, primary_key=True)
    username = db.Column(db.String(80), unique=True, nullable=False)
    money = db.Column(db.Float)
    password = db.Column(db.String(80), unique=False, nullable=False)
    date_registered = db.Column(db.DateTime, nullable=False, default=datetime.utcnow)
    is_admin = db.Column(db.Boolean, nullable=False, default=False)

    def __repr__(self):
        return f"<User {self.username} - £{self.money} - {self.password[0:16]}... - Admin:{self.is_admin}"
