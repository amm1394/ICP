# main.py
import sys
import logging
from PyQt6.QtWidgets import QApplication
from login_window import LoginWindow
from app import MainWindow

logging.basicConfig(level=logging.INFO)
logger = logging.getLogger(__name__)

if __name__ == "__main__":
    app = QApplication(sys.argv)
    app.setStyle("Fusion")

    login = LoginWindow()

    def open_main(email: str, name: str, position: str):
        logger.info(f"Login: {name} ({position}) - {email}")
        window = MainWindow(user_email=email, user_name=name, user_position=position)
        window.setWindowTitle(f"RASF PROCESSING - {name} ({position})")
        window.resize(1200, 750)
        window.show()
        login.close()

    login.login_successful.connect(open_main)
    login.show()

    sys.exit(app.exec())