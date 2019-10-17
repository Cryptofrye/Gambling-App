import random
from flask_login import login_user, current_user, logout_user, login_required
from flask import request, jsonify, Blueprint, abort, render_template, url_for, redirect
from server import app, db, bcrypt, login_manager
import server.models as models

game = Blueprint('game', __name__)

@game.route("/game/dice/play/", methods=["POST"])
@login_required
def diceplay():
    user = models.User.query.filter_by(username=current_user.username).first()
    if ableToPlay(user, request.form.get("amount")):
        return diceplaygame(user, float(request.form.get("amount")))

@game.route("/game/dice/leaderboard/", methods=["GET"])
def leaderboard():
    # Perhaps add a way the user can make queries?
    users = models.User.query.order_by(models.User.money.desc()).all()
    return jsonify([{'username':user.username,'money':user.money} for user in users])

@game.route("/game/dice/howtoplay/", methods=["GET"])
def dicehowtoplay():
    return jsonify(
        instructions = """You start with £5 in your bank account
You can choose how much you bet, the minimum amount you can bet is £0.20
You roll 3 dice
If you get 2 the same, you win your bet with a x5 multiplier
If you get 3 the same, you win your bet with a x10 multiplier
Play for as long as you like, or until you go broke!""")

def diceplaygame(user, amount):
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

# BLACKJACK

@game.route("/game/blackjack/play/", methods=["GET", "POST"])
@login_required
def blackjackplay():
    if sum(current_user.blackJackHand.getCardsAsList()) > 21:
        # Return computer's cards as empty, because the user went bust - the computer didn't even draw a card.
        return createBlackJackGameJsonResponse(False, current_user.blackJackHand.getCardsAsList(), [], amount, -amount)
    return jsonify(
        playerCards = [current_user.blackJackHand.getCardsAsList()[x] for x in range(len(current_user.blackJackHand.getCardsAsList()))]
    )

@game.route("/game/blackjack/play/hit/", methods=["GET", "POST"])
@login_required
def blackjackplayhit():
    # Add a card to the player's deck
    current_user.blackJackHand.addToCards(random.randint(1,11))
    db.session.commit()
    return redirect(url_for('game.blackjackplay'), code=307)

@game.route("/game/blackjack/play/stand/", methods=["GET", "POST"])
@login_required
def blackjackplaystand():
    if ableToPlay(current_user, request.form.get("amount")):    
        current_user.blackjackGameStats.BlackjackPlays += 1
        db.session.commit()
        computerCards = [random.randint(1,11), random.randint(1,11)]
        amount = float(request.form.get("amount"))
        while True:
            # Computer goes bust - Player wins
            if sum(computerCards) > 21:
                current_user.blackjackGameStats.BlackjackWins += 1
                current_user.blackjackGameStats.totalMoneyEarned += (amount * 2)
                current_user.money += (amount * 2)
                db.session.commit()
                return createBlackJackGameJsonResponse(True, current_user.blackJackHand.getCardsAsList(), computerCards, amount, (amount * 2))
            # Computers cards value more than the player's - computer wins
            elif sum(computerCards) > sum(current_user.blackJackHand.getCardsAsList()):
                current_user.blackjackGameStats.totalMoneyLost += amount
                current_user.money -= amount
                db.session.commit()
                return createBlackJackGameJsonResponse(False, current_user.blackJackHand.getCardsAsList(), computerCards, amount, -amount)
            computerCards.append(random.randint(1,11))
        return f"Computer cards : {computerCards}"

@game.route("/game/blackjack/howtoplay/")
def blackjackhowtoplay():
    return jsonify(
        instructions="""You start with 2 cards
You can hit or stand
hit meaning you get another card, stand meaning you don't.
You have to make your total as close to, but not more than 21.
If you do better than the computer, you double your bet.""")

def createBlackJackGameJsonResponse(wonGame, userCards, computerCards, amountBet, amountWon):
    current_user.blackJackHand.resetCards()
    db.session.commit()
    return jsonify(
        wonGame = wonGame,
        playerCards = [userCards[x] for x in range(len(userCards))],
        computerCards = [computerCards[x] for x in range(len(computerCards))],
        amountBet = amountBet,
        amountWon = amountWon,
        newBalance = current_user.money
    )

def ableToPlay(user, amount):
    if not amount:
        return abort(500, "Missing parameter - amount")
    if not amount.replace(".", "", 1).isdigit() or float(amount) < 0.2 or len(amount.split(" ")) != 1:
        return abort(500, "Amount parameter must only contain a positive float and must be at least £0.20")
    if not user.money >= float(amount):
        return abort(403, "You don't have enough money to perform this action")
    return True
