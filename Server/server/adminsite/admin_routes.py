from server.forms import AdminLoginForm
from server.models import User
from flask_admin import Admin, BaseView, expose, AdminIndexView
from flask_admin.contrib.sqla import ModelView
from flask import request, url_for, render_template, abort, redirect, flash
from flask_login import current_user, login_user

class CustomAdminIndexView(AdminIndexView):
    def is_accessible(self):
        if not current_user.is_authenticated or not current_user.is_admin:
            return False
        else:
            return True   

    def inaccessible_callback(self, name, **kwargs):
        print("red")
        return redirect(url_for("login.index"))

class AdminModelView(ModelView):
    def is_accessible(self):
        if not current_user.is_authenticated or not current_user.is_admin:
            return False
        else:
            return True

    def inaccessible_callback(self, name, **kwargs):
        return redirect(url_for("login.index"))

class AdminLogin(BaseView):
    def is_accessible(self):
        return not current_user.is_authenticated

    def inaccessible_callback(self, name, **kwargs):
        flash("You're already logged in", "info")
        return redirect(url_for("admin.index"))

    @expose("/", methods=["GET", "POST"])
    def index(self):
        if current_user.is_authenticated:
            return redirect(url_for("admin.index"))
        form = AdminLoginForm()
        if form.validate_on_submit():
            username = request.form['username']
            password = request.form['password']
            user = User.query.filter_by(username=username).first()
            # HASH, SALT, CHANGE AND PRIVATIZE THE PASSWORD
            # THIS IS A FUCKING DUMPSTER FIRE
            if user and password == "admin":
                login_user(user)
                flash("Logged in", "success")
                return redirect(url_for("admin.index"))
            else:
                flash("Invalid Credentials, please try again", "danger")
                return redirect(url_for("admin.index"))
        return render_template("admin.html", form=form)