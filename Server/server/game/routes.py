import random
from flask_login import login_user, current_user, logout_user, login_required
from flask import request, jsonify, Blueprint, abort, render_template, url_for, redirect
from server import app, db, bcrypt, login_manager
import server.models as models

game = Blueprint('game', __name__)

@game.route("/game/dice/play/", methods=["POST"])
@login_required
def diceplay():
    amount = request.form.get("amount")
    user = models.User.query.filter_by(username=current_user.username).first()
    if ableToPlay(user, amount):
        return playgame(user, float(amount))

@game.route("/game/dice/leaderboard/", methods=["GET"])
def leaderboard():
    # Perhaps add a way the user can make queries?
    users = models.User.query.order_by(models.User.money.desc()).all()
    return jsonify([{'username':user.username,'money':user.money} for user in users])

@game.route("/game/dice/howtoplay/", methods=["GET"])
def howtoplay():
    return jsonify(
        instructions = """You start with £5 in your bank account
You can choose how much you bet, the minimum amount you can bet is £0.20
You roll 3 dice
If you get 2 the same, you win your bet with a x5 multiplier
If you get 3 the same, you win your bet with a x10 multiplier
Play for as long as you like, or until you go broke!""")

@game.route("/game/blackjack/play/")
def blackjackplay():
    if sum(current_user.blackJackHand.getCardsAsList()) > 21:
        # Return computer's cards as empty, because the user went bust - the computer didn't even draw a card.
        return createBlackJackGameJsonResponse(False, current_user.blackJackHand.getCardsAsList(), [])
    return jsonify(
        playerCards = [current_user.blackJackHand.getCardsAsList()[x] for x in range(len(current_user.blackJackHand.getCardsAsList()))]
    )

@game.route("/game/blackjack/play/hit/")
def blackjackplayhit():
    # Add a card to the player's deck
    current_user.blackJackHand.addToCards(random.randint(1,11))
    db.session.commit()
    return redirect(url_for('game.blackjackplay'))

@game.route("/game/blackjack/play/stand/")
def blackjackplaystand():
    computerCards = [random.randint(1,11), random.randint(1,11)]
    while True:
        # Computer goes bust
        if sum(computerCards) > 21:
            return createBlackJackGameJsonResponse(True, current_user.blackJackHand.getCardsAsList(), computerCards)
        # Computers cards value more than the player's - computer wins
        elif sum(computerCards) > sum(current_user.blackJackHand.getCardsAsList()):
            return createBlackJackGameJsonResponse(False, current_user.blackJackHand.getCardsAsList(), computerCards)
        computerCards.append(random.randint(1,11))
    return f"Computer cards : {computerCards}"


def createBlackJackGameJsonResponse(wonGame, userCards, computerCards):
    current_user.blackJackHand.resetCards()
    db.session.commit()
    return jsonify(
        wonGame = wonGame,
        playerCards = [userCards[x] for x in range(len(userCards))],
        computerCards = [computerCards[x] for x in range(len(computerCards))]
    )

def ableToPlay(user, amount):
    if not amount:
        return abort(500, "Missing parameter - amount")
    if not amount.replace(".", "", 1).isdigit() or float(amount) < 0.2 or len(amount.split(" ")) != 1:
        return abort(500, "Amount parameter must only contain a positive float and must be at least £0.20")
    if not user.money >= float(amount):
        return abort(403, "You don't have enough money to perform this action")
    return True

def playgame(user, amount):
    # Roll 3 dice, if 2 are the same, you get £1, if they're all the same, you get £2
    user.diceGameStats.diceGamePlays += 1
    dice = []
    for x in range(3):
        dice.append(random.randint(1,6))
    for die in dice:
        if dice.count(die) == 3:
            user.money += (amount * 10)
            user.diceGameStats.totalMoneyEarned += (amount * 10)
            user.diceGameStats.diceGameWins += 1
            db.session.commit()
            return createDiceGameJsonResponse(dice, True, amount, (amount*10), round(user.money, 1))
        elif dice.count(die) == 2:
            user.money += (amount * 5)
            user.diceGameStats.totalMoneyEarned += (amount * 5)
            user.diceGameStats.diceGameWins += 1
            db.session.commit()
            return createDiceGameJsonResponse(dice, True, amount, (amount*5), round(user.money, 1))
    user.money -= amount
    user.diceGameStats.totalMoneyLost += amount
    db.session.commit()
    return createDiceGameJsonResponse(dice, False, amount, (-amount), round(user.money, 1))

def createDiceGameJsonResponse(dice, wonGame, amount, amountWon, newBalance):
    return jsonify(
        dice1 = dice[0],
        dice2 = dice[1],
        dice3 = dice[2],
        wonGame = wonGame,
        amountBet = amount,
        amountWon = amountWon,
        newBalance = newBalance
    )
