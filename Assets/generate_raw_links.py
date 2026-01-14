import os

# Ejecutar desde la carpeta raíz del repositorio
# Ejecutar con python Assets\generate_raw_links.py

# 🔧 CONFIGURA ESTO -------------------------------------------------------
GITHUB_USER = "Tato2001"
REPO_NAME = "PruebaUnity"
BRANCH = "main"
ROOT_FOLDER = r"Scripts"  # 👈 ajustado
OUTPUT_FILE = "FILES.md"
# ------------------------------------------------------------------------

def get_repo_root():
    current = os.path.abspath(os.path.dirname(__file__))
    # Si el script está en /Assets, sube un nivel
    if os.path.basename(current).lower() == "assets":
        current = os.path.dirname(current)
    while current != os.path.dirname(current):
        if os.path.isdir(os.path.join(current, ".git")):
            return current
        current = os.path.dirname(current)
    return os.path.dirname(os.path.abspath(__file__))

def generate_raw_links():
    repo_root = get_repo_root()
    scripts_path = os.path.join(repo_root, "Assets", ROOT_FOLDER)
    if not os.path.exists(scripts_path):
        scripts_path = os.path.join(repo_root, ROOT_FOLDER)

    if not os.path.exists(scripts_path):
        print(f"❌ No se encontró la carpeta: {scripts_path}")
        return

    base_url = f"https://raw.githubusercontent.com/{GITHUB_USER}/{REPO_NAME}/{BRANCH}/"
    lines = [f"# 📂 Enlaces RAW del proyecto {REPO_NAME}\n"]

    count = 0
    for root, _, files in os.walk(scripts_path):
        for file in files:
            if file.endswith(".cs"):
                rel_path = os.path.relpath(os.path.join(root, file), repo_root)
                rel_path = rel_path.replace("\\", "/")
                raw_url = base_url + rel_path
                lines.append(f"- `{rel_path}`\n  {raw_url}\n")
                count += 1

    output_path = os.path.join(repo_root, OUTPUT_FILE)
    with open(output_path, "w", encoding="utf-8") as f:
        f.write("\n".join(lines))

    print(f"✅ Archivo {OUTPUT_FILE} generado con éxito ({count} scripts encontrados).")
    print(f"📄 Guardado en: {output_path}")

if __name__ == "__main__":
    generate_raw_links()