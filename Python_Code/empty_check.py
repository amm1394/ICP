from PyQt6.QtWidgets import QWidget, QVBoxLayout, QHBoxLayout, QFrame, QLabel, QLineEdit, QPushButton, QTableView, QHeaderView, QGroupBox, QMessageBox, QCheckBox, QDialog, QScrollArea, QTabWidget
from PyQt6.QtCore import Qt,pyqtSignal
from PyQt6.QtGui import QStandardItemModel, QStandardItem
import pandas as pd
import numpy as np
import time
import logging

# Setup logging with minimal output
logging.basicConfig(level=logging.DEBUG, format="%(asctime)s - %(levelname)s - %(message)s")
logger = logging.getLogger(__name__)

class ColumnFilterDialog(QDialog):
    """Dialog for filtering a specific column with list and numeric filters."""
    def __init__(self, parent, col_name):
        super().__init__(parent)
        self.setWindowTitle(f"Filter Column: {col_name}")
        self.parent = parent
        self.col_name = col_name
        self.checkboxes = {}
        self.min_edit = None
        self.max_edit = None

        if self.parent.empty_rows is None or self.col_name not in self.parent.empty_rows.columns:
            logger.warning(f"No data available for filtering column {col_name}")
            return

        # Check if column is numeric by attempting conversion
        try:
            test_series = pd.to_numeric(self.parent.empty_rows[self.col_name], errors='coerce')
            self.is_numeric_col = test_series.notna().any() and self.col_name != 'Solution Label'
        except Exception as e:
            self.is_numeric_col = False
            logger.error(f"Error checking numeric type for {col_name}: {str(e)}")

        # Get unique values, including NaN
        unique_values = self.parent.empty_rows[self.col_name].replace(np.nan, 'NaN').astype(str).unique()
        sorted_unique = sorted(unique_values, key=str)

        layout = QVBoxLayout(self)
        self.setMinimumSize(400, 400)

        # Tab widget for list and numeric filters
        tab_widget = QTabWidget()
        list_tab = QWidget()
        tab_widget.addTab(list_tab, "List Filter")
        
        if self.is_numeric_col:
            number_tab = QWidget()
            tab_widget.addTab(number_tab, "Number Filter")
            self.setup_number_tab(number_tab)
        
        layout.addWidget(tab_widget)
        self.setup_list_tab(list_tab, sorted_unique)

        # OK and Cancel buttons
        action_buttons = QHBoxLayout()
        ok_btn = QPushButton("OK")
        ok_btn.clicked.connect(self.apply_filters)
        action_buttons.addWidget(ok_btn)

        cancel_btn = QPushButton("Cancel")
        cancel_btn.clicked.connect(self.reject)
        action_buttons.addWidget(cancel_btn)

        layout.addLayout(action_buttons)

    def setup_list_tab(self, widget, sorted_unique):
        """Set up the list filter tab with checkboxes for unique values."""
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

        curr_filter = self.parent.filters.get(self.col_name, {})
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
        """Set up the numeric filter tab with min/max inputs."""
        number_layout = QVBoxLayout(widget)

        # Min filter
        min_layout = QHBoxLayout()
        min_label = QLabel("Minimum:")
        self.min_edit = QLineEdit()
        self.min_edit.setPlaceholderText("Enter min value")
        self.min_edit.setFixedWidth(100)
        min_layout.addWidget(min_label)
        min_layout.addWidget(self.min_edit)
        min_layout.addStretch()
        number_layout.addLayout(min_layout)

        # Max filter
        max_layout = QHBoxLayout()
        max_label = QLabel("Maximum:")
        self.max_edit = QLineEdit()
        self.max_edit.setPlaceholderText("Enter max value")
        self.max_edit.setFixedWidth(100)
        max_layout.addWidget(max_label)
        max_layout.addWidget(self.max_edit)
        max_layout.addStretch()
        number_layout.addLayout(max_layout)

        # Display data range
        try:
            data_range = pd.to_numeric(self.parent.empty_rows[self.col_name], errors='coerce').dropna()
            if not data_range.empty:
                min_val = data_range.min()
                max_val = data_range.max()
                range_label = QLabel(f"Data Range: {min_val:.2f} to {max_val:.2f}")
                range_label.setStyleSheet("color: blue; font-size: 10px;")
                number_layout.addWidget(range_label)
            else:
                logger.warning(f"No valid numeric data for range display in column {self.col_name}")
        except Exception as e:
            logger.error(f"Error computing data range for {self.col_name}: {str(e)}")

        # Load current filter values
        curr_filter = self.parent.filters.get(self.col_name, {})
        if 'min_val' in curr_filter and curr_filter['min_val'] is not None:
            self.min_edit.setText(str(curr_filter['min_val']))
        if 'max_val' in curr_filter and curr_filter['max_val'] is not None:
            self.max_edit.setText(str(curr_filter['max_val']))

    def filter_checkboxes(self, text):
        """Filter checkboxes based on search text."""
        text = text.lower()
        for val, cb in self.checkboxes.items():
            if text == '' or text in str(val).lower():
                cb.setVisible(True)
            else:
                cb.setVisible(False)
                cb.setChecked(False)

    def update_filter(self, value, state):
        """Update the checkbox state for a value."""
        self.checkboxes[value].setChecked(state == Qt.CheckState.Checked.value)

    def toggle_all(self, checked):
        """Select or deselect all visible checkboxes."""
        for cb in self.checkboxes.values():
            if cb.isVisible():
                cb.setChecked(checked)

    def apply_filters(self):
        """Apply the selected filters and update the parent table."""
        selected_values = {val for val, cb in self.checkboxes.items() if cb.isChecked()}
        if not selected_values and not self.is_numeric_col:
            QMessageBox.warning(self, "Warning", "No values selected. Please select at least one value.")
            return

        min_val = None
        max_val = None
        if self.is_numeric_col:
            try:
                if self.min_edit and self.min_edit.text().strip():
                    min_val = float(self.min_edit.text())
                if self.max_edit and self.max_edit.text().strip():
                    max_val = float(self.max_edit.text())
                if min_val is not None and max_val is not None and min_val > max_val:
                    QMessageBox.warning(self, "Invalid Filter", "Minimum value cannot be greater than maximum value.")
                    return
            except ValueError:
                QMessageBox.warning(self, "Invalid Input", "Min and Max must be numeric values.")
                return

        # Convert string values to numeric where possible, keep 'NaN' as np.nan
        converted_values = set()
        for val in selected_values:
            if val == 'NaN':
                converted_values.add(np.nan)
            else:
                try:
                    converted_values.add(float(val))
                except ValueError:
                    converted_values.add(val)

        filter_settings = {'selected_values': converted_values}
        if self.is_numeric_col:
            filter_settings['min_val'] = min_val
            filter_settings['max_val'] = max_val

        self.parent.filters[self.col_name] = filter_settings
        self.parent.update_empty_table()
        self.accept()

