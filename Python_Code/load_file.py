# file_loader.py
import pandas as pd
import csv
import os
import logging
import re
from PyQt6.QtWidgets import (
    QFileDialog, QMessageBox, QProgressDialog
)
from PyQt6.QtCore import QThread, pyqtSignal, Qt
from screens.pivot.pivot_creator import PivotCreator

# Setup logging
logger = logging.getLogger(__name__)


def split_element_name(element):
    """Split element name like 'Ce140' into 'Ce 140'."""
    if not isinstance(element, str):
        return element
    match = re.match(r'^([A-Za-z]+)(\d+\.?\d*)$', element.strip())
    if match:
        symbol, number = match.groups()
        return f"{symbol} {number}"
    return element


class FileLoaderThread(QThread):
    """Worker thread to load and parse Excel/CSV files with progress updates."""
    progress = pyqtSignal(int, str)  # Signal for progress (value, message)
    finished = pyqtSignal(object, str)  # Signal for completion with DataFrame and file path
    error = pyqtSignal(str)  # Signal for errors

    def __init__(self, file_path, parent=None):
        super().__init__(parent)
        self.file_path = file_path
        self.is_canceled = False

    def cancel(self):
        """Mark the thread as canceled."""
        self.is_canceled = True

    def run(self):
        """Run the file loading process with progress updates."""
        try:
            logger.debug(f"Starting file loading in thread for: {self.file_path}")
            self.progress.emit(0, "Initializing file loading...")

            # Step 1: Preview to determine format (10% of progress)
            is_new_format = False
            preview_steps = 10
            if self.file_path.lower().endswith('.csv'):
                try:
                    with open(self.file_path, 'r', encoding='utf-8') as f:
                        preview_lines = [f.readline().strip() for _ in range(10)]
                    is_new_format = any("Sample ID:" in line for line in preview_lines) or \
                                    any("Net Intensity" in line for line in preview_lines)
                except Exception as e:
                    logger.warning(f"Preview read failed: {str(e)}. Assuming new format for CSV.")
                    is_new_format = True
            else:
                try:
                    engine = 'openpyxl' if self.file_path.lower().endswith('.xlsx') else 'xlrd'
                    preview = pd.read_excel(self.file_path, header=None, nrows=10, engine=engine)
                    is_new_format = any(preview[0].str.contains("Sample ID:", na=False)) or \
                                    any(preview[0].str.contains("Net Intensity", na=False))
                except Exception as e:
                    logger.error(f"Failed to read Excel preview: {str(e)}")
                    self.error.emit(f"Failed to read Excel preview: {str(e)}")
                    return
            self.progress.emit(preview_steps, "Preview complete, parsing file...")

            if self.is_canceled:
                self.error.emit("File loading canceled by user")
                return

            data_rows = []
            current_sample = None
            parse_steps = 70  # 70% of progress for parsing

            if is_new_format:
                logger.debug("Detected new file format (Sample ID-based)")
                if self.file_path.lower().endswith('.csv'):
                    try:
                        with open(self.file_path, 'r', encoding='utf-8') as f:
                            reader = list(csv.reader(f, delimiter=',', quotechar='"'))
                            total_rows = len(reader)
                            rows_per_step = max(1, total_rows // parse_steps) if total_rows > 0 else 1
                            for idx, row in enumerate(reader):
                                if self.is_canceled:
                                    self.error.emit("File loading canceled by user")
                                    return
                                if idx == total_rows - 1:
                                    logger.debug("Skipping last row of CSV")
                                    continue
                                if not row or all(cell.strip() == "" for cell in row):
                                    continue
                                if len(row) > 0 and row[0].startswith("Sample ID:"):
                                    current_sample = row[1].strip()
                                    logger.debug(f"Found Sample ID: {current_sample}")
                                    continue
                                if len(row) > 0 and (row[0].startswith("Method File:") or row[0].startswith("Calibration File:")):
                                    continue
                                if current_sample is None:
                                    current_sample = "Unknown_Sample"
                                element = split_element_name(row[0].strip())
                                try:
                                    intensity = float(row[1]) if len(row) > 1 and row[1].strip() else None
                                    concentration = float(row[5]) if len(row) > 5 and row[5].strip() else None
                                    if intensity is not None or concentration is not None:
                                        data_rows.append({
                                            "Solution Label": current_sample,
                                            "Element": element,
                                            "Int": intensity,
                                            "Corr Con": concentration,
                                            "Type": 'Sample'
                                        })
                                except Exception as e:
                                    logger.warning(f"Invalid data for element {element} in sample {current_sample}: {str(e)}")
                                    continue
                                if idx % rows_per_step == 0:
                                    progress = preview_steps + (idx // rows_per_step)
                                    self.progress.emit(min(progress, preview_steps + parse_steps), f"Parsing row {idx}/{total_rows}")
                    except Exception as e:
                        logger.error(f"Failed to parse CSV: {str(e)}")
                        self.error.emit(f"Failed to parse CSV: {str(e)}")
                        return
                else:
                    try:
                        engine = 'openpyxl' if self.file_path.lower().endswith('.xlsx') else 'xlrd'
                        raw_data = pd.read_excel(self.file_path, header=None, engine=engine)
                        total_rows = raw_data.shape[0]
                        rows_per_step = max(1, total_rows // parse_steps) if total_rows > 0 else 1
                        for index, row in raw_data.iterrows():
                            if self.is_canceled:
                                self.error.emit("File loading canceled by user")
                                return
                            if index == total_rows - 1:
                                logger.debug("Skipping last row of Excel")
                                continue
                            row_list = row.tolist()
                            if any("No valid data found in the file" in str(cell) for cell in row_list):
                                continue
                            if isinstance(row[0], str) and row[0].startswith("Sample ID:"):
                                current_sample = row[0].split("Sample ID:")[1].strip()
                                logger.debug(f"Found Sample ID: {current_sample}")
                                continue
                            if isinstance(row[0], str) and (row[0].startswith("Method File:") or row[0].startswith("Calibration File:")):
                                continue
                            if current_sample and pd.notna(row[0]):
                                element = split_element_name(str(row[0]).strip())
                                try:
                                    intensity = float(row[1]) if pd.notna(row[1]) else None
                                    concentration = float(row[5]) if pd.notna(row[5]) else None
                                    if intensity is not None or concentration is not None:
                                        type_value = "Blk" if "BLANK" in current_sample.upper() else "Sample"
                                        data_rows.append({
                                            "Solution Label": current_sample,
                                            "Element": element,
                                            "Int": intensity,
                                            "Corr Con": concentration,
                                            "Type": type_value
                                        })
                                except Exception as e:
                                    logger.warning(f"Invalid data for element {element} in sample {current_sample}: {str(e)}")
                                    continue
                            if index % rows_per_step == 0:
                                progress = preview_steps + (index // rows_per_step)
                                self.progress.emit(min(progress, preview_steps + parse_steps), f"Parsing row {index}/{total_rows}")
                    except Exception as e:
                        logger.error(f"Failed to parse Excel: {str(e)}")
                        self.error.emit(f"Failed to parse Excel: {str(e)}")
                        return
            else:
                logger.debug("Detected previous file format (tabular)")
                if self.file_path.lower().endswith('.csv'):
                    try:
                        temp_df = pd.read_csv(self.file_path, header=None, nrows=1, on_bad_lines='skip')
                        if temp_df.iloc[0].notna().sum() == 1:
                            df = pd.read_csv(self.file_path, header=1, on_bad_lines='skip')
                        else:
                            df = pd.read_csv(self.file_path, header=0, on_bad_lines='skip')
                    except Exception as e:
                        logger.error(f"Failed to read CSV as tabular: {str(e)}")
                        self.error.emit(f"Could not parse CSV as tabular format: {str(e)}")
                        return
                else:
                    try:
                        engine = 'openpyxl' if self.file_path.lower().endswith('.xlsx') else 'xlrd'
                        temp_df = pd.read_excel(self.file_path, header=None, nrows=1, engine=engine)
                        if temp_df.iloc[0].notna().sum() == 1:
                            df = pd.read_excel(self.file_path, header=1, engine=engine)
                        else:
                            df = pd.read_excel(self.file_path, header=0, engine=engine)
                    except Exception as e:
                        logger.error(f"Failed to read Excel as tabular: {str(e)}")
                        self.error.emit(f"Could not parse Excel as tabular format: {str(e)}")
                        return

                self.progress.emit(preview_steps + parse_steps // 2, "Reading tabular data...")
                if self.is_canceled:
                    self.error.emit("File loading canceled by user")
                    return

                df = df.iloc[:-1]
                expected_columns = ["Solution Label", "Element", "Int", "Corr Con"]
                column_mapping = {"Sample ID": "Solution Label"}
                df.rename(columns=column_mapping, inplace=True)

                if not all(col in df.columns for col in expected_columns):
                    missing = set(expected_columns) - set(df.columns)
                    logger.error(f"Missing columns in tabular format: {missing}")
                    self.error.emit(f"Required columns missing: {', '.join(missing)}")
                    return

                total_rows = df.shape[0]
                rows_per_step = max(1, total_rows // (parse_steps // 2)) if total_rows > 0 else 1
                df['Element'] = df['Element'].apply(split_element_name)
                for idx in range(total_rows):
                    if self.is_canceled:
                        self.error.emit("File loading canceled by user")
                        return
                    if idx % rows_per_step == 0:
                        progress = preview_steps + parse_steps // 2 + (idx // rows_per_step)
                        self.progress.emit(min(progress, preview_steps + parse_steps), f"Processing row {idx}/{total_rows}")
                if 'Type' not in df.columns:
                    df['Type'] = df['Solution Label'].apply(lambda x: "Blk" if "BLANK" in str(x).upper() else "Sample")
                self.finished.emit(df, self.file_path)
                return

            if not data_rows and is_new_format:
                logger.error("No valid data rows were parsed")
                self.error.emit("No valid data found in the file")
                return

            df = pd.DataFrame(data_rows, columns=["Solution Label", "Element", "Int", "Corr Con", "Type"])
            total_rows = df.shape[0]
            rows_per_step = max(1, total_rows // (parse_steps // 2)) if total_rows > 0 else 1
            for idx in range(total_rows):
                if self.is_canceled:
                    self.error.emit("File loading canceled by user")
                    return
                df.loc[idx, 'Element'] = split_element_name(df.loc[idx, 'Element'])
                if idx % rows_per_step == 0:
                    progress = preview_steps + parse_steps // 2 + (idx // rows_per_step)
                    self.progress.emit(min(progress, preview_steps + parse_steps), f"Processing row {idx}/{total_rows}")
            self.finished.emit(df, self.file_path)

        except Exception as e:
            logger.error(f"Unexpected error in thread: {str(e)}")
            self.error.emit(f"Unexpected error: {str(e)}")


def load_excel(app):
    """Load and parse Excel/CSV file, update UI, and reset PivotTab filters."""
    logger.debug("Starting load_excel")

    # Reset application state
    app.reset_app_state()

    # پاک کردن تمام فیلترها و کش PivotTab
    if hasattr(app, 'pivot_tab') and app.pivot_tab:
        logger.debug("Resetting PivotTab filters and cache on new file load")
        app.pivot_tab.reset_cache()

    file_path, _ = QFileDialog.getOpenFileName(
        app,
        "Open File",
        "",
        "CSV files (*.csv);;Excel files (*.xlsx *.xls)"
    )

    if not file_path:
        logger.debug("No file selected")
        app.file_path_label.setText("File Path: No file selected")
        app.setWindowTitle("RASF Data Processor")
        return None

    app.file_path_label.setText(f"File Path: {file_path}")

    progress_dialog = QProgressDialog("Loading file and updating UI...", "Cancel", 0, 100, app)
    progress_dialog.setWindowTitle("Processing")
    progress_dialog.setWindowModality(Qt.WindowModality.WindowModal)
    progress_dialog.setMinimumDuration(0)
    progress_dialog.setValue(0)
    progress_dialog.show()

    worker = FileLoaderThread(file_path, app)

    def on_progress(value, message):
        progress_dialog.setValue(value)
        progress_dialog.setLabelText(message)
        if progress_dialog.wasCanceled():
            worker.cancel()

    def on_finished(df, file_path):
        try:
            app.data = df
            app.file_path = file_path
            logger.debug(f"Final DataFrame shape: {df.shape}")
            progress_dialog.setValue(80)
            progress_dialog.setLabelText("Updating UI...")

            ui_steps = 4
            step_value = (100 - 80) // ui_steps

            if hasattr(app, 'main_content'):
                if hasattr(app, 'elements_tab') and app.elements_tab:
                    app.elements_tab.process_blk_elements()
                else:
                    if hasattr(app, 'elements_tab'):
                        app.elements_tab.display_elements(["Cu", "Zn", "Fe"])
                progress_dialog.setValue(80 + step_value)
                progress_dialog.setLabelText("Updating elements tab...")

                if "Raw Data" in app.main_content.tab_subtab_map:
                    pivot_subtabs = app.main_content.tab_subtab_map["Raw Data"]["widgets"]
                    if "Display" in pivot_subtabs:
                        pivot_creator = PivotCreator(app.pivot_tab)
                        pivot_creator.create_pivot()
                progress_dialog.setValue(80 + 2 * step_value)
                progress_dialog.setLabelText("Creating pivot table...")

                if "Process" in app.main_content.tab_subtab_map:
                    process_subtabs = app.main_content.tab_subtab_map["Process"]["widgets"]
                    if "Weight Check" in process_subtabs:
                        if hasattr(app.results, 'show_processed_data'):
                            app.results.show_processed_data()
                progress_dialog.setValue(80 + 3 * step_value)
                progress_dialog.setLabelText("Updating process tab...")

                app.main_content.switch_tab("Process")
                progress_dialog.setValue(100)
                progress_dialog.setLabelText("Finalizing...")

            logger.info("File loaded successfully")
            app.setWindowTitle(f"RASF Data Processor - {os.path.basename(file_path)}")

        except Exception as e:
            logger.error(f"Error during UI update: {str(e)}")
            app.reset_app_state()
            QMessageBox.warning(app, "Error", f"Failed to update UI:\n{str(e)}")
        finally:
            progress_dialog.close()

    def on_error(error_message):
        progress_dialog.close()
        logger.error(f"Failed to load file: {error_message}")
        app.reset_app_state()
        QMessageBox.warning(app, "Error", f"Failed to load file:\n{error_message}")

    worker.progress.connect(on_progress)
    worker.finished.connect(on_finished)
    worker.error.connect(on_error)
    worker.start()

    return None


def load_additional(app):
    """Load additional CSV and append to existing data, reset PivotTab filters."""
    logger.debug("Starting load_additional")

    if app.data is None:
        QMessageBox.warning(app, "Warning", "Please open a file first before importing additional data.")
        return None

    # پاک کردن فیلترها و کش PivotTab
    if hasattr(app, 'pivot_tab') and app.pivot_tab:
        logger.debug("Resetting PivotTab filters and cache on additional file load")
        app.pivot_tab.reset_cache()

    file_path, _ = QFileDialog.getOpenFileName(
        app,
        "Import Additional CSV",
        "",
        "CSV files (*.csv)"
    )

    if not file_path:
        logger.debug("No additional file selected")
        return None

    app.file_path_label.setText(f"File Path: {app.file_path} + Additional: {os.path.basename(file_path)}")

    progress_dialog = QProgressDialog("Loading additional file and updating UI...", "Cancel", 0, 100, app)
    progress_dialog.setWindowTitle("Processing Additional")
    progress_dialog.setWindowModality(Qt.WindowModality.WindowModal)
    progress_dialog.setMinimumDuration(0)
    progress_dialog.setValue(0)
    progress_dialog.show()

    worker = FileLoaderThread(file_path, app)

    def on_progress(value, message):
        progress_dialog.setValue(value)
        progress_dialog.setLabelText(message)
        if progress_dialog.wasCanceled():
            worker.cancel()

    def on_finished_additional(df, additional_file_path):
        try:
            app.data = pd.concat([app.data, df], ignore_index=True)
            logger.debug(f"Appended additional data. New DataFrame shape: {app.data.shape}")

            # Reset process tabs
            if hasattr(app, 'weight_check'): app.weight_check.reset_state()
            if hasattr(app, 'volume_check'): app.volume_check.reset_state()
            if hasattr(app, 'df_check'): app.df_check.reset_state()
            if hasattr(app, 'results'): app.results.reset_state()

            progress_dialog.setValue(80)
            progress_dialog.setLabelText("Updating UI with additional data...")

            ui_steps = 4
            step_value = (100 - 80) // ui_steps

            if hasattr(app, 'main_content'):
                if hasattr(app, 'elements_tab') and app.elements_tab:
                    app.elements_tab.process_blk_elements()
                progress_dialog.setValue(80 + step_value)
                progress_dialog.setLabelText("Updating elements tab...")

                if "Raw Data" in app.main_content.tab_subtab_map:
                    pivot_subtabs = app.main_content.tab_subtab_map["Raw Data"]["widgets"]
                    if "Display" in pivot_subtabs:
                        pivot_creator = PivotCreator(app.pivot_tab)
                        pivot_creator.create_pivot()
                progress_dialog.setValue(80 + 2 * step_value)
                progress_dialog.setLabelText("Creating pivot table...")

                if "Process" in app.main_content.tab_subtab_map:
                    process_subtabs = app.main_content.tab_subtab_map["Process"]["widgets"]
                    if "Weight Check" in process_subtabs:
                        if hasattr(app.results, 'show_processed_data'):
                            app.results.show_processed_data()
                progress_dialog.setValue(80 + 3 * step_value)
                progress_dialog.setLabelText("Updating process tab...")

                app.main_content.switch_tab("Process")
                progress_dialog.setValue(100)
                progress_dialog.setLabelText("Finalizing...")

            logger.info("Additional file imported successfully")
            app.setWindowTitle(f"RASF Data Processor - {os.path.basename(app.file_path)} + Additional")

        except Exception as e:
            logger.error(f"Error during additional UI update: {str(e)}")
            QMessageBox.warning(app, "Error", f"Failed to update UI with additional data:\n{str(e)}")
        finally:
            progress_dialog.close()

    def on_error(error_message):
        progress_dialog.close()
        logger.error(f"Failed to load additional file: {error_message}")
        QMessageBox.warning(app, "Error", f"Failed to load additional file:\n{error_message}")

    worker.progress.connect(on_progress)
    worker.finished.connect(on_finished_additional)
    worker.error.connect(on_error)
    worker.start()

    return None