import app

class User(app.db.Model):
    id = app.db.Column(app.db.Integer, primary_key=True)
    username = app.db.Column(app.db.String(80), unique=True, nullable=False)
    money = app.db.Column(app.db.Float)
    password = app.db.Column(app.db.String(80), unique=False, nullable=False)

    def __repr__(self):
        return f"<User {self.username} - £{self.money} - {self.password[0:16]}..."
