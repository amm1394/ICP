# main_window.py
import sys
import os
import logging
from PyQt6.QtWidgets import (
    QApplication, QMainWindow, QLabel, QTabWidget
)
from PyQt6.QtCore import Qt

from tab import MainTabContent
from screens.calibration_tab import ElementsTab
from screens.pivot.pivot_tab import PivotTab
from screens.CRM import CRMTab
from utils.load_file import load_excel, load_additional
from screens.process.result import ResultsFrame
from screens.process.RM_check import CheckRMFrame
from screens.process.weight_check import WeightCheckFrame
from screens.process.volume_check import VolumeCheckFrame
from screens.process.DF_check import DFCheckFrame
from screens.process.empty_check import EmptyCheckFrame
from screens.process.CRM_check import CrmCheck
from screens.process.report import ReportTab
from screens.compare_tab import CompareTab
import sqlite3  # اضافه شد
import pandas as pd
# وارد کردن توابع ذخیره/بارگذاری
from project_manager import save_project, load_project

# وارد کردن LoginWindow
from login_window import LoginWindow

logger = logging.getLogger(__name__)
logging.basicConfig(level=logging.DEBUG)


class MainWindow(QMainWindow):
    open_windows = []

    def __init__(self, user_email="guest@rasf.local", user_name="Guest", user_position="Guest"):
        super().__init__()
        self.user_email = user_email
        self.user_name = user_name
        self.user_position = user_position
        logger.debug(f"Creating MainWindow for user: {self.user_name}")

        # داده‌ها
        self.data = None
        self.file_path = None
        self.file_path_label = QLabel("File Path: No file selected")

        # تب‌ها
        self.pivot_tab = PivotTab(self, self)
        self.elements_tab = ElementsTab(self, self)
        self.crm_tab = CRMTab(self, self)
        self.results = ResultsFrame(self, self)
        self.rm_check = CheckRMFrame(self, self)
        self.weight_check = WeightCheckFrame(self, self)
        self.volume_check = VolumeCheckFrame(self, self)
        self.df_check = DFCheckFrame(self, self)
        self.compare_tab = CompareTab(self, self)
        self.empty_check = EmptyCheckFrame(self, self)
        self.crm_check = CrmCheck(self, self.results)
        self.report = ReportTab(self, self.results)

        # اتصالات سیگنال
        self.weight_check.data_changed.connect(self.results.data_changed)
        self.volume_check.data_changed.connect(self.results.data_changed)
        self.df_check.data_changed.connect(self.results.data_changed)
        self.rm_check.data_changed.connect(self.results.data_changed)
        self.empty_check.empty_rows_found.connect(self.rm_check.on_empty_rows_received)

        # تعریف تب‌ها با دکمه‌های جدید + Logout
        tab_info = {
            "File": {
                "Open": self.handle_excel,
                "Save Project": self.save_project,
                "Load Project": self.load_project,
                "Additional": self.handle_additional,
                "New": self.new_window,
                "Close": self.close_window,
                "Logout": self.logout  # اضافه شده
            },
            "Find similarity": {"display": self.compare_tab},
            "Process": {
                "Weight Check": self.weight_check,
                "Volume Check": self.volume_check,
                "DF check": self.df_check,
                "Empty check": self.empty_check,
                "CRM Calibraton": self.crm_check,
                "Drift Calibraton": self.rm_check,
                "Result": self.results,
                "Report": self.report
            },
            "Elements": {"Display": self.elements_tab},
            "Raw Data": {"Display": self.pivot_tab},
            "CRM": {"CRM": self.crm_tab},
        }

        self.main_content = MainTabContent(tab_info)
        self.setCentralWidget(self.main_content)
        self.setWindowTitle(f"RASF Data Processor - {self.user_name}")
        MainWindow.open_windows.append(self)

    # دکمه‌ها
    def new_window(self):
        new_win = MainWindow(self.user_email, self.user_name, self.user_position)
        new_win.show()

    def close_window(self):
        self.close()

    def closeEvent(self, event):
        MainWindow.open_windows.remove(self)
        if hasattr(self.crm_tab, 'close_db_connection'):
            self.crm_tab.close_db_connection()
        event.accept()

    def reset_app_state(self):
        logger.debug("Resetting application state")
        self.data = None
        self.file_path = None
        self.file_path_label.setText("File Path: No file selected")
        self.setWindowTitle(f"RASF Data Processor - {self.user_name}")

        for attr in [
            'pivot_tab', 'elements_tab', 'crm_tab', 'results', 'rm_check',
            'weight_check', 'volume_check', 'df_check', 'compare_tab',
            'empty_check', 'crm_check', 'report'
        ]:
            obj = getattr(self, attr, None)
            if obj and hasattr(obj, 'reset_state'):
                obj.reset_state()

    def handle_excel(self):
        self.reset_app_state()
        load_excel(self)

    def handle_additional(self):
        load_additional(self)

    # فراخوانی توابع ذخیره/بارگذاری
    def save_project(self):
        save_project(self)

    def load_project(self):
        load_project(self)

    def resource_path(self, relative_path):
        try:
            base_path = sys._MEIPASS
        except Exception:
            base_path = os.path.abspath(".")
        return os.path.join(base_path, relative_path)
    # متد Logout
    def logout(self):
        """فراموش کردن کاربر و بازگشت به صفحه لاگین"""
        from PyQt6.QtWidgets import QMessageBox

        reply = QMessageBox.question(
            self, "Logout",
            f"Are you sure you want to log out, {self.user_name}?",
            QMessageBox.StandardButton.Yes | QMessageBox.StandardButton.No
        )
        if reply != QMessageBox.StandardButton.Yes:
            return

        # 1. فراموش کردن کاربر فعلی در دیتابیس (remember_me = 0)
        try:
            db_path = self.resource_path("crm_data.db")
            conn = sqlite3.connect(db_path)
            cur = conn.cursor()
            # فقط کاربر فعلی را فراموش کن
            cur.execute("UPDATE users SET remember_me = 0 WHERE email = ?", (self.user_email,))
            # بهتر: همه کاربران را فراموش کن (در صورت نیاز)
            # cur.execute("UPDATE users SET remember_me = 0")
            conn.commit()
            conn.close()
            logger.info(f"User {self.user_email} logged out and remember_me cleared.")
        except Exception as e:
            logger.error(f"Failed to update remember_me: {e}")
            QMessageBox.warning(self, "Warning", "Could not clear login memory.")

        # 2. بستن تمام پنجره‌های باز
        for win in list(MainWindow.open_windows):
            win.close()

        # 3. باز کردن دوباره صفحه لاگین
        login_win = LoginWindow()
        def open_main_after_login(email, name, pos):
            new_main = MainWindow(email, name, pos)
            new_main.show()
        login_win.login_successful.connect(open_main_after_login)
        login_win.show()

    # تابع کمکی برای مسیر فایل‌ها (مثل PyInstaller)
    def resource_path(self, relative_path):
        """Get absolute path to resource, works for dev and PyInstaller."""
        try:
            base_path = sys._MEIPASS
        except Exception:
            base_path = os.path.abspath(".")
        return os.path.join(base_path, relative_path)

    # داده‌ها
    def set_data(self, df, for_results=False):
        if not isinstance(df, pd.DataFrame):
            return
        self.data = df.copy(deep=True)
        if for_results:
            self.notify_data_changed()

    def notify_data_changed(self):
        for i in range(self.main_content.tabs.count()):
            tab = self.main_content.tabs.widget(i)
            if isinstance(tab, QTabWidget):
                for j in range(tab.count()):
                    sub = tab.widget(j)
                    if hasattr(sub, 'data_changed'):
                        sub.data_changed()

    def get_data(self): return self.data
    def get_excluded_samples(self): return []
    def get_excluded_volumes(self): return []
    def get_excluded_dfs(self): return []

