import json

json_path = r"C:\Users\jdeacutis\Downloads\forge_halo.json"

tags = json.load(open(json_path))

id_map = {}

i = 0
for category in tags['Data']['Map Variant Palettes']:
    for item in category['Entries']:
        entry_name = item['Name']
        variants = item['Variants']
        do_inc = len(variants)
        for var in variants:
            display_name = var['Display Name']
            id_map[i] = entry_name if display_name == "" else display_name
            if do_inc: i += 1
        i = (i + (1 << 8)) & 0xFF00

print(id_map)