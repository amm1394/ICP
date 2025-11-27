import sys
from PyQt6.QtWidgets import (
    QWidget, QVBoxLayout, QHBoxLayout, QFrame, QLabel, QLineEdit, QPushButton,
    QTableView, QHeaderView, QFileDialog, QAbstractItemView, QMessageBox,
    QComboBox, QGroupBox, QProgressDialog, QCheckBox, QGridLayout
)
from PyQt6.QtCore import Qt, QThread, pyqtSignal
from PyQt6.QtGui import QStandardItemModel, QStandardItem, QColor, QBrush
import pandas as pd
import logging
import re
import random
import sqlite3
import numpy as np

# Setup logging
logging.basicConfig(level=logging.DEBUG, format="%(asctime)s - %(levelname)s - %(message)s")
logger = logging.getLogger(__name__)

class FreezeTableWidget(QTableView):
    """A QTableView with a frozen first column that does not scroll horizontally."""
    def __init__(self, model, parent=None):
        super().__init__(parent)
        self.frozenTableView = QTableView(self)
        self.setModel(model)
        self.frozenTableView.setModel(model)
        self.init_ui()
        self.horizontalHeader().sectionResized.connect(self.updateSectionWidth)
        self.verticalHeader().sectionResized.connect(self.updateSectionHeight)
        self.frozenTableView.verticalScrollBar().valueChanged.connect(self.frozenVerticalScroll)
        self.verticalScrollBar().valueChanged.connect(self.mainVerticalScroll)
        self.model().modelReset.connect(self.resetFrozenTable)

    def init_ui(self):
        self.frozenTableView.setFocusPolicy(Qt.FocusPolicy.NoFocus)
        self.frozenTableView.verticalHeader().hide()
        self.frozenTableView.horizontalHeader().setSectionResizeMode(QHeaderView.ResizeMode.Fixed)
        self.viewport().stackUnder(self.frozenTableView)
        self.frozenTableView.setStyleSheet("QTableView { border: none; selection-background-color: #999; }")
        self.frozenTableView.setSelectionModel(self.selectionModel())
        self.updateFrozenColumns()
        self.frozenTableView.setHorizontalScrollBarPolicy(Qt.ScrollBarPolicy.ScrollBarAlwaysOff)
        self.frozenTableView.setVerticalScrollBarPolicy(Qt.ScrollBarPolicy.ScrollBarAlwaysOff)
        self.frozenTableView.show()
        self.setHorizontalScrollMode(QAbstractItemView.ScrollMode.ScrollPerItem)
        self.setVerticalScrollMode(QAbstractItemView.ScrollMode.ScrollPerItem)
        self.frozenTableView.setVerticalScrollMode(QAbstractItemView.ScrollMode.ScrollPerItem)
        self.updateFrozenTableGeometry()

    def updateFrozenColumns(self):
        for col in range(self.model().columnCount()):
            self.frozenTableView.setColumnHidden(col, col != 0)
        if self.model().columnCount() > 0:
            self.frozenTableView.setColumnWidth(0, self.columnWidth(0) or 100)

    def resetFrozenTable(self):
        self.updateFrozenColumns()
        self.updateFrozenTableGeometry()

    def updateSectionWidth(self, logicalIndex, oldSize, newSize):
        if logicalIndex == 0:
            self.frozenTableView.setColumnWidth(0, newSize)
            self.updateFrozenTableGeometry()
            self.frozenTableView.viewport().repaint()

    def updateSectionHeight(self, logicalIndex, oldSize, newSize):
        self.frozenTableView.setRowHeight(logicalIndex, newSize)
        self.frozenTableView.viewport().repaint()

    def frozenVerticalScroll(self, value):
        self.viewport().stackUnder(self.frozenTableView)
        self.verticalScrollBar().setValue(value)
        self.frozenTableView.viewport().repaint()
        self.viewport().update()

    def mainVerticalScroll(self, value):
        self.viewport().stackUnder(self.frozenTableView)
        self.frozenTableView.verticalScrollBar().setValue(value)
        self.frozenTableView.viewport().repaint()
        self.viewport().update()

    def updateFrozenTableGeometry(self):
        self.frozenTableView.setGeometry(
            self.verticalHeader().width() + self.frameWidth(),
            self.frameWidth(),
            self.columnWidth(0),
            self.viewport().height() + self.horizontalHeader().height()
        )
        self.frozenTableView.viewport().repaint()

    def resizeEvent(self, event):
        super().resizeEvent(event)
        self.updateFrozenTableGeometry()
        self.frozenTableView.viewport().repaint()

    def moveCursor(self, cursorAction, modifiers):
        current = super().moveCursor(cursorAction, modifiers)
        if cursorAction == QAbstractItemView.CursorAction.MoveLeft and current.column() > 0:
            visual_x = self.visualRect(current).topLeft().x()
            if visual_x < self.frozenTableView.columnWidth(0):
                new_value = self.horizontalScrollBar().value() + visual_x - self.frozenTableView.columnWidth(0)
                self.horizontalScrollBar().setValue(int(new_value))
        self.frozenTableView.viewport().repaint()
        return current

    def scrollTo(self, index, hint=QAbstractItemView.ScrollHint.EnsureVisible):
        if index.column() > 0:
            super().scrollTo(index, hint)
        self.frozenTableView.viewport().repaint()


