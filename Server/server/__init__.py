from flask import Flask, abort, render_template
from flask_sqlalchemy import SQLAlchemy
from flask_bcrypt import Bcrypt
from flask_login import LoginManager, current_user
from flask_admin import Admin, BaseView, expose
from flask_admin.contrib.sqla import ModelView
from flask_migrate import Migrate, MigrateCommand  
from server.config import Config


db = SQLAlchemy()
bcrypt = Bcrypt()
login_manager = LoginManager()
migrate = Migrate()
app = Flask(__name__)

def create_app():
    app = Flask(__name__)
    app.config.from_object(Config)
    # Change this - needs to be secret.
    db.init_app(app)
    bcrypt.init_app(app)
    login_manager.init_app(app)
    from server.models import User, DiceGameStats, BlackjackGameStats, BlackJackHand
    from server.adminsite.admin_routes import AdminLogin, AdminModelView, CustomAdminIndexView
    migrate.init_app(app, db)
    admin = Admin(app, name="Gambling Application Administrator's Panel", template_mode='bootstrap3', index_view=CustomAdminIndexView())
    admin.add_view(AdminModelView(User, db.session))
    admin.add_view(AdminModelView(DiceGameStats, db.session))
    admin.add_view(AdminModelView(BlackJackHand, db.session))
    admin.add_view(AdminModelView(BlackjackGameStats, db.session))
    admin.add_view(AdminLogin(endpoint="login"))
    from server.main.routes import main
    from server.users.routes import users
    from server.game.routes import game
    app.register_blueprint(main)
    app.register_blueprint(users)
    app.register_blueprint(game)

    return app