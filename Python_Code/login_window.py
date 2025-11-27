# login_window.py
import os
import sys
import sqlite3
from PyQt6.QtWidgets import *
from PyQt6.QtGui import QPixmap, QFont, QIcon, QColor
from PyQt6.QtCore import Qt, pyqtSignal, QTimer

class LoginWindow(QWidget):
    login_successful = pyqtSignal(str, str, str)  # email, full_name, position

    def __init__(self, parent=None):
        super().__init__(parent)
        self.setWindowTitle("RASF - Login")
        self.setFixedSize(1000, 620)
        self.setWindowIcon(QIcon(self.resource_path("icon.png")))
        self.db_path = self.resource_path("crm_data.db")

        self.remembered = False  # اضافه شده برای چک remember_me

        self.init_db()

        # اگر کاربر قبلاً تیک زده بود → مستقیم وارد شود (UI ساخته نشود)
        if self.check_remembered_user():
            return  # فقط سیگنال emit می‌شود، UI نمایش داده نمی‌شود

        self.init_ui()
        self.apply_light_style()

    def resource_path(self, relative_path):
        """Get absolute path to resource, works for dev and PyInstaller."""
        try:
            base_path = sys._MEIPASS
        except Exception:
            base_path = os.path.abspath(".")
        return os.path.join(base_path, relative_path)

    def init_db(self):
        """Create 'users' table + add 'remember_me' column if missing + create 'changes_log' table"""
        try:
            conn = sqlite3.connect(self.db_path)
            cursor = conn.cursor()

            # ساخت جدول users (اگر وجود نداشته باشد)
            cursor.execute('''
                CREATE TABLE IF NOT EXISTS users (
                    id INTEGER PRIMARY KEY AUTOINCREMENT,
                    email TEXT UNIQUE NOT NULL,
                    password TEXT NOT NULL,
                    full_name TEXT,
                    position TEXT,
                    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
                    remember_me INTEGER DEFAULT 0
                )
            ''')

            # بررسی و اضافه کردن ستون remember_me اگر وجود نداشته باشد
            cursor.execute("PRAGMA table_info(users)")
            columns = [info[1] for info in cursor.fetchall()]
            if 'remember_me' not in columns:
                cursor.execute("ALTER TABLE users ADD COLUMN remember_me INTEGER DEFAULT 0")

            # ساخت جدول changes_log (اگر وجود نداشته باشد)
            cursor.execute('''
                CREATE TABLE IF NOT EXISTS changes_log (
                    id INTEGER PRIMARY KEY AUTOINCREMENT,
                    timestamp TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
                    user_name TEXT,
                    user_position TEXT,
                    file_path TEXT,
                    column_name TEXT,
                    solution_label TEXT,
                    original_value TEXT,
                    new_value TEXT,
                    weight_correction TEXT,
                    volume_correction TEXT,
                    df_correction TEXT,
                    crm_calibration TEXT,
                    drift_calibration TEXT
                )
            ''')

            conn.commit()
            print(f"Database ready: {self.db_path}")
        except Exception as e:
            QMessageBox.critical(self, "Database Error", f"Could not initialize database:\n{e}")
        finally:
            if 'conn' in locals():
                conn.close()

    def check_remembered_user(self):
        """Auto-login if user checked 'Remember Me'"""
        try:
            conn = sqlite3.connect(self.db_path)
            cur = conn.cursor()
            cur.execute("SELECT email, full_name, position FROM users WHERE remember_me = 1 LIMIT 1")
            user = cur.fetchone()
            conn.close()

            if user:
                email, name, pos = user
                name = name or email.split("@")[0].capitalize()
                pos = pos or "User"
                print(f"Auto-login successful: {name} ({email})")

                self.remembered = True  # علامت‌گذاری که از remember_me وارد شده

                QTimer.singleShot(0, lambda: self.login_successful.emit(email, name, pos))
                return True
            return False
        except Exception as e:
            print(f"Auto-login failed: {e}")
            return False

    def init_ui(self):
        main_layout = QHBoxLayout(self)
        main_layout.setContentsMargins(0, 0, 0, 0)
        main_layout.setSpacing(0)

        # LEFT: Logo – perfectly centered
        left_panel = QFrame()
        left_panel.setFixedWidth(420)
        left_layout = QVBoxLayout(left_panel)
        left_layout.setAlignment(Qt.AlignmentFlag.AlignCenter)

        self.logo = QLabel()
        logo_path = self.resource_path("logo.png")
        if os.path.exists(logo_path):
            pixmap = QPixmap(logo_path).scaled(
                300, 300,
                Qt.AspectRatioMode.KeepAspectRatio,
                Qt.TransformationMode.SmoothTransformation
            )
            self.logo.setPixmap(pixmap)
        else:
            self.logo.setText("RASF")
            self.logo.setFont(QFont("Segoe UI", 90, QFont.Weight.Bold))
            self.logo.setStyleSheet("color: #2c3e50;")
        self.logo.setAlignment(Qt.AlignmentFlag.AlignCenter)

        left_layout.addStretch()
        left_layout.addWidget(self.logo)
        left_layout.addStretch()

        # RIGHT: Form
        right_panel = QFrame()
        right_layout = QVBoxLayout(right_panel)
        right_layout.setContentsMargins(70, 70, 70, 70)
        right_layout.setSpacing(22)

        self.stacked = QStackedWidget()
        self.stacked.addWidget(self.create_login_page())
        self.stacked.addWidget(self.create_signup_page())

        right_layout.addWidget(self.stacked)
        right_layout.addStretch()

        main_layout.addWidget(left_panel)
        main_layout.addWidget(right_panel, 1)

    def create_login_page(self):
        page = QWidget()
        layout = QVBoxLayout(page)
        layout.setSpacing(18)

        title = QLabel("Welcome Back")
        title.setFont(QFont("Segoe UI", 32, QFont.Weight.Bold))
        title.setAlignment(Qt.AlignmentFlag.AlignCenter)
        title.setStyleSheet("color: #2c3e50;")

        self.email = QLineEdit()
        self.email.setPlaceholderText("Email Address")
        self.email.setMinimumHeight(56)

        self.password = QLineEdit()
        self.password.setPlaceholderText("Password")
        self.password.setEchoMode(QLineEdit.EchoMode.Password)
        self.password.setMinimumHeight(56)

        login_btn = QPushButton("Sign In")
        login_btn.setMinimumHeight(60)
        login_btn.setCursor(Qt.CursorShape.PointingHandCursor)
        login_btn.clicked.connect(self.handle_login)
        self.add_shadow(login_btn)

        guest_btn = QPushButton("Continue as Guest")
        guest_btn.setMinimumHeight(52)
        guest_btn.setObjectName("guestBtn")
        guest_btn.clicked.connect(self.guest_login)

        # Remember Me
        self.remember_me = QCheckBox("Remember Me")
        self.remember_me.setFont(QFont("Segoe UI", 11))
        self.remember_me.setStyleSheet("color: #475569;")
        self.remember_me.setChecked(True)

        link = QLabel('Don\'t have an account? <a href="#" style="color:#3498db; font-weight:bold;">Sign up</a>')
        link.setAlignment(Qt.AlignmentFlag.AlignCenter)
        link.setOpenExternalLinks(False)
        link.linkActivated.connect(lambda: self.stacked.setCurrentIndex(1))

        layout.addWidget(title)
        layout.addSpacing(30)
        layout.addWidget(self.email)
        layout.addWidget(self.password)
        layout.addWidget(login_btn)
        layout.addSpacing(15)
        layout.addWidget(guest_btn)
        layout.addSpacing(10)
        layout.addWidget(self.remember_me, alignment=Qt.AlignmentFlag.AlignCenter)
        layout.addSpacing(20)
        layout.addWidget(link)
        layout.addStretch()
        return page

    def create_signup_page(self):
        page = QWidget()
        layout = QVBoxLayout(page)
        layout.setSpacing(18)

        title = QLabel("Create Account")
        title.setFont(QFont("Segoe UI", 28, QFont.Weight.Bold))
        title.setAlignment(Qt.AlignmentFlag.AlignCenter)
        title.setStyleSheet("color: #2c3e50;")

        self.fullname = QLineEdit()
        self.fullname.setPlaceholderText("Full Name (Optional)")
        self.fullname.setMinimumHeight(56)

        self.signup_email = QLineEdit()
        self.signup_email.setPlaceholderText("Email Address *")
        self.signup_email.setMinimumHeight(56)

        self.position = QComboBox()
        self.position.addItems([
            "Select Position...",
            "Lab Manager", "Senior Analyst", "Chemist",
            "QA/QC Engineer", "Researcher", "Technician",
            "Student", "Other"
        ])
        self.position.setMinimumHeight(56)

        self.signup_password = QLineEdit()
        self.signup_password.setPlaceholderText("Password *")
        self.signup_password.setEchoMode(QLineEdit.EchoMode.Password)
        self.signup_password.setMinimumHeight(56)

        signup_btn = QPushButton("Create Account")
        signup_btn.setMinimumHeight(60)
        signup_btn.setCursor(Qt.CursorShape.PointingHandCursor)
        signup_btn.clicked.connect(self.handle_signup)
        self.add_shadow(signup_btn)

        back = QLabel('<a href="#" style="color:#3498db; font-weight:bold;">Back to Sign In</a>')
        back.setAlignment(Qt.AlignmentFlag.AlignCenter)
        back.linkActivated.connect(lambda: self.stacked.setCurrentIndex(0))

        layout.addWidget(title)
        layout.addSpacing(20)
        layout.addWidget(self.fullname)
        layout.addWidget(self.signup_email)
        layout.addWidget(self.position)
        layout.addWidget(self.signup_password)
        layout.addWidget(signup_btn)
        layout.addSpacing(20)
        layout.addWidget(back)
        layout.addStretch()
        return page

    def add_shadow(self, widget):
        shadow = QGraphicsDropShadowEffect()
        shadow.setBlurRadius(25)
        shadow.setXOffset(0)
        shadow.setYOffset(8)
        shadow.setColor(QColor(0, 0, 0, 60))
        widget.setGraphicsEffect(shadow)

    def handle_login(self):
        email = self.email.text().strip()
        password = self.password.text().strip()
        remember = 1 if self.remember_me.isChecked() else 0

        if not email or not password:
            QMessageBox.warning(self, "Error", "Email and password are required.")
            return
        if "@" not in email or "." not in email:
            QMessageBox.warning(self, "Error", "Please enter a valid email.")
            return

        try:
            conn = sqlite3.connect(self.db_path)
            cur = conn.cursor()
            cur.execute("SELECT full_name, position FROM users WHERE email=? AND password=?", (email, password))
            user = cur.fetchone()

            if user:
                name, pos = user
                name = name or email.split("@")[0].capitalize()
                pos = pos or "User"

                # ذخیره وضعیت Remember Me
                cur.execute("UPDATE users SET remember_me = ? WHERE email = ?", (remember, email))
                conn.commit()
                conn.close()

                QMessageBox.information(self, "Success", f"Welcome back, {name}!")
                self.login_successful.emit(email, name, pos)
                self.close()
            else:
                conn.close()
                QMessageBox.critical(self, "Error", "Invalid email or password.")
        except Exception as e:
            QMessageBox.critical(self, "Error", f"Database error:\n{e}")

    def handle_signup(self):
        email = self.signup_email.text().strip()
        pwd = self.signup_password.text().strip()
        name = self.fullname.text().strip()
        pos = self.position.currentText()

        if not email or not pwd:
            QMessageBox.warning(self, "Error", "Email and password are required.")
            return
        if "@" not in email or "." not in email:
            QMessageBox.warning(self, "Error", "Valid email required.")
            return
        if pos == "Select Position...":
            QMessageBox.warning(self, "Error", "Please select your position.")
            return

        try:
            conn = sqlite3.connect(self.db_path)
            cur = conn.cursor()
            cur.execute("INSERT INTO users (email, password, full_name, position, remember_me) VALUES (?, ?, ?, ?, 0)",
                        (email, pwd, name or None, pos))
            conn.commit()
            conn.close()
            QMessageBox.information(self, "Success", "Account created! Please sign in.")
            self.stacked.setCurrentIndex(0)
            self.clear_signup()
        except sqlite3.IntegrityError:
            QMessageBox.critical(self, "Error", "Email already registered.")
        except Exception as e:
            QMessageBox.critical(self, "Error", f"Database error:\n{e}")

    def clear_signup(self):
        self.fullname.clear()
        self.signup_email.clear()
        self.signup_password.clear()
        self.position.setCurrentIndex(0)

    def guest_login(self):
        # برای Guest، remember_me = 0
        self.login_successful.emit("guest@rasf.local", "Guest", "Guest")
        self.close()

    def apply_light_style(self):
        self.setStyleSheet("""
            QWidget {
                font-family: "Segoe UI", sans-serif;
                background: #f8fafc;
            }
            QFrame:first-child {
                background: qlineargradient(x1:0, y1:0, x2:0, y2:1,
                    stop:0 #ffffff, stop:1 #f1f5f9);
                border-top-left-radius: 20px;
                border-bottom-left-radius: 20px;
            }
            QFrame:last-child {
                background: #ffffff;
                border-top-right-radius: 20px;
                border-bottom-right-radius: 20px;
                box-shadow: 0 10px 30px rgba(0,0,0,0.08);
            }
            QLineEdit, QComboBox {
                border: 2px solid #cbd5e1;
                border-radius: 16px;
                padding: 14px 20px;
                background: #ffffff;
                color: #64748b;
                font-size: 15px;
                font-weight: 500;
            }
            QLineEdit::placeholder, QComboBox::placeholder {
                color: #94a3b8;
            }
            QLineEdit:focus, QComboBox:focus {
                border: 2px solid #3b82f6;
                background: #f8fafc;
                color: #1e293b;
            }
            QComboBox::drop-down {
                border: 0;
                width: 40px;
            }
            QPushButton {
                background: qlineargradient(x1:0, y1:0, x2:0, y2:1,
                    stop:0 #3b82f6, stop:1 #2563eb);
                border: none;
                border-radius: 18px;
                color: white;
                font-weight: bold;
                font-size: 16px;
                padding: 16px;
            }
            QPushButton:hover {
                background: qlineargradient(x1:0, y1:0, x2:0, y2:1,
                    stop:0 #60a5fa, stop:1 #3b82f6);
            }
            QPushButton:pressed {
                background: #1d4ed8;
            }
            QPushButton#guestBtn {
                background: transparent;
                border: 2px solid #94a3b8;
                color: #64748b;
                font-weight: 600;
            }
            QPushButton#guestBtn:hover {
                background: #f1f5f9;
                border-color: #64748b;
            }
            QCheckBox {
                color: #475569;
                spacing: 10px;
            }
            QCheckBox::indicator {
                width: 20px;
                height: 20px;
                border-radius: 6px;
                border: 2px solid #94a3b8;
                background: white;
            }
            QCheckBox::indicator:checked {
                background: #3b82f6;
                border-color: #3b82f6;
            }
            QLabel {
                color: #475569;
            }
            QLabel a {
                color: #3b82f6;
                text-decoration: none;
            }
            QLabel a:hover {
                color: #1d4ed8;
                text-decoration: underline;
            }
        """)

        for btn in self.findChildren(QPushButton):
            if btn.text() == "Continue as Guest":
                btn.setObjectName("guestBtn")