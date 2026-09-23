"""Census the state-colour setters in our style layer, so #56 starts from a measured matrix instead of the
number the ledger carries (that reading was taken on an earlier tree).

Two rulers, deliberately kept apart:
  ruler A - every <Setter TargetName="..." Property="...">   (the 569-shape, what the layout cells write)
  ruler B - every <Setter Property="Foreground" ...>         (the 228-shape, the state COLOURS)

Outputs, per file: counts, and for ruler B the distinct (TargetName, value-key) pairs so a state cell that
writes an unresolvable key stands out. Reads only.
"""

import glob
import os
import re
import sys
from collections import Counter, defaultdict

SETTER = re.compile(r'<Setter\b([^>]*)/?>')
ATTR = re.compile(r'(\w+(?:\.\w+)?|Name|TargetName|Property|Value)\s*=\s*"([^"]*)"')
STYLE = re.compile(r'<Style\b[^>]*TargetType="([^"]+)"')
THEME = re.compile(r'\{ThemeResource\s+([A-Za-z0-9_.]+)\s*\}')


def attrs(raw):
    return {key: value for key, value in ATTR.findall(raw)}


def main(root):
    rows = []
    for path in sorted(glob.glob(os.path.join(root, '*.jalxaml'))):
        name = os.path.basename(path)
        current_style = None
        with open(path, encoding='utf-8') as handle:
            for number, line in enumerate(handle, start=1):
                style = STYLE.search(line)
                if style:
                    current_style = style.group(1)
                for match in SETTER.finditer(line):
                    found = attrs(match.group(1))
                    if 'Property' not in found:
                        continue
                    keys = THEME.findall(found.get('Value', ''))
                    rows.append({
                        'file': name,
                        'line': number,
                        'owner': current_style,
                        'target': found.get('TargetName', ''),
                        'property': found['Property'],
                        'value': found.get('Value', ''),
                        'key': keys[0] if keys else '',
                    })

    ruler_a = [row for row in rows if row['target']]
    ruler_b = [row for row in rows if row['property'] == 'Foreground']
    print(f'total <Setter> rows: {len(rows)}')
    print(f'ruler A (TargetName writes): {len(ruler_a)}')
    print(f'ruler B (Property="Foreground"): {len(ruler_b)}')
    print(f'ruler B that also name a target: {len([row for row in ruler_b if row["target"]])}')
    print()
    print('ruler B per file:')
    for file, count in Counter(row['file'] for row in ruler_b).most_common():
        print(f'  {file:26} {count:4}')
    print()
    print('ruler B owners:')
    for owner, count in Counter(str(row['owner']) for row in ruler_b).most_common():
        print(f'  {owner:46} {count:4}')
    print()
    print('ruler B rows whose Value carries no {ThemeResource} key (literal or TemplateBinding):')
    for row in ruler_b:
        if not row['key']:
            print(f"  {row['file']}:{row['line']} target={row['target'] or '-'} value={row['value'][:60]}")
    print()
    keys = defaultdict(int)
    for row in ruler_b:
        if row['key']:
            keys[row['key']] += 1
    print(f'ruler B distinct theme keys: {len(keys)}')
    for key, count in sorted(keys.items(), key=lambda item: -item[1])[:12]:
        print(f'  {key:46} {count:4}')


if __name__ == '__main__':
    main(sys.argv[1] if len(sys.argv) > 1 else 'src/FluentJalium/Styles')
