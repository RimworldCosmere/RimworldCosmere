import os
from PIL import Image

def add_padding_to_pngs(directory: str, padding: int):
    if not os.path.isdir(directory):
        print(f"Directory '{directory}' does not exist.")
        return

    for filename in os.listdir(directory):
        if filename.lower().endswith(".png"):
            filepath = os.path.join(directory, filename)
            img = Image.open(filepath).convert("RGBA")

            new_width = img.width + padding * 2
            new_height = img.height + padding * 2

            padded_img = Image.new("RGBA", (new_width, new_height), (0, 0, 0, 0))
            padded_img.paste(img, (padding, padding))

            padded_img.save(filepath)
            print(f"Overwritten with padded image: {filepath}")

if __name__ == "__main__":
    import sys

    if len(sys.argv) < 3:
        print("Usage: python add_padding.py <directory_path> <padding_in_pixels>")
    else:
        directory_path = sys.argv[1]
        try:
            padding_amount = int(sys.argv[2])
            add_padding_to_pngs(directory_path, padding_amount)
        except ValueError:
            print("Please provide a valid integer for padding.")
