# screens/process/changes_report.py
from PyQt6.QtWidgets import (
    QDialog, QVBoxLayout, QHBoxLayout, QPushButton, QTableView, QHeaderView,
    QMessageBox, QComboBox, QLabel, QProgressDialog
)
from PyQt6.QtCore import Qt, QThread, pyqtSignal
from PyQt6.QtGui import QStandardItemModel, QStandardItem
import numpy as np
import pandas as pd
import logging
import sqlite3
import os  # برای clean_filename

# Setup logging
logging.basicConfig(level=logging.DEBUG, format="%(asctime)s - %(levelname)s - %(message)s")
logger = logging.getLogger(__name__)


class ReportGenerationThread(QThread):
    """Thread for generating the changes report in the background."""
    progress = pyqtSignal(int)
    finished = pyqtSignal(list)  # Emits list of row items for the table
    error = pyqtSignal(str)

    def __init__(self, app, results_frame, selected_column):
        super().__init__()
        self.app = app
        self.results_frame = results_frame
        self.selected_column = selected_column

    def get_original_value(self, solution_label, original_column):
        data = self.app.pivot_tab.original_df
        if data is None or data.empty:
            return np.nan

        condition = (data['Solution Label'] == solution_label) & \
                    (data['Element'] == original_column) & \
                    (data['Type'].isin(['Samp', 'Sample']))

        matching_rows = data[condition]
        if matching_rows.empty:
            return np.nan

        raw_value = matching_rows['Corr Con'].iloc[0]

        if pd.isna(raw_value) or str(raw_value).strip() in ['', '-', 'N/A', '<LOD>', 'ND']:
            return np.nan

        try:
            return float(raw_value)
        except (ValueError, TypeError):
            return np.nan

    def get_new_value(self, solution_label, selected_column):
        data = self.results_frame.last_filtered_data
        if data is None or data.empty:
            return np.nan

        condition = data['Solution Label'] == solution_label
        matching_rows = data[condition]
        if matching_rows.empty or selected_column not in matching_rows.columns:
            return np.nan

        raw_value = matching_rows[selected_column].iloc[0]

        if pd.isna(raw_value) or str(raw_value).strip() in ['', '-', 'N/A', '<LOD>', 'ND']:
            return np.nan

        try:
            return float(raw_value)
        except (ValueError, TypeError):
            return np.nan

    def map_column_to_original(self, column):
        return column

    def get_weight_corrections(self):
        corrections = {}
        if hasattr(self.app, 'weight_check') and hasattr(self.app.weight_check, 'corrected_weights'):
            corrections = self.app.weight_check.corrected_weights.copy()
        return corrections

    def get_volume_corrections(self):
        corrections = {}
        if hasattr(self.app, 'volume_check') and hasattr(self.app.volume_check, 'corrected_volumes'):
            for sl, params in self.app.volume_check.corrected_volumes.items():
                if 'old_volume' in params and 'new_volume' in params:
                    corrections[sl] = {'old_volume': params['old_volume'], 'new_volume': params['new_volume']}
        return corrections

    def get_df_corrections(self):
        corrections = {}
        if hasattr(self.app, 'df_check') and hasattr(self.app.df_check, 'corrected_dfs'):
            corrections = self.app.df_check.corrected_dfs.copy()
        return corrections

    def get_crm_corrections(self, column):
        corrections = {}
        if hasattr(self.app.crm_check, 'corrected_crm') and column in self.app.crm_check.corrected_crm:
            corrections = self.app.crm_check.corrected_crm[column].copy()
        return corrections

    def get_drift_corrections(self, column):
        corrections = {}
        if hasattr(self.app.rm_check, 'corrected_drift'):
            for (sl, elem), factor in self.app.rm_check.corrected_drift.items():
                if elem == column or elem == self.map_column_to_original(column):
                    corrections[sl] = factor
        return corrections

    def run(self):
        try:
            # اولویت: last_filtered_data، fallback: get_data()
            if (hasattr(self.results_frame, 'last_filtered_data') and 
                self.results_frame.last_filtered_data is not None and 
                not self.results_frame.last_filtered_data.empty):
                data = self.results_frame.last_filtered_data
            else:
                data = self.app.get_data()
                if data is None or data.empty:
                    self.error.emit("No pivoted data available!")
                    return

            weight_corrections = self.get_weight_corrections()
            volume_corrections = self.get_volume_corrections()
            df_corrections = self.get_df_corrections()
            crm_corrections = self.get_crm_corrections(self.selected_column)
            drift_corrections = self.get_drift_corrections(self.selected_column)

            original_column = self.map_column_to_original(self.selected_column)

            rows = []
            total_rows = len(data)
            for i, (_, row) in enumerate(data.iterrows()):
                sl = str(row['Solution Label']).strip()
                orig_val = self.get_original_value(sl, original_column)
                new_val = self.get_new_value(sl, self.selected_column)

                wp = weight_corrections.get(sl, {})
                weight_text = f"Old: {wp.get('old_weight', ''):.3f}, New: {wp.get('new_weight', ''):.3f}" if wp else ""

                vp = volume_corrections.get(sl, {})
                volume_text = f"Old: {vp.get('old_volume', ''):.3f}, New: {vp.get('new_volume', ''):.3f}" if vp else ""

                dfp = df_corrections.get(sl, {})
                df_text = f"Old: {dfp.get('old_df', ''):.3f}, New: {dfp.get('new_df', ''):.3f}" if dfp else ""

                cp = crm_corrections.get(sl, {})
                crm_text = f"Scale: {cp.get('scale', ''):.3f}, Blank: {cp.get('blank', ''):.3f}" if cp else ""

                dp = drift_corrections.get(sl, '')
                drift_text = f"Ratio: {dp:.3f}" if isinstance(dp, (int, float)) and not np.isnan(dp) else ""

                row_items = [
                    sl,
                    f"{orig_val:.3f}" if pd.notna(orig_val) else "N/A",
                    f"{new_val:.3f}" if pd.notna(new_val) else "N/A",
                    weight_text,
                    volume_text,
                    df_text,
                    crm_text,
                    drift_text
                ]
                rows.append(row_items)

                if total_rows > 10:
                    self.progress.emit(int((i + 1) / total_rows * 100))

            self.finished.emit(rows)
        except Exception as e:
            self.error.emit(str(e))


