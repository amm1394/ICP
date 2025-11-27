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

logging.basicConfig(level=logging.DEBUG, format="%(asctime)s - %(levelname)s - %(message)s")
logger = logging.getLogger(__name__)

class ColumnFilterDialog(QDialog):
    """Dialog for filtering a specific column with search and min/max for numeric columns."""
    def __init__(self, parent, col_name):
        super().__init__(parent)
        self.setWindowTitle(f"Filter Column: {col_name}")
        self.parent = parent
        self.col_name = col_name
        self.checkboxes = {}
        self.min_edit = None
        self.max_edit = None

        # NEW: Use last_filtered_data as fallback if last_pivot_data is None
        data_source = self.parent.last_pivot_data if self.parent.last_pivot_data is not None else self.parent.last_filtered_data
        if data_source is None or data_source.empty:
            logger.warning(f"No data available for filtering column {col_name}")
            QMessageBox.warning(self, "Warning", f"No data available for column {col_name}!")
            self.reject()
            return

        if col_name not in data_source.columns:
            logger.warning(f"Column {col_name} not found in data")
            QMessageBox.warning(self, "Warning", f"Column {col_name} not found!")
            self.reject()
            return

        try:
            test_series = pd.to_numeric(data_source[self.col_name], errors='coerce')
            self.is_numeric_col = test_series.notna().any() and self.col_name != 'Solution Label'
            logger.debug(f"Column {col_name} is_numeric after coercion: {self.is_numeric_col}")
        except Exception as e:
            self.is_numeric_col = False
            logger.error(f"Error checking numeric type for {col_name}: {str(e)}")

        unique_values = data_source[self.col_name].dropna().unique()
        if self.is_numeric_col:
            sorted_unique = sorted(unique_values, key=lambda x: float(x) if self.is_numeric(str(x)) else float('inf'))
        else:
            sorted_unique = sorted(unique_values, key=str)

        layout = QVBoxLayout(self)
        self.setMinimumSize(400, 400)

        tab_widget = QTabWidget()
        list_tab = QWidget()
        tab_widget.addTab(list_tab, "List Filter")
        
        if self.is_numeric_col:
            logger.debug(f"Adding Number Filter tab for column {col_name}")
            number_tab = QWidget()
            tab_widget.addTab(number_tab, "Number Filter")
            self.setup_number_tab(number_tab)
        else:
            logger.debug(f"Skipping Number Filter tab for column {col_name}")

        layout.addWidget(tab_widget)
        self.setup_list_tab(list_tab, sorted_unique)

        buttons = QHBoxLayout()
        ok_btn = QPushButton("OK")
        ok_btn.clicked.connect(self.apply_filters)
        buttons.addWidget(ok_btn)

        cancel_btn = QPushButton("Cancel")
        cancel_btn.clicked.connect(self.reject)
        buttons.addWidget(cancel_btn)

        layout.addLayout(buttons)

    def setup_list_tab(self, widget, sorted_unique):
        list_layout = QVBoxLayout(widget)

        self.search_edit = QLineEdit()
        self.search_edit.setPlaceholderText("Search...")
        self.search_edit.textChanged.connect(self.filter_checkboxes)
        list_layout.addWidget(self.search_edit)

        scroll = QScrollArea()
        scroll_widget = QWidget()
        scroll_layout = QVBoxLayout(scroll_widget)
        scroll.setWidget(scroll_widget)
        scroll.setWidgetResizable(True)
        list_layout.addWidget(scroll)

        if not hasattr(self.parent, 'column_filters'):
            self.parent.column_filters = {}
        
        curr_filter = self.parent.column_filters.get(self.col_name, {})
        selected_values = curr_filter.get('selected_values', set(sorted_unique))

        for val in sorted_unique:
            cb = QCheckBox(str(val))
            cb.setChecked(val in selected_values)
            cb.stateChanged.connect(lambda state, v=val: self.update_filter(v, state))
            self.checkboxes[val] = cb
            scroll_layout.addWidget(cb)

        buttons = QHBoxLayout()
        select_all_btn = QPushButton("Select All")
        select_all_btn.clicked.connect(lambda: self.toggle_all(True))
        buttons.addWidget(select_all_btn)

        deselect_all_btn = QPushButton("Deselect All")
        deselect_all_btn.clicked.connect(lambda: self.toggle_all(False))
        buttons.addWidget(deselect_all_btn)

        list_layout.addLayout(buttons)

    def setup_number_tab(self, widget):
        number_layout = QVBoxLayout(widget)

        min_layout = QHBoxLayout()
        min_label = QLabel("Minimum:")
        self.min_edit = QLineEdit()
        self.min_edit.setPlaceholderText("Enter min value")
        self.min_edit.setFixedWidth(100)
        min_layout.addWidget(min_label)
        min_layout.addWidget(self.min_edit)
        min_layout.addStretch()
        number_layout.addLayout(min_layout)

        max_layout = QHBoxLayout()
        max_label = QLabel("Maximum:")
        self.max_edit = QLineEdit()
        self.max_edit.setPlaceholderText("Enter max value")
        self.max_edit.setFixedWidth(100)
        max_layout.addWidget(max_label)
        max_layout.addWidget(self.max_edit)
        max_layout.addStretch()
        number_layout.addLayout(max_layout)

        # NEW: Use last_filtered_data as fallback
        data_source = self.parent.last_pivot_data if self.parent.last_pivot_data is not None else self.parent.last_filtered_data
        try:
            data_range = pd.to_numeric(data_source[self.col_name], errors='coerce').dropna()
            if not data_range.empty:
                min_val = data_range.min()
                max_val = data_range.max()
                range_label = QLabel(f"Data Range: {min_val:.2f} to {max_val:.2f}")
                range_label.setStyleSheet("color: blue; font-size: 10px;")
                number_layout.addWidget(range_label)
                logger.debug(f"Data range for {self.col_name}: {min_val} to {max_val}")
            else:
                logger.warning(f"No valid numeric data for range display in column {self.col_name}")
        except Exception as e:
            logger.error(f"Error computing data range for {self.col_name}: {str(e)}")

        curr_filter = self.parent.column_filters.get(self.col_name, {})
        if 'min_val' in curr_filter and curr_filter['min_val'] is not None:
            self.min_edit.setText(str(curr_filter['min_val']))
        if 'max_val' in curr_filter and curr_filter['max_val'] is not None:
            self.max_edit.setText(str(curr_filter['max_val']))

    def filter_checkboxes(self, text):
        text = text.lower()
        for val, cb in self.checkboxes.items():
            if text == '' or text in str(val).lower():
                cb.setVisible(True)
                cb.setChecked(True)
            else:
                cb.setVisible(False)
                cb.setChecked(False)

    def update_filter(self, value, state):
        self.checkboxes[value].setChecked(state == Qt.CheckState.Checked.value)

    def toggle_all(self, checked):
        for cb in self.checkboxes.values():
            if cb.isVisible():
                cb.setChecked(checked)

    def is_numeric(self, value):
        try:
            float(value)
            return True
        except (ValueError, TypeError):
            return False

    def apply_filters(self):
        selected_values = {val for val, cb in self.checkboxes.items() if cb.isChecked()}
        min_val = None
        max_val = None
        if self.is_numeric_col:
            try:
                if self.min_edit and self.min_edit.text().strip():
                    min_val = float(self.min_edit.text())
                    logger.debug(f"Min filter set for {self.col_name}: {min_val}")
                if self.max_edit and self.max_edit.text().strip():
                    max_val = float(self.max_edit.text())
                    logger.debug(f"Max filter set for {self.col_name}: {max_val}")
                if min_val is not None or max_val is not None:
                    data_source = self.parent.last_pivot_data if self.parent.last_pivot_data is not None else self.parent.last_filtered_data
                    data_range = pd.to_numeric(data_source[self.col_name], errors='coerce').dropna()
                    if not data_range.empty:
                        min_data, max_data = data_range.min(), data_range.max()
                        if (min_val is not None and min_val > max_data) or (max_val is not None and max_val < min_data):
                            QMessageBox.warning(self, "Invalid Filter", f"Filter range ({min_val}, {max_val}) is outside data range ({min_data:.2f}, {max_data:.2f})")
                            return
            except ValueError:
                QMessageBox.warning(self, "Invalid Input", "Min and Max must be numeric values.")
                return

        filter_settings = {'selected_values': selected_values}
        if self.is_numeric_col:
            if min_val is not None:
                filter_settings['min_val'] = min_val
            if max_val is not None:
                filter_settings['max_val'] = max_val

        self.parent.column_filters[self.col_name] = filter_settings
        logger.debug(f"Applied filters for {self.col_name}: {filter_settings}")
        self.parent.show_processed_data()
        self.accept()


