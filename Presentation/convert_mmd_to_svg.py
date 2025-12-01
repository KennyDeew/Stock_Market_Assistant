#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
Скрипт для конвертации Mermaid диаграмм из MMD файла в SVG формат
Требует: requests
"""

import os
import sys
import base64
import time
import requests
from pathlib import Path

# Количество попыток рендеринга при неудаче
MAX_RETRIES = 5
# Задержка между попытками (в секундах)
RETRY_DELAY = 2


def render_mermaid_to_svg(mermaid_code, max_retries=MAX_RETRIES, retry_delay=RETRY_DELAY):
    """
    Рендеринг Mermaid диаграммы в SVG через mermaid.ink API с повторными попытками

    Args:
        mermaid_code: Код Mermaid диаграммы
        max_retries: Максимальное количество попыток (по умолчанию MAX_RETRIES)
        retry_delay: Задержка между попытками в секундах (по умолчанию RETRY_DELAY)

    Returns:
        SVG содержимое или None при неудаче
    """
    # Кодируем диаграмму в base64 URL-safe
    encoded = base64.urlsafe_b64encode(mermaid_code.encode('utf-8')).decode('utf-8').rstrip('=')
    url = f"https://mermaid.ink/svg/{encoded}"

    # Пытаемся загрузить SVG с повторными попытками
    for attempt in range(1, max_retries + 1):
        try:
            response = requests.get(url, timeout=30)

            if response.status_code == 200:
                # Проверяем, что получили валидный SVG (не страницу с ошибкой)
                svg_content = response.text
                if svg_content.strip().startswith('<svg') or svg_content.strip().startswith('<?xml'):
                    if attempt > 1:
                        print(f"  ✓ Успешно после {attempt} попытки")
                    return svg_content
                else:
                    # Получили не SVG, возможно ошибка от API
                    if attempt < max_retries:
                        print(f"  Попытка {attempt}/{max_retries}: получен невалидный ответ, повтор...")
                    else:
                        print(f"  Ошибка: получен невалидный SVG после {max_retries} попыток")
            else:
                # HTTP ошибка
                if attempt < max_retries:
                    print(f"  Попытка {attempt}/{max_retries}: HTTP {response.status_code}, повтор через {retry_delay}с...")
                else:
                    print(f"  Ошибка при рендеринге диаграммы: HTTP {response.status_code} после {max_retries} попыток")

        except requests.exceptions.Timeout:
            if attempt < max_retries:
                print(f"  Попытка {attempt}/{max_retries}: таймаут, повтор через {retry_delay}с...")
            else:
                print(f"  Ошибка: таймаут после {max_retries} попыток")

        except requests.exceptions.RequestException as e:
            if attempt < max_retries:
                print(f"  Попытка {attempt}/{max_retries}: ошибка сети ({str(e)[:50]}...), повтор через {retry_delay}с...")
            else:
                print(f"  Ошибка при рендеринге Mermaid после {max_retries} попыток: {e}")

        except Exception as e:
            if attempt < max_retries:
                print(f"  Попытка {attempt}/{max_retries}: неожиданная ошибка ({str(e)[:50]}...), повтор через {retry_delay}с...")
            else:
                print(f"  Неожиданная ошибка при рендеринге Mermaid после {max_retries} попыток: {e}")

        # Задержка перед следующей попыткой (кроме последней)
        if attempt < max_retries:
            time.sleep(retry_delay)

    return None


def extract_mermaid_diagrams(mmd_file):
    """
    Извлечь все блоки Mermaid диаграмм из файла
    Возвращает список кортежей (номер, код)
    """
    diagrams = []

    with open(mmd_file, 'r', encoding='utf-8') as f:
        content = f.read()

    lines = content.split('\n')
    i = 0
    diagram_num = 0

    while i < len(lines):
        line = lines[i].strip()

        # Ищем начало блока mermaid
        if line.startswith('```mermaid'):
            code_lines = []
            i += 1
            # Собираем код до закрывающего ```
            while i < len(lines) and not lines[i].strip().startswith('```'):
                code_lines.append(lines[i])
                i += 1

            if code_lines:
                diagram_num += 1
                code = '\n'.join(code_lines)
                diagrams.append((diagram_num, code))

        i += 1

    return diagrams


def sanitize_filename(filename):
    """
    Очистить имя файла от недопустимых символов
    """
    # Заменяем недопустимые символы на подчеркивания
    invalid_chars = '<>:"/\\|?*'
    for char in invalid_chars:
        filename = filename.replace(char, '_')
    return filename


def convert_mmd_to_svg(mmd_file):
    """
    Конвертировать MMD файл в SVG файлы
    """
    if not os.path.exists(mmd_file):
        print(f"Ошибка: файл {mmd_file} не найден!")
        return False

    # Проверяем расширение файла
    if not mmd_file.lower().endswith('.mmd'):
        print(f"Предупреждение: файл {mmd_file} не имеет расширения .mmd")

    # Получаем путь и имя файла
    mmd_path = Path(mmd_file)
    mmd_dir = mmd_path.parent
    mmd_name = mmd_path.stem  # имя без расширения

    # Создаем папку для SVG файлов
    output_dir = mmd_dir / mmd_name
    output_dir.mkdir(exist_ok=True)

    print(f"Конвертация {mmd_file}")
    print(f"Выходная папка: {output_dir}")
    print("=" * 50)

    # Извлекаем все диаграммы
    diagrams = extract_mermaid_diagrams(mmd_file)

    if not diagrams:
        print("Диаграммы Mermaid не найдены в файле!")
        return False

    print(f"Найдено диаграмм: {len(diagrams)}")
    print()

    # Конвертируем каждую диаграмму
    success_count = 0
    for diagram_num, mermaid_code in diagrams:
        print(f"Обработка диаграммы {diagram_num}/{len(diagrams)}...")

        # Рендерим в SVG
        svg_content = render_mermaid_to_svg(mermaid_code)

        if svg_content:
            # Сохраняем SVG файл
            svg_filename = f"diagram_{diagram_num:03d}.svg"
            svg_path = output_dir / svg_filename

            with open(svg_path, 'w', encoding='utf-8') as f:
                f.write(svg_content)

            print(f"  ✓ Сохранено: {svg_filename}")
            success_count += 1
        else:
            print(f"  ✗ Ошибка при конвертации диаграммы {diagram_num}")

    print()
    print("=" * 50)
    print(f"✓ Конвертация завершена: {success_count}/{len(diagrams)} диаграмм")
    print(f"  Файлы сохранены в: {output_dir}")

    return success_count > 0


if __name__ == '__main__':
    # Определяем путь к файлу
    if len(sys.argv) > 1:
        mmd_file = sys.argv[1]
        if not os.path.isabs(mmd_file):
            # Если путь относительный, делаем его абсолютным относительно рабочей директории
            mmd_file = os.path.abspath(mmd_file)
    else:
        print("Использование: python convert_mmd_to_svg.py <путь_к_mmd_файлу>")
        print("Или используйте конфигурацию запуска в VS Code/Cursor")
        sys.exit(1)

    success = convert_mmd_to_svg(mmd_file)
    sys.exit(0 if success else 1)

