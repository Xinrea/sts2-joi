#!/usr/bin/env python3
"""
Extract card frame assets from STS2 atlas
"""
from PIL import Image
from pathlib import Path

# Atlas paths
ATLAS_0 = Path("D:/Projects/sts2/images/atlases/ui_atlas_0.png")
ATLAS_1 = Path("D:/Projects/sts2/images/atlases/ui_atlas_1.png")

# Output directory
OUTPUT_DIR = Path("D:/Projects/sts2-joi/docs/static/card_frames")
OUTPUT_DIR.mkdir(parents=True, exist_ok=True)

# Assets to extract: (atlas_file, x, y, width, height, output_name)
assets = [
    # Card frames
    (ATLAS_0, 1320, 83, 598, 844, "card_frame_attack.png"),
    (ATLAS_0, 621, 929, 598, 844, "card_frame_power.png"),
    (ATLAS_0, 1221, 929, 598, 844, "card_frame_skill.png"),

    # Portrait borders
    (ATLAS_1, 1329, 1, 551, 420, "card_portrait_border_attack.png"),
    (ATLAS_1, 674, 148, 551, 420, "card_portrait_border_power.png"),
    (ATLAS_1, 1313, 423, 551, 420, "card_portrait_border_skill.png"),

    # Banner
    (ATLAS_1, 674, 1, 653, 145, "card_banner.png"),
]

def extract_assets():
    for atlas_path, x, y, w, h, output_name in assets:
        print(f"Extracting {output_name}...")

        # Load atlas
        atlas = Image.open(atlas_path)

        # Crop region
        region = atlas.crop((x, y, x + w, y + h))

        # Save
        output_path = OUTPUT_DIR / output_name
        region.save(output_path)
        print(f"  Saved to {output_path}")

    print(f"\nExtracted {len(assets)} assets")

if __name__ == "__main__":
    extract_assets()
