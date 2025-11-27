# project_manager.py
import joblib
import os
import logging
from datetime import datetime
import pandas as pd
import numpy as np
from PyQt6.QtWidgets import QFileDialog, QMessageBox, QCheckBox

logger = logging.getLogger(__name__)


def _is_serializable(obj):
    """Check if an object can be safely serialized (no PyQt widgets)."""
    if obj is None:
        return True
    if isinstance(obj, (str, int, float, bool, list, dict, tuple, set)):
        return True
    if isinstance(obj, (pd.DataFrame, pd.Series, np.ndarray, np.generic)):
        return True
    return False


def save_project(app):
    """Save the complete project – only logical data (no GUI widgets)."""
    if app.data is None or app.data.empty:
        QMessageBox.warning(app, "Warning", "No data to save.\nPlease open a file first.")
        return

    # Suggest name
    if app.file_path and os.path.isfile(app.file_path):
        base_name = os.path.splitext(os.path.basename(app.file_path))[0]
        default_name = f"{base_name}.RASF"
        default_dir = os.path.dirname(app.file_path)
    else:
        default_name = f"RASF_Project_{datetime.now().strftime('%Y%m%d_%H%M%S')}.RASF"
        default_dir = os.path.join(os.path.expanduser("~"), "Desktop")

    file_path, _ = QFileDialog.getSaveFileName(
        app, "Save Project", os.path.join(default_dir, default_name), "RASF Project Files (*.RASF)"
    )
    if not file_path:
        return
    if not file_path.lower().endswith('.rasf'):
        file_path += '.RASF'

    try:
        project_data = {
            'main_window': {
                'data': app.data,
                'file_path': app.file_path,
            },
            'timestamp': datetime.now().isoformat(),
            'version': '1.5',  # نسخه نهایی
            'tabs': {}
        }

        # === Tab-specific state keys ===
        tab_states = {
            'pivot_tab': [
                'original_pivot_data', 'pivot_data',
                'filters', 'column_widths',
                'row_filter_values', 'column_filter_values',
                '_inline_duplicates_display',
            ],
            'elements_tab': ['blk_elements', 'selected_elements'],
            'weight_check': ['excluded_samples', 'weight_data', 'corrected_weights'],
            'volume_check': ['excluded_volumes', 'volume_data', 'corrected_volumes'],
            'df_check': ['excluded_dfs', 'df_data', 'corrected_dfs'],
            'empty_check': ['empty_rows'],
            'crm_check': [
                'corrected_crm', '_inline_crm_rows', '_inline_crm_rows_display',
                'included_crms', 'column_widths', 'crm_selections',
                'range_low', 'range_mid', 'range_high1', 'range_high2', 'range_high3', 'range_high4',
                'scale_range_min', 'scale_range_max', 'scale_above_50',
                'excluded_outliers', 'excluded_from_correct',
                'preview_blank', 'preview_scale'
            ],
            'rm_check': [
                'rm_df', 'positions_df', 'original_df', 'corrected_df', 'pivot_df',
                'initial_rm_df', 'empty_rows_from_check', 'corrected_drift',
                'undo_stack', 'navigation_list', 'current_nav_index',
                'selected_element', 'current_label', 'elements', 'solution_labels',
                'selected_row', 'original_rm_values', 'display_rm_values',
                'current_valid_row_ids', 'current_slope', 'keyword',
                'stepwise_state'
            ],
            'results': [
                'search_var', 'filter_field', 'filter_values', 'column_filters',
                'column_widths', 'solution_label_order', 'element_order',
                'decimal_places', 'last_filtered_data', 'last_pivot_data',
                '_last_cache_key', 'data_hash'
            ],
            'report': ['report_data'],
            'compare_tab': ['comparison_results'],
            'crm_tab': ['crm_database_path'],
        }

        for tab_name, keys in tab_states.items():
            tab_obj = getattr(app, tab_name, None)
            if tab_obj:
                state = {}
                for key in keys:
                    if hasattr(tab_obj, key):
                        value = getattr(tab_obj, key)
                        if _is_serializable(value):
                            if isinstance(value, np.ndarray):
                                value = value.tolist()
                            elif isinstance(value, set):
                                value = list(value)
                            elif isinstance(value, pd.DataFrame):
                                value = value.copy(deep=True)
                            elif hasattr(value, 'isChecked'):
                                value = value.isChecked()
                            elif key == 'included_crms' and isinstance(value, dict):
                                # فقط مقدار bool ذخیره شود
                                value = {k: v.isChecked() if hasattr(v, 'isChecked') else v for k, v in value.items()}
                            state[key] = value

                # === Special: PivotTab UI States ===
                if tab_name == 'pivot_tab':
                    if hasattr(tab_obj, 'decimal_places') and tab_obj.decimal_places:
                        state['decimal_places'] = tab_obj.decimal_places.currentText()
                    if hasattr(tab_obj, 'use_int_var') and tab_obj.use_int_var:
                        state['use_int'] = tab_obj.use_int_var.isChecked()
                    if hasattr(tab_obj, 'use_oxide_var') and tab_obj.use_oxide_var:
                        state['use_oxide'] = tab_obj.use_oxide_var.isChecked()
                    if hasattr(tab_obj, 'duplicate_threshold_edit') and tab_obj.duplicate_threshold_edit:
                        try:
                            state['duplicate_threshold'] = float(tab_obj.duplicate_threshold_edit.text() or 10)
                        except:
                            state['duplicate_threshold'] = 10.0
                    if hasattr(tab_obj, 'search_var') and tab_obj.search_var:
                        state['search_text'] = tab_obj.search_var.text()

                # === Special: RM Stepwise Checkbox ===
                if tab_name == 'rm_check' and hasattr(tab_obj, 'stepwise_checkbox'):
                    state['stepwise_state'] = tab_obj.stepwise_checkbox.isChecked()

                # === Special: CRM Text Inputs ===
                if tab_name == 'crm_check':
                    if hasattr(tab_obj, 'crm_diff_min') and tab_obj.crm_diff_min:
                        state['crm_diff_min_text'] = tab_obj.crm_diff_min.text()
                    if hasattr(tab_obj, 'crm_diff_max') and tab_obj.crm_diff_max:
                        state['crm_diff_max_text'] = tab_obj.crm_diff_max.text()

                project_data['tabs'][tab_name] = state

        joblib.dump(project_data, file_path, compress=6)
        logger.info(f"Project saved: {file_path}")
        QMessageBox.information(app, "Success", f"Project saved:\n{os.path.basename(file_path)}")
        app.setWindowTitle(f"RASF Data Processor - {os.path.basename(file_path)}")

    except Exception as e:
        logger.error(f"Error saving project: {str(e)}", exc_info=True)
        QMessageBox.critical(app, "Error", f"Saving failed:\n{str(e)}")


