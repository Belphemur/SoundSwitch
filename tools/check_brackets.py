import sys
import os
import re
import xml.etree.ElementTree as ET


def check_brackets(file_path):
    if file_path.lower().endswith(".resx"):
        return check_resx_brackets(file_path)
    else:
        return check_regular_brackets(file_path)


def check_regular_brackets(file_path):
    with open(file_path, "r", encoding="utf-8") as file:
        content = file.read()

    stack = []
    for i, char in enumerate(content):
        if char == "{":
            stack.append(i)
        elif char == "}":
            if not stack:
                print(f"Unmatched closing bracket at position {i} in file {file_path}")
                return False
            stack.pop()

    if stack:
        for pos in stack:
            print(f"Unmatched opening bracket at position {pos} in file {file_path}")
        return False

    print(f"All brackets are properly closed in file {file_path}.")
    return True


def check_resx_brackets(file_path):
    try:
        tree = ET.parse(file_path)
        root = tree.getroot()

        all_brackets_valid = True

        # Find all data elements with a name attribute
        for data in root.findall(".//data"):
            name_attr = data.get("name")
            if name_attr is None:
                continue

            # Get the value element within this data element
            value_elem = data.find("value")
            if value_elem is None or value_elem.text is None:
                continue

            value_text = value_elem.text

            # Check brackets in the value text
            stack = []
            for i, char in enumerate(value_text):
                if char == "{":
                    stack.append(i)
                elif char == "}":
                    if not stack:
                        print(
                            f"Unmatched closing bracket at position {i} in language key '{name_attr}' in file {file_path}"
                        )
                        all_brackets_valid = False
                        break
                    stack.pop()

            if stack:
                print(
                    f"Unmatched opening bracket at position(s) {', '.join(str(pos) for pos in stack)} in language key '{name_attr}' in file {file_path}"
                )
                all_brackets_valid = False

        if all_brackets_valid:
            print(
                f"All brackets are properly closed in all language keys in file {file_path}."
            )
            return True
        return False

    except Exception as e:
        print(f"Error parsing RESX file {file_path}: {e}")
        return False


def main():
    if len(sys.argv) < 2:
        print("Usage: python check_brackets.py <file_path1> <file_path2> ...")
        sys.exit(1)

    all_files_valid = True
    for file_path in sys.argv[1:]:
        if not check_brackets(file_path):
            all_files_valid = False

    if not all_files_valid:
        sys.exit(1)


if __name__ == "__main__":
    main()
