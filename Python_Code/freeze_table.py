from PyQt6.QtWidgets import (
    QWidget, QVBoxLayout, QHBoxLayout, QPushButton, QLabel, QTableView, QAbstractItemView,
    QHeaderView, QScrollBar, QComboBox, QLineEdit, QDialog, QFileDialog, QMessageBox, QGroupBox, QProgressBar,QProgressDialog,
    QTabWidget, QScrollArea, QCheckBox
)
from PyQt6.QtCore import Qt, QAbstractTableModel, QTimer, QThread, pyqtSignal
from PyQt6.QtGui import QStandardItemModel, QStandardItem, QFont, QColor
import pandas as pd
from openpyxl import Workbook
from openpyxl.styles import PatternFill, Font, Alignment, Border, Side
from openpyxl.utils import get_column_letter
import numpy as np
import os
import platform
import math
from functools import reduce
import re
import logging

from .changeReport import ChangesReportDialog
from .column_filter import ColumnFilterDialog,FilterDialog
# Setup logging
logging.basicConfig(level=logging.DEBUG, format="%(asctime)s - %(levelname)s - %(message)s")
logger = logging.getLogger(__name__)

# Global stylesheet
global_style = """
    QWidget {
        background-color: #F5F7FA;
        font-family: 'Inter', 'Segoe UI', sans-serif;
        font-size: 13px;
    }
    QGroupBox {
        font-weight: bold;
        color: #1A3C34;
        margin-top: 15px;
        border: 1px solid #D0D7DE;
        border-radius: 6px;
        padding: 10px;
    }
    QGroupBox::title {
        subcontrol-origin: margin;
        subcontrol-position: top left;
        padding: 0 5px;
        left: 10px;
    }
    QPushButton {
        background-color: #2E7D32;
        color: white;
        border: none;
        padding: 8px 16px;
        font-weight: 600;
        font-size: 13px;
        border-radius: 6px;
    }
    QPushButton:hover {
        background-color: #1B5E20;
    }
    QPushButton:disabled {
        background-color: #E0E0E0;
        color: #6B7280;
    }
    QTableView {
        background-color: #FFFFFF;
        border: 1px solid #D0D7DE;
        gridline-color: #E5E7EB;
        font-size: 12px;
        selection-background-color: #DBEAFE;
        selection-color: #1A3C34;
    }
    QHeaderView::section {
        background-color: #F9FAFB;
        font-weight: 600;
        color: #1A3C34;
        border: 1px solid #D0D7DE;
        padding: 6px;
    }
    QTableView::item:selected {
        background-color: #DBEAFE;
        color: #1A3C34;
    }
    QTableView::item {
        padding: 0px;
    }
    QLineEdit {
        padding: 6px;
        border: 1px solid #D0D7DE;
        border-radius: 4px;
        font-size: 13px;
    }
    QLineEdit:focus {
        border: 1px solid #2E7D32;
    }
    QLabel {
        font-size: 13px;
        color: #1A3C34;
    }
    QComboBox {
        padding: 6px;
        border: 1px solid #D0D7DE;
        border-radius: 4px;
        font-size: 13px;
    }
    QComboBox:focus {
        border: 1px solid #2E7D32;
    }
    QProgressBar {
        border: 1px solid #D0D7DE;
        border-radius: 4px;
        text-align: center;
    }
    QProgressBar::chunk {
        background-color: #2E7D32;
    }
"""

