import re
import pandas as pd
from PyQt6.QtWidgets import (
    QCheckBox, QMessageBox, QDialog, QVBoxLayout, QRadioButton,
    QPushButton, QLabel, QWidget, QHBoxLayout,QLineEdit
)
from PyQt6.QtCore import Qt
import logging

logging.basicConfig(level=logging.DEBUG, format="%(asctime)s - %(levelname)s - %(message)s")
logger = logging.getLogger(__name__)

class CRMManager:
    """Manages CRM-related operations for the PivotTab."""
    def __init__(self, pivot_tab):
        self.pivot_tab = pivot_tab
        self.logger = logger
        self.crm_selections = {}

    def check_rm(self):
        """Check Reference Materials (RM) against the CRM database and update inline CRM rows."""
        self.crm_selections = {}
        if self.pivot_tab.results_frame.last_filtered_data is None or self.pivot_tab.results_frame.last_filtered_data.empty:
            QMessageBox.warning(self.pivot_tab, "Warning", "No pivot data available!")
            self.logger.warning("No pivot data available in check_rm")
            return

        try:
            conn = self.pivot_tab.app.crm_tab.conn
            if conn is None:
                self.pivot_tab.app.crm_tab.init_db()
                conn = self.pivot_tab.app.crm_tab.conn
                if conn is None:
                    QMessageBox.warning(self.pivot_tab, "Error", "Failed to connect to CRM database!")
                    self.logger.error("Failed to connect to CRM database")
                    return

            crm_ids = ['258', '252', '906', '506', '233', '255', '263', '260']

            def is_crm_label(label):
                label = str(label).strip().lower()
                for crm_id in crm_ids:
                    pattern = rf'(?i)(?:(?:^|(?<=\s))(?:CRM|OREAS)?\s*{crm_id}(?:[a-zA-Z0-9]{{0,2}})?\b)'
                    if re.search(pattern, label):
                        return True
                return False

            crm_rows = self.pivot_tab.results_frame.last_filtered_data[
                self.pivot_tab.results_frame.last_filtered_data['Solution Label'].apply(is_crm_label)
            ].copy()

            if crm_rows.empty:
                QMessageBox.information(self.pivot_tab, "Info", "No CRM rows found in pivot data!")
                return

            cursor = conn.cursor()
            cursor.execute("PRAGMA table_info(pivot_crm)")
            cols = [x[1] for x in cursor.fetchall()]
            required = {'CRM ID', 'Analysis Method'}
            if not required.issubset(cols):
                QMessageBox.warning(self.pivot_tab, "Error", "pivot_crm table missing required columns!")
                return

            element_to_columns = {}
            for col in self.pivot_tab.results_frame.last_filtered_data.columns:
                if col == 'Solution Label':
                    continue
                element = col.split()[0].strip()
                element_to_columns.setdefault(element, []).append(col)

            try:
                dec = int(self.pivot_tab.results_frame.decimal_combo.currentText())
            except (AttributeError, ValueError):
                self.logger.warning("decimal_combo not available or invalid, using default decimal places (1)")
                dec = 1

            self.pivot_tab._inline_crm_rows.clear()
            self.pivot_tab.included_crms.clear()

            for _, row in crm_rows.iterrows():
                label = row['Solution Label']
                found_crm_id = None
                for crm_id in crm_ids:
                    pattern = rf'(?i)(?:(?:^|(?<=\s))(?:CRM|OREAS)?\s*({crm_id}(?:[a-zA-Z0-9]{{0,2}})?)\b)'
                    m = re.search(pattern, str(label))
                    if m:
                        found_crm_id = m.group(1).strip()
                        break
                if not found_crm_id:
                    continue

                crm_id_string = f"OREAS {found_crm_id}"
                cursor.execute(
                    "SELECT * FROM pivot_crm WHERE [CRM ID] LIKE ?",
                    (f"OREAS {found_crm_id}%",)
                )
                crm_data = cursor.fetchall()
                if not crm_data:
                    continue

                cursor.execute("PRAGMA table_info(pivot_crm)")
                db_columns = [x[1] for x in cursor.fetchall()]
                non_element_columns = ['CRM ID', 'Solution Label', 'Analysis Method', 'Type']

                all_crm_options = {}
                filtered_crm_options = {}
                allowed_methods = {'4-Acid Digestion', 'Aqua Regia Digestion'}

                for db_row in crm_data:
                    crm_id = db_row[db_columns.index('CRM ID')]
                    analysis_method = db_row[db_columns.index('Analysis Method')]
                    key = f"{crm_id} ({analysis_method})"
                    all_crm_options[key] = []
                    if analysis_method in allowed_methods:
                        filtered_crm_options[key] = []

                    for col in db_columns:
                        if col in non_element_columns:
                            continue
                        value = db_row[db_columns.index(col)]
                        if value not in (None, ''):
                            try:
                                symbol = col.split('_')[0].strip()
                                val = float(value)
                                all_crm_options[key].append((symbol, val))
                                if analysis_method in allowed_methods:
                                    filtered_crm_options[key].append((symbol, val))
                            except (ValueError, TypeError):
                                continue

                selected_crm_key = self.crm_selections.get(label)
                if selected_crm_key is None and len(filtered_crm_options) > 1:
                    dialog = QDialog(self.pivot_tab)
                    dialog.setWindowTitle(f"Select CRM for {label}")
                    layout = QVBoxLayout(dialog)
                    layout.setSpacing(5)
                    layout.setContentsMargins(10, 10, 10, 10)
                    layout.addWidget(QLabel(f"Multiple CRMs found for {label}. Please select one:"))
                    radio_container = QWidget()
                    radio_group_layout = QVBoxLayout(radio_container)
                    radio_group_layout.setSpacing(2)
                    radio_group_layout.setContentsMargins(0, 0, 0, 0)
                    layout.addWidget(radio_container)
                    more_checkbox = QCheckBox("More")
                    layout.addWidget(more_checkbox)
                    radio_buttons = []
                    radio_button_group = []

                    def update_radio_buttons(show_all=False):
                        for rb in radio_button_group:
                            rb.setParent(None)
                        radio_button_group.clear()
                        radio_buttons.clear()
                        options = all_crm_options if show_all else filtered_crm_options
                        for key in sorted(options.keys()):
                            rb = QRadioButton(key)
                            rb.setStyleSheet("margin:0px; padding:0px;")
                            radio_group_layout.addWidget(rb)
                            radio_button_group.append(rb)
                            radio_buttons.append((key, rb))
                        if radio_buttons:
                            if selected_crm_key in options:
                                for key, rb in radio_buttons:
                                    if key == selected_crm_key:
                                        rb.setChecked(True)
                                        break
                            else:
                                radio_buttons[0][1].setChecked(True)
                        radio_container.updateGeometry()
                        layout.invalidate()
                        dialog.adjustSize()

                    update_radio_buttons(show_all=False)
                    more_checkbox.toggled.connect(lambda checked: update_radio_buttons(show_all=checked))
                    button_layout = QHBoxLayout()
                    button_layout.setSpacing(10)
                    button_layout.setContentsMargins(0, 8, 0, 0)
                    confirm_btn = QPushButton("Confirm")
                    cancel_btn = QPushButton("Cancel")
                    button_layout.addWidget(confirm_btn)
                    button_layout.addWidget(cancel_btn)
                    layout.addLayout(button_layout)

                    def on_confirm():
                        nonlocal selected_crm_key
                        for key, rb in radio_buttons:
                            if rb.isChecked():
                                selected_crm_key = key
                                break
                        self.crm_selections[label] = selected_crm_key
                        dialog.accept()

                    confirm_btn.clicked.connect(on_confirm)
                    cancel_btn.clicked.connect(dialog.reject)
                    if dialog.exec() == QDialog.DialogCode.Rejected:
                        return

                if selected_crm_key is None:
                    selected_crm_key = (list(filtered_crm_options.keys())[0]
                                       if filtered_crm_options else list(all_crm_options.keys())[0])
                    self.crm_selections[label] = selected_crm_key

                crm_data = all_crm_options.get(selected_crm_key, [])
                crm_dict = {symbol: grade for symbol, grade in crm_data}
                crm_values = {'Solution Label': selected_crm_key}
                for element, columns in element_to_columns.items():
                    value = crm_dict.get(element)
                    if value is not None:
                        for col in columns:
                            crm_values[col] = value

                if len(crm_values) > 1:
                    self.pivot_tab._inline_crm_rows[label] = [crm_values]
                    self.pivot_tab.included_crms[label] = QCheckBox(label, checked=True)

            if not self.pivot_tab._inline_crm_rows:
                QMessageBox.information(self.pivot_tab, "Info", "No matching CRM elements found!")
                return

            self.pivot_tab._inline_crm_rows_display = self._build_crm_row_lists_for_columns(
                list(self.pivot_tab.results_frame.last_filtered_data.columns)
            )
            self.pivot_tab.update_pivot_display()
            self.pivot_tab.data_changed.emit()
            self.logger.debug("Emitted data_changed signal after check_rm")

        except Exception as e:
            self.logger.error(f"Failed to check RM: {str(e)}")
            QMessageBox.warning(self.pivot_tab, "Error", f"Failed to check RM: {str(e)}")

    def open_manual_crm_dialog(self, solution_label):
        """Open a dialog to search and select a CRM manually."""
        try:
            conn = self.pivot_tab.app.crm_tab.conn
            if conn is None:
                self.pivot_tab.app.crm_tab.init_db()
                conn = self.pivot_tab.app.crm_tab.conn
                if conn is None:
                    QMessageBox.warning(self.pivot_tab, "Error", "Failed to connect to CRM database!")
                    self.logger.error("Failed to connect to CRM database")
                    return

            cursor = conn.cursor()
            cursor.execute("SELECT DISTINCT [CRM ID] FROM pivot_crm WHERE [CRM ID] LIKE 'OREAS%'")
            crm_ids = [row[0] for row in cursor.fetchall()]

            if not crm_ids:
                QMessageBox.warning(self.pivot_tab, "Warning", "No CRMs found in the database!")
                self.logger.warning("No CRMs found in the database")
                return

            dialog = QDialog(self.pivot_tab)
            dialog.setWindowTitle(f"Select CRM for {solution_label}")
            layout = QVBoxLayout(dialog)
            layout.setSpacing(5)
            layout.setContentsMargins(10, 10, 10, 10)

            # Search input and button
            search_layout = QHBoxLayout()
            search_label = QLabel("Search OREAS:")
            search_input = QLineEdit()
            search_input.setPlaceholderText("Enter OREAS ID (e.g., 258)")
            search_button = QPushButton("Search")
            search_layout.addWidget(search_label)
            search_layout.addWidget(search_input)
            search_layout.addWidget(search_button)
            layout.addLayout(search_layout)

            # CRM selection
            radio_container = QWidget()
            radio_group_layout = QVBoxLayout(radio_container)
            radio_group_layout.setSpacing(2)
            radio_group_layout.setContentsMargins(0, 0, 0, 0)
            layout.addWidget(radio_container)

            radio_buttons = []
            radio_button_group = []

            def update_crm_list():
                for rb in radio_button_group:
                    rb.setParent(None)
                radio_button_group.clear()
                radio_buttons.clear()

                search_text = search_input.text().strip()
                if not search_text:  # If search is empty, show no CRMs
                    radio_container.updateGeometry()
                    layout.invalidate()
                    dialog.adjustSize()
                    confirm_btn.setEnabled(False)
                    return

                filtered_crms = [crm_id for crm_id in crm_ids if search_text.lower() in crm_id.lower()]
                for crm_id in sorted(filtered_crms):
                    cursor.execute(
                        "SELECT DISTINCT [Analysis Method] FROM pivot_crm WHERE [CRM ID] = ?",
                        (crm_id,)
                    )
                    methods = [row[0] for row in cursor.fetchall()]
                    for method in sorted(methods):
                        key = f"{crm_id} ({method})"
                        rb = QRadioButton(key)
                        rb.setStyleSheet("margin:0px; padding:0px;")
                        radio_group_layout.addWidget(rb)
                        radio_button_group.append(rb)
                        radio_buttons.append((key, rb))

                if radio_buttons:
                    radio_buttons[0][1].setChecked(True)
                    confirm_btn.setEnabled(True)
                else:
                    confirm_btn.setEnabled(False)
                radio_container.updateGeometry()
                layout.invalidate()
                dialog.adjustSize()

            # Connect search button to update_crm_list
            search_button.clicked.connect(update_crm_list)

            # Buttons
            button_layout = QHBoxLayout()
            button_layout.setSpacing(10)
            button_layout.setContentsMargins(0, 8, 0, 0)
            confirm_btn = QPushButton("Confirm")
            confirm_btn.setEnabled(False)  # Disable Confirm button initially
            cancel_btn = QPushButton("Cancel")
            button_layout.addWidget(confirm_btn)
            button_layout.addWidget(cancel_btn)
            layout.addLayout(button_layout)

            def on_confirm():
                selected_crm_key = None
                for key, rb in radio_buttons:
                    if rb.isChecked():
                        selected_crm_key = key
                        break
                if selected_crm_key:
                    self.crm_selections[solution_label] = selected_crm_key
                    self.add_manual_crm(solution_label, selected_crm_key)
                dialog.accept()

            confirm_btn.clicked.connect(on_confirm)
            cancel_btn.clicked.connect(dialog.reject)
            dialog.exec()

        except Exception as e:
            self.logger.error(f"Failed to open manual CRM dialog: {str(e)}")
            QMessageBox.warning(self.pivot_tab, "Error", f"Failed to open manual CRM dialog: {str(e)}")

    def add_manual_crm(self, solution_label, selected_crm_key):
        """Add manually selected CRM to the pivot table."""
        try:
            conn = self.pivot_tab.app.crm_tab.conn
            cursor = conn.cursor()
            cursor.execute("PRAGMA table_info(pivot_crm)")
            db_columns = [x[1] for x in cursor.fetchall()]
            non_element_columns = ['CRM ID', 'Solution Label', 'Analysis Method', 'Type']

            crm_id = selected_crm_key.split(' (')[0]
            cursor.execute(
                "SELECT * FROM pivot_crm WHERE [CRM ID] = ? AND [Analysis Method] = ?",
                (crm_id, selected_crm_key.split(' (')[1][:-1])
            )
            crm_data = cursor.fetchall()
            if not crm_data:
                self.logger.warning(f"No data found for CRM {selected_crm_key}")
                return

            element_to_columns = {}
            for col in self.pivot_tab.results_frame.last_filtered_data.columns:
                if col == 'Solution Label':
                    continue
                element = col.split()[0].strip()
                element_to_columns.setdefault(element, []).append(col)

            crm_dict = {}
            for db_row in crm_data:
                for col in db_columns:
                    if col in non_element_columns:
                        continue
                    value = db_row[db_columns.index(col)]
                    if value not in (None, ''):
                        try:
                            symbol = col.split('_')[0].strip()
                            val = float(value)
                            crm_dict[symbol] = val
                        except (ValueError, TypeError):
                            continue

            crm_values = {'Solution Label': selected_crm_key}
            for element, columns in element_to_columns.items():
                value = crm_dict.get(element)
                if value is not None:
                    for col in columns:
                        crm_values[col] = value

            if len(crm_values) > 1:
                self.pivot_tab._inline_crm_rows[solution_label] = [crm_values]
                self.pivot_tab.included_crms[solution_label] = QCheckBox(solution_label, checked=True)
                self.pivot_tab._inline_crm_rows_display = self._build_crm_row_lists_for_columns(
                    list(self.pivot_tab.results_frame.last_filtered_data.columns)
                )
                self.pivot_tab.update_pivot_display()
                self.pivot_tab.data_changed.emit()
                self.logger.debug(f"Manually added CRM {selected_crm_key} for {solution_label}")

        except Exception as e:
            self.logger.error(f"Failed to add manual CRM: {str(e)}")
            QMessageBox.warning(self.pivot_tab, "Error", f"Failed to add manual CRM: {str(e)}")

    def check_rm_with_diff_range(self, min_diff, max_diff):
        """Update CRM rows display based on the specified difference range."""
        self.pivot_tab._inline_crm_rows_display = self._build_crm_row_lists_for_columns(
            list(self.pivot_tab.results_frame.last_filtered_data.columns)
        )
        self.pivot_tab.update_pivot_display()
        self.pivot_tab.data_changed.emit()
        self.logger.debug("Emitted data_changed signal after check_rm_with_diff_range")

    def _build_crm_row_lists_for_columns(self, columns):
        """Build CRM row lists for display."""
        crm_display = {}
        try:
            dec = int(self.pivot_tab.results_frame.decimal_combo.currentText())
        except (AttributeError, ValueError):
            self.logger.warning("decimal_combo not available or invalid, using default decimal places (1)")
            dec = 1

        try:
            min_diff = float(self.pivot_tab.crm_diff_min.text())
            max_diff = float(self.pivot_tab.crm_diff_max.text())
        except ValueError:
            min_diff, max_diff = -12, 12

        for sol_label, list_of_dicts in self.pivot_tab._inline_crm_rows.items():
            crm_display[sol_label] = []
            pivot_row = self.pivot_tab.results_frame.last_filtered_data[
                self.pivot_tab.results_frame.last_filtered_data['Solution Label'].str.strip().str.lower() == sol_label.strip().lower()
            ]
            if pivot_row.empty:
                continue
            pivot_values = pivot_row.iloc[0].to_dict()

            for d in list_of_dicts:
                crm_row_list = []
                for col in columns:
                    if col == 'Solution Label':
                        crm_row_list.append(f"{d.get('Solution Label', sol_label)} CRM")
                    else:
                        val = d.get(col, "")
                        if pd.isna(val) or val == "":
                            crm_row_list.append("")
                        else:
                            try:
                                crm_row_list.append(f"{float(val):.{dec}f}")
                            except Exception:
                                crm_row_list.append(str(val))
                crm_display[sol_label].append((crm_row_list, ["crm"] * len(columns)))

                diff_row_list = []
                diff_tags = []
                for col in columns:
                    if col == 'Solution Label':
                        diff_row_list.append(f"{sol_label} Diff (%)")
                        diff_tags.append("diff")
                    else:
                        pivot_val = pivot_values.get(col, None)
                        crm_val = d.get(col, None)
                        if pivot_val is not None and crm_val is not None:
                            try:
                                pivot_val = float(pivot_val)
                                crm_val = float(crm_val)
                                if crm_val != 0:
                                    diff = ((crm_val - pivot_val) / crm_val) * 100
                                    diff_row_list.append(f"{diff:.{dec}f}")
                                    diff_tags.append("in_range" if min_diff <= diff <= max_diff else "out_range")
                                else:
                                    diff_row_list.append("N/A")
                                    diff_tags.append("diff")
                            except Exception:
                                diff_row_list.append("")
                                diff_tags.append("diff")
                        else:
                            diff_row_list.append("")
                            diff_tags.append("diff")
                crm_display[sol_label].append((diff_row_list, diff_tags))

        return crm_display