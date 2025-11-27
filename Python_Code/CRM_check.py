from PyQt6.QtWidgets import (
    QWidget, QVBoxLayout, QHBoxLayout, QPushButton, QTableView, QHeaderView, 
    QGroupBox, QMessageBox, QLineEdit, QLabel, QComboBox, QFileDialog,
    QDialog, QRadioButton, QCheckBox,QLineEdit
)
from PyQt6.QtCore import Qt, pyqtSignal
import pandas as pd
import re
import logging
from .crm_manager import CRMManager
from ..pivot.freeze_table_widget import FreezeTableWidget
from .pivot_table_model import PivotTableModel
from .pivot_plot_dialog import PivotPlotWindow

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
"""

class CrmCheck(QWidget):
    data_changed = pyqtSignal()

    def __init__(self, app, results_frame, parent=None):
        super().__init__(parent)
        self.app = app
        self.results_frame = results_frame
        self.corrected_crm = {}  # Store CRM corrections: {element: {solution_label: {'scale': scale, 'blank': blank}}}
        self._inline_crm_rows = {}
        self._inline_crm_rows_display = {}
        self.included_crms = {}
        self.column_widths = {}
        self.crm_manager = CRMManager(self)
        self.crm_diff_min = QLineEdit("-12")
        self.crm_diff_max = QLineEdit("12")
        self.current_plot_window = None
        self.setup_ui()
        self.results_frame.app.notify_data_changed = self.on_data_changed
        if hasattr(self.results_frame, 'decimal_combo') and self.results_frame.decimal_combo is not None:
            self.results_frame.decimal_combo.currentTextChanged.connect(self.update_pivot_display)
        else:
            logger.error("decimal_combo is not defined in ResultsFrame")
            raise AttributeError("decimal_combo is not defined in ResultsFrame")

    def setup_ui(self):
        """Set up the UI with styling matching EmptyCheckFrame."""
        self.setStyleSheet(global_style)
        main_layout = QVBoxLayout(self)
        main_layout.setContentsMargins(15, 15, 15, 15)
        main_layout.setSpacing(15)

        # Control group
        control_group = QGroupBox("Pivot Controls")
        control_layout = QHBoxLayout(control_group)
        control_layout.setSpacing(10)

        # Check CRM button
        check_crm_btn = QPushButton("Check CRM")
        check_crm_btn.setMinimumWidth(80)
        check_crm_btn.clicked.connect(self.crm_manager.check_rm)
        control_layout.addWidget(check_crm_btn)

        # Manual CRM button
        manual_crm_btn = QPushButton("Manual CRM")
        manual_crm_btn.setMinimumWidth(80)
        manual_crm_btn.clicked.connect(self.manual_crm_selection)
        control_layout.addWidget(manual_crm_btn)

        # CRM Range input
        crm_range_label = QLabel("CRM Range (%):")
        control_layout.addWidget(crm_range_label)
        self.crm_diff_min.setFixedWidth(50)
        self.crm_diff_min.textChanged.connect(self.validate_crm_diff_range)
        control_layout.addWidget(self.crm_diff_min)
        control_layout.addWidget(QLabel("to"))
        self.crm_diff_max.setFixedWidth(50)
        self.crm_diff_max.textChanged.connect(self.validate_crm_diff_range)
        control_layout.addWidget(self.crm_diff_max)

        # Decimal places input
        decimal_label = QLabel("Decimal Places:")
        control_layout.addWidget(decimal_label)
        if hasattr(self.results_frame, 'decimal_combo') and self.results_frame.decimal_combo is not None:
            self.results_frame.decimal_combo.setFixedWidth(60)
            self.results_frame.decimal_combo.setToolTip("Set the number of decimal places for numeric values")
            control_layout.addWidget(self.results_frame.decimal_combo)
        else:
            logger.warning("decimal_combo not available, creating a local one")
            local_decimal_combo = QComboBox()
            local_decimal_combo.addItems(["0", "1", "2", "3"])
            local_decimal_combo.setCurrentText("1")
            local_decimal_combo.setFixedWidth(60)
            local_decimal_combo.setToolTip("Set the number of decimal places for numeric values")
            local_decimal_combo.currentTextChanged.connect(self.update_pivot_display)
            control_layout.addWidget(local_decimal_combo)

        # Clear CRM button
        clear_crm_btn = QPushButton("Clear CRM")
        clear_crm_btn.setMinimumWidth(80)
        clear_crm_btn.clicked.connect(self.clear_inline_crm)
        control_layout.addWidget(clear_crm_btn)

        # Calib button
        calib_btn = QPushButton("Calib")
        calib_btn.setMinimumWidth(80)
        calib_btn.clicked.connect(self.show_element_plot)
        control_layout.addWidget(calib_btn)

        # Export button
        export_btn = QPushButton("Export")
        export_btn.setMinimumWidth(80)
        export_btn.clicked.connect(self.export_table)
        control_layout.addWidget(export_btn)

        control_layout.addStretch()
        main_layout.addWidget(control_group)

        # Table group
        table_group = QGroupBox("Pivot Table")
        table_layout = QVBoxLayout(table_group)
        table_layout.setSpacing(10)

        # Table view
        self.table_view = FreezeTableWidget(PivotTableModel(self))
        self.table_view.setAlternatingRowColors(True)
        self.table_view.setSelectionBehavior(QTableView.SelectionBehavior.SelectRows)
        self.table_view.setSortingEnabled(True)
        self.table_view.horizontalHeader().setSectionResizeMode(QHeaderView.ResizeMode.Interactive)
        table_layout.addWidget(self.table_view)

        main_layout.addWidget(table_group, stretch=1)

    def manual_crm_selection(self):
        """Open a dialog to manually select a CRM for a selected row."""
        selected_rows = self.table_view.selectionModel().selectedRows()
        if not selected_rows:
            QMessageBox.warning(self, "Warning", "Please select a row to assign a CRM!")
            logger.warning("No row selected for manual CRM assignment")
            return

        if len(selected_rows) > 1:
            QMessageBox.warning(self, "Warning", "Please select only one row!")
            logger.warning("Multiple rows selected for manual CRM assignment")
            return

        row_index = selected_rows[0].row()
        model = self.table_view.model()
        if not model:
            QMessageBox.warning(self, "Warning", "No data available in table!")
            logger.warning("No data available in table for manual CRM")
            return

        solution_label = model.data(model.index(row_index, 0))
        if not solution_label:
            QMessageBox.warning(self, "Warning", "Invalid row selected!")
            logger.warning("Invalid row selected for manual CRM")
            return

        self.crm_manager.open_manual_crm_dialog(solution_label)
        self.update_pivot_display()
        self.data_changed.emit()

    def on_data_changed(self):
        """Update pivot table when data in ResultsFrame changes."""
        logger.debug("Data changed in ResultsFrame, updating pivot display")
        self.update_pivot_display()

    def validate_crm_diff_range(self):
        """Validate CRM difference range inputs and update display."""
        try:
            min_val = float(self.crm_diff_min.text())
            max_val = float(self.crm_diff_max.text())
            if min_val > max_val:
                self.crm_diff_min.setText(str(max_val - 1))
            logger.debug(f"CRM diff range set to {min_val} to {max_val}")
            self.crm_manager.check_rm_with_diff_range(min_val, max_val)
            self.update_pivot_display()
            self.data_changed.emit()
        except ValueError:
            logger.debug("Invalid CRM diff range input, skipping update")
            pass

    def update_pivot_display(self):
        """Update the pivot table display using ResultsFrame's last_filtered_data."""
        logger.debug("Starting update_pivot_display")
        pivot_data = self.results_frame.last_filtered_data

        if pivot_data is None or pivot_data.empty:
            logger.warning("No data loaded for pivot display")
            self.table_view.setModel(None)
            self.table_view.frozenTableView.setModel(None)
            return

        logger.debug(f"Current view data shape: {pivot_data.shape}")
        self._inline_crm_rows_display = self.crm_manager._build_crm_row_lists_for_columns(list(pivot_data.columns))
        combined_rows = []
        for sol_label in pivot_data['Solution Label']:
            if sol_label in self._inline_crm_rows_display:
                combined_rows.append((sol_label, self._inline_crm_rows_display[sol_label]))

        model = PivotTableModel(self, pivot_data, combined_rows)
        self.table_view.setModel(model)
        self.table_view.frozenTableView.setModel(model)
        self.table_view.updateFrozenColumns()
        self.table_view.model().layoutChanged.emit()
        self.table_view.frozenTableView.model().layoutChanged.emit()
        self.table_view.horizontalHeader().setSectionResizeMode(QHeaderView.ResizeMode.Interactive)
        for col, width in self.column_widths.items():
            if col < len(pivot_data.columns):
                self.table_view.horizontalHeader().resizeSection(col, width)
        self.table_view.viewport().update()
        logger.debug("Completed update_pivot_display")

    def clear_inline_crm(self):
        """Clear inline CRM data."""
        logger.debug("Clearing inline CRM data")
        self._inline_crm_rows.clear()
        self._inline_crm_rows_display.clear()
        self.included_crms.clear()
        self.update_pivot_display()
        self.data_changed.emit()

    def show_element_plot(self):
        """Show element calibration plot in a resizable window."""
        logger.debug("Attempting to show element plot")
        pivot_data = self.results_frame.last_filtered_data
        if pivot_data is None or pivot_data.empty:
            logger.warning("No data to plot")
            QMessageBox.warning(self, "Warning", "No data to plot!")
            return
        logger.debug("Opening element plot window")
        if hasattr(self, 'current_plot_window') and self.current_plot_window:
            self.current_plot_window.close()
        annotations = []
        self.current_plot_window = PivotPlotWindow(self, annotations)
        self.current_plot_window.show()

    def backup_column(self, column):
        """Backup a column before applying corrections."""
        pivot_data = self.results_frame.last_filtered_data
        if column in pivot_data.columns:
            self.results_frame.column_backups[column] = pivot_data[column].copy()
            logger.debug(f"Backed up column: {column}")
            self.data_changed.emit()

    def restore_column(self, column):
        """Restore a column from backup."""
        if column in self.results_frame.column_backups:
            self.results_frame.last_filtered_data[column] = self.results_frame.column_backups[column].copy()
            self.update_pivot_display()
            logger.debug(f"Restored column: {column}")
            del self.results_frame.column_backups[column]
            self.data_changed.emit()
        else:
            logger.warning(f"No backup found for column: {column}")
            QMessageBox.warning(self, "Warning", f"No backup found for column {column}")

    def export_table(self):
        """Export the current pivot table to a CSV or Excel file."""
        logger.debug("Attempting to export table")
        pivot_data = self.results_frame.last_filtered_data
        if pivot_data is None or pivot_data.empty:
            logger.warning("No data to export")
            QMessageBox.warning(self, "Warning", "No data to export!")
            return

        try:
            file_path, selected_filter = QFileDialog.getSaveFileName(
                self,
                "Save Pivot Table",
                "",
                "CSV Files (*.csv);;Excel Files (*.xlsx);;All Files (*)"
            )
            if not file_path:
                logger.debug("Export cancelled by user")
                return

            if selected_filter.startswith("CSV"):
                pivot_data.to_csv(file_path, index=True)
            elif selected_filter.startswith("Excel"):
                pivot_data.to_excel(file_path, index=True, engine='openpyxl')
            
            logger.debug(f"Table exported successfully to {file_path}")
            QMessageBox.information(self, "Success", f"Table exported successfully to {file_path}")
        except Exception as e:
            logger.error(f"Failed to export table: {str(e)}")
            QMessageBox.warning(self, "Error", f"Failed to export table: {str(e)}")