class FilterDialog(QDialog):
    def __init__(self, parent, filter_values, filter_field, solution_label_order, element_order):
        super().__init__(parent)
        self.setWindowTitle("Filter Pivot Table")
        self.setFixedSize(400, 600)
        self.filter_values = filter_values
        self.filter_field = filter_field
        self.solution_label_order = solution_label_order
        self.element_order = element_order
        self.setStyleSheet(global_style)
        self.setup_ui()

    def setup_ui(self):
        layout = QVBoxLayout(self)
        layout.setContentsMargins(20, 20, 20, 20)
        layout.setSpacing(15)

        filter_group = QGroupBox("Select Filter Column")
        filter_layout = QVBoxLayout(filter_group)
        filter_layout.setSpacing(10)
        self.filter_combo = QComboBox()
        self.filter_combo.addItems(["Solution Label", "Element"])
        self.filter_combo.setCurrentText(self.filter_field)
        self.filter_combo.setToolTip("Choose the column to filter by")
        self.filter_combo.setFixedHeight(30)
        self.filter_combo.currentTextChanged.connect(self.update_checkboxes)
        filter_layout.addWidget(self.filter_combo)
        layout.addWidget(filter_group)

        self.filter_table = QTableView()
        self.filter_table.setSelectionMode(QTableView.SelectionMode.NoSelection)
        self.filter_table.setStyleSheet(global_style)
        self.filter_table.horizontalHeader().setSectionResizeMode(QHeaderView.ResizeMode.Stretch)
        self.filter_table.setToolTip("Select values to include in the pivot table")
        layout.addWidget(self.filter_table, stretch=1)

        button_group = QGroupBox("Filter Actions")
        button_layout = QHBoxLayout(button_group)
        button_layout.setSpacing(12)

        select_all_btn = QPushButton("Select All")
        select_all_btn.setToolTip("Select all filter values")
        select_all_btn.setMinimumWidth(90)
        select_all_btn.clicked.connect(lambda: self.set_all_checkboxes(True))
        button_layout.addWidget(select_all_btn)

        deselect_all_btn = QPushButton("Deselect All")
        deselect_all_btn.setToolTip("Deselect all filter values")
        deselect_all_btn.setMinimumWidth(90)
        deselect_all_btn.clicked.connect(lambda: self.set_all_checkboxes(False))
        button_layout.addWidget(deselect_all_btn)

        apply_btn = QPushButton("Apply")
        apply_btn.setToolTip("Apply filters to the pivot table")
        apply_btn.setMinimumWidth(90)
        apply_btn.clicked.connect(self.apply_filters)
        button_layout.addWidget(apply_btn)

        close_btn = QPushButton("Close")
        close_btn.setToolTip("Close the filter window without applying changes")
        close_btn.setMinimumWidth(90)
        close_btn.clicked.connect(self.reject)
        button_layout.addWidget(close_btn)

        layout.addWidget(button_group)
        self.update_checkboxes()

    def update_checkboxes(self):
        field = self.filter_combo.currentText()
        self.parent().filter_field = field
        model = QStandardItemModel()
        model.setHorizontalHeaderLabels(["Value", "Select"])

        unique_values = (
            self.solution_label_order if field == "Solution Label"
            else self.element_order if self.element_order else []
        )
        if field not in self.filter_values or not self.filter_values[field]:
            self.filter_values[field] = {val: True for val in unique_values}

        for value in unique_values:
            value_item = QStandardItem(str(value))
            value_item.setEditable(False)
            check_item = QStandardItem()
            check_item.setCheckable(True)
            check_item.setCheckState(
                Qt.CheckState.Checked if self.filter_values[field].get(value, True)
                else Qt.CheckState.Unchecked
            )
            model.appendRow([value_item, check_item])

        self.filter_table.setModel(model)
        self.filter_table.setColumnWidth(0, 250)
        self.filter_table.setColumnWidth(1, 100)
        model.itemChanged.connect(lambda item: self.toggle_filter(item, field))

    def toggle_filter(self, item, field):
        if item.column() == 1:
            value = self.filter_table.model().item(item.row(), 0).text()
            self.filter_values[field][value] = (item.checkState() == Qt.CheckState.Checked)

    def set_all_checkboxes(self, value):
        field = self.filter_combo.currentText()
        if field in self.filter_values:
            for val in self.filter_values[field]:
                self.filter_values[field][val] = value
            self.update_checkboxes()

    def apply_filters(self):
        self.parent().reset_filter_cache()
        self.parent().show_processed_data()
        self.accept()
