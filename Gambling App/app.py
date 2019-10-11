from flask_sqlalchemy import SQLAlchemy
from flask import Flask

app = Flask(__name__)
app.config['SQLALCHEMY_DATABASE_URI'] = 'sqlite:///GamblingApp.db'
db = SQLAlchemy(app)

@app.route("/ping")
def ping():
    return "Pong"

import models

if __name__ == '__main__':
    app.run(debug=True, host="0.0.0.0", port=1337)