class ElementSelectionDialog(QDialog):
    """Dialog for selecting main elements with checkboxes."""
    def __init__(self, parent, columns):
        super().__init__(parent)
        self.setWindowTitle("Select Main Elements")
        self.parent = parent
        self.columns = [col for col in columns if col != 'Solution Label']
        self.checkboxes = {}
        self.setMinimumSize(300, 400)

        layout = QVBoxLayout(self)

        # Search bar
        self.search_edit = QLineEdit()
        self.search_edit.setPlaceholderText("Search elements...")
        self.search_edit.textChanged.connect(self.filter_checkboxes)
        layout.addWidget(self.search_edit)

        # Scroll area for checkboxes
        scroll = QScrollArea()
        scroll_widget = QWidget()
        scroll_layout = QVBoxLayout(scroll_widget)
        scroll.setWidget(scroll_widget)
        scroll.setWidgetResizable(True)
        layout.addWidget(scroll)

        # Extract unique elements from column names (e.g., 'Na' from 'Na 326.068')
        unique_elements = sorted(set(col.split()[0] for col in self.columns if ' ' in col))

        # Pre-select main elements
        default_elements = {'Na', 'Ca', 'Al', 'Mg', 'K'}
        selected_elements = self.parent.main_elements if hasattr(self.parent, 'main_elements') else default_elements

        for elem in unique_elements:
            cb = QCheckBox(elem)
            cb.setChecked(elem in selected_elements)
            self.checkboxes[elem] = cb
            scroll_layout.addWidget(cb)

        # Select/Deselect all buttons
        buttons = QHBoxLayout()
        select_all_btn = QPushButton("Select All")
        select_all_btn.clicked.connect(lambda: self.toggle_all(True))
        buttons.addWidget(select_all_btn)

        deselect_all_btn = QPushButton("Deselect All")
        deselect_all_btn.clicked.connect(lambda: self.toggle_all(False))
        buttons.addWidget(deselect_all_btn)
        layout.addLayout(buttons)

        # OK and Cancel buttons
        action_buttons = QHBoxLayout()
        ok_btn = QPushButton("OK")
        ok_btn.clicked.connect(self.apply_selection)
        action_buttons.addWidget(ok_btn)

        cancel_btn = QPushButton("Cancel")
        cancel_btn.clicked.connect(self.reject)
        action_buttons.addWidget(cancel_btn)
        layout.addLayout(action_buttons)

    def filter_checkboxes(self, text):
        """Filter checkboxes based on search text."""
        text = text.lower()
        for elem, cb in self.checkboxes.items():
            cb.setVisible(text == '' or text in elem.lower())
            if not cb.isVisible():
                cb.setChecked(False)

    def toggle_all(self, checked):
        """Select or deselect all visible checkboxes."""
        for cb in self.checkboxes.values():
            if cb.isVisible():
                cb.setChecked(checked)

    def apply_selection(self):
        """Save selected elements and close dialog."""
        self.parent.main_elements = {elem for elem, cb in self.checkboxes.items() if cb.isChecked()}
        if not self.parent.main_elements:
            QMessageBox.warning(self, "Warning", "At least one element must be selected!")
            return
        logger.debug(f"Selected main elements: {self.parent.main_elements}")
        self.accept()

