import sys
from git import Repo

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

def get_changed_resx_files(repo_path):
    repo = Repo(repo_path)
    changed_files = [item.a_path for item in repo.index.diff(None) if item.a_path.endswith('.resx')]
    return changed_files

def main(repo_path):
    changed_resx_files = get_changed_resx_files(repo_path)
    if not changed_resx_files:
        print("No .resx files changed.")
        return
    
    all_files_valid = True
    for file_path in changed_resx_files:
        if not check_brackets(file_path):
            all_files_valid = False
    
    if not all_files_valid:
        sys.exit(1)

if __name__ == "__main__":
    if len(sys.argv) != 2:
        print("Usage: python check_brackets.py <repo_path>")
        sys.exit(1)
    
    repo_path = sys.argv[1]
    main(repo_path)