def load_project(app):
    """Load a complete project – only logical data."""
    file_path, _ = QFileDialog.getOpenFileName(
        app, "Load Project", "", "RASF Project Files (*.RASF)"
    )
    if not file_path:
        return

    try:
        project_data = joblib.load(file_path)
        logger.debug(f"Project loaded: {file_path}")

        # Full reset
        app.reset_app_state()

        # Restore main data
        main_state = project_data.get('main_window', {})
        if 'data' in main_state:
            app.data = main_state['data']
        if 'file_path' in main_state:
            app.file_path = main_state['file_path']

        project_name = os.path.basename(file_path)
        app.file_path_label.setText(f"Project: {project_name}")
        app.setWindowTitle(f"RASF Data Processor - {project_name}")

        # === UI Keys that must NOT be set with setattr ===
        ui_keys = [
            'decimal_places', 'use_int_var', 'use_oxide_var',
            'duplicate_threshold_edit', 'search_var',
            'crm_diff_min', 'crm_diff_max', 'stepwise_checkbox', 'keyword_entry',
            'included_crms'  # این را هم اضافه کردیم
        ]

        # Restore tab states
        for tab_name, state in project_data.get('tabs', {}).items():
            tab_obj = getattr(app, tab_name, None)
            if tab_obj and isinstance(state, dict):

                # === Restore logical data (safe with setattr) ===
                for key, value in state.items():
                    if key in ui_keys:
                        continue  # Skip UI widgets
                    if hasattr(tab_obj, key):
                        if key in ['original_rm_values', 'display_rm_values'] and isinstance(value, list):
                            setattr(tab_obj, key, np.array(value, dtype=float))
                        elif key in ['current_valid_row_ids'] and isinstance(value, list):
                            setattr(tab_obj, key, np.array(value, dtype=int))
                        elif key in ['excluded_outliers'] and isinstance(value, dict):
                            setattr(tab_obj, key, {k: set(v) for k, v in value.items()})
                        elif key == 'excluded_from_correct' and isinstance(value, list):
                            setattr(tab_obj, key, set(value))
                        else:
                            setattr(tab_obj, key, value)

                # === Restore PivotTab UI & Data ===
                if tab_name == 'pivot_tab':
                    if 'original_pivot_data' in state and state['original_pivot_data'] is not None:
                        tab_obj.original_pivot_data = state['original_pivot_data']
                    if 'pivot_data' in state and state['pivot_data'] is not None:
                        tab_obj.pivot_data = state['pivot_data']
                    else:
                        tab_obj.pivot_data = tab_obj.original_pivot_data.copy(deep=True) if tab_obj.original_pivot_data is not None else None

                    if 'decimal_places' in state and hasattr(tab_obj, 'decimal_places') and hasattr(tab_obj.decimal_places, 'setCurrentText'):
                        decimal_str = state['decimal_places']
                        if tab_obj.decimal_places.findText(decimal_str) != -1:
                            tab_obj.decimal_places.setCurrentText(decimal_str)
                        else:
                            tab_obj.decimal_places.setCurrentIndex(2)

                    if 'use_int' in state and hasattr(tab_obj, 'use_int_var') and hasattr(tab_obj.use_int_var, 'setChecked'):
                        tab_obj.use_int_var.setChecked(state['use_int'])

                    if 'use_oxide' in state and hasattr(tab_obj, 'use_oxide_var') and hasattr(tab_obj.use_oxide_var, 'setChecked'):
                        tab_obj.use_oxide_var.setChecked(state['use_oxide'])

                    if 'duplicate_threshold' in state and hasattr(tab_obj, 'duplicate_threshold_edit') and hasattr(tab_obj.duplicate_threshold_edit, 'setText'):
                        threshold_val = state['duplicate_threshold']
                        tab_obj.duplicate_threshold = threshold_val
                        tab_obj.duplicate_threshold_edit.setText(str(threshold_val))

                    if 'search_text' in state and hasattr(tab_obj, 'search_var') and hasattr(tab_obj.search_var, 'setText'):
                        tab_obj.search_var.setText(state['search_text'])

                    if 'filters' in state:
                        tab_obj.filters = state['filters']
                    if 'column_widths' in state:
                        tab_obj.column_widths = state['column_widths']
                    if 'row_filter_values' in state:
                        tab_obj.row_filter_values = state['row_filter_values']
                    if 'column_filter_values' in state:
                        tab_obj.column_filter_values = state['column_filter_values']
                    if '_inline_duplicates_display' in state:
                        tab_obj._inline_duplicates_display = state['_inline_duplicates_display']

                    tab_obj.update_pivot_display()

                # === Restore CRM Check UI ===
                if tab_name == 'crm_check':
                    if 'crm_diff_min_text' in state and hasattr(tab_obj, 'crm_diff_min') and hasattr(tab_obj.crm_diff_min, 'setText'):
                        tab_obj.crm_diff_min.setText(state['crm_diff_min_text'])
                    if 'crm_diff_max_text' in state and hasattr(tab_obj, 'crm_diff_max') and hasattr(tab_obj.crm_diff_max, 'setText'):
                        tab_obj.crm_diff_max.setText(state['crm_diff_max_text'])

                    # Rebuild included_crms checkboxes
                    if 'included_crms' in state and isinstance(state['included_crms'], dict):
                        tab_obj.included_crms = {}  # پاک کن
                        for label, checked in state['included_crms'].items():
                            checkbox = QCheckBox(label)
                            checkbox.setChecked(bool(checked))
                            tab_obj.included_crms[label] = checkbox
                        # اگر UI قبلاً ساخته شده، باید دوباره نمایش داده شود
                        if hasattr(tab_obj, 'update_crm_checkboxes'):
                            tab_obj.update_crm_checkboxes()

                    if hasattr(tab_obj, 'current_plot_window') and tab_obj.current_plot_window:
                        plot_win = tab_obj.current_plot_window
                        for attr in ['range_low', 'range_mid', 'range_high1', 'range_high2',
                                     'range_high3', 'range_high4', 'scale_range_min', 'scale_range_max',
                                     'preview_blank', 'preview_scale', 'excluded_outliers',
                                     'excluded_from_correct', 'scale_above_50']:
                            if attr in state:
                                val = state[attr]
                                if attr == 'excluded_outliers' and isinstance(val, dict):
                                    val = {k: set(v) for k, v in val.items()}
                                elif attr == 'excluded_from_correct' and isinstance(val, list):
                                    val = set(val)
                                setattr(plot_win, attr, val)
                        plot_win.blank_edit.setText(f"{plot_win.preview_blank:.3f}")
                        plot_win.scale_slider.setValue(int(plot_win.preview_scale * 100))
                        plot_win.scale_label.setText(f"Scale: {plot_win.preview_scale:.2f}")
                        if plot_win.scale_range_min is not None:
                            plot_win.scale_range_min_edit.setText(str(plot_win.scale_range_min))
                        if plot_win.scale_range_max is not None:
                            plot_win.scale_range_max_edit.setText(str(plot_win.scale_range_max))
                        plot_win.scale_range_display.setText(
                            f"Scale Range: [{plot_win.scale_range_min} to {plot_win.scale_range_max}]"
                            if plot_win.scale_range_min and plot_win.scale_range_max else "Scale Range: Not Set"
                        )
                        if hasattr(plot_win, 'scale_above_50'):
                            plot_win.scale_above_50.setChecked(state.get('scale_above_50', False))

                # === RM Check: Full UI Restore ===
                if tab_name == 'rm_check':
                    if 'stepwise_state' in state and hasattr(tab_obj, 'stepwise_checkbox') and hasattr(tab_obj.stepwise_checkbox, 'setChecked'):
                        tab_obj.stepwise_checkbox.setChecked(state['stepwise_state'])
                    if 'keyword' in state and hasattr(tab_obj, 'keyword_entry') and hasattr(tab_obj.keyword_entry, 'setText'):
                        tab_obj.keyword_entry.setText(state['keyword'])
                    if 'undo_stack' in state:
                        restored = []
                        for cdf, rdf, cdrift in state['undo_stack']:
                            restored.append((cdf.copy(deep=True), rdf.copy(deep=True), cdrift.copy()))
                        tab_obj.undo_stack = restored
                        tab_obj.undo_button.setEnabled(len(restored) > 0)

                    if 'elements' in state and 'solution_labels' in state:
                        tab_obj.elements = state['elements']
                        tab_obj.solution_labels = state['solution_labels']
                        tab_obj.navigation_list = [(el, lb) for el in tab_obj.elements for lb in tab_obj.solution_labels]
                        tab_obj.current_nav_index = state.get('current_nav_index', 0)
                        if tab_obj.navigation_list:
                            idx = min(tab_obj.current_nav_index, len(tab_obj.navigation_list) - 1)
                            tab_obj.selected_element, tab_obj.current_label = tab_obj.navigation_list[idx]
                        else:
                            tab_obj.selected_element = tab_obj.elements[0] if tab_obj.elements else None
                            tab_obj.current_label = tab_obj.solution_labels[0] if tab_obj.solution_labels else None

                    if hasattr(tab_obj, 'element_combo') and tab_obj.elements:
                        tab_obj.element_combo.blockSignals(True)
                        tab_obj.element_combo.clear()
                        tab_obj.element_combo.addItems(tab_obj.elements)
                        if tab_obj.selected_element:
                            tab_obj.element_combo.setCurrentText(tab_obj.selected_element)
                        tab_obj.element_combo.blockSignals(False)

                    if (hasattr(tab_obj, 'rm_df') and tab_obj.rm_df is not None and 
                        not tab_obj.rm_df.empty and 
                        hasattr(tab_obj, 'current_label') and tab_obj.current_label and
                        'Solution Label' in tab_obj.rm_df.columns and
                        tab_obj.current_label in tab_obj.rm_df['Solution Label'].values):
                        tab_obj.update_labels()
                        tab_obj.display_rm_table()
                        tab_obj.update_plot()
                        tab_obj.update_detail_plot()
                        tab_obj.update_detail_table()
                        tab_obj.update_navigation_buttons()
                        tab_obj.auto_optimize_flat_button.setEnabled(True)
                        tab_obj.auto_optimize_zero_button.setEnabled(True)
                    else:
                        tab_obj.update_labels()
                        tab_obj.update_navigation_buttons()
                        tab_obj.auto_optimize_flat_button.setEnabled(False)
                        tab_obj.auto_optimize_zero_button.setEnabled(False)

        # Final refresh
        app.notify_data_changed()

        # Restore ResultsFrame
        if hasattr(app, 'results') and app.results:
            results = app.results
            if results.last_filtered_data is not None and not results.last_filtered_data.empty:
                results.update_table(results.last_filtered_data)
            else:
                results.show_processed_data()
            if hasattr(results, 'search_entry') and results.search_var:
                results.search_entry.setText(results.search_var)
            if hasattr(results, 'decimal_combo') and results.decimal_places:
                results.decimal_combo.setCurrentText(results.decimal_places)

        # === PivotTab: فقط نمایش بروز شود ===
        if hasattr(app, 'pivot_tab') and app.pivot_tab:
            app.pivot_tab.update_pivot_display()

        # Restore ElementsTab
        if hasattr(app, 'elements_tab') and app.elements_tab:
            app.elements_tab.process_blk_elements()

        # CRM Check: Update display
        if hasattr(app, 'crm_check') and app.crm_check:
            app.crm_check.update_pivot_display()

        app.main_content.switch_tab("Process")

        QMessageBox.information(app, "Success", f"Project loaded:\n{project_name}")
        logger.info(f"Project fully loaded: {file_path}")

    except Exception as e:
        logger.error(f"Error loading project: {str(e)}", exc_info=True)
        QMessageBox.critical(app, "Error", f"Loading failed:\n{str(e)}")
        app.reset_app_state()