class EmptyCheckFrame(QWidget):
    empty_rows_found = pyqtSignal(pd.DataFrame)  # ارسال DataFrame با ایندکس اصلی
    def __init__(self, app, parent=None):
        super().__init__(parent)
        self.app = app
        self.df_cache = None
        self.empty_rows = None
        self.mean_percentage_threshold = 70  # Threshold for mean comparison
        self.filters = {}
        self.main_elements = {'Na', 'Ca', 'Al', 'Mg', 'K'}  # Default main elements
        self.setup_ui()

    def setup_ui(self):
        """Set up the UI with enhanced controls."""
        self.setStyleSheet("""
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
            QLineEdit {
                background-color: #FFFFFF;
                border: 1px solid #D0D7DE;
                padding: 6px;
                border-radius: 6px;
                font-size: 13px;
            }
            QLineEdit:focus {
                border: 1px solid #2E7D32;
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
            QLabel {
                color: #1A3C34;
                font-size: 13px;
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
            QCheckBox {
                color: #1A3C34;
                font-size: 13px;
            }
        """)

        main_layout = QVBoxLayout(self)
        main_layout.setContentsMargins(15, 15, 15, 15)
        main_layout.setSpacing(15)

        input_group = QGroupBox("Empty Rows Check")
        input_layout = QHBoxLayout(input_group)
        input_layout.setSpacing(10)

        # Button to select main elements
        self.select_elements_btn = QPushButton("Select Main Elements")
        self.select_elements_btn.clicked.connect(self.open_element_selection)
        self.select_elements_btn.setToolTip("Select the main elements to consider for empty row detection")
        input_layout.addWidget(self.select_elements_btn)

        # Mean percentage threshold input
        input_layout.addWidget(QLabel("Mean % Threshold:"))
        self.mean_percentage_entry = QLineEdit()
        self.mean_percentage_entry.setText(str(self.mean_percentage_threshold))
        self.mean_percentage_entry.setFixedWidth(120)
        self.mean_percentage_entry.setToolTip("Enter the percentage below column mean (e.g., 70, range: 0 to 100)")
        input_layout.addWidget(self.mean_percentage_entry)

        self.check_button = QPushButton("Check Empty Rows")
        self.update_button_tooltip()
        self.check_button.clicked.connect(self.check_empty_rows)
        input_layout.addWidget(self.check_button)

        self.clear_filters_button = QPushButton("Clear Filters")
        self.clear_filters_button.clicked.connect(self.clear_filters)
        input_layout.addWidget(self.clear_filters_button)
        input_layout.addStretch()

        main_layout.addWidget(input_group)

        main_container = QFrame()
        main_layout.addWidget(main_container, stretch=1)
        container_layout = QHBoxLayout(main_container)
        container_layout.setSpacing(15)

        empty_group = QGroupBox("Empty Rows")
        empty_layout = QVBoxLayout(empty_group)
        empty_layout.setSpacing(10)

        self.empty_table = QTableView()
        self.empty_table.setSelectionMode(QTableView.SelectionMode.SingleSelection)
        self.empty_table.setSelectionBehavior(QTableView.SelectionBehavior.SelectRows)
        self.empty_table.horizontalHeader().setSectionResizeMode(QHeaderView.ResizeMode.Interactive)
        self.empty_table.verticalHeader().setVisible(False)
        self.empty_table.setToolTip("List of empty rows")
        self.empty_table.horizontalHeader().sectionClicked.connect(self.on_header_clicked)
        empty_layout.addWidget(self.empty_table)

        container_layout.addWidget(empty_group, stretch=1)

    def update_button_tooltip(self):
        """Update the tooltip of the Check Empty Rows button."""
        self.check_button.setToolTip(
            f"Check for rows where all main elements "
            f"are {self.mean_percentage_threshold}% below column mean"
        )

    def open_element_selection(self):
        """Open dialog to select main elements."""
        df = self.app.results.last_filtered_data
        if df is None or df.empty:
            QMessageBox.warning(self, "Warning", "No data available to select elements!")
            return
        dialog = ElementSelectionDialog(self, df.columns)
        dialog.exec()

    def check_empty_rows(self):
        """Check for rows where all main elements are significantly below column means."""
        try:
            mean_percentage = float(self.mean_percentage_entry.text())
            if not 0 <= mean_percentage <= 100:
                raise ValueError("Mean percentage must be between 0 and 100")
            self.mean_percentage_threshold = mean_percentage
        except ValueError as e:
            QMessageBox.warning(self, "Warning", f"Invalid mean percentage: {e}")
            return

        self.update_button_tooltip()

        df = self.app.results.last_filtered_data
        if df is None or df.empty:
            QMessageBox.warning(self, "Warning", "No pivoted data available! Please check the Results tab.")
            logger.warning("No pivoted data available")
            return

        # --- اضافه کردن original_index ---
        if 'original_index' not in df.columns:
            df = df.reset_index(drop=True)
            df['original_index'] = df.index  # ایجاد ایندکس اصلی
        else:
            df = df.copy()  # جلوگیری از warning

        # Get valid elements
        valid_elements = [col for col in df.columns if col != 'Solution Label' and col.split()[0] in self.main_elements]
        if not valid_elements:
            QMessageBox.warning(self, "Warning", "No valid main elements selected or available!")
            logger.warning("No valid main elements selected")
            return

        # Convert to numeric
        df_numeric = df[valid_elements].apply(pd.to_numeric, errors='coerce')

        # Calculate column means and thresholds
        column_means = df_numeric.mean()
        threshold_values = column_means * (1 - self.mean_percentage_threshold / 100)

        # Mask: all main elements below threshold
        below_threshold = df_numeric < threshold_values
        empty_rows_mask = below_threshold.all(axis=1)

        # --- استخراج ردیف‌های خالی با original_index ---
        self.empty_rows = df[empty_rows_mask][['Solution Label'] + valid_elements].drop_duplicates(subset=['Solution Label'])

        if not self.empty_rows.empty:
            empty_with_index = df[empty_rows_mask][['Solution Label', 'original_index'] + valid_elements].drop_duplicates(subset=['Solution Label'])
            self.empty_rows_found.emit(empty_with_index)
        else:
            self.empty_rows_found.emit(pd.DataFrame())

        self.update_empty_table()
        QMessageBox.information(
            self, "Info",
            f"Found {len(self.empty_rows)} empty rows." if not self.empty_rows.empty else "No empty rows found."
        )

    def on_header_clicked(self, section):
        """Handle header click to open filter dialog."""
        if self.empty_rows is None or self.empty_rows.empty:
            QMessageBox.warning(self, "Warning", "No data to filter!")
            return
        model = self.empty_table.model()
        col_name = model.headerData(section, Qt.Orientation.Horizontal, Qt.ItemDataRole.DisplayRole)
        dialog = ColumnFilterDialog(self, col_name)
        dialog.exec()

    def clear_filters(self):
        """Clear all column filters and update the table."""
        self.filters.clear()
        self.update_empty_table()
        QMessageBox.information(self, "Filters Cleared", "All column filters have been cleared.")

    def update_empty_table(self):
        """Update the empty rows table, applying any column filters."""
        model = QStandardItemModel()
        headers = ["Solution Label"] + (list(self.empty_rows.columns[1:]) if self.empty_rows is not None else [])
        model.setHorizontalHeaderLabels(headers)

        if self.empty_rows is not None and not self.empty_rows.empty:
            df = self.empty_rows.copy()
            # Convert all non-Solution Label columns to numeric
            for col in df.columns:
                if col != 'Solution Label':
                    df[col] = pd.to_numeric(df[col], errors='coerce')

            # Apply filters
            for col, filt in self.filters.items():
                if col not in df.columns:
                    logger.warning(f"Column {col} not found in DataFrame")
                    continue

                col_data = df[col] if col == 'Solution Label' else pd.to_numeric(df[col], errors='coerce')
                mask = pd.Series(True, index=df.index)

                # Apply list filter
                if 'selected_values' in filt and filt['selected_values']:
                    list_mask = pd.Series(False, index=df.index)
                    if np.nan in filt['selected_values']:
                        list_mask |= col_data.isna()
                    non_nan_values = {x for x in filt['selected_values'] if not pd.isna(x)}
                    if non_nan_values:
                        if col != 'Solution Label':
                            try:
                                non_nan_values = {float(x) for x in non_nan_values}
                            except ValueError as e:
                                logger.error(f"Error converting selected values to float for {col}: {str(e)}")
                                continue
                        list_mask |= col_data.isin(non_nan_values)
                    mask &= list_mask
                    logger.debug(f"Applied list filter on {col}: {filt['selected_values']}, rows left: {len(df[mask])}")

                # Apply numeric filter for non-Solution Label columns
                if col != 'Solution Label' and ('min_val' in filt or 'max_val' in filt):
                    try:
                        num_mask = pd.Series(True, index=df.index)
                        if 'min_val' in filt and filt['min_val'] is not None:
                            num_mask &= (col_data >= filt['min_val']) | col_data.isna()
                        if 'max_val' in filt and filt['max_val'] is not None:
                            num_mask &= (col_data <= filt['max_val']) | col_data.isna()
                        mask &= num_mask
                        logger.debug(f"Applied numeric filter on {col}: min={filt.get('min_val', None)}, max={filt.get('max_val', None)}, rows left: {len(df[mask])}")
                    except Exception as e:
                        logger.error(f"Error applying numeric filter on {col}: {str(e)}")
                        continue

                df = df[mask]
                if df.empty:
                    logger.debug(f"No rows remain after filtering column {col}")
                    break

            if df.empty:
                logger.debug("No rows remain after applying all filters")
            else:
                for _, row in df.iterrows():
                    solution_label = row['Solution Label']
                    label_item = QStandardItem(str(solution_label))
                    label_item.setTextAlignment(Qt.AlignmentFlag.AlignLeft)

                    row_items = [label_item]
                    for col in df.columns[1:]:
                        value = row[col]
                        item = QStandardItem(f"{value:.3f}" if pd.notna(value) else "")
                        item.setTextAlignment(Qt.AlignmentFlag.AlignRight)
                        row_items.append(item)

                    model.appendRow(row_items)

        self.empty_table.setModel(model)
        self.empty_table.horizontalHeader().setSectionResizeMode(0, QHeaderView.ResizeMode.Interactive)
        self.empty_table.resizeColumnToContents(0)
        for col in range(1, len(headers)):
            self.empty_table.horizontalHeader().setSectionResizeMode(col, QHeaderView.ResizeMode.Fixed)
            self.empty_table.setColumnWidth(col, 100)

    def data_changed(self):
        """Handle data change notifications."""
        self.df_cache = None
        self.empty_rows = None
        self.filters.clear()
        self.empty_table.setModel(QStandardItemModel())

    def reset_state(self):
        """Reset all internal state and UI."""
        self.df_cache = None
        self.empty_rows = None
        self.mean_percentage_threshold = 70
        self.filters.clear()
        self.main_elements = {'Na', 'Ca', 'Al', 'Mg', 'K'}
        
        if hasattr(self, 'mean_percentage_entry'):
            self.mean_percentage_entry.setText(str(self.mean_percentage_threshold))
        if hasattr(self, 'empty_table'):
            self.empty_table.setModel(QStandardItemModel())
        if hasattr(self, 'check_button'):
            self.update_button_tooltip()