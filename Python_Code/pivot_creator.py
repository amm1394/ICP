import re
import pandas as pd
from PyQt6.QtWidgets import QMessageBox
from .oxide_factors import oxide_factors
import math
from functools import reduce

class PivotCreator:
    """Handles pivot table creation for the PivotTab."""
    def __init__(self, pivot_tab):
        self.pivot_tab = pivot_tab

    def create_pivot(self):
        """Create and populate the pivot table from the application data."""
        df = self.pivot_tab.app.get_data()
        if df is None or df.empty:
            QMessageBox.warning(self.pivot_tab, "Warning", "No data to display!")
            return

        try:
            self.pivot_tab.original_df = df.copy()
            df_filtered = df[df['Type'].isin(['Samp', 'Sample'])].copy()
            if df_filtered.empty:
                QMessageBox.warning(self.pivot_tab, "Warning", "No sample data found after filtering!")
                return

            df_filtered['original_index'] = df_filtered.index
            df_filtered = df_filtered.reset_index(drop=True)

            def calculate_set_size(solution_label, df_subset):
                counts = df_subset['Element'].value_counts().values
                if len(counts) > 0:
                    g = reduce(math.gcd, counts)
                    total_rows = len(df_subset)
                    if g > 0 and total_rows % g == 0:
                        most_common_size = total_rows // g
                    else:
                        most_common_size = total_rows
                else:
                    most_common_size = 1
                return most_common_size

            most_common_sizes = {}
            for solution_label in df_filtered['Solution Label'].unique():
                df_subset = df_filtered[df_filtered['Solution Label'] == solution_label]
                most_common_sizes[solution_label] = calculate_set_size(solution_label, df_subset)

            df_filtered['set_size'] = df_filtered['Solution Label'].map(most_common_sizes)
            element_counts = df_filtered.groupby(['Solution Label', df_filtered.groupby('Solution Label').cumcount() // df_filtered['set_size'], 'Element']).size().reset_index(name='count')
            has_repeats = (element_counts['count'] > 1).any()

            def clean_label(label):
                m = re.search(r'(\d+)', str(label).replace(' ', ''))
                if m:
                    return f"{label.split()[0]} {m.group(1)}"
                return label

            if not has_repeats:
                df_filtered['Element'] = df_filtered['Element'].str.split('_').str[0]
                df_filtered['unique_id'] = df_filtered.groupby(['Solution Label', 'Element']).cumcount()

                self.pivot_tab.solution_label_order = sorted(df_filtered['Solution Label'].drop_duplicates().apply(clean_label).unique().tolist())
                self.pivot_tab.element_order = df_filtered['Element'].drop_duplicates().tolist()

                value_column = 'Int' if self.pivot_tab.use_int_var.isChecked() else 'Corr Con'
                if value_column not in df_filtered.columns:
                    QMessageBox.warning(self.pivot_tab, "Error", f"Column '{value_column}' not found in data!")
                    return

                pivot_df = df_filtered.pivot_table(
                    index=['Solution Label', 'unique_id'],
                    columns='Element',
                    values=value_column,
                    aggfunc='first',
                    sort=False
                ).reset_index()
                pivot_df = pivot_df.merge(
                    df_filtered[['original_index', 'Solution Label', 'unique_id']],
                    on=['Solution Label', 'unique_id'],
                    how='left'
                ).sort_values('original_index').drop(columns=['original_index', 'unique_id']).drop_duplicates()

            else:
                df_filtered['group_id'] = 0
                for solution_label in df_filtered['Solution Label'].unique():
                    df_subset = df_filtered[df_filtered['Solution Label'] == solution_label].copy()
                    expected_size = most_common_sizes.get(solution_label, 1)
                    df_subset['group_id'] = df_subset.groupby('Solution Label').cumcount() // expected_size
                    df_filtered.loc[df_filtered['Solution Label'] == solution_label, 'group_id'] = df_subset['group_id']

                element_counts = df_filtered.groupby(['Solution Label', 'group_id', 'Element']).size().reset_index(name='count')
                df_filtered = df_filtered.merge(
                    element_counts[['Solution Label', 'group_id', 'Element', 'count']],
                    on=['Solution Label', 'group_id', 'Element'],
                    how='left'
                )
                df_filtered['count'] = df_filtered['count'].fillna(1).astype(int)
                df_filtered['element_count'] = df_filtered.groupby(['Solution Label', 'group_id', 'Element']).cumcount() + 1

                df_filtered['Element_with_id'] = df_filtered.apply(
                    lambda x: f"{x['Element']}_{x['element_count']}" if x['count'] > 1 else x['Element'],
                    axis=1
                )

                expected_columns_dict = {}
                for solution_label in df_filtered['Solution Label'].unique():
                    expected_size = most_common_sizes.get(solution_label, 1)
                    set_sizes_subset = df_filtered[df_filtered['Solution Label'] == solution_label].groupby('group_id').size().reset_index(name='set_size')
                    valid_groups = set_sizes_subset[set_sizes_subset['set_size'] == expected_size]['group_id']
                    if not valid_groups.empty:
                        first_group_id = valid_groups.min()
                        first_set_elements = df_filtered[
                            (df_filtered['Solution Label'] == solution_label) & 
                            (df_filtered['group_id'] == first_group_id)
                        ]['Element_with_id'].unique().tolist()
                        expected_columns_dict[solution_label] = first_set_elements
                    else:
                        expected_columns_dict[solution_label] = []

                self.pivot_tab.solution_label_order = df_filtered[['Solution Label', 'group_id']].drop_duplicates().sort_values('group_id')['Solution Label'].apply(clean_label).tolist()

                value_column = 'Int' if self.pivot_tab.use_int_var.isChecked() else 'Corr Con'
                if value_column not in df_filtered.columns:
                    QMessageBox.warning(self.pivot_tab, "Error", f"Column '{value_column}' not found in data!")
                    return

                pivot_dfs = []
                min_index_per_group = {}
                for solution_label, expected_columns in expected_columns_dict.items():
                    if not expected_columns:
                        continue
                    df_subset = df_filtered[df_filtered['Solution Label'] == solution_label].copy()
                    self.pivot_tab.element_order = expected_columns
                    min_index_per_group[solution_label] = df_subset.groupby('group_id')['original_index'].min().to_dict()

                    pivot_subset = df_subset.pivot_table(
                        index=['Solution Label', 'group_id'],
                        columns='Element_with_id',
                        values=value_column,
                        aggfunc='first',
                        sort=False
                    )
                    pivot_subset = pivot_subset.reset_index()
                    pivot_subset = pivot_subset.reindex(columns=['Solution Label', 'group_id'] + expected_columns)
                    pivot_subset['min_original_index'] = pivot_subset['group_id'].map(min_index_per_group[solution_label])
                    pivot_dfs.append(pivot_subset)

                if not pivot_dfs:
                    QMessageBox.warning(self.pivot_tab, "Error", "No valid pivot tables created!")
                    return
                pivot_df = pd.concat(pivot_dfs, ignore_index=False)

                if 'min_original_index' in pivot_df.columns:
                    pivot_df = pivot_df.sort_values(by='min_original_index').reset_index(drop=True)
                columns_to_drop = [col for col in ['group_id', 'min_original_index'] if col in pivot_df.columns]
                if columns_to_drop:
                    pivot_df = pivot_df.drop(columns=columns_to_drop)

            if self.pivot_tab.use_oxide_var.isChecked():
                rename_dict = {}
                for col in pivot_df.columns:
                    if col != 'Solution Label':
                        element = col.split()[0]
                        if element in oxide_factors:
                            oxide_formula, factor = oxide_factors[element]
                            suffix = col.split('_')[-1] if '_' in col and has_repeats else ''
                            new_col = f"{oxide_formula}_{suffix}" if suffix else oxide_formula
                            rename_dict[col] = new_col
                            pivot_df[col] = pd.to_numeric(pivot_df[col], errors='coerce') * factor
                pivot_df.rename(columns=rename_dict, inplace=True)

            self.pivot_tab.pivot_data = pivot_df
            self.pivot_tab.column_widths.clear()
            self.pivot_tab.cached_formatted.clear()
            # self.pivot_tab._inline_crm_rows.clear()
            # self.pivot_tab._inline_crm_rows_display.clear()
            self.pivot_tab.row_filter_values.clear()
            self.pivot_tab.column_filter_values.clear()
            self.pivot_tab.update_pivot_display()

        except Exception as e:
            QMessageBox.warning(self.pivot_tab, "Pivot Error", f"Failed to create pivot table: {str(e)}")