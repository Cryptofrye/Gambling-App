import app

class User(app.db.Model):
    id = app.db.Column(app.db.Integer, primary_key=True)
    username = app.db.Column(app.db.String(80), unique=True, nullable=False)
    money = app.db.Column(app.db.Float)

    def __repr__(self):
        return f"<User {self.username} - £{self.money}"