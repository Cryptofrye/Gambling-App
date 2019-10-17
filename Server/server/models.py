import json
import random
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
    diceGameStats = db.relationship('DiceGameStats', cascade="all,delete", backref='parentUser', uselist=False)
    blackjackGameStats = db.relationship('BlackjackGameStats', cascade="all,delete", backref='parentUser', uselist=False)
    blackJackHand = db.relationship('BlackJackHand', cascade="all,delete", backref='player', uselist=False)
    is_admin = db.Column(db.Boolean, nullable=False, default=False)

    def __repr__(self):
        return f"<User {self.username} - £{self.money} - Admin:{self.is_admin}"

class DiceGameStats(db.Model, UserMixin):
    id = db.Column(db.Integer, primary_key=True)
    diceGameWins = db.Column(db.Integer, nullable=False, default=0)
    diceGamePlays = db.Column(db.Integer, nullable=False, default=0)
    totalMoneyEarned = db.Column(db.Float, nullable=False, default=0.0)
    totalMoneyLost = db.Column(db.Float, nullable=False, default=0.0)
    # User that the object belongs to
    user_id = db.Column(db.Integer, db.ForeignKey('user.id'), nullable=False)

    def __repr__(self):
        return f"DiceGameStats object belonging to user with ID {self.user_id}"

class BlackjackGameStats(db.Model, UserMixin):
    id = db.Column(db.Integer, primary_key=True)
    BlackjackWins = db.Column(db.Integer, nullable=False, default=0)
    BlackjackPlays = db.Column(db.Integer, nullable=False, default=0)
    totalMoneyEarned = db.Column(db.Float, nullable=False, default=0.0)
    totalMoneyLost = db.Column(db.Float, nullable=False, default=0.0)
    # User that the object belongs to
    user_id = db.Column(db.Integer, db.ForeignKey('user.id'), nullable=False)

    def __repr__(self):
        return f"BlackjackGameStats object belonging to user with ID {self.user_id}"

class BlackJackHand(db.Model, UserMixin):
    id = db.Column(db.Integer, primary_key=True)
    user_id = db.Column(db.Integer, db.ForeignKey('user.id'), nullable=False)
    cards = db.Column(db.String, nullable=False, default=f"[{random.randint(1,11)},{random.randint(1,11)}]")
    is_playing = db.Column(db.Boolean, nullable=False, default=False)

    def getCardsAsList(self):
        return json.loads(self.cards)

    def addToCards(self, number):
        jsonCards = json.loads(self.cards)
        jsonCards.append(number)
        self.cards = json.dumps(jsonCards)

    def resetCards(self):
        self.cards = f"[{random.randint(1,11)},{random.randint(1,11)}]"