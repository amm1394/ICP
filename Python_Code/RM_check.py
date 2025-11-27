from PyQt6.QtWidgets import (
    QWidget, QFrame, QVBoxLayout, QHBoxLayout, QPushButton, QLineEdit, QLabel,
    QTableView, QHeaderView, QMessageBox, QGroupBox, QDoubleSpinBox, QProgressDialog, QCheckBox,
    QComboBox
)
from PyQt6.QtCore import Qt, QThread, pyqtSignal
from PyQt6.QtGui import QFont, QStandardItemModel, QStandardItem, QColor
import pyqtgraph as pg
import pandas as pd
import numpy as np
import logging
from functools import reduce
import math
import re

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
    QTableView {
        background-color: #FFFFFF;
        gridline-color: #D0D7DE;
        selection-background-color: #E5E7EB;
        selection-color: #000000;
    }
    QTableView::item:selected {
        color: #000000;
        background-color: #E5E7EB;
    }
    QComboBox::item:selected {
        color: #000000;
    }
    QHeaderView::section {
        background-color: #E5E7EB;
        padding: 4px;
        border: 1px solid #D0D7DE;
    }
    QDoubleSpinBox {
        padding: 6px;
        border: 1px solid #D0D7DE;
        border-radius: 4px;
        font-size: 13px;
    }
    QDoubleSpinBox:focus {
        border: 1px solid #2E7D32;
    }