class FreezeTableWidget(QTableView):
    def __init__(self, model, parent=None):
        super().__init__(parent)
        self.frozenTableView = QTableView(self)
        self.setModel(model)
        self.frozenTableView.setModel(model)
        self._is_dialog_open = False  # Flag to prevent multiple dialogs
        self.init()

        self.horizontalHeader().sectionResized.connect(self.updateSectionWidth)
        self.verticalHeader().sectionResized.connect(self.updateSectionHeight)
        self.frozenTableView.verticalScrollBar().valueChanged.connect(self.frozenVerticalScroll)
        self.verticalScrollBar().valueChanged.connect(self.mainVerticalScroll)

    def init(self):
        self.frozenTableView.setFocusPolicy(Qt.FocusPolicy.NoFocus)
        self.frozenTableView.verticalHeader().hide()
        self.frozenTableView.horizontalHeader().setSectionResizeMode(QHeaderView.ResizeMode.Fixed)
        self.viewport().stackUnder(self.frozenTableView)
        self.frozenTableView.setStyleSheet(global_style)
        self.frozenTableView.setSelectionModel(self.selectionModel())
        self.setHorizontalScrollMode(QAbstractItemView.ScrollMode.ScrollPerPixel)
        self.setVerticalScrollMode(QAbstractItemView.ScrollMode.ScrollPerPixel)
        self.frozenTableView.setHorizontalScrollMode(QAbstractItemView.ScrollMode.ScrollPerPixel)
        self.frozenTableView.setVerticalScrollMode(QAbstractItemView.ScrollMode.ScrollPerPixel)
        self.update_frozen_columns()
        self.frozenTableView.setHorizontalScrollBarPolicy(Qt.ScrollBarPolicy.ScrollBarAlwaysOff)
        self.frozenTableView.setVerticalScrollBarPolicy(Qt.ScrollBarPolicy.ScrollBarAlwaysOff)
        self.frozenTableView.horizontalHeader().sectionClicked.connect(self.on_frozen_header_clicked)
        self.updateFrozenTableGeometry()
        self.frozenTableView.show()

    def update_frozen_columns(self):
        if self.model() is None or self.model().columnCount() == 0:
            self.frozenTableView.hide()
            return
        for col in range(self.model().columnCount()):
            self.frozenTableView.setColumnHidden(col, col != 0)
        column_width = self.columnWidth(0) if self.model().columnCount() > 0 else 100
        self.frozenTableView.setColumnWidth(0, column_width)
        self.frozenTableView.show()
        self.updateFrozenTableGeometry()

    def updateSectionWidth(self, logicalIndex, oldSize, newSize):
        if logicalIndex == 0:
            self.frozenTableView.setColumnWidth(0, newSize)
            self.updateFrozenTableGeometry()
            self.frozenTableView.viewport().update()

    def updateSectionHeight(self, logicalIndex, oldSize, newSize):
        self.frozenTableView.setRowHeight(logicalIndex, newSize)

    def frozenVerticalScroll(self, value):
        self.viewport().stackUnder(self.frozenTableView)
        self.verticalScrollBar().setValue(value)
        self.frozenTableView.viewport().update()
        self.viewport().update()

    def mainVerticalScroll(self, value):
        self.viewport().stackUnder(self.frozenTableView)
        self.frozenTableView.verticalScrollBar().setValue(value)
        self.frozenTableView.viewport().update()
        self.viewport().update()

    def updateFrozenTableGeometry(self):
        if self.model() is None or self.model().columnCount() == 0:
            return
        self.frozenTableView.setGeometry(
            self.verticalHeader().width() + self.frameWidth(),
            self.frameWidth(),
            self.columnWidth(0),
            self.viewport().height() + self.horizontalHeader().height()
        )
        self.frozenTableView.setFixedWidth(self.columnWidth(0))
        self.frozenTableView.viewport().update()

    def resizeEvent(self, event):
        super().resizeEvent(event)
        self.updateFrozenTableGeometry()
        self.frozenTableView.viewport().update()

    def moveCursor(self, cursorAction, modifiers):
        current = super().moveCursor(cursorAction, modifiers)
        if cursorAction == QAbstractItemView.CursorAction.MoveLeft and current.column() > 0:
            visual_x = self.visualRect(current).topLeft().x()
            if visual_x < self.frozenTableView.columnWidth(0):
                new_value = self.horizontalScrollBar().value() + visual_x - self.frozenTableView.columnWidth(0)
                self.horizontalScrollBar().setValue(int(new_value))
        return current

    def scrollTo(self, index, hint=QAbstractItemView.ScrollHint.EnsureVisible):
        if index.column() > 0:
            super().scrollTo(index, hint)
        self.frozenTableView.viewport().update()

    def on_frozen_header_clicked(self, section):
        """Redirect frozen header click to ResultsFrame's header click handler"""
        logger.debug(f"Frozen header clicked for section: {section}")
        if self.model() is None:
            logger.warning("No model set for frozen table")
            QMessageBox.warning(self, "Error", "Table model not initialized.")
            return
        parent = self.parent().parent() if self.parent() else None
        logger.debug(f"ResultsFrame parent: {parent}, type: {type(parent).__name__ if parent else 'None'}")
        if section == 0 and not self._is_dialog_open:
            if parent is not None and hasattr(parent, 'on_header_clicked'):
                self._is_dialog_open = True
                logger.debug("Calling ResultsFrame on_header_clicked for Solution Label")
                try:
                    parent.on_header_clicked(section, col_name="Solution Label")
                finally:
                    self._is_dialog_open = False
            else:
                logger.warning(f"Cannot call on_header_clicked. Parent: {parent}, has_method: {hasattr(parent, 'on_header_clicked') if parent else False}")
                QMessageBox.warning(self, "Error", "Cannot open filter dialog: ResultsFrame not found.")
        else:
            if section != 0:
                logger.warning(f"Unexpected section {section} clicked in frozen table")
            if self._is_dialog_open:
                logger.debug("Dialog already open, ignoring click")

    def setModel(self, model):
        super().setModel(model)
        if self.frozenTableView is not None:
            self.frozenTableView.setModel(model)
            self.update_frozen_columns()
            self.updateFrozenTableGeometry()
