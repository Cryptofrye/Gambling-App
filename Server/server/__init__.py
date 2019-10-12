from flask import Flask
from flask_sqlalchemy import SQLAlchemy
from flask_bcrypt import Bcrypt
from flask_login import LoginManager
from flask_admin import Admin
from flask_admin.contrib.sqla import ModelView
from server.config import Config


db = SQLAlchemy()
bcrypt = Bcrypt()
login_manager = LoginManager()
admin = Admin(name='gambling app', template_mode='bootstrap3')
app = Flask(__name__)

def create_app():
    app = Flask(__name__)
    app.config.from_object(Config)
    # Change this - needs to be secret.
    db.init_app(app)
    bcrypt.init_app(app)
    login_manager.init_app(app)
    from server.models import User
    admin.init_app(app)
    admin.add_view(ModelView(User, db.session))
    from server.routes import routes
    app.register_blueprint(routes)

    return app