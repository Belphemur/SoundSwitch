import sys
import os

def check_brackets(file_path):
    with open(file_path, 'r', encoding='utf-8') as file:
        content = file.read()
    
    stack = []
    for i, char in enumerate(content):
        if char == '{':
            stack.append(i)
        elif char == '}':
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