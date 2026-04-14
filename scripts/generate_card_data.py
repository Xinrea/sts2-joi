#!/usr/bin/env python3
"""
Extract card data from localization files and C# code
"""
import json
import os
import re
from pathlib import Path

def get_card_type(card_id, base_path):
    """Extract card type from C# file"""
    # Convert snake_case to PascalCase
    class_name = ''.join(word.capitalize() for word in card_id.split('_'))
    cs_file = base_path / f"JoiCode/Cards/{class_name}.cs"

    if not cs_file.exists():
        return "attack"  # default

    with open(cs_file, 'r', encoding='utf-8') as f:
        content = f.read()

    # Look for CardType in constructor
    match = re.search(r'CardType\.(\w+)', content)
    if match:
        card_type = match.group(1).lower()
        return card_type

    return "attack"  # default

def extract_card_data():
    """Extract card information from Chinese localization"""
    base_path = Path(__file__).parent.parent
    cards_json = base_path / "Joi/localization/zhs/cards.json"

    with open(cards_json, 'r', encoding='utf-8') as f:
        data = json.load(f)

    cards = []
    card_ids = set()

    # Extract unique card IDs
    for key in data.keys():
        if key.startswith("JOI-") and key.endswith(".title"):
            card_id = key.replace(".title", "").replace("JOI-", "")
            card_ids.add(card_id)

    # Build card objects
    for card_id in sorted(card_ids):
        title_key = f"JOI-{card_id}.title"
        desc_key = f"JOI-{card_id}.description"

        if title_key in data:
            # Convert card ID to image filename (snake_case)
            image_name = card_id.lower()
            image_path = base_path / f"Joi/images/card_portraits/{image_name}.png"

            # Only include if image exists
            if image_path.exists():
                card_type = get_card_type(image_name, base_path)
                card = {
                    "id": card_id,
                    "name": data[title_key],
                    "description": data.get(desc_key, ""),
                    "image": f"card_portraits/{image_name}.png",
                    "type": card_type
                }
                cards.append(card)

    return cards

def main():
    cards = extract_card_data()

    # Write to Hugo data file
    output_path = Path(__file__).parent.parent / "docs/data/cards.json"
    output_path.parent.mkdir(parents=True, exist_ok=True)

    with open(output_path, 'w', encoding='utf-8') as f:
        json.dump(cards, f, indent=2, ensure_ascii=False)

    print(f"Generated {len(cards)} cards to {output_path}")

if __name__ == "__main__":
    main()
