"""Diff the geometry written *inside* each control style, upstream vs Astra.

The published Thickness rows already match upstream (survey.py proves it). What that survey cannot see is
geometry that never became a row - a MinHeight, Padding or Margin literal typed into a setter or a template
part. Those are where a spacing error hides from every resource gate, so this joins the two style sets by
TargetType and prints their geometry side by side.

Upstream is the read-only reference tree; nothing here writes outside spike/SpacingSweep.
"""
import os
import re
import sys

UPSTREAM = r"C:\git\Jalium\microsoft-ui-xaml\controls\dev"
OURS = r"C:\git\Jalium\FluentJalium\src\FluentJalium"

STYLE = re.compile(r'<Style\s+[^>]*TargetType="?(\w+)"?[^>]*>(.*?)</Style>', re.S)
ATTR = re.compile(r'\b(MinHeight|MinWidth|Height|Width|Padding|Margin|CornerRadius|Spacing|BorderThickness)="([^"]+)"')
SETTER = re.compile(r'<Setter\s+Property="(MinHeight|MinWidth|Height|Width|Padding|Margin|CornerRadius|Spacing|BorderThickness)"\s+Value="([^"]+)"')
# Only shipped theme dictionaries and common styles count as the spec. Sample pages, tests and the perf
# variants declare their own geometry and would drown the comparison.
UPSTREAM_FILE = re.compile(r"(_themeresources.*|CommonStyles.*|.*\.xaml)$")


def is_upstream_spec(path):
    name = os.path.basename(path)
    if "perf2026" in name or name.endswith("Page.xaml") or "Tests" in path:
        return False
    return name.endswith("_themeresources.xaml") or name.endswith("_themeresources_any.xaml") or (
        os.sep + "CommonStyles" + os.sep in path and name.endswith(".xaml")
    ) or name in ("Generic.xaml", "ControlsResources.xaml")


def geometry(body):
    values = {}
    for attr, value in list(ATTR.findall(body)) + list(SETTER.findall(body)):
        value = " ".join(value.split())
        if value.startswith(("{", "@")) or "TemplateBinding" in value or "ThemeResource" in value:
            value = "key:" + value.split(":")[-1].strip("}")
        values.setdefault(attr, set()).add(value)
    return values


def fmt(values):
    order = ("MinHeight", "MinWidth", "Height", "Width", "Padding", "Margin", "CornerRadius", "Spacing", "BorderThickness")
    return "  ".join(f"{a}=" + "/".join(sorted(values[a])) for a in order if a in values)


def collect(root, upstream):
    out = {}
    for dirpath, dirnames, filenames in os.walk(root):
        for name in filenames:
            if not name.endswith((".xaml", ".jalxaml")):
                continue
            path = os.path.join(dirpath, name)
            if upstream and not is_upstream_spec(path):
                continue
            text = open(path, encoding="utf-8-sig").read()
            for target, body in STYLE.findall(text):
                out.setdefault(target, []).append((name, geometry(body)))
    return out


def main():
    ours = collect(OURS, False)
    upstream = collect(UPSTREAM, True)
    print(f"target types - upstream spec: {len(upstream)}  ours: {len(ours)}")
    for target in sorted(set(ours) | set(upstream)):
        up_rows = upstream.get(target, [])
        our_rows = ours.get(target, [])
        print(f"\n### {target}   upstream {len(up_rows)} / ours {len(our_rows)}")
        for name, values in up_rows:
            print(f"  UP   {name}: {fmt(values)}")
        for name, values in our_rows:
            print(f"  OURS {name}: {fmt(values)}")


if __name__ == "__main__":
    sys.stdout.reconfigure(encoding="utf-8")
    main()
