#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
Скрипт для конвертации Mermaid диаграмм из MMD файла в SVG формат
Использует онлайн API mermaid.ink (не требует Node.js)
Требует: библиотека requests
Установка: pip install requests
"""

import os
import sys
import base64
import time
import platform
from pathlib import Path

try:
    import requests
except ImportError:
    print("Ошибка: библиотека requests не установлена!")
    print("Установите: pip install requests")
    sys.exit(1)

# Количество попыток рендеринга при неудаче
MAX_RETRIES = 5
# Задержка между попытками (в секундах)
RETRY_DELAY = 2


def clear_screen():
    """
    Очистить экран терминала
    """
    if platform.system() == 'Windows':
        os.system('cls')
    else:
        os.system('clear')


def render_mermaid_to_svg(
    mermaid_code, max_retries=MAX_RETRIES, retry_delay=RETRY_DELAY
):
    """
    Рендеринг Mermaid диаграммы в SVG через онлайн API mermaid.ink
    с повторными попытками

    Args:
        mermaid_code: Код Mermaid диаграммы
        max_retries: Максимальное количество попыток
        retry_delay: Задержка между попытками в секундах

    Returns:
        SVG содержимое или None при неудаче
    """
    # Пытаемся загрузить SVG с повторными попытками
    for attempt in range(1, max_retries + 1):
        try:
            # Кодируем диаграмму в base64 URL-safe
            encoded = base64.urlsafe_b64encode(
                mermaid_code.encode('utf-8')
            ).decode('utf-8').rstrip('=')

            # Формируем URL для получения SVG
            url = f"https://mermaid.ink/svg/{encoded}"

            # Загружаем SVG
            response = requests.get(url, timeout=30)

            if response.status_code == 200:
                svg_content = response.text

                # Проверяем, что это валидный SVG
                is_valid = (
                    svg_content.strip().startswith('<svg') or
                    svg_content.strip().startswith('<?xml')
                )
                if is_valid:
                    if attempt > 1:
                        print(f"  ✓ Успешно после {attempt} попытки")
                    return svg_content
                else:
                    # Получили не SVG
                    if attempt < max_retries:
                        msg = f"  Попытка {attempt}/{max_retries}: "
                        msg += "получен невалидный SVG, повтор..."
                        print(msg)
                    else:
                        msg = "  Ошибка: получен невалидный SVG "
                        msg += f"после {max_retries} попыток"
                        print(msg)
            else:
                # HTTP ошибка
                if attempt < max_retries:
                    msg = f"  Попытка {attempt}/{max_retries}: "
                    msg += f"HTTP {response.status_code}, повтор..."
                    print(msg)
                else:
                    msg = "  Ошибка при рендеринге диаграммы "
                    msg += f"после {max_retries} попыток: "
                    msg += f"HTTP {response.status_code}"
                    print(msg)
                    if response.text:
                        error_preview = response.text[:200]
                        print(f"    Ответ сервера: {error_preview}...")

        except requests.exceptions.Timeout:
            if attempt < max_retries:
                msg = f"  Попытка {attempt}/{max_retries}: "
                msg += "таймаут запроса, повтор..."
                print(msg)
            else:
                print(f"  Ошибка: таймаут после {max_retries} попыток")

        except requests.exceptions.ConnectionError as e:
            if attempt < max_retries:
                msg = f"  Попытка {attempt}/{max_retries}: "
                msg += "ошибка подключения, повтор..."
                print(msg)
            else:
                error_msg = (
                    "\n"
                    "=" * 50 + "\n"
                    "КРИТИЧЕСКАЯ ОШИБКА: не удалось подключиться к API!\n"
                    "=" * 50 + "\n"
                    "Проверьте подключение к интернету\n"
                    "API: https://mermaid.ink\n"
                    "\n"
                    f"Детали ошибки: {e}\n"
                    "=" * 50
                )
                print(error_msg)
                raise RuntimeError(
                    "Не удалось подключиться к mermaid.ink API. "
                    "Проверьте подключение к интернету."
                ) from e

        except Exception as e:
            if attempt < max_retries:
                msg = f"  Попытка {attempt}/{max_retries}: "
                msg += f"неожиданная ошибка ({str(e)[:50]}...), повтор..."
                print(msg)
            else:
                msg = "  Неожиданная ошибка при рендеринге Mermaid "
                msg += f"после {max_retries} попыток: {e}"
                print(msg)

        # Задержка перед следующей попыткой (кроме последней)
        if attempt < max_retries:
            time.sleep(retry_delay)

    return None


def extract_mermaid_diagrams(mmd_file):
    """
    Извлечь все блоки Mermaid диаграмм из файла
    Возвращает список кортежей (номер, код)

    Raises:
        IOError: если файл не может быть прочитан
        UnicodeDecodeError: если файл имеет неверную кодировку
    """
    diagrams = []

    try:
        with open(mmd_file, 'r', encoding='utf-8') as f:
            content = f.read()
    except IOError as e:
        raise IOError(
            f"Не удалось прочитать файл {mmd_file}: {e}"
        ) from e
    except UnicodeDecodeError as e:
        # Перевыбрасываем оригинальное исключение с дополнительным контекстом
        error_msg = f"Ошибка кодировки в файле {mmd_file}: {e}"
        raise UnicodeDecodeError(
            e.encoding, e.object, e.start, e.end, error_msg
        ) from e

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
    # Очищаем экран в начале работы
    clear_screen()

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
    try:
        diagrams = extract_mermaid_diagrams(mmd_file)
    except (IOError, UnicodeDecodeError) as e:
        print(f"Критическая ошибка при чтении файла: {e}")
        raise  # Останавливаем выполнение

    if not diagrams:
        print("Диаграммы Mermaid не найдены в файле!")
        return False

    print(f"Найдено диаграмм: {len(diagrams)}")
    print()

    # Конвертируем каждую диаграмму
    success_count = 0
    for diagram_num, mermaid_code in diagrams:
        print(f"Обработка диаграммы {diagram_num}/{len(diagrams)}...")

        try:
            # Рендерим в SVG
            svg_content = render_mermaid_to_svg(mermaid_code)

            if svg_content:
                # Сохраняем SVG файл
                svg_filename = f"diagram_{diagram_num:03d}.svg"
                svg_path = output_dir / svg_filename

                try:
                    with open(svg_path, 'w', encoding='utf-8') as f:
                        f.write(svg_content)
                    print(f"  ✓ Сохранено: {svg_filename}")
                    success_count += 1
                except IOError as e:
                    print(f"  ✗ Ошибка при сохранении {svg_filename}: {e}")
                    raise  # Останавливаем выполнение
            else:
                print(f"  ✗ Ошибка при конвертации диаграммы {diagram_num}")
        except RuntimeError:
            # RuntimeError от FileNotFoundError - уже обработано,
            # просто пробрасываем
            raise
        except Exception as e:
            error_msg = (
                "\n"
                "=" * 50 + "\n"
                f"КРИТИЧЕСКАЯ ОШИБКА при обработке диаграммы {diagram_num}\n"
                "=" * 50 + "\n"
                f"Ошибка: {e}\n"
                "=" * 50
            )
            print(error_msg)
            raise  # Останавливаем выполнение

    print()
    print("=" * 50)
    if success_count == len(diagrams):
        msg = "✓ Конвертация завершена успешно: "
        msg += f"{success_count}/{len(diagrams)} диаграмм"
        print(msg)
    elif success_count > 0:
        msg = "⚠ Конвертация завершена частично: "
        msg += f"{success_count}/{len(diagrams)} диаграмм"
        print(msg)
        print("  Некоторые диаграммы не удалось конвертировать")
    else:
        msg = f"✗ Конвертация не удалась: 0/{len(diagrams)} диаграмм"
        print(msg)
        msg2 = "  Проверьте наличие mermaid-cli и корректность диаграмм"
        print(msg2)

    print(f"  Файлы сохранены в: {output_dir}")
    print()
    print("=" * 50)
    print("Программа завершена.")
    print("=" * 50)

    # Возвращаем True если хотя бы одна диаграмма была успешно конвертирована
    return success_count > 0


if __name__ == '__main__':
    try:
        # Определяем путь к файлу
        if len(sys.argv) > 1:
            mmd_file = sys.argv[1]
            if not os.path.isabs(mmd_file):
                # Если путь относительный, делаем его абсолютным
                mmd_file = os.path.abspath(mmd_file)
        else:
            usage = "Использование: python convert_mmd_to_svg.py "
            usage += "<путь_к_mmd_файлу>"
            print(usage)
            print("Или используйте конфигурацию запуска в VS Code/Cursor")
            sys.exit(1)

        success = convert_mmd_to_svg(mmd_file)
        # Завершаем с кодом 0 при успехе, 1 при полной неудаче
        exit_code = 0 if success else 1
        print()  # Пустая строка перед завершением
        sys.exit(exit_code)
    except KeyboardInterrupt:
        print("\n\nПрервано пользователем")
        sys.exit(130)
    except RuntimeError:
        # RuntimeError уже содержит понятное сообщение об ошибке
        # (например, когда npx/mmdc не найдены)
        print("\n\nПрограмма остановлена из-за критической ошибки.")
        sys.exit(1)
    except Exception as e:
        print(f"\n\nКритическая ошибка: {e}")
        import traceback
        traceback.print_exc()
        sys.exit(1)