class ChangesReportDialog(QDialog):
    def __init__(self, app, results_frame, parent=None):
        super().__init__(parent)
        self.app = app
        self.results_frame = results_frame
        self.db_path = self.app.resource_path("crm_data.db")
        self.selected_column = None
        self.setup_ui()

    def setup_ui(self):
        self.setStyleSheet("""
            QWidget { font-family: 'Segoe UI', sans-serif; font-size: 13px; background-color: #F5F7FA; }
            QComboBox { padding: 6px; border: 1px solid #D0D7DE; border-radius: 4px; }
            QComboBox::drop-down { width: 20px; }
            QPushButton { background-color: #2E7D32; color: white; padding: 6px 12px; border: none; border-radius: 6px; }
            QPushButton:hover { background-color: #1B5E20; }
            QTableView { background-color: #FFFFFF; gridline-color: #D0D7DE; selection-background-color: #E5E7EB; }
            QHeaderView::section { background-color: #E5E7EB; padding: 4px; border: 1px solid #D0D7DE; }
        """)
        layout = QVBoxLayout(self)
        layout.setContentsMargins(15, 15, 15, 15)
        layout.setSpacing(10)

        top_layout = QHBoxLayout()
        top_layout.addWidget(QLabel("Select Column:"))
        self.column_combo = QComboBox()
        self.column_combo.setMaximumWidth(250)
        top_layout.addWidget(self.column_combo)

        show_button = QPushButton("Show Report")
        show_button.setFixedWidth(120)
        show_button.clicked.connect(self.start_report_generation)
        top_layout.addWidget(show_button)
        top_layout.addStretch()

        layout.addLayout(top_layout)

        self.report_table = QTableView()
        self.report_table.horizontalHeader().setSectionResizeMode(QHeaderView.ResizeMode.Stretch)
        layout.addWidget(self.report_table)

        self.setMinimumSize(1200, 800)
        self.setWindowTitle("Changes Report")

        self.update_column_combo()

    def get_valid_data(self):
        """کمکی برای گرفتن داده معتبر بدون ارزیابی بولین DataFrame"""
        if (hasattr(self.results_frame, 'last_filtered_data') and 
            self.results_frame.last_filtered_data is not None and 
            not self.results_frame.last_filtered_data.empty):
            return self.results_frame.last_filtered_data
        else:
            data = self.app.get_data()
            if data is not None and not data.empty:
                return data
            return None

    def update_column_combo(self):
        """به‌روزرسانی کمبوباکس بدون خطای بولین"""
        data = self.get_valid_data()
        if data is None:
            logger.warning("No data available to populate column combo")
            self.column_combo.clear()
            return

        columns = [col for col in data.columns if col != 'Solution Label']
        self.column_combo.clear()
        self.column_combo.addItems(columns)
        logger.debug(f"Updated column combo with {len(columns)} columns")

    def start_report_generation(self):
        self.selected_column = self.column_combo.currentText()
        if not self.selected_column:
            QMessageBox.warning(self, "Warning", "Please select a column!")
            return

        data = self.get_valid_data()
        if data is None:
            QMessageBox.warning(self, "Warning", "No pivoted data available!")
            return

        self.progress_dialog = QProgressDialog("Generating report...", "Cancel", 0, 100, self)
        self.progress_dialog.setWindowModality(Qt.WindowModality.WindowModal)
        self.progress_dialog.setAutoClose(True)
        self.progress_dialog.setMinimumDuration(0)

        self.thread = ReportGenerationThread(self.app, self.results_frame, self.selected_column)
        self.progress_dialog.canceled.connect(self.thread.terminate)
        self.thread.progress.connect(self.progress_dialog.setValue)
        self.thread.finished.connect(self.on_report_finished)
        self.thread.error.connect(self.on_report_error)
        self.thread.start()

    def on_report_finished(self, rows):
        model = QStandardItemModel()
        headers = [
            "Solution Label", "Original Value", "New Value",
            "Weight Correction", "Volume Correction", "DF Correction",
            "CRM Calibration", "Drift Calibration"
        ]
        model.setHorizontalHeaderLabels(headers)

        for row_items in rows:
            model.appendRow([QStandardItem(str(item)) for item in row_items])

        self.report_table.setModel(model)
        self.report_table.resizeColumnsToContents()
        self.progress_dialog.close()
        QMessageBox.information(self, "Success", f"Report generated with {len(rows)} rows")

        self.save_changes_to_db(rows)

    def clean_filename(self, full_path):
        if not full_path:
            return "Unknown"
        filename = os.path.basename(full_path)
        name, _ = os.path.splitext(filename)
        return name

    def save_changes_to_db(self, rows):
        try:
            conn = sqlite3.connect(self.db_path)
            cursor = conn.cursor()

            clean_file = self.clean_filename(self.app.file_path)

            for row_items in rows:
                cursor.execute('''
                    INSERT INTO changes_log (
                        user_name, user_position, file_path, column_name,
                        solution_label, original_value, new_value,
                        weight_correction, volume_correction, df_correction,
                        crm_calibration, drift_calibration
                    ) VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?)
                ''', (
                    self.app.user_name,
                    self.app.user_position,
                    clean_file,
                    self.selected_column,
                    row_items[0],
                    row_items[1],
                    row_items[2],
                    row_items[3],
                    row_items[4],
                    row_items[5],
                    row_items[6],
                    row_items[7]
                ))
            conn.commit()
            conn.close()
            logger.info(f"Inserted {len(rows)} changes | File: {clean_file}")
        except Exception as e:
            logger.error(f"DB save error: {e}")
            QMessageBox.warning(self, "DB Error", f"Could not save changes: {e}")

    def on_report_error(self, error_msg):
        QMessageBox.warning(self, "Error", f"Failed to generate report: {error_msg}")
        logger.error(f"Report error: {error_msg}")
        self.progress_dialog.close()