"""

def extract_rm_info(label, keyword="RM"):
    """
    استخراج عدد و نوع RM از Solution Label
    مثال‌ها:
        RM1 → (1, 'Base')
        RM1check → (1, 'Check')
        RM2 cone → (2, 'Cone')
        RMcheck → (0, 'Check')
        RM → (0, 'Base')
    """
    label = str(label).strip()
    label_lower = label.lower()
    # حذف keyword از اول (RM, rm, Rm, ...)
    cleaned = re.sub(rf'^{re.escape(keyword)}\s*[-_]?\s*', '', label_lower, flags=re.IGNORECASE)
    rm_type = 'Base'
    rm_number = 0
    # تشخیص نوع (check/cone) — حتی چسبیده
    type_match = re.search(r'(chek|check|cone)', cleaned)
    if type_match:
        typ = type_match.group(1)
        rm_type = 'Check' if typ in ['chek', 'check'] else 'Cone'
        before_text = cleaned[:type_match.start()]
    else:
        before_text = cleaned
    # استخراج عدد از قبل نوع
    numbers = re.findall(r'\d+', before_text)
    if numbers:
        rm_number = int(numbers[-1])
    return rm_number, rm_type

class CheckRMThread(QThread):
    progress = pyqtSignal(int)
    finished = pyqtSignal(dict)
    error = pyqtSignal(str)

    def __init__(self, app, keyword):
        super().__init__()
        self.app = app
        self.keyword = keyword

    def run(self):
        try:
            df = self.app.get_data()
            if df is None or df.empty:
                self.error.emit("No data loaded.")
                return

            required_columns = ['Solution Label', 'Element', 'Type', 'Corr Con']
            missing_columns = [col for col in required_columns if col not in df.columns]
            if missing_columns:
                self.error.emit(f"Missing required columns: {missing_columns}")
                return

            # --- مرحله 1: آماده‌سازی original_df ---
            original_df = df.copy(deep=True)
            for col in ['original_index', 'row_id']:
                if col in original_df.columns:
                    original_df = original_df.drop(columns=[col])
            original_df = original_df.reset_index(drop=True)
            original_df['original_index'] = original_df.index

            # --- مرحله 2: فیلتر داده‌های Samp ---
            df_filtered = df[df['Type'].isin(['Samp', 'Sample'])].copy(deep=True)
            if df_filtered.empty:
                self.error.emit("No data with Type='Samp' found.")
                return
            for col in ['original_index', 'row_id']:
                if col in df_filtered.columns:
                    df_filtered = df_filtered.drop(columns=[col])
            df_filtered = df_filtered.reset_index(drop=True)
            df_filtered['original_index'] = df_filtered.index

            # --- مرحله 3: تمیز کردن Solution Label ---
            df_filtered['Solution Label'] = df_filtered['Solution Label'].str.replace(
                rf'^{self.keyword}(?:\s*[-]?\s*(\d+|\w+\)?))?(?:\s*{self.keyword}.*)?$',
                rf'{self.keyword}\1',
                regex=True
            )

            # --- مرحله 4: محاسبه set_size ---
            def calculate_set_size(df_subset):
                counts = df_subset['Element'].value_counts().values
                if len(counts) == 0:
                    return 1
                g = reduce(math.gcd, counts)
                total = len(df_subset)
                return total // g if g > 0 and total % g == 0 else total

            most_common_sizes = {}
            for sl in df_filtered['Solution Label'].unique():
                sub = df_filtered[df_filtered['Solution Label'] == sl]
                most_common_sizes[sl] = calculate_set_size(sub)
            df_filtered['set_size'] = df_filtered['Solution Label'].map(most_common_sizes)

            # --- مرحله 5: بررسی تکرار عناصر ---
            temp_group = df_filtered.groupby('Solution Label').cumcount() // df_filtered['set_size']
            element_counts = df_filtered.groupby(['Solution Label', temp_group, 'Element']).size().reset_index(name='count')
            has_repeats = (element_counts['count'] > 1).any()

            # --- مرحله 6: ادغام با original_df و ساخت corrected_df ---
            df_filtered['row_id'] = df_filtered.groupby(['Solution Label', 'Element']).cumcount()
            corrected_df = df_filtered.copy(deep=True)

            original_df = original_df.merge(
                df_filtered[['original_index', 'Solution Label', 'Element', 'row_id']],
                on=['Solution Label', 'Element', 'original_index'],
                how='left'
            )
            original_df['row_id'] = original_df['row_id'].fillna(-1).astype(int)

            # --- مرحله 7: تبدیل Corr Con ---
            df_filtered['Corr Con'] = pd.to_numeric(df_filtered['Corr Con'], errors='coerce')

            # --- مرحله 8: ساخت pivot_df ---
            if not has_repeats:
                df_filtered['group_id'] = df_filtered.groupby('Solution Label').cumcount() // df_filtered['set_size']
                pivot_df = df_filtered.pivot_table(
                    index=['Solution Label', 'group_id'],
                    columns='Element',
                    values='Corr Con',
                    aggfunc='first'
                ).reset_index()
                min_idx = df_filtered.groupby(['Solution Label', 'group_id'])['original_index'].min()
                pivot_df['original_index'] = pivot_df.apply(
                    lambda row: min_idx.get((row['Solution Label'], row['group_id'])), axis=1
                )
                pivot_df = pivot_df.sort_values('original_index').reset_index(drop=True)
                pivot_df = pivot_df.drop(columns=['group_id'])
            else:
                df_filtered['group_id'] = 0
                for sl in df_filtered['Solution Label'].unique():
                    sub = df_filtered[df_filtered['Solution Label'] == sl].copy()
                    size = most_common_sizes.get(sl, 1)
                    sub['group_id'] = (sub.index // size).astype(int)
                    df_filtered.loc[sub.index, 'group_id'] = sub['group_id']
                counts = df_filtered.groupby(['Solution Label', 'group_id', 'Element']).size().reset_index(name='count')
                df_filtered = df_filtered.merge(counts, on=['Solution Label', 'group_id', 'Element'])
                df_filtered['element_count'] = df_filtered.groupby(['Solution Label', 'group_id', 'Element']).cumcount() + 1
                df_filtered['Element_with_id'] = df_filtered.apply(
                    lambda x: f"{x['Element']}_{x['element_count']}" if x['count'] > 1 else x['Element'], axis=1
                )
                expected_cols = {}
                for sl in df_filtered['Solution Label'].unique():
                    sub = df_filtered[df_filtered['Solution Label'] == sl]
                    valid = sub.groupby('group_id').size()
                    valid = valid[valid == most_common_sizes.get(sl, 1)]
                    if not valid.empty:
                        first = valid.index.min()
                        cols = sub[sub['group_id'] == first]['Element_with_id'].unique().tolist()
                        expected_cols[sl] = cols
                pivot_dfs = []
                for sl, cols in expected_cols.items():
                    if not cols:
                        continue
                    sub = df_filtered[df_filtered['Solution Label'] == sl]
                    p = sub.pivot_table(
                        index=['Solution Label', 'group_id'],
                        columns='Element_with_id',
                        values='Corr Con',
                        aggfunc='first'
                    ).reset_index()
                    p = p.reindex(columns=['Solution Label', 'group_id'] + cols)
                    min_idx = sub.groupby('group_id')['original_index'].min()
                    p['original_index'] = p['group_id'].map(min_idx)
                    pivot_dfs.append(p)
                if not pivot_dfs:
                    self.error.emit("No valid pivot tables created!")
                    return
                pivot_df = pd.concat(pivot_dfs, ignore_index=True)
                pivot_df = pivot_df.sort_values('original_index').reset_index(drop=True)
                pivot_df = pivot_df.drop(columns=['group_id'], errors='ignore')

            # --- مرحله 9: استخراج num و نوع برای همه RMها ---
            rm_data = df_filtered[
                df_filtered['Solution Label'].str.match(rf'^{re.escape(self.keyword)}', na=False, flags=re.IGNORECASE)
            ].copy()
            if not rm_data.empty:
                info = rm_data['Solution Label'].apply(extract_rm_info, keyword=self.keyword)
                rm_data[['rm_num', 'rm_type']] = pd.DataFrame(info.tolist(), index=rm_data.index)
                rm_data['rm_num'] = rm_data['rm_num'].astype(int)
                # فقط Baseها رو برای keep فیلتر کن
                base_data = rm_data[rm_data['rm_type'] == 'Base']
                check_cone_data = rm_data[rm_data['rm_type'].isin(['Check', 'Cone'])]
                keep_nums = []
                if not base_data.empty:
                    base_valid = base_data.loc[base_data.groupby('rm_num')['original_index'].idxmin()]
                    nums = sorted(base_valid['rm_num'].unique())
                    prev = 0
                    for num in nums:
                        if num >= prev:
                            keep_nums.append(num)
                            prev = num
                        elif num == 1 and prev == max(nums):
                            keep_nums.append(num)
                            prev = num
                # همه Check و Cone نگه داشته بشن
                valid_rm_labels = []
                if not base_data.empty:
                    valid_rm_labels.extend(base_data[base_data['rm_num'].isin(keep_nums)]['Solution Label'].unique().tolist())
                if not check_cone_data.empty:
                    valid_rm_labels.extend(check_cone_data['Solution Label'].unique().tolist())
            else:
                valid_rm_labels = []

            # --- مرحله 10: ساخت rm_df با فیلتر گسترده ---
            pivot_df['Solution Label'] = pivot_df['Solution Label'].fillna('')
            rm_df = pivot_df[
                pivot_df['Solution Label'].str.match(rf'^{re.escape(self.keyword)}', na=False, flags=re.IGNORECASE)
            ].copy()
            if rm_df.empty:
                labels = df_filtered['Solution Label'].unique().tolist()
                self.error.emit(f"No {self.keyword} found. Labels: {labels[:10]}{'...' if len(labels)>10 else ''}")
                return

            pivot_df = pivot_df.reset_index(drop=True)
            pivot_df['pivot_index'] = pivot_df.index

            # --- مرحله 11: تبدیل ستون‌ها ---
            element_cols = [c for c in rm_df.columns if c not in ['Solution Label', 'original_index', 'pivot_index', 'row_id']]
            for c in element_cols:
                rm_df[c] = pd.to_numeric(rm_df[c], errors='coerce')
                pivot_df[c] = pd.to_numeric(pivot_df[c], errors='coerce')
            solution_labels = sorted(rm_df['Solution Label'].unique(),
                                    key=lambda x: extract_rm_info(x, self.keyword)[0])

            # --- مرحله 12: ساخت positions_df با فیلتر keep فقط برای Base ---
            positions_df = df_filtered.groupby(['Solution Label', 'row_id'])['original_index'].agg(['min', 'max']).reset_index()
            rm_positions = positions_df[
                positions_df['Solution Label'].str.match(rf'^{re.escape(self.keyword)}', na=False, flags=re.IGNORECASE)
            ].copy()
            if not rm_positions.empty:
                info_pos = rm_positions['Solution Label'].apply(extract_rm_info, keyword=self.keyword)
                rm_positions[['rm_num', 'rm_type']] = pd.DataFrame(info_pos.tolist(), index=rm_positions.index)
                rm_positions['rm_num'] = rm_positions['rm_num'].astype(int)
                base_pos = rm_positions[rm_positions['rm_type'] == 'Base']
                check_cone_pos = rm_positions[rm_positions['rm_type'].isin(['Check', 'Cone'])]
                keep_mask = pd.Series([True] * len(rm_positions), index=rm_positions.index)
                if not base_pos.empty:
                    sorted_base = base_pos.sort_values('min')
                    nums = sorted_base['rm_num'].values
                    prev = 0
                    base_keep = []
                    for num in nums:
                        if num >= prev:
                            base_keep.append(True)
                            prev = num
                        else:
                            if num == 1 and prev == max(nums):
                                base_keep.append(True)
                                prev = num
                            else:
                                base_keep.append(False)
                    sorted_base['keep'] = base_keep
                    keep_mask.loc[sorted_base.index] = sorted_base['keep']
                rm_positions['keep'] = keep_mask
                positions_df = positions_df.merge(rm_positions[['Solution Label', 'row_id', 'keep']], on=['Solution Label', 'row_id'], how='left')
                positions_df['keep'] = positions_df['keep'].fillna(True)
                positions_df = positions_df[positions_df['keep']].drop(columns=['keep'])
                df_filtered = df_filtered.merge(rm_positions[['Solution Label', 'row_id', 'keep']], on=['Solution Label', 'row_id'], how='left')
                df_filtered['keep'] = df_filtered['keep'].fillna(True)
                df_filtered = df_filtered[df_filtered['keep']].drop(columns=['keep'])
                corrected_df = df_filtered.copy(deep=True)
            # Add rm_num and rm_type to corrected_df
            corrected_df['rm_num'] = np.nan
            corrected_df['rm_type'] = np.nan
            mask = corrected_df['Solution Label'].str.match(rf'^{re.escape(self.keyword)}', na=False, flags=re.IGNORECASE)
            if mask.any():
                info = corrected_df.loc[mask, 'Solution Label'].apply(lambda x: extract_rm_info(x, keyword=self.keyword))
                corrected_df.loc[mask, ['rm_num', 'rm_type']] = pd.DataFrame(info.tolist(), index=corrected_df.loc[mask].index)
                corrected_df['rm_num'] = corrected_df['rm_num'].astype(float)
            # --- مرحله 13: بازسازی rm_df بدون فیلتر keep ---
            rm_df = pivot_df[
                pivot_df['Solution Label'].str.match(rf'^{re.escape(self.keyword)}', na=False, flags=re.IGNORECASE)
            ].copy()
            rm_with_row_id = df_filtered[
                df_filtered['Solution Label'].str.match(rf'^{re.escape(self.keyword)}', na=False, flags=re.IGNORECASE)
            ][['Solution Label', 'original_index', 'row_id']].drop_duplicates()
            rm_df = rm_df.merge(rm_with_row_id, on=['Solution Label', 'original_index'], how='left')
            rm_df['row_id'] = rm_df['row_id'].fillna(-1).astype(int)
            # --- مرحله 14: اضافه کردن rm_num و rm_type به rm_df ---
            info_rm = rm_df['Solution Label'].apply(extract_rm_info, keyword=self.keyword)
            rm_df[['rm_num', 'rm_type']] = pd.DataFrame(info_rm.tolist(), index=rm_df.index)
            rm_df['rm_num'] = rm_df['rm_num'].astype(int)
            # --- مرحله 15: تقسیم‌بندی بر اساس Cone ---
            rm_df = rm_df.sort_values('original_index').reset_index(drop=True)
            positions_list = []

            current_segment = 0
            ref_rm_num = None  # اولین Base بعد از Cone

            for idx, row in rm_df.iterrows():
                rm_type = row['rm_type']
                rm_num = row['rm_num']

                # Cone → شروع بخش جدید
                if rm_type == 'Cone':
                    current_segment += 1
                    ref_rm_num = None  # مرجع جدید در این بخش

                # اولین Base/Check در بخش → مرجع
                if ref_rm_num is None and rm_type in ['Base', 'Check']:
                    ref_rm_num = rm_num

                min_pos = rm_df.iloc[idx-1]['original_index'] if idx > 0 else -1
                max_pos = row['original_index']

                positions_list.append({
                    'Solution Label': row['Solution Label'],
                    'row_id': row['row_id'],
                    'pivot_index': row['pivot_index'],
                    'min': min_pos,
                    'max': max_pos,
                    'rm_num': rm_num,
                    'rm_type': rm_type,
                    'segment_id': current_segment,
                    'ref_rm_num': ref_rm_num if ref_rm_num is not None else rm_num
                })

            positions_df = pd.DataFrame(positions_list)
            positions_df.loc[0, 'min'] = -1

            # --- مرحله 16: ساخت segments ---
            segments = []
            for seg_id in positions_df['segment_id'].unique():
                seg_df = positions_df[positions_df['segment_id'] == seg_id].copy()
                ref_num = seg_df['ref_rm_num'].iloc[0]
                segments.append({
                    'segment_id': seg_id,
                    'ref_rm_num': ref_num,
                    'positions': seg_df
                })

            results = {
                'rm_df': rm_df,
                'positions_df': positions_df,
                'segments': segments,
                'original_df': original_df,
                'corrected_df': corrected_df,
                'pivot_df': pivot_df,
                'solution_labels': solution_labels
            }
            self.finished.emit(results)
        except Exception as e:
            logger.error(f"Error in CheckRMThread: {str(e)}", exc_info=True)
            self.error.emit(str(e))

class ApplySingleRMThread(QThread):
    progress = pyqtSignal(int)
    finished = pyqtSignal(dict)
    error = pyqtSignal(str)

    def __init__(self, app, keyword, element, rm_num, rm_df, initial_rm_df, segments, corrected_df, stepwise):
        super().__init__()
        self.app = app
        self.keyword = keyword
        self.element = element
        self.rm_num = rm_num
        self.rm_df = rm_df.copy(deep=True)
        self.initial_rm_df = initial_rm_df.copy(deep=True)
        self.segments = segments
        self.corrected_df = corrected_df.copy(deep=True)
        self.stepwise = stepwise
        self.corrected_drift = {}

    def run(self):
        try:
            element = self.element
            total_steps = len(self.segments)
            step = 0

            for segment in self.segments:
                seg_id = segment['segment_id']
                ref_rm_num = segment['ref_rm_num']
                positions_df = segment['positions']

                # فقط اگر RM در این بخش باشه
                seg_rm_df = self.rm_df[self.rm_df['rm_num'].isin(positions_df['rm_num'])]
                if seg_rm_df.empty:
                    continue

                # فقط از RMهایی که بعد از ref_rm_num هستن
                valid_rows = positions_df[positions_df['rm_num'] >= ref_rm_num]
                if valid_rows.empty:
                    continue

                row_ids = valid_rows['row_id'].values
                rm_nums = valid_rows['rm_num'].values

                # مقادیر اولیه و فعلی
                initial_vals = pd.to_numeric(self.initial_rm_df[self.initial_rm_df['rm_num'].isin(rm_nums)][element], errors='coerce').values
                current_vals = pd.to_numeric(seg_rm_df[element], errors='coerce').values

                # شروع از اولین RM بعد از ref
                start_idx = list(rm_nums).index(ref_rm_num) if ref_rm_num in rm_nums else 0
                if start_idx >= len(rm_nums) - 1:
                    step += 1
                    self.progress.emit(int((step / total_steps) * 100))
                    continue

                effective_row_ids = row_ids[start_idx + 1:]
                effective_initial = initial_vals[start_idx + 1:]
                effective_current = current_vals[start_idx + 1:]

                ratios = np.where(effective_initial != 0, effective_current / effective_initial, 1.0)

                # اعمال در بخش
                for i in range(len(effective_row_ids)):
                    ratio = ratios[i]
                    if np.isnan(ratio) or ratio <= 0:
                        continue

                    pos_row = valid_rows[valid_rows['row_id'] == effective_row_ids[i]].iloc[0]
                    min_pos = pos_row['min']
                    max_pos = pos_row['max']

                    condition = (
                        (self.corrected_df['original_index'] > min_pos) &
                        (self.corrected_df['original_index'] < max_pos) &
                        (self.corrected_df['Element'] == element) &
                        (self.corrected_df['Corr Con'].notna()) &
                        ~self.corrected_df['Solution Label'].str.match(rf'^{self.keyword}\d*$', na=False)
                    )
                    data_to_correct = self.corrected_df[condition].copy()

                    if data_to_correct.empty:
                        continue

                    original_values_to_correct = data_to_correct['Corr Con'].values
                    corrected_values = self.calculate_corrected_values(original_values_to_correct, ratio)
                    self.corrected_df.loc[data_to_correct.index, 'Corr Con'] = corrected_values

                    # ذخیره drift برای هر Sample
                    for idx, row in data_to_correct.iterrows():
                        solution_label = row['Solution Label']
                        if self.stepwise:
                            n = len(original_values_to_correct)
                            delta = ratio - 1.0
                            step_delta = delta / n if n > 0 else 0.0
                            step_index = list(data_to_correct.index).index(idx)
                            effective_ratio = 1.0 + step_delta * (step_index + 1)
                        else:
                            effective_ratio = ratio
                        self.corrected_drift[(solution_label, element)] = effective_ratio

                # به‌روزرسانی خود RMها
                for j, row_id in enumerate(effective_row_ids):
                    rm_num_j = rm_nums[start_idx + 1 + j]
                    condition = (
                        (self.corrected_df['rm_num'] == rm_num_j) &
                        (self.corrected_df['row_id'] == row_id) &
                        (self.corrected_df['Element'] == element)
                    )
                    if not self.corrected_df[condition].empty and not np.isnan(effective_current[j]):
                        self.corrected_df.loc[condition, 'Corr Con'] = effective_current[j]

                step += 1
                self.progress.emit(int((step / total_steps) * 100))

            results = {
                'corrected_df': self.corrected_df,
                'rm_df': self.rm_df,
                'corrected_drift': self.corrected_drift
            }
            self.finished.emit(results)

        except Exception as e:
            logger.error(f"Error in ApplySingleRMThread: {str(e)}", exc_info=True)
            self.error.emit(str(e))

    def calculate_corrected_values(self, original_values, current_ratio):
        n = len(original_values)
        if n == 0:
            return np.array([])
        delta = current_ratio - 1.0
        step_delta = delta / n if n > 0 else 0.0
        return original_values * np.array([1.0 + step_delta * (j + 1) if self.stepwise else current_ratio for j in range(n)])

class CheckRMFrame(QWidget):
    data_changed = pyqtSignal()

    def __init__(self, app, parent=None):
        super().__init__(parent)
        self.app = app
        self.empty_rows_from_check = pd.DataFrame()
        self.initial_rm_df = None
        self.undo_stack = []
        self.navigation_list = []
        self.current_nav_index = -1
        self.segments = []
        self.reset_state()
        self.setup_ui()
        if hasattr(self.app, 'empty_check_frame'):
            self.app.empty_check_frame.empty_rows_found.connect(self.on_empty_rows_received)

    def reset_state(self):
        self.rm_df = self.positions_df = self.original_df = self.corrected_df = self.pivot_df = self.initial_rm_df = None
        self.selected_element = self.current_rm_num = None
        self.elements = self.unique_rm_nums = []
        self.current_slope = 0.0
        self.original_rm_values = self.display_rm_values = np.array([])
        self.current_valid_pivot_indices = []
        self.selected_row = -1
        self.undo_stack = []
        self.corrected_drift = {}
        self.navigation_list = []
        self.current_nav_index = -1
        self.segments = []
        if hasattr(self, 'keyword_entry'): self.keyword_entry.setText("RM")
        if hasattr(self, 'element_combo'): self.element_combo.clear()
        if hasattr(self, 'label_label'): self.label_label.setText("Current RM: None")
        if hasattr(self, 'slope_label'): self.slope_label.setText("Current Slope: 0.000")
        if hasattr(self, 'slope_spinbox'): self.slope_spinbox.blockSignals(True); self.slope_spinbox.setValue(0.0); self.slope_spinbox.blockSignals(False)
        if hasattr(self, 'rm_table'): self.rm_table.setModel(QStandardItemModel())
        if hasattr(self, 'detail_table'): self.detail_table.setModel(QStandardItemModel())
        if hasattr(self, 'plot_widget'): self.plot_widget.clear()
        if hasattr(self, 'detail_plot_widget'): self.detail_plot_widget.clear()
        if hasattr(self, 'auto_optimize_flat_button'): self.auto_optimize_flat_button.setEnabled(False)
        if hasattr(self, 'auto_optimize_zero_button'): self.auto_optimize_zero_button.setEnabled(False)
        if hasattr(self, 'undo_button'): self.undo_button.setEnabled(False)
        if hasattr(self, 'stepwise_checkbox'): self.stepwise_checkbox.setChecked(False)

    def setup_ui(self):
        self.setStyleSheet(global_style)
        main_layout = QVBoxLayout(self)
        main_layout.setContentsMargins(20, 20, 20, 20)
        content_frame = QFrame()
        content_layout = QHBoxLayout(content_frame)
        content_layout.setSpacing(15)
        left_frame = QGroupBox("Controls")
        left_layout = QVBoxLayout(left_frame)
        left_layout.setSpacing(15)
        control_frame = QFrame()
        control_layout = QHBoxLayout(control_frame)
        control_layout.addWidget(QLabel("Keyword:"))
        self.keyword_entry = QLineEdit("RM")
        self.keyword_entry.setFixedWidth(100)
        control_layout.addWidget(self.keyword_entry)
        self.run_button = QPushButton("Check RM Changes")
        self.run_button.clicked.connect(self.start_check_rm_thread)
        control_layout.addWidget(self.run_button)
        self.undo_button = QPushButton("Undo Last Correction")
        self.undo_button.clicked.connect(self.undo_correction)
        self.undo_button.setEnabled(False)
        control_layout.addWidget(self.undo_button)
        self.stepwise_checkbox = QCheckBox("Apply Stepwise Changes")
        control_layout.addWidget(self.stepwise_checkbox)
        left_layout.addWidget(control_frame)
        optimize_frame = QFrame()
        optimize_layout = QHBoxLayout(optimize_frame)
        self.auto_optimize_flat_button = QPushButton("Auto Optimize to Flat")
        self.auto_optimize_flat_button.clicked.connect(self.auto_optimize_to_flat)
        self.auto_optimize_flat_button.setEnabled(False)
        optimize_layout.addWidget(self.auto_optimize_flat_button)
        self.auto_optimize_zero_button = QPushButton("Auto Optimize Slope to Zero")
        self.auto_optimize_zero_button.clicked.connect(self.auto_optimize_slope_to_zero)
        self.auto_optimize_zero_button.setEnabled(False)
        optimize_layout.addWidget(self.auto_optimize_zero_button)
        left_layout.addWidget(optimize_frame)
        element_label_layout = QHBoxLayout()
        self.element_combo = QComboBox()
        self.element_combo.currentTextChanged.connect(self.on_element_changed)
        element_label_layout.addWidget(self.element_combo)
        self.label_label = QLabel("Current RM: None")
        self.label_label.setAlignment(Qt.AlignmentFlag.AlignCenter)
        element_label_layout.addWidget(self.label_label)
        left_layout.addLayout(element_label_layout)
        nav_layout = QHBoxLayout()
        self.prev_btn = QPushButton("Previous"); self.prev_btn.clicked.connect(self.prev); nav_layout.addWidget(self.prev_btn)
        self.next_btn = QPushButton("Next"); self.next_btn.clicked.connect(self.next); nav_layout.addWidget(self.next_btn)
        left_layout.addLayout(nav_layout)
        left_layout.addWidget(QLabel("RM Points and Ratios"))
        self.rm_table = QTableView()
        self.rm_table.setSelectionMode(QTableView.SelectionMode.SingleSelection)
        self.rm_table.setSelectionBehavior(QTableView.SelectionBehavior.SelectRows)
        self.rm_table.horizontalHeader().setSectionResizeMode(QHeaderView.ResizeMode.Stretch)
        self.rm_table.verticalHeader().setVisible(False)
        self.rm_table.clicked.connect(self.on_table_row_clicked)
        left_layout.addWidget(self.rm_table)
        left_layout.addWidget(QLabel("Data Between Selected RM Points"))
        self.detail_table = QTableView()
        self.detail_table.horizontalHeader().setSectionResizeMode(QHeaderView.ResizeMode.Stretch)
        self.detail_table.verticalHeader().setVisible(False)
        left_layout.addWidget(self.detail_table)
        slope_controls = QGroupBox("Slope Optimization")
        slope_layout = QVBoxLayout(slope_controls)
        self.slope_label = QLabel("Current Slope: 0.000")
        slope_layout.addWidget(self.slope_label)
        slope_controls_layout = QHBoxLayout()
        slope_controls_layout.addWidget(QLabel("Adjust Slope:"))
        self.slope_spinbox = QDoubleSpinBox(); self.slope_spinbox.setRange(-1000, 1000); self.slope_spinbox.setSingleStep(0.1)
        self.slope_spinbox.valueChanged.connect(self.update_slope)
        slope_controls_layout.addWidget(self.slope_spinbox)
        self.up_button = QPushButton("Rotate Up"); self.up_button.clicked.connect(self.rotate_up); slope_controls_layout.addWidget(self.up_button)
        self.down_button = QPushButton("Rotate Down"); self.down_button.clicked.connect(self.rotate_down); slope_controls_layout.addWidget(self.down_button)
        self.reset_button = QPushButton("Reset to Original"); self.reset_button.clicked.connect(self.reset_to_original); slope_controls_layout.addWidget(self.reset_button)
        slope_layout.addLayout(slope_controls_layout)
        left_layout.addWidget(slope_controls)
        content_layout.addWidget(left_frame, stretch=1)
        right_frame = QFrame()
        right_layout = QVBoxLayout(right_frame)
        right_layout.addWidget(QLabel("RM Points Plot"))
        self.plot_widget = pg.PlotWidget()
        self.plot_widget.setLabel('left', 'Value'); self.plot_widget.setLabel('bottom', 'Sample Index')
        self.plot_widget.showGrid(x=True, y=True); self.plot_widget.addLegend(); self.plot_widget.setBackground('w')
        right_layout.addWidget(self.plot_widget, stretch=2)
        right_layout.addWidget(QLabel("Data Between RM Points"))
        self.detail_plot_widget = pg.PlotWidget()
        self.detail_plot_widget.setLabel('left', 'Value'); self.detail_plot_widget.setLabel('bottom', 'Index')
        self.detail_plot_widget.showGrid(x=True, y=True); self.detail_plot_widget.addLegend(); self.detail_plot_widget.setBackground('w')
        right_layout.addWidget(self.detail_plot_widget, stretch=1)
        content_layout.addWidget(right_frame, stretch=2)
        main_layout.addWidget(content_frame)
        self.update_navigation_buttons()
    def on_empty_rows_received(self, empty_df):
        self.empty_rows_from_check = empty_df.copy()
    def update_navigation_buttons(self):
        self.prev_btn.setEnabled(self.current_nav_index > 0)
        self.next_btn.setEnabled(self.current_nav_index < len(self.navigation_list) - 1)
        enabled = bool(self.current_rm_num is not None and self.selected_element)
        self.up_button.setEnabled(enabled); self.down_button.setEnabled(enabled); self.reset_button.setEnabled(enabled)
        self.slope_spinbox.setEnabled(enabled); self.auto_optimize_flat_button.setEnabled(enabled); self.auto_optimize_zero_button.setEnabled(enabled)
    def update_labels(self):
        self.label_label.setText(f"Current RM: {self.current_rm_num if self.current_rm_num is not None else 'None'}")
        if self.element_combo.count() > 0:
            self.element_combo.blockSignals(True); self.element_combo.setCurrentText(self.selected_element or ''); self.element_combo.blockSignals(False)
    def has_changes(self):
        return len(self.original_rm_values) > 0 and not np.array_equal(self.original_rm_values, self.display_rm_values)
    def prompt_apply_changes(self):
        if self.has_changes():
            reply = QMessageBox.question(self, 'Apply Changes', 'Do you want to apply the changes to this RM?', QMessageBox.StandardButton.Yes | QMessageBox.StandardButton.No, QMessageBox.StandardButton.No)
            if reply == QMessageBox.StandardButton.Yes:
                self.apply_to_single_rm()
    def prev(self):
        if self.current_nav_index > 0:
            self.prompt_apply_changes()
            self.current_nav_index -= 1
            self.selected_element, self.current_rm_num = self.navigation_list[self.current_nav_index]
            self.selected_row = -1
            self.update_labels(); self.update_displays(); self.update_navigation_buttons()
    def next(self):
        if self.current_nav_index < len(self.navigation_list) - 1:
            self.prompt_apply_changes()
            self.current_nav_index += 1
            self.selected_element, self.current_rm_num = self.navigation_list[self.current_nav_index]
            self.selected_row = -1
            self.update_labels(); self.update_displays(); self.update_navigation_buttons()
    def on_element_changed(self, text):
        if text and text in self.elements:
            self.selected_element = text
            for idx, (el, num) in enumerate(self.navigation_list):
                if el == self.selected_element and num == self.current_rm_num:
                    self.current_nav_index = idx
                    break
            self.selected_row = -1
            self.update_displays(); self.update_navigation_buttons()
    def update_displays(self):
        if self.current_rm_num is not None and self.selected_element:
            self.display_rm_table()
            self.update_plot()
            self.update_detail_plot()
            self.update_detail_table()
    def start_check_rm_thread(self):
        keyword = self.keyword_entry.text().strip()
        if not keyword:
            QMessageBox.critical(self, "Error", "Please enter a valid keyword.")
            return
        self.keyword = keyword
        self.progress_dialog = QProgressDialog("Processing RM Changes...", "Cancel", 0, 100, self)
        self.progress_dialog.setWindowModality(Qt.WindowModality.WindowModal)
        self.thread = CheckRMThread(self.app, keyword)
        self.thread.progress.connect(self.progress_dialog.setValue)
        self.thread.finished.connect(self.on_check_rm_finished)
        self.thread.error.connect(self.on_check_rm_error)
        self.thread.start()
    def on_check_rm_finished(self, results):
        self.progress_dialog.close()
        self.initial_rm_df = results['rm_df'].copy(deep=True)
        self.rm_df = results['rm_df'].copy(deep=True)
        self.positions_df = results['positions_df']
        self.segments = results['segments']  # اضافه شد!
        self.original_df = results['original_df']
        self.corrected_df = results['corrected_df']
        self.pivot_df = results['pivot_df'].copy(deep=True)
        self.elements = [col for col in self.rm_df.columns if col not in ['Solution Label', 'original_index', 'pivot_index', 'row_id', 'rm_num', 'rm_type']]
        self.unique_rm_nums = sorted(self.rm_df['rm_num'].unique())
        if self.unique_rm_nums and self.elements:
            self.navigation_list = [(el, num) for el in self.elements for num in self.unique_rm_nums]
            self.current_nav_index = 0
            self.selected_element, self.current_rm_num = self.navigation_list[0]
            self.element_combo.addItems(self.elements)
            self.update_labels(); self.update_displays()
            self.auto_optimize_flat_button.setEnabled(True); self.auto_optimize_zero_button.setEnabled(True)
        std_data = self.original_df[self.original_df['Type'] == 'Std'].copy(deep=True)
        updated_df = pd.concat([self.corrected_df, std_data], ignore_index=True)
        self.app.set_data(updated_df, for_results=True)
        self.save_corrected_drift()
        self.data_changed.emit(); self.update_navigation_buttons()
    def on_check_rm_error(self, message):
        self.progress_dialog.close()
        QMessageBox.critical(self, "Error", message)
    def display_rm_table(self):
        model = QStandardItemModel()
        model.setHorizontalHeaderLabels(["RM Label", "Next RM", "Type", "Original Value", "Current Value", "Ratio"])
       
        label_df = self.rm_df[self.rm_df['rm_num'] == self.current_rm_num].sort_values('pivot_index')
        initial_label_df = self.initial_rm_df[self.initial_rm_df['rm_num'] == self.current_rm_num].sort_values('pivot_index')
       
        pivot_indices = label_df['pivot_index'].values
        original_values = pd.to_numeric(initial_label_df[self.selected_element], errors='coerce').values
        display_values = pd.to_numeric(label_df[self.selected_element], errors='coerce').values
       
        valid_mask = ~np.isnan(original_values) & ~np.isnan(display_values)
        self.current_valid_pivot_indices = pivot_indices[valid_mask]
        self.original_rm_values = original_values[valid_mask]
        self.display_rm_values = display_values[valid_mask]
        # استفاده از rm_type از rm_df
        self.rm_types = label_df.loc[label_df.index.isin(label_df.index[valid_mask]), 'rm_type'].values
        self.solution_labels_for_group = label_df.loc[label_df.index.isin(label_df.index[valid_mask]), 'Solution Label'].values
        empty_pivot_set = set(self.empty_rows_from_check['original_index'].dropna().astype(int).tolist()) if not self.empty_rows_from_check.empty and 'original_index' in self.empty_rows_from_check.columns else set()
        is_empty = np.array([p in empty_pivot_set for p in self.current_valid_pivot_indices], dtype=bool)
        blue_pivot_indices = self.current_valid_pivot_indices[~is_empty]
        blue_index_to_pos = {idx: i for i, idx in enumerate(blue_pivot_indices)}
        if len(self.display_rm_values) == 0:
            model.appendRow([QStandardItem("No Data")] * 6)
        else:
            for i in range(len(self.display_rm_values)):
                current_rm_label = f"{self.solution_labels_for_group[i]}-{self.current_valid_pivot_indices[i]}"
                next_rm_label = "N/A"
                if not is_empty[i]:
                    pos = blue_index_to_pos.get(self.current_valid_pivot_indices[i])
                    if pos is not None and pos < len(blue_pivot_indices) - 1:
                        next_rm_label = f"{self.solution_labels_for_group[pos + 1]}-{blue_pivot_indices[pos + 1]}"
                orig_val = self.original_rm_values[i]
                curr_val = self.display_rm_values[i]
                ratio = curr_val / orig_val if orig_val != 0 else np.nan
                rm_type = self.rm_types[i]
                row_items = [
                    QStandardItem(current_rm_label),
                    QStandardItem(next_rm_label),
                    QStandardItem(rm_type),
                    QStandardItem(f"{orig_val:.3f}"),
                    QStandardItem(f"{curr_val:.3f}"),
                    QStandardItem(f"{ratio:.3f}" if pd.notna(ratio) else "N/A")
                ]
                # رنگ‌آمیزی
                if is_empty[i]:
                    for item in row_items:
                        item.setBackground(Qt.GlobalColor.red)
                        item.setForeground(Qt.GlobalColor.white)
                        item.setEditable(False)
                else:
                    row_items[2].setEditable(False) # نوع قابل ویرایش نیست
                    for j in [0, 1, 3, 4, 5]: row_items[j].setEditable(False)
                    # رنگ بر اساس نوع
                    color_map = {
                        'Base': '#2E7D32',
                        'Check': '#FF6B00',
                        'Cone': '#7B1FA2'
                    }
                    color = QColor(color_map.get(rm_type, '#000000'))
                    row_items[2].setForeground(color)
                    row_items[2].setFont(QFont("Segoe UI", 9, QFont.Weight.Bold))
                model.appendRow(row_items)
       
        self.rm_table.setModel(model)
        try: model.itemChanged.disconnect()
        except: pass
        model.itemChanged.connect(self.on_rm_value_changed)
        self.update_slope_from_data()
        if 0 <= self.selected_row < len(self.current_valid_pivot_indices):
            self.rm_table.selectRow(self.selected_row)
    def on_rm_value_changed(self, item):
        row = item.row()
        try:
            if item.column() == 3:
                val = float(item.text())
                self.display_rm_values[row] = val
            elif item.column() == 4:
                ratio = float(item.text())
                val = self.original_rm_values[row] * ratio
                self.display_rm_values[row] = val
                self.rm_table.model().item(row, 3).setText(f"{val:.3f}")
            self.selected_row = row
            self.update_rm_data()
            self.update_plot(); self.update_rm_table_ratios(); self.update_slope_from_data()
            self.update_detail_plot(); self.update_detail_table()
        except ValueError as e:
            QMessageBox.warning(self, "Invalid Value", str(e))
            if item.column() == 3:
                item.setText(f"{self.display_rm_values[row]:.3f}")
            elif item.column() == 4:
                ratio = self.display_rm_values[row] / self.original_rm_values[row] if self.original_rm_values[row] != 0 else np.nan
                item.setText(f"{ratio:.3f}" if pd.notna(ratio) else "N/A")
    def on_table_row_clicked(self, index):
        self.selected_row = index.row()
        self.update_detail_plot(); self.update_detail_table()
    def update_rm_data(self):
        if len(self.display_rm_values) == 0: return
        valid_mask = ~np.isnan(self.display_rm_values)
        valid_pivot_indices = np.array(self.current_valid_pivot_indices)[valid_mask]
        valid_display_values = self.display_rm_values[valid_mask]
        label_df = self.rm_df[(self.rm_df['rm_num'] == self.current_rm_num) & (self.rm_df['pivot_index'].isin(valid_pivot_indices))].sort_values('pivot_index').reset_index(drop=True)
        if len(label_df) != len(valid_display_values): return
        for i, row in label_df.iterrows():
            self.rm_df.loc[self.rm_df['pivot_index'] == row['pivot_index'], self.selected_element] = valid_display_values[i]
            cond = (self.corrected_df['original_index'] == row['original_index']) & (self.corrected_df['Element'] == self.selected_element)
            if not self.corrected_df[cond].empty:
                self.corrected_df.loc[cond, 'Corr Con'] = valid_display_values[i]
    def update_rm_table_ratios(self):
        model = self.rm_table.model()
        for i in range(model.rowCount()):
            if i < len(self.original_rm_values):
                ratio = self.display_rm_values[i] / self.original_rm_values[i] if self.original_rm_values[i] != 0 else np.nan
                model.item(i, 4).setText(f"{ratio:.3f}" if pd.notna(ratio) else "N/A")
    def update_slope_from_data(self):
        if len(self.display_rm_values) >= 2:
            x = np.arange(len(self.display_rm_values))
            valid_mask = ~np.isnan(self.display_rm_values)
            x_valid = x[valid_mask]; y_valid = self.display_rm_values[valid_mask]
            pivot_valid = np.array(self.current_valid_pivot_indices)[valid_mask]
            empty_set = set(self.empty_rows_from_check['original_index'].dropna().astype(int).tolist()) if not self.empty_rows_from_check.empty else set()
            is_empty = np.array([p in empty_set for p in pivot_valid], dtype=bool)
            normal_mask = ~is_empty
            if np.sum(normal_mask) >= 2:
                self.current_slope = np.polyfit(x_valid[normal_mask], y_valid[normal_mask], 1)[0]
            else:
                self.current_slope = 0.0
        else:
            self.current_slope = 0.0
        self.slope_spinbox.blockSignals(True); self.slope_spinbox.setValue(self.current_slope); self.slope_spinbox.blockSignals(False)
        self.slope_label.setText(f"Current Slope: {self.current_slope:.3f}")
    def update_plot(self):
        self.plot_widget.clear()
        if len(self.display_rm_values) == 0: return
        x = np.arange(len(self.display_rm_values))
        valid_mask = ~np.isnan(self.display_rm_values)
        x_valid = x[valid_mask]
        y_valid = self.display_rm_values[valid_mask]
        pivot_valid = np.array(self.current_valid_pivot_indices)[valid_mask]
        types_valid = self.rm_types[valid_mask]
        empty_set = set(self.empty_rows_from_check['original_index'].dropna().astype(int).tolist()) if not self.empty_rows_from_check.empty else set()
        is_empty = np.array([p in empty_set for p in pivot_valid], dtype=bool)
        normal_mask = ~is_empty

        # تنظیمات رنگ و شکل
        symbol_map = {'Base': 'o', 'Check': 't', 'Cone': 's', 'Missing': 'o'}
        color_map = {'Base': '#2E7D32', 'Check': '#FF6B00', 'Cone': '#7B1FA2', 'Missing': 'r'}

        legend_added = {'Base': False, 'Check': False, 'Cone': False, 'Missing': False}

        # 1. اول خط سبز رو بکش (به همه نقاط غیر قرمز)
        line_colors = ['#43A047', '#FF6B00', '#7B1FA2', '#1A3C34']  # رنگ‌های مختلف برای بخش‌ها
        for seg in self.segments:
            seg_positions = seg['positions']
            seg_pivot = seg_positions['pivot_index'].values
            seg_mask = np.isin(pivot_valid, seg_pivot) & normal_mask
            if np.any(seg_mask):
                x_n = x_valid[seg_mask]
                y_n = y_valid[seg_mask]
                color_idx = seg['segment_id'] % len(line_colors)
                self.plot_widget.plot(
                    x_n, y_n,
                    pen=pg.mkPen(line_colors[color_idx], width=2.5),
                    name=f'Segment {seg["segment_id"]} Line'
                )

                # خط روند (اختیاری)
                if len(x_n) >= 2:
                    z = np.polyfit(x_n, y_n, 1)
                    p = np.poly1d(z)
                    self.plot_widget.plot(
                        x_n, p(x_n),
                        pen=pg.mkPen(line_colors[color_idx], width=2, style=Qt.PenStyle.DashLine),
                        name=f'Segment {seg["segment_id"]} Trendline'
                    )

        # 2. بعد نقاط رو روی خط بذار
        for i in range(len(x_valid)):
            rm_type = 'Missing' if is_empty[i] else types_valid[i]
            symbol = symbol_map[rm_type]
            color = color_map[rm_type]
            size = 14 if rm_type == 'Missing' else 11

            name = rm_type if not legend_added[rm_type] else None
            if name:
                legend_added[rm_type] = True

            self.plot_widget.plot(
                [x_valid[i]], [y_valid[i]],
                pen=None,
                symbol=symbol,
                symbolSize=size,
                symbolBrush=color,
                symbolPen=pg.mkPen('w', width=1.5),
                name=name
            )

        self.plot_widget.setXRange(-0.5, len(x_valid) - 0.5)
        self.plot_widget.autoRange()

    def get_data_between_rm(self):
        if self.selected_row < 0 or self.selected_row >= len(self.current_valid_pivot_indices) - 1: return pd.DataFrame()
        pivot_prev = self.current_valid_pivot_indices[self.selected_row]
        pivot_curr = self.current_valid_pivot_indices[self.selected_row + 1]
        min_row = self.positions_df[self.positions_df['pivot_index'] == pivot_prev]
        max_row = self.positions_df[self.positions_df['pivot_index'] == pivot_curr]
        if min_row.empty or max_row.empty: return pd.DataFrame()
        min_pos = min_row['min'].values[0]; max_pos = max_row['max'].values[0]
        cond = (self.pivot_df['original_index'] > min_pos) & (self.pivot_df['original_index'] < max_pos) & (self.pivot_df[self.selected_element].notna())
        return self.pivot_df[cond].copy().sort_values('original_index')
    def calculate_corrected_values(self, original_values, current_ratio):
        n = len(original_values)
        if n == 0: return np.array([])
        delta = current_ratio - 1.0
        step_delta = delta / n if n > 0 else 0.0
        stepwise = self.stepwise_checkbox.isChecked()
        return original_values * np.array([1.0 + step_delta * (j + 1) if stepwise else current_ratio for j in range(n)])
    def update_detail_plot(self):
        self.detail_plot_widget.clear()
        data = self.get_data_between_rm()
        if data.empty: return
        x = data['original_index'].values; orig = data[self.selected_element].values
        ratio = self.display_rm_values[self.selected_row + 1] / self.original_rm_values[self.selected_row + 1] if self.original_rm_values[self.selected_row + 1] != 0 else 1.0
        corr = self.calculate_corrected_values(orig, ratio)
        self.detail_plot_widget.plot(x, orig, pen=None, symbol='o', symbolSize=6, symbolPen='b', name='Original')
        self.detail_plot_widget.plot(x, corr, pen=None, symbol='x', symbolPen='r', symbolSize=8, name='Corrected')
        self.detail_plot_widget.setXRange(min(x)-0.5, max(x)+0.5); self.detail_plot_widget.autoRange()
    def update_detail_table(self):
        model = QStandardItemModel()
        model.setHorizontalHeaderLabels(["Solution Label", "Original Value", "Corrected Value"])
        data = self.get_data_between_rm()
        if data.empty:
            self.detail_table.setModel(model)
            return
        orig = data[self.selected_element].values
        ratio = self.display_rm_values[self.selected_row + 1] / self.original_rm_values[self.selected_row + 1] if self.original_rm_values[self.selected_row + 1] != 0 else 1.0
        corr = self.calculate_corrected_values(orig, ratio)
        for i, row in data.iterrows():
            model.appendRow([QStandardItem(row['Solution Label']), QStandardItem(f"{orig[i]:.3f}"), QStandardItem(f"{corr[i]:.3f}")])
        self.detail_table.setModel(model)
    def update_slope(self, value):
        if len(self.display_rm_values) >= 2:
            delta = value - self.current_slope
            x = np.arange(len(self.display_rm_values))
            self.display_rm_values += delta * x
            self.current_slope = value
            self.slope_label.setText(f"Current Slope: {self.current_slope:.3f}")
            self.update_rm_data(); self.update_plot(); self.update_rm_table_ratios()
            self.update_detail_plot(); self.update_detail_table()
    def rotate_up(self): self.current_slope += 0.1; self.slope_spinbox.setValue(self.current_slope); self.update_slope(self.current_slope)
    def rotate_down(self): self.current_slope -= 0.1; self.slope_spinbox.setValue(self.current_slope); self.update_slope(self.current_slope)
    def reset_to_original(self):
        self.display_rm_values = self.original_rm_values.copy()
        self.update_rm_data(); self.update_plot(); self.update_rm_table_ratios(); self.update_slope_from_data()
        self.update_detail_plot(); self.update_detail_table()
        model = self.rm_table.model()
        for i in range(model.rowCount()):
            if i < len(self.display_rm_values):
                model.item(i, 3).setText(f"{self.display_rm_values[i]:.3f}")
        QMessageBox.information(self, "Info", "Reset to original values.")
    def auto_optimize_to_flat(self):
        if len(self.display_rm_values) == 0:
            return
        empty_set = set(self.empty_rows_from_check['original_index'].dropna().astype(int).tolist()) if not self.empty_rows_from_check.empty else set()
        optimized = False
        for seg in self.segments:
            seg_positions = seg['positions']
            seg_pivot = seg_positions['pivot_index'].values
            mask = self.rm_df['pivot_index'].isin(seg_pivot)
            if not mask.any():
                continue
            y_seg = self.rm_df.loc[mask, self.selected_element].astype(float).values
            pivot_seg = self.rm_df.loc[mask, 'pivot_index'].values
            is_empty_seg = np.array([p in empty_set for p in pivot_seg])
            normal_mask_seg = ~is_empty_seg & ~np.isnan(y_seg)
            if normal_mask_seg.sum() == 0:
                continue
            first_idx = np.where(normal_mask_seg)[0][0]
            first_val = y_seg[first_idx]
            y_seg[normal_mask_seg] = first_val
            self.rm_df.loc[mask, self.selected_element] = y_seg
            optimized = True
        if optimized:
            self.update_displays()
            self.update_slope_from_data()
            QMessageBox.information(self, "Info", "All segments optimized to flat relative to their first valid RM point.")
        else:
            QMessageBox.warning(self, "Warning", "No valid segments to optimize.")
    def auto_optimize_slope_to_zero(self):
        if len(self.display_rm_values) < 2:
            return
        empty_set = set(self.empty_rows_from_check['original_index'].dropna().astype(int).tolist()) if not self.empty_rows_from_check.empty else set()
        optimized = False
        for seg in self.segments:
            seg_positions = seg['positions']
            seg_pivot = seg_positions['pivot_index'].values
            mask = self.rm_df['pivot_index'].isin(seg_pivot)
            if not mask.any():
                continue
            y_seg = self.rm_df.loc[mask, self.selected_element].astype(float).values
            pivot_seg = self.rm_df.loc[mask, 'pivot_index'].values
            is_empty_seg = np.array([p in empty_set for p in pivot_seg])
            normal_mask_seg = ~is_empty_seg & ~np.isnan(y_seg)
            if normal_mask_seg.sum() < 2:
                continue
            x_seg = np.arange(len(y_seg))
            x_n = x_seg[normal_mask_seg]
            y_n = y_seg[normal_mask_seg].copy()
            for _ in range(10):
                slope = np.polyfit(x_n, y_n, 1)[0]
                if abs(slope) < 1e-6:
                    break
                y_n -= slope * x_n
            y_seg[normal_mask_seg] = y_n
            self.rm_df.loc[mask, self.selected_element] = y_seg
            optimized = True
        if optimized:
            self.update_displays()
            self.update_slope_from_data()
            QMessageBox.information(self, "Info", "Slope optimized to zero in all segments relative to their first valid RM point.")
        else:
            QMessageBox.warning(self, "Warning", "No valid segments with sufficient points to optimize.")
    def apply_to_single_rm(self):
        if not self.selected_element or self.current_rm_num is None:
            QMessageBox.critical(self, "Error", "No element or RM number selected.")
            return
        self.progress_dialog = QProgressDialog("Applying corrections...", "Cancel", 0, 100, self)
        self.progress_dialog.setWindowModality(Qt.WindowModality.WindowModal)
        self.thread = ApplySingleRMThread(self.app, self.keyword, self.selected_element, self.current_rm_num, self.rm_df, self.initial_rm_df, self.segments, self.corrected_df, self.stepwise_checkbox.isChecked())
        self.thread.progress.connect(self.progress_dialog.setValue)
        self.thread.finished.connect(self.on_apply_single_finished)
        self.thread.error.connect(self.on_apply_single_error)
        self.thread.start()
    def on_apply_single_finished(self, results):
        self.progress_dialog.close()
        self.undo_stack.append((self.corrected_df.copy(deep=True), self.rm_df.copy(deep=True), self.corrected_drift.copy()))
        self.undo_button.setEnabled(True)
        self.corrected_df = results['corrected_df']
        self.rm_df = results['rm_df']
        self.corrected_drift.update(results['corrected_drift'])
        std_data = self.original_df[self.original_df['Type'] == 'Std'].copy(deep=True)
        updated_df = pd.concat([self.corrected_df, std_data], ignore_index=True)
        self.app.set_data(updated_df, for_results=True)
        self.save_corrected_drift()
        self.data_changed.emit(); self.app.notify_data_changed()
        self.update_displays()
        QMessageBox.information(self, "Success", "Corrections applied.")
    def on_apply_single_error(self, message):
        self.progress_dialog.close()
        QMessageBox.critical(self, "Error", message)
    def save_corrected_drift(self):
        try:
            if not hasattr(self.app.results, 'corrected_drift'): self.app.results.corrected_drift = {}
            self.app.results.corrected_drift.update(self.corrected_drift)
            drift_data = [{'Solution Label': k[0], 'Element': k[1], 'Ratio': v} for k, v in self.corrected_drift.items()]
            drift_df = pd.DataFrame(drift_data)
            if not hasattr(self.app.results, 'report_change'): self.app.results.report_change = pd.DataFrame(columns=['Solution Label', 'Element', 'Ratio'])
            if not drift_df.empty:
                self.app.results.report_change = self.app.results.report_change[~self.app.results.report_change['Element'].isin(drift_df['Element'])]
                self.app.results.report_change = pd.concat([self.app.results.report_change, drift_df], ignore_index=True)
        except Exception as e:
            logger.error(f"Error saving corrected_drift: {str(e)}")
    def undo_correction(self):
        if self.undo_stack:
            self.corrected_df, self.rm_df, self.corrected_drift = self.undo_stack.pop()
            std_data = self.original_df[self.original_df['Type'] == 'Std'].copy(deep=True)
            updated_df = pd.concat([self.corrected_df, std_data], ignore_index=True)
            self.app.set_data(updated_df, for_results=True)
            self.save_corrected_drift()
            self.data_changed.emit(); self.app.notify_data_changed()
            self.update_displays()
            self.undo_button.setEnabled(bool(self.undo_stack))
            QMessageBox.information(self, "Success", "Last correction undone.")
            try:
                self.app.results.reset_cache()
                self.app.results.show_processed_data()
            except Exception as e:
                logger.error(f"Error updating results: {str(e)}")
        else:
            QMessageBox.warning(self, "Warning", "No corrections to undo.")