class FilterThread(QThread):
    progress = pyqtSignal(int)
    finished = pyqtSignal(list)
    error = pyqtSignal(str)

    def __init__(self, control_df, sample_df, control_col_map, sample_col_map, control_element_map, selected_elements, num_matches, min_value):
        super().__init__()
        self.control_df = control_df
        self.sample_df = sample_df
        self.control_col_map = control_col_map
        self.sample_col_map = sample_col_map
        self.control_element_map = control_element_map
        self.selected_elements = selected_elements
        self.num_matches = num_matches
        self.min_value = min_value

    def run(self):
        try:
            match_data = []
            total_rows = len(self.control_df)
            THRESHOLD = 0.2
            for idx, (_, control_row) in enumerate(self.control_df.iterrows()):
                control_id = control_row["SAMPLE ID"]
                matching_samples = []
                for _, sample_row in self.sample_df.iterrows():
                    sample_id = sample_row["SAMPLE ID"]
                    rel_diffs = []
                    mismatched_elements = []
                    for elem in self.selected_elements:
                        # تمام طول موج‌های این عنصر
                        control_wls = self.control_element_map.get(elem, [])
                        for wl in control_wls:
                            control_val = control_row.get(self.control_col_map.get(wl, wl), pd.NA)
                            sample_val = sample_row.get(self.sample_col_map.get(wl, wl), pd.NA)
                            if pd.isna(control_val) or pd.isna(sample_val) or control_val < self.min_value or control_val == 0:
                                continue
                            rel_diff = abs(control_val - sample_val) / abs(control_val)
                            rel_diffs.append(rel_diff)
                            if rel_diff > THRESHOLD:
                                mismatched_elements.append(elem)
                    if rel_diffs:
                        avg_diff = sum(rel_diffs) / len(rel_diffs)
                        matching_samples.append((sample_id, sample_row, avg_diff, list(set(mismatched_elements))))
                matching_samples.sort(key=lambda x: (x[2], len(x[3])))
                match_data.append({
                    "Control ID": control_id,
                    "Matching Samples": matching_samples[:self.num_matches],
                    "Control Row": control_row
                })
                self.progress.emit(int((idx + 1) / total_rows * 100))
            match_data.sort(key=lambda m: min(s[2] for s in m["Matching Samples"]) if m["Matching Samples"] else float('inf'))
            self.finished.emit(match_data)
        except Exception as e:
            logger.error(f"Error during filtering: {str(e)}")
            self.error.emit(str(e))


class CompareTab(QWidget):
    update_results = pyqtSignal(dict)
    def __init__(self, app, parent=None):
        super().__init__(parent)
        self.app = app
        self.control_df = None
        self.sample_df = None
        self.control_sheet = None
        self.sample_sheet = None
        self.file_path = None
        self.all_numeric_columns = []
        self.non_numeric_columns = []
        self.headers = []
        self.has_esi_code = False
        self.element_checkboxes = {}
        self.selected_elements = []
        self.control_loaded_from_results = False
        self.control_element_map = {}  # elem -> [wl1, wl2, ...]
        self.default_elements = ["Al", "Ca", "Ba", "As", "Cu", "Fe", "K", "Mg", "Mn", "Na"]
        self.results_windows = []
        self.num_matches_input = None
        self.min_value_input = None
        self.setup_ui()

    def setup_ui(self):
        self.setStyleSheet("""
            QWidget { background-color: #F5F6F5; font: 13px 'Segoe UI'; }
            QLineEdit { background-color: #FFFFFF; border: 1px solid #B0BEC5; padding: 4px; border-radius: 4px; font: 12px 'Segoe UI'; max-width: 80px; }
            QPushButton { background-color: #2E7D32; color: white; border: none; padding: 8px 16px; font: bold 13px 'Segoe UI'; border-radius: 5px; }
            QPushButton:hover { background-color: #1B5E20; }
            QPushButton:disabled { background-color: #B0BEC5; }
            QPushButton#filterButton { background-color: #FF9800; }
            QPushButton#filterButton:hover { background-color: #F57C00; }
            QPushButton#exportButton { background-color: #D32F2F; }
            QPushButton#exportButton:hover { background-color: #B71C1C; }
            QPushButton#correctButton { background-color: #0288D1; }
            QPushButton#correctButton:hover { background-color: #01579B; }
            QPushButton#calculateErrorButton { background-color: #0288D1; }
            QPushButton#calculateErrorButton:hover { background-color: #01579B; }
            QLabel { font: 14px 'Segoe UI'; color: #212121; }
            QTableWidget { background-color: white; gridline-color: #E0E0E0; font: 12px 'Segoe UI'; border: 1px solid #E0E0E0; }
            QHeaderView::section { background-color: #ECEFF1; font: bold 13px 'Segoe UI'; border: 1px solid #E0E0E0; padding: 8px; height: 30px; }
            QTableWidget::item { padding: 5px; }
            QTableWidget::item:selected { background-color: #BBDEFB; color: black; }
            QComboBox { background-color: #FFFFFF; border: 1px solid #B0BEC5; padding: 6px; border-radius: 4px; }
            QComboBox::drop-down { border-left: 1px solid #B0BEC5; }
            QGroupBox { font: bold 15px 'Segoe UI'; border: 1px solid #B0BEC5; border-radius: 6px; margin-top: 1.5em; }
            QGroupBox::title { subcontrol-origin: margin; subcontrol-position: top center; padding: 0 5px; }
            QProgressDialog { min-width: 300px; }
            QCheckBox { font: 13px 'Segoe UI'; color: #212121; padding: 4px; }
        """)
        main_layout = QVBoxLayout(self)
        main_layout.setContentsMargins(0, 0, 0, 0)
        main_layout.setSpacing(0)

        subtab_bar = QWidget()
        subtab_bar.setFixedHeight(50)
        subtab_bar.setStyleSheet("background-color: #e6f3ff;")
        subtab_layout = QHBoxLayout()
        subtab_layout.setContentsMargins(8, 6, 8, 6)
        subtab_layout.setSpacing(8)
        subtab_layout.setAlignment(Qt.AlignmentFlag.AlignLeft)
        subtab_bar.setLayout(subtab_layout)

        self.file_label = QLabel("No file loaded")
        self.file_label.setStyleSheet("color: #6c757d; font: 13px 'Segoe UI';")
        load_button = QPushButton("Load File")
        load_button.setStyleSheet('color:gray')
        load_button.setFixedWidth(160)
        load_button.clicked.connect(self.load_file)

        self.oreas_checkbox = QCheckBox("Compare with OREAS")
        self.oreas_checkbox.stateChanged.connect(self.toggle_oreas)
        logger.debug("OREAS checkbox created")

        subtab_layout.addWidget(QLabel("File:"))
        subtab_layout.addWidget(self.file_label)
        subtab_layout.addWidget(load_button)
        subtab_layout.addWidget(self.oreas_checkbox)

        self.control_combo = QComboBox()
        self.control_combo.setFixedWidth(250)
        self.control_combo.addItem("Select Control Sheet")
        self.control_combo.currentTextChanged.connect(self.update_sheets)
        subtab_layout.addWidget(QLabel("Control Sheet:"))
        subtab_layout.addWidget(self.control_combo)

        self.sample_combo = QComboBox()
        self.sample_combo.setFixedWidth(250)
        self.sample_combo.addItem("Select Sample Sheet")
        self.sample_combo.currentTextChanged.connect(self.update_sheets)
        subtab_layout.addWidget(QLabel("Sample Sheet:"))
        subtab_layout.addWidget(self.sample_combo)

        subtab_layout.addStretch()

        content_area = QWidget()
        content_layout = QVBoxLayout()
        content_layout.setContentsMargins(20, 20, 20, 20)
        content_layout.setSpacing(20)
        content_area.setLayout(content_layout)

        header_label = QLabel("Element Comparison Tool")
        header_label.setStyleSheet("font: bold 18px 'Segoe UI'; color: #2E7D32; margin-bottom: 10px;")
        content_layout.addWidget(header_label, alignment=Qt.AlignmentFlag.AlignCenter)

        self.status_label = QLabel("Load an Excel file to begin")
        self.status_label.setStyleSheet("color: #6c757d; font: 13px 'Segoe UI'; background-color: #E3F2FD; padding: 10px; border-radius: 5px; border: 1px solid #BBDEFB;")
        content_layout.addWidget(self.status_label)

        self.input_frame = QFrame()
        self.input_layout = QVBoxLayout(self.input_frame)
        self.input_layout.setSpacing(15)
        self.input_layout.setContentsMargins(10, 10, 10, 10)
        content_layout.addWidget(self.input_frame, stretch=1)

        button_frame = QFrame()
        button_layout = QHBoxLayout(button_frame)
        button_layout.setSpacing(15)
        button_layout.setContentsMargins(0, 10, 0, 10)

        filter_button = QPushButton("Filter")
        filter_button.setObjectName("filterButton")
        filter_button.clicked.connect(self.perform_filtering)
        filter_button.setFixedWidth(160)
        button_layout.addWidget(filter_button)

        button_layout.addStretch()
        content_layout.addWidget(button_frame, alignment=Qt.AlignmentFlag.AlignCenter)

        main_layout.addWidget(subtab_bar)
        main_layout.addWidget(content_area, stretch=1)

        self.update()
        self.repaint()
        logger.debug("CompareTab UI setup completed")

    def toggle_oreas(self, state):
        logger.debug(f"OREAS checkbox state changed: {state}")
        self.sample_combo.setEnabled(state == Qt.CheckState.Unchecked.value)
        if state == Qt.CheckState.Checked.value:
            self.sample_combo.setCurrentText("Select Sample Sheet")
        self.update_sheets()

    def load_file(self):
        logger.debug("Loading Excel file")
        self.status_label.setText("Loading...")
        self.status_label.setStyleSheet("color: #ff9800; font: 13px 'Segoe UI'; background-color: #FFF3E0; padding: 10px; border-radius: 5px; border: 1px solid #FFE082;")
        file_path, _ = QFileDialog.getOpenFileName(
            self, "Open Excel File", "", "Excel files (*.xlsx *.xls)"
        )
        if not file_path:
            logger.debug("No file selected")
            self.status_label.setText("No file selected")
            self.status_label.setStyleSheet("color: #d32f2f; font: 13px 'Segoe UI'; background-color: #FFEBEE; padding: 10px; border-radius: 5px; border: 1px solid #EF9A9A;")
            QMessageBox.warning(self, "Warning", "No file selected!")
            return
        try:
            xl = pd.ExcelFile(file_path)
            if len(xl.sheet_names) < 1:
                logger.error(f"Expected at least 1 sheet, found {len(xl.sheet_names)}")
                self.status_label.setText("Error: Need at least 1 sheet")
                self.status_label.setStyleSheet("color: #d32f2f; font: 13px 'Segoe UI'; background-color: #FFEBEE; padding: 10px; border-radius: 5px; border: 1px solid #EF9A9A;")
                QMessageBox.critical(self, "Error", "Excel file must have at least one sheet!")
                return
            self.file_path = file_path
            self.file_label.setText(file_path.split('/')[-1])
            self.control_combo.clear()
            self.sample_combo.clear()
            self.control_combo.addItem("Select Control Sheet")
            self.sample_combo.addItem("Select Sample Sheet")
            for sheet in xl.sheet_names:
                self.control_combo.addItem(sheet)
                self.sample_combo.addItem(sheet)
            self.status_label.setText("Select control sheet (and sample sheet if not using OREAS)")
            self.status_label.setStyleSheet("color: #6c757d; font: 13px 'Segoe UI'; background-color: #E3F2FD; padding: 10px; border-radius: 5px; border: 1px solid #BBDEFB;")
            self.control_df = None
            self.sample_df = None
            self.control_sheet = None
            self.sample_sheet = None
            self.clear_input_frame()
        except Exception as e:
            logger.error(f"Error loading file: {str(e)}")
            self.status_label.setText(f"Error: {str(e)}")
            self.status_label.setStyleSheet("color: #d32f2f; font: 13px 'Segoe UI'; background-color: #FFEBEE; padding: 10px; border-radius: 5px; border: 1px solid #EF9A9A;")
            QMessageBox.critical(self, "Error", f"Failed to load file:\n{str(e)}")

    def clear_input_frame(self):
        for i in reversed(range(self.input_layout.count())):
            widget = self.input_layout.itemAt(i).widget()
            if widget:
                widget.deleteLater()
        self.element_checkboxes.clear()
        self.selected_elements.clear()

    def strip_wavelength(self, column_name):
        cleaned = re.split(r'\s+\d+.\d+', column_name)[0].strip()
        return cleaned

    def convert_limit_values(self, value):
        if isinstance(value, str):
            if value.startswith('<'):
                try:
                    return float(value[1:])
                except ValueError:
                    logger.warning(f"Cannot convert limit value: {value}")
                    return float('nan')
            else:
                try:
                    return float(value)
                except ValueError:
                    logger.warning(f"Cannot convert value to float: {value}")
                    return float('nan')
        return value

    def load_oreas_data(self):
        logger.debug("Loading OREAS data from crm_data.db")
        try:
            conn = sqlite3.connect("crm_data.db")
            query = "SELECT * FROM pivot_crm"
            sample_df = pd.read_sql_query(query, conn)
            conn.close()
            if "CRM ID" not in sample_df.columns:
                logger.error("CRM ID column not found in pivot_crm table")
                raise ValueError("CRM ID column not found in pivot_crm table")
            sample_df = sample_df.rename(columns={"CRM ID": "SAMPLE ID"})
            return sample_df
        except Exception as e:
            logger.error(f"Error loading OREAS data: {str(e)}")
            raise

    def update_sheets(self):
        if (self.control_combo.currentText() == "Select Control Sheet" and not self.control_loaded_from_results):
            self.clear_input_frame()
            return
        if not self.oreas_checkbox.isChecked() and self.sample_combo.currentText() == "Select Sample Sheet":
            self.clear_input_frame()
            return
        if not self.oreas_checkbox.isChecked() and self.control_combo.currentText() == self.sample_combo.currentText():
            self.status_label.setText("Error: Control and Sample sheets must be different")
            self.status_label.setStyleSheet("color: #d32f2f; font: 13px 'Segoe UI'; background-color: #FFEBEE; padding: 10px; border-radius: 5px; border: 1px solid #EF9A9A;")
            self.clear_input_frame()
            return

        try:
            # ========================================
            # 1. Load Control Data
            # ========================================
            if self.control_loaded_from_results:
                control_df = self.control_df.copy()
                control_headers = control_df.columns.tolist()
                logger.debug("Control data loaded from ResultsFrame")
            else:
                self.control_sheet = self.control_combo.currentText()
                logger.debug(f"Reading control sheet: {self.control_sheet}")
                control_df = pd.read_excel(self.file_path, sheet_name=self.control_sheet, header=None)
                control_headers = pd.read_excel(self.file_path, sheet_name=self.control_sheet, nrows=1).columns.tolist()
                first_row = control_df.iloc[0].astype(str).str.lower()
                start_row = 3 if first_row.str.contains('ppm', case=False, na=False).any() else 0
                control_df = control_df.iloc[start_row:].reset_index(drop=True)
                control_df.columns = control_headers

            if "SAMPLE ID" not in control_headers:
                raise ValueError("SAMPLE ID column not found in control sheet!")

            cols = ["SAMPLE ID"]
            if "ESI CODE" in control_headers:
                cols.append("ESI CODE")
            cols += [col for col in control_headers if col not in ["SAMPLE ID", "ESI CODE"]]
            control_df = control_df[cols]

            # ========================================
            # 2. Load Sample Data (OREAS or File)
            # ========================================
            if self.oreas_checkbox.isChecked():
                sample_df_raw = self.load_oreas_data()
                sample_headers = sample_df_raw.columns.tolist()
                self.sample_sheet = "OREAS pivot_crm"

                # ساخت نگاشت: عنصر → تمام طول موج‌های آن در control_df
                self.control_element_map = {}
                for col in control_df.columns:
                    if col not in ["SAMPLE ID", "ESI CODE"]:
                        elem = self.strip_wavelength(col)
                        if elem not in self.control_element_map:
                            self.control_element_map[elem] = []
                        self.control_element_map[elem].append(col)

                logger.debug(f"Control element map: {self.control_element_map}")

                # گسترش OREAS: تکرار مقدار زیر هر طول موج
                expanded_rows = []
                for _, oreas_row in sample_df_raw.iterrows():
                    oreas_id = oreas_row["SAMPLE ID"]
                    new_row = {"SAMPLE ID": oreas_id}
                    if "ESI CODE" in sample_headers:
                        new_row["ESI CODE"] = oreas_row.get("ESI CODE", "")
                    for elem, wl_cols in self.control_element_map.items():
                        oreas_val = oreas_row.get(elem, pd.NA)
                        for wl_col in wl_cols:
                            new_row[wl_col] = oreas_val
                    expanded_rows.append(new_row)

                sample_df = pd.DataFrame(expanded_rows)
                logger.debug(f"Expanded OREAS DataFrame: {sample_df.shape}")
            else:
                self.sample_sheet = self.sample_combo.currentText()
                sample_df = pd.read_excel(self.file_path, sheet_name=self.sample_sheet, header=None)
                sample_headers = pd.read_excel(self.file_path, sheet_name=self.sample_sheet, nrows=1).columns.tolist()
                first_row = sample_df.iloc[0].astype(str).str.lower()
                start_row = 3 if first_row.str.contains('ppm', case=False, na=False).any() else 0
                sample_df = sample_df.iloc[start_row:].reset_index(drop=True)
                sample_df.columns = sample_headers

                if "SAMPLE ID" not in sample_headers:
                    raise ValueError("SAMPLE ID column not found in sample sheet!")

                cols = ["SAMPLE ID"]
                if "ESI CODE" in sample_headers:
                    cols.append("ESI CODE")
                cols += [col for col in sample_headers if col not in ["SAMPLE ID", "ESI CODE"]]
                sample_df = sample_df[cols]

            # ========================================
            # 3. ESI CODE Detection
            # ========================================
            self.has_esi_code = "ESI CODE" in control_df.columns or "ESI CODE" in sample_df.columns
            esi_offset = 1 if self.has_esi_code else 0

            # ========================================
            # 4. Column Mapping & Common Columns
            # ========================================
            control_columns = [col for col in control_df.columns[1 + esi_offset:]]
            sample_columns = [col for col in sample_df.columns[1 + esi_offset:]]

            self.control_col_map = {col: col for col in control_columns}
            self.sample_col_map = {col: col for col in sample_columns}

            common_columns = list(set(control_columns) & set(sample_columns))
            if not common_columns:
                raise ValueError("No common wavelength columns found!")

            logger.debug(f"Common wavelength columns: {len(common_columns)}")

            # ========================================
            # 5. Convert to Numeric
            # ========================================
            self.all_numeric_columns = []
            self.non_numeric_columns = []
            for col in common_columns:
                control_df[col] = control_df[col].apply(self.convert_limit_values)
                sample_df[col] = sample_df[col].apply(self.convert_limit_values)

                numeric_control = pd.to_numeric(control_df[col], errors='coerce')
                numeric_sample = pd.to_numeric(sample_df[col], errors='coerce')

                if numeric_control.dropna().empty or numeric_sample.dropna().empty:
                    self.non_numeric_columns.append(col)
                else:
                    control_df[col] = numeric_control
                    sample_df[col] = numeric_sample
                    self.all_numeric_columns.append(col)

            # ========================================
            # 6. Finalize
            # ========================================
            self.control_df = control_df
            self.sample_df = sample_df
            self.headers = self.non_numeric_columns + self.all_numeric_columns

            # ========================================
            # 7. Update UI
            # ========================================
            self.create_element_selection()
            self.status_label.setText(
                f"Loaded: {len(control_df)} control rows, {len(sample_df)} OREAS rows, "
                f"{len(self.all_numeric_columns)} wavelength columns ready."
            )
            self.status_label.setStyleSheet("color: #2e7d32; font: 13px 'Segoe UI'; background-color: #E8F5E9; padding: 10px; border-radius: 5px; border: 1px solid #A5D6A7;")

        except Exception as e:
            logger.error(f"Error in update_sheets: {str(e)}")
            if "Worksheet named '' not found" in str(e):
                return
            self.status_label.setText(f"Error: {str(e)}")
            self.status_label.setStyleSheet("color: #d32f2f; font: 13px 'Segoe UI'; background-color: #FFEBEE; padding: 10px; border-radius: 5px; border: 1px solid #EF9A9A;")
            QMessageBox.critical(self, "Error", f"Failed to process sheets:\n{str(e)}")

    def create_element_selection(self):
        self.clear_input_frame()

        # فقط عناصر پایه (مثل Al, Fe)
        base_elements = sorted(set(self.strip_wavelength(col) for col in self.all_numeric_columns))

        selection_group = QGroupBox("Select Elements")
        selection_group.setStyleSheet("QGroupBox { background-color: #FFFFFF; border-radius: 6px; }")
        selection_layout = QGridLayout()
        selection_layout.setSpacing(10)
        selection_layout.setContentsMargins(10, 10, 10, 10)

        for idx, elem in enumerate(base_elements):
            row = idx // 10
            col_idx = idx % 10
            checkbox = QCheckBox(elem)
            checkbox.setChecked(elem in self.default_elements)
            checkbox.stateChanged.connect(self.update_selected_elements)
            self.element_checkboxes[elem] = checkbox
            selection_layout.addWidget(checkbox, row, col_idx)

        selection_group.setLayout(selection_layout)
        self.input_layout.addWidget(selection_group)

        # Parameters
        params_group = QGroupBox("Parameters")
        params_layout = QHBoxLayout()
        self.num_matches_input = QLineEdit("5")
        self.min_value_input = QLineEdit("50")
        params_layout.addWidget(QLabel("Number of Matching Samples:"))
        params_layout.addWidget(self.num_matches_input)
        params_layout.addWidget(QLabel("Minimum Value Threshold:"))
        params_layout.addWidget(self.min_value_input)
        params_layout.addStretch()
        params_group.setLayout(params_layout)
        self.input_layout.addWidget(params_group)

        self.input_layout.addStretch()
        self.update_selected_elements()

    def update_selected_elements(self):
        self.selected_elements = [col for col, cb in self.element_checkboxes.items() if cb.isChecked()]
        logger.debug(f"Selected elements (base): {self.selected_elements}")

    def perform_filtering(self):
        logger.debug("Starting filtering")
        self.status_label.setText("Filtering...")
        self.status_label.setStyleSheet("color: #ff9800; font: 13px 'Segoe UI'; background-color: #FFF3E0; padding: 10px; border-radius: 5px; border: 1px solid #FFE082;")
        if self.control_df is None or self.sample_df is None or self.control_df.empty or self.sample_df.empty:
            logger.error("Control or sample data not loaded or empty")
            self.status_label.setText("Error: Control or sample data not loaded or empty")
            self.status_label.setStyleSheet("color: #d32f2f; font: 13px 'Segoe UI'; background-color: #FFEBEE; padding: 10px; border-radius: 5px; border: 1px solid #EF9A9A;")
            QMessageBox.critical(self, "Error", "Control or sample data not loaded or empty!")
            return
        if not self.selected_elements:
            logger.error("No elements selected for filtering")
            self.status_label.setText("Error: No elements selected")
            self.status_label.setStyleSheet("color: #d32f2f; font: 13px 'Segoe UI'; background-color: #FFEBEE; padding: 10px; border-radius: 5px; border: 1px solid #EF9A9A;")
            QMessageBox.critical(self, "Error", "Please select at least one element for filtering!")
            return
        try:
            num_matches = int(self.num_matches_input.text())
            min_value = float(self.min_value_input.text())
        except ValueError:
            logger.error("Invalid input for number of matches or min value")
            self.status_label.setText("Error: Invalid number of matches or min value")
            self.status_label.setStyleSheet("color: #d32f2f; font: 13px 'Segoe UI'; background-color: #FFEBEE; padding: 10px; border-radius: 5px; border: 1px solid #EF9A9A;")
            QMessageBox.critical(self, "Error", "Please enter valid numbers for matching samples and min value!")
            return

        self.progress_dialog = QProgressDialog("Performing filtering...", "Cancel", 0, 100, self)
        self.progress_dialog.setWindowTitle("Filtering Progress")
        self.progress_dialog.setWindowModality(Qt.WindowModality.WindowModal)
        self.progress_dialog.setAutoClose(True)
        self.progress_dialog.canceled.connect(self.cancel_filtering)

        self.thread = FilterThread(
            self.control_df, self.sample_df, self.control_col_map, self.sample_col_map,
            self.control_element_map, self.selected_elements, num_matches, min_value
        )
        self.thread.progress.connect(self.progress_dialog.setValue)
        self.thread.finished.connect(self.on_filtering_finished)
        self.thread.error.connect(self.on_filtering_error)
        self.thread.start()

    def cancel_filtering(self):
        if hasattr(self, 'thread') and self.thread.isRunning():
            self.thread.terminate()
            self.status_label.setText("Filtering cancelled")
            self.status_label.setStyleSheet("color: #6c757d; font: 13px 'Segoe UI'; background-color: #E3F2FD; padding: 10px; border-radius: 5px; border: 1px solid #BBDEFB;")
            self.progress_dialog.close()

    def on_filtering_finished(self, match_data):
        self.progress_dialog.close()
        self.show_results_window(match_data)
        self.status_label.setText("Filtering completed")
        self.status_label.setStyleSheet("color: #2e7d32; font: 13px 'Segoe UI'; background-color: #E8F5E9; padding: 10px; border-radius: 5px; border: 1px solid #A5D6A7;")

    def on_filtering_error(self, error_msg):
        self.progress_dialog.close()
        logger.error(f"Filtering error: {error_msg}")
        self.status_label.setText(f"Error: {error_msg}")
        self.status_label.setStyleSheet("color: #d32f2f; font: 13px 'Segoe UI'; background-color: #FFEBEE; padding: 10px; border-radius: 5px; border: 1px solid #EF9A9A;")
        QMessageBox.critical(self, "Error", f"Filtering failed:\n{error_msg}")

    def show_results_window(self, match_data):
        window = QWidget()
        window.setWindowTitle("Filtering Results")
        window.setStyleSheet("""
            QWidget { background-color: #FFFFFF; }
            QTableView { background-color: white; gridline-color: #E0E0E0; font: 12px 'Segoe UI'; border: 1px solid #E0E0E0; }
            QHeaderView::section { background-color: #ECEFF1; font: bold 13px 'Segoe UI'; border: 1px solid #E0E0E0; padding: 8px; }
            QTableView::item:selected { background-color: #BBDEFB; color: black; }
        """)
        window.setMinimumSize(1000, 700)
        layout = QVBoxLayout(window)
        layout.setSpacing(15)
        layout.setContentsMargins(15, 15, 15, 15)

        header_label = QLabel("Filtering Results")
        header_label.setStyleSheet("font: bold 16px 'Segoe UI'; color: #2E7D32; margin-bottom: 10px;")
        layout.addWidget(header_label, alignment=Qt.AlignmentFlag.AlignCenter)

        button_frame = QFrame()
        button_layout = QHBoxLayout(button_frame)
        button_layout.setSpacing(10)

        export_button = QPushButton("Export")
        export_button.setObjectName("exportButton")
        export_button.setFixedWidth(120)
        export_button.clicked.connect(lambda: self.export_report(match_data))
        button_layout.addWidget(export_button)

        correct_button = QPushButton("Correct")
        correct_button.setObjectName("correctButton")
        correct_button.setFixedWidth(120)
        correct_button.clicked.connect(lambda: self.correct_values(window, match_data))
        button_layout.addWidget(correct_button)

        calculate_error_button = QPushButton("Calculate Error")
        calculate_error_button.setObjectName("calculateErrorButton")
        calculate_error_button.setFixedWidth(130)
        calculate_error_button.clicked.connect(lambda: self.calculate_error(window))
        button_layout.addWidget(calculate_error_button)

        button_layout.addStretch()
        layout.addWidget(button_frame)

        self.overall_avg_label = QLabel("Overall Average Error: 0.00")
        self.overall_avg_label.setStyleSheet("font: bold 14px 'Segoe UI'; color: #D32F2F;")
        layout.addWidget(self.overall_avg_label)

        # ========================================
        # مرتب‌سازی ستون‌های عنصر + طول موج
        # ========================================
        def sort_key(col):
            match = re.match(r"([A-Za-z]+)\s+(\d+\.\d+)", col)
            if not match:
                return (col, 0)
            elem = match.group(1).upper()
            wavelength = float(match.group(2))
            return (elem, wavelength)

        sorted_numeric_columns = sorted(self.all_numeric_columns, key=sort_key)

        # ========================================
        # ساخت هدرها
        # ========================================
        headers = ["Type", "ID"]
        if self.has_esi_code:
            headers.append("ESI CODE")
        headers.append("Mismatched Elements")
        headers.append("Similarity (%)")
        headers.extend(sorted_numeric_columns)

        model = QStandardItemModel(0, len(headers))
        model.setHorizontalHeaderLabels(headers)

        # رنگ‌آمیزی هدر عناصر انتخاب‌شده
        for col_idx, header in enumerate(headers):
            item = QStandardItem(header)
            base_elem = self.strip_wavelength(header)
            if base_elem in self.selected_elements:
                item.setForeground(QBrush(QColor("#000000")))
            elif header in sorted_numeric_columns:
                item.setForeground(QBrush(QColor("#A0A0A0")))
            model.setHorizontalHeaderItem(col_idx, item)

        table = FreezeTableWidget(model, window)
        table.horizontalHeader().setSectionResizeMode(QHeaderView.ResizeMode.ResizeToContents)
        table.horizontalHeader().setStretchLastSection(True)
        table.setSelectionBehavior(QAbstractItemView.SelectionBehavior.SelectRows)
        table.setSelectionMode(QAbstractItemView.SelectionMode.ExtendedSelection)
        table.verticalHeader().setDefaultSectionSize(30)

        # --- ادامه پر کردن جدول (همان کد قبلی، فقط با sorted_numeric_columns) ---
        for m_idx, match in enumerate(match_data):
            control_row = match["Control Row"]
            top_samples = match["Matching Samples"]

            # Control row
            items = [QStandardItem("Control"), QStandardItem(str(match["Control ID"]))]
            if self.has_esi_code:
                esi_code = control_row.get("ESI CODE", "")
                items.append(QStandardItem(str(esi_code)))
            items.append(QStandardItem(""))
            items.append(QStandardItem(""))
            for col in sorted_numeric_columns:
                val = control_row.get(self.control_col_map.get(col, col), "")
                val_str = f"{val:.2f}" if isinstance(val, (int, float)) and not pd.isna(val) else str(val) if not pd.isna(val) else ""
                items.append(QStandardItem(val_str))
            for item in items:
                item.setTextAlignment(Qt.AlignmentFlag.AlignCenter)
                item.setBackground(QBrush(QColor("#E8F5E9")))
            model.appendRow(items)

            # Sample + Error rows
            for _, sample_row, avg_diff, mismatched in top_samples:
                # Sample
                items = [QStandardItem("Sample"), QStandardItem(str(sample_row["SAMPLE ID"]))]
                if self.has_esi_code:
                    esi_code = sample_row.get("ESI CODE", "")
                    items.append(QStandardItem(str(esi_code)))
                mismatched_str = ", ".join(mismatched) if mismatched else "None"
                items.append(QStandardItem(mismatched_str))
                similarity = (1 - avg_diff) * 100 if avg_diff is not None else 0
                items.append(QStandardItem(f"{similarity:.2f}%"))
                for col in sorted_numeric_columns:
                    val = sample_row.get(self.sample_col_map.get(col, col), "")
                    val_str = f"{val:.2f}" if isinstance(val, (int, float)) and not pd.isna(val) else str(val) if not pd.isna(val) else ""
                    items.append(QStandardItem(val_str))
                for item in items:
                    item.setTextAlignment(Qt.AlignmentFlag.AlignCenter)
                    item.setBackground(QBrush(QColor("#E3F2FD")))
                model.appendRow(items)

                # Error row
                error_items = [QStandardItem("Error"), QStandardItem(str(sample_row["SAMPLE ID"]))]
                if self.has_esi_code:
                    error_items.append(QStandardItem(""))
                error_items.append(QStandardItem(""))
                error_items.append(QStandardItem(""))
                for _ in sorted_numeric_columns:
                    error_items.append(QStandardItem(""))
                for item in error_items:
                    item.setTextAlignment(Qt.AlignmentFlag.AlignCenter)
                    item.setBackground(QBrush(QColor("#FFEBEE")))
                model.appendRow(error_items)

            # Blank row
            if m_idx < len(match_data) - 1:
                blank_items = [QStandardItem("") for _ in headers]
                for item in blank_items:
                    item.setTextAlignment(Qt.AlignmentFlag.AlignCenter)
                    item.setBackground(QBrush(QColor("#FFFFFF")))
                model.appendRow(blank_items)

        table.resizeColumnsToContents()
        table.resizeRowsToContents()
        layout.addWidget(table)
        logger.debug(f"Results table created with {model.rowCount()} rows")
        self.results_windows.append(window)
        window.show()
        self.calculate_error(window)

    def calculate_error(self, dialog):
        table = dialog.findChild(QTableView)
        if not table:
            return
        model = table.model()
        row_count = model.rowCount()
        esi_offset = 1 if self.has_esi_code else 0
        start_col = 2 + esi_offset
        mismatched_col = start_col
        similarity_col = mismatched_col + 1
        numeric_start_col = similarity_col + 1
        column_sums = np.zeros(len(self.all_numeric_columns), dtype=float)
        total_errors = []

        i = 0
        while i < row_count:
            if model.item(i, 0).text() == "Control":
                control_row = i
                sample_rows = []
                error_rows = []
                i += 1
                while i < row_count and model.item(i, 0).text() in ["Sample", "Error"]:
                    if model.item(i, 0).text() == "Sample":
                        sample_rows.append(i)
                    elif model.item(i, 0).text() == "Error":
                        error_rows.append(i)
                    i += 1
                control_vals = []
                for col_idx in range(numeric_start_col, numeric_start_col + len(self.all_numeric_columns)):
                    control_item = model.item(control_row, col_idx)
                    try:
                        control_val = float(control_item.text() or '0')
                    except (ValueError, AttributeError):
                        control_val = 0.0
                    control_vals.append(control_val)

                for s_row, e_row in zip(sample_rows, error_rows):
                    sample_vals = []
                    for col_idx in range(numeric_start_col, numeric_start_col + len(self.all_numeric_columns)):
                        sample_item = model.item(s_row, col_idx)
                        try:
                            sample_val = float(sample_item.text() or '0')
                        except (ValueError, AttributeError):
                            sample_val = 0.0
                        sample_vals.append(sample_val)

                    control_vals_np = np.array(control_vals, dtype=float)
                    sample_vals_np = np.array(sample_vals, dtype=float)
                    sum_vals = control_vals_np + sample_vals_np
                    try:
                        min_value = float(self.min_value_input.text())
                    except ValueError:
                        min_value = 50.0
                    valid_mask = (sum_vals != 0) & (control_vals_np >= min_value)
                    differences = np.zeros_like(control_vals_np)
                    differences[valid_mask] = np.abs(control_vals_np[valid_mask] - sample_vals_np[valid_mask]) / np.abs(sum_vals[valid_mask]) * 100

                    for idx, (col_idx, diff) in enumerate(zip(range(numeric_start_col, numeric_start_col + len(self.all_numeric_columns)), differences)):
                        if valid_mask[idx]:
                            item = QStandardItem(f"{diff:.2f}")
                            if diff > 20:
                                item.setForeground(QBrush(QColor("red")))
                            column_sums[idx] += diff
                            total_errors.append(diff)
                        else:
                            item = QStandardItem("")
                        item.setTextAlignment(Qt.AlignmentFlag.AlignCenter)
                        item.setBackground(QBrush(QColor("#FFEBEE")))
                        model.setItem(e_row, col_idx, item)

            else:
                i += 1

        overall_avg = np.mean(total_errors) if total_errors else 0
        self.overall_avg_label.setText(f"Overall Average Error: {overall_avg:.2f}")
        table.viewport().update()

    def correct_values(self, window, match_data):
        table = window.findChild(QTableView)
        if not table:
            return
        selected_rows = table.selectionModel().selectedRows()
        if not selected_rows:
            QMessageBox.warning(window, "Warning", "No sample rows selected for correction!")
            return

        model = table.model()
        numeric_start_col = 4 + (1 if self.has_esi_code else 0)
        min_value = float(self.min_value_input.text()) if self.min_value_input.text().strip() else 50.0
        correction_threshold = 0.05
        max_acceptable_error = 0.20

        new_match_data = match_data.copy()
        updates_to_send = {}  # {control_id: {col: new_val, ...}}

        for sel_row in selected_rows:
            row_idx = sel_row.row()
            row_type = model.item(row_idx, 0).text()
            if row_type != "Sample":
                continue

            sample_id = model.item(row_idx, 1).text()  # OREAS ID

            # پیدا کردن ردیف Control
            control_row_idx = row_idx
            while control_row_idx >= 0 and model.item(control_row_idx, 0).text() != "Control":
                control_row_idx -= 1
            if control_row_idx < 0:
                continue

            control_id = model.item(control_row_idx, 1).text()  # این ID اصلی است
            if control_id not in updates_to_send:
                updates_to_send[control_id] = {}

            # پیدا کردن match
            current_match = None
            sample_index = -1
            for match in new_match_data:
                if str(match["Control ID"]) == control_id:
                    for i, (sid, _, _, _) in enumerate(match["Matching Samples"]):
                        if str(sid) == sample_id:
                            current_match = match
                            sample_index = i
                            break
                    if current_match:
                        break
            if not current_match:
                continue

            control_row = current_match["Control Row"]
            old_sample_row = current_match["Matching Samples"][sample_index][1]
            new_sample_row = old_sample_row.copy()

            valid_rel_diffs = []
            correction_rel_diffs = []
            new_mismatched = set()
            has_any_match = False

            for col in self.all_numeric_columns:
                base_elem = self.strip_wavelength(col)
                if base_elem not in self.selected_elements:
                    continue

                control_col = self.control_col_map.get(col, col)
                sample_col = self.sample_col_map.get(col, col)

                try:
                    control_val = float(control_row.get(control_col, 0))
                    sample_val = float(old_sample_row.get(sample_col, 0))
                except (ValueError, TypeError):
                    continue

                if pd.isna(control_val) or pd.isna(sample_val) or control_val < min_value or control_val == 0:
                    continue

                rel_error = abs(control_val - sample_val) / control_val

                if rel_error <= max_acceptable_error:
                    has_any_match = True
                    valid_rel_diffs.append(rel_error)

                if rel_error > correction_threshold:
                    correction_factor = 1.0 + random.uniform(-0.03, 0.03)
                    new_sample_val = control_val * correction_factor
                    new_sample_row[sample_col] = new_sample_val

                    # به‌روزرسانی جدول Compare
                    col_idx = numeric_start_col + self.all_numeric_columns.index(col)
                    model.setItem(row_idx, col_idx, QStandardItem(f"{new_sample_val:.2f}"))

                    # ذخیره برای ارسال به ResultsFrame (با Control ID)
                    updates_to_send[control_id][col] = new_sample_val

                    new_rel_error = abs(control_val - new_sample_val) / control_val
                    correction_rel_diffs.append(new_rel_error)
                    new_mismatched.add(base_elem)
                else:
                    correction_rel_diffs.append(rel_error)

            # تشخیص عدم تطابق
            if not has_any_match:
                new_avg_diff = 1.0
                similarity = 0.0
                mismatched_str = "All"
            else:
                new_avg_diff = sum(correction_rel_diffs) / len(correction_rel_diffs)
                similarity = (1 - new_avg_diff) * 100
                mismatched_str = ", ".join(new_mismatched) if new_mismatched else "None"

            mismatched_col = 2 + (1 if self.has_esi_code else 0)
            similarity_col = mismatched_col + 1
            model.setItem(row_idx, mismatched_col, QStandardItem(mismatched_str))
            model.setItem(row_idx, similarity_col, QStandardItem(f"{similarity:.2f}%" if has_any_match else "—"))

            current_match["Matching Samples"][sample_index] = (
                sample_id, new_sample_row, new_avg_diff, list(new_mismatched) if has_any_match else ["All"]
            )

        # مرتب‌سازی
        for match in new_match_data:
            match["Matching Samples"].sort(key=lambda x: (x[2], len(x[3])))
        new_match_data.sort(key=lambda m: min(s[2] for s in m["Matching Samples"]) if m["Matching Samples"] else float('inf'))

        window.close()
        self.show_results_window(new_match_data)

        # ارسال به‌روزرسانی به ResultsFrame
        if updates_to_send:
            self.update_results.emit(updates_to_send)
            updated_count = sum(len(v) for v in updates_to_send.values())
            QMessageBox.information(window, "Success", f"Correction applied!\n{updated_count} cell(s) updated in Results (using Control ID).")
        else:
            QMessageBox.information(window, "Info", "No changes to apply.")

    def export_report(self, match_data):
        file_path, _ = QFileDialog.getSaveFileName(self, "Save Report", "", "Excel files (*.xlsx)")
        if not file_path:
            return
        try:
            workbook = Workbook(file_path)
            worksheet = workbook.add_worksheet()
            bold_format = workbook.add_format({'bold': True})
            control_format = workbook.add_format({'bg_color': '#E8F5E9', 'align': 'center'})
            sample_format = workbook.add_format({'bg_color': '#E3F2FD', 'align': 'center'})
            error_format = workbook.add_format({'bg_color': '#FFEBEE', 'align': 'center'})
            blank_format = workbook.add_format({'bg_color': '#FFFFFF'})
            red_format = workbook.add_format({'font_color': 'red', 'bg_color': '#FFEBEE', 'align': 'center'})

            headers = ["Type", "ID"]
            if self.has_esi_code:
                headers.append("ESI CODE")
            headers.append("Mismatched Elements")
            headers.append("Similarity (%)")
            headers.extend(self.all_numeric_columns)
            for col, header in enumerate(headers):
                worksheet.write(0, col, header, bold_format)

            row_idx = 1
            for match in match_data:
                control_row = match["Control Row"]
                worksheet.write(row_idx, 0, "Control", control_format)
                worksheet.write(row_idx, 1, str(match["Control ID"]), control_format)
                col_offset = 2
                if self.has_esi_code:
                    esi_code = control_row.get("ESI CODE", "")
                    worksheet.write(row_idx, col_offset, str(esi_code), control_format)
                    col_offset += 1
                worksheet.write(row_idx, col_offset, "", control_format)
                worksheet.write(row_idx, col_offset + 1, "", control_format)
                col_offset += 2
                for col in self.all_numeric_columns:
                    val = control_row.get(self.control_col_map.get(col, col), "")
                    val_str = f"{val:.2f}" if isinstance(val, (int, float)) and not pd.isna(val) else str(val) if not pd.isna(val) else ""
                    worksheet.write(row_idx, col_offset, val_str, control_format)
                    col_offset += 1
                row_idx += 1

                for _, sample_row, avg_diff, mismatched in match["Matching Samples"]:
                    worksheet.write(row_idx, 0, "Sample", sample_format)
                    worksheet.write(row_idx, 1, str(sample_row["SAMPLE ID"]), sample_format)
                    col_offset = 2
                    if self.has_esi_code:
                        esi_code = sample_row.get("ESI CODE", "")
                        worksheet.write(row_idx, col_offset, str(esi_code), sample_format)
                        col_offset += 1
                    mismatched_str = ", ".join(mismatched) if mismatched else "None"
                    worksheet.write(row_idx, col_offset, mismatched_str, sample_format)
                    similarity = (1 - avg_diff) * 100 if avg_diff is not None else 0
                    worksheet.write(row_idx, col_offset + 1, f"{similarity:.2f}%", sample_format)
                    col_offset += 2
                    for col in self.all_numeric_columns:
                        val = sample_row.get(self.sample_col_map.get(col, col), "")
                        val_str = f"{val:.2f}" if isinstance(val, (int, float)) and not pd.isna(val) else str(val) if not pd.isna(val) else ""
                        worksheet.write(row_idx, col_offset, val_str, sample_format)
                        col_offset += 1
                    row_idx += 1

                    worksheet.write(row_idx, 0, "Error", error_format)
                    worksheet.write(row_idx, 1, str(sample_row["SAMPLE ID"]), error_format)
                    col_offset = 2
                    if self.has_esi_code:
                        worksheet.write(row_idx, col_offset, "", error_format)
                        col_offset += 1
                    worksheet.write(row_idx, col_offset, "", error_format)
                    worksheet.write(row_idx, col_offset + 1, "", error_format)
                    col_offset += 2
                    for _ in self.all_numeric_columns:
                        worksheet.write(row_idx, col_offset, "", error_format)
                        col_offset += 1
                    row_idx += 1

                for col in range(len(headers)):
                    worksheet.write(row_idx, col, "", blank_format)
                row_idx += 1

            for col in range(len(headers)):
                worksheet.set_column(col, col, 15)
            workbook.close()
            QMessageBox.information(self, "Success", f"Report exported to {file_path}")
        except Exception as e:
            logger.error(f"Error exporting report: {str(e)}")
            QMessageBox.critical(self, "Error", f"Failed to export report:\n{str(e)}")

    def set_control_from_results(self, df):
        self.control_loaded_from_results = True
        self.control_df = df.copy()
        self.control_sheet = "From Results"
        self.oreas_checkbox.setChecked(True)
        self.sample_combo.setEnabled(False)
        self.sample_combo.setCurrentText("OREAS")
        self.update_sheets()
        self.control_loaded_from_results = False