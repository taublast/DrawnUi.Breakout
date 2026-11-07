"""
Builds the font subsets served by the Blazor web app (src/Web/wwwroot/fonts).

Browsers give WASM no system fonts, so the web app downloads every font it uses at startup.
The full fonts are ~37 MB; the game only draws its own localized strings, so each font is cut
to the characters it can actually show:
  - text from every ResStrings*.resx (src/Common),
  - non-ASCII characters used in shared code (emoji, symbols),
  - the native language names shown in the language picker,
  - printable ASCII, Latin-1 and common punctuation for numbers, scores and key names.

Rerun after changing resx strings or emoji in code:
  pip install fonttools
  python dev/fonts/subset_web_fonts.py
"""
import glob
import os
import xml.etree.ElementTree as ET

from fontTools import subset
from fontTools.ttLib import TTFont

ROOT = os.path.abspath(os.path.join(os.path.dirname(__file__), "..", ".."))
MOBILE_FONTS = os.path.join(ROOT, "src", "Mobile", "Resources", "Fonts")
EMOJI_SOURCE = os.path.join(ROOT, "dev", "fonts", "NotoColorEmoji-Regular.ttf")
OUT = os.path.join(ROOT, "src", "Web", "wwwroot", "fonts")

# what CultureInfo.NativeName gives for the enabled languages (picker labels)
LANGUAGE_NAMES = (
    "English (United States) Deutsch (Deutschland) español (España) français (France) "
    "italiano (Italia) русский (Россия) 日本語 (日本) 한국어(대한민국) 中文（中国）"
)

BASE = "".join(chr(c) for c in range(0x20, 0x7F))
BASE += "".join(chr(c) for c in range(0xA0, 0x100))
BASE += "–—‘’“”…•·　、。！，：？"


def resx_text(lang):
    name = "ResStrings.resx" if lang == "en" else f"ResStrings.{lang}.resx"
    path = glob.glob(os.path.join(ROOT, "src", "Common", "**", name), recursive=True)[0]
    root = ET.parse(path).getroot()
    return "".join(v.text or "" for v in root.iter("value"))


def code_text():
    chars = set()
    for path in glob.glob(os.path.join(ROOT, "src", "Shared", "**", "*.cs"), recursive=True):
        with open(path, encoding="utf-8-sig") as f:
            chars.update(c for c in f.read() if ord(c) > 0x7E)
    return "".join(chars)


def build(source, target, text):
    font = TTFont(source)
    cmap = font.getBestCmap()
    # the game upper/lowercases strings at runtime (buttons, HUD): keep both forms of every character
    wanted = sorted(set(text + text.upper() + text.lower()) - set("\r\n\t"))
    missing = [c for c in wanted if ord(c) not in cmap and ord(c) > 0x7E]

    options = subset.Options()
    options.name_IDs = ["*"]
    options.name_languages = ["*"]
    options.ignore_missing_unicodes = True
    options.ignore_missing_glyphs = True
    options.layout_features = ["*"]
    options.notdef_outline = True

    sub = subset.Subsetter(options)
    sub.populate(unicodes=[ord(c) for c in wanted])
    sub.subset(font)
    font.save(target)

    size = os.path.getsize(target)
    print(f"{os.path.basename(target):34} {os.path.getsize(source) // 1024:>7} KB -> {size // 1024:>5} KB")
    return missing


def main():
    langs = ["en", "de", "es", "fr", "it", "ru", "ja", "ko", "zh"]
    strings = {lang: resx_text(lang) for lang in langs}
    code = code_text()
    everything = BASE + code + LANGUAGE_NAMES + "".join(strings.values())

    os.makedirs(OUT, exist_ok=True)
    mobile = lambda name: os.path.join(MOBILE_FONTS, name)
    out = lambda name: os.path.join(OUT, name)

    plan = [
        # UI text, every language (dialogs, settings, language picker)
        (mobile("ZenMaruGothic-Bold.ttf"), out("ZenMaruGothic-Bold-Subset.ttf"), everything),
        # game font for every language except ko / zh (AppLanguage.Set)
        (mobile("DelaGothicOne-Regular.ttf"), out("DelaGothicOne-Regular-Subset.ttf"),
         BASE + code + "".join(s for l, s in strings.items() if l not in ("ko", "zh"))),
        # ko / zh game fonts, also the web fallback for UI text glyphs ZenMaruGothic lacks (hangul, simplified hanzi)
        (mobile("BlackHanSans-Regular.ttf"), out("BlackHanSans-Regular-Subset.ttf"),
         BASE + code + LANGUAGE_NAMES + strings["ko"]),
        (mobile("MaShanZheng-Regular.ttf"), out("MaShanZheng-Regular-Subset.ttf"),
         BASE + code + LANGUAGE_NAMES + strings["zh"]),
        # every emoji/symbol the game draws (the font keeps only the ones it has)
        (EMOJI_SOURCE, out("NotoColorEmoji-Subset.ttf"), code + "".join(strings.values()) + "️‍"),
    ]

    for source, target, text in plan:
        missing = build(source, target, text)
        if missing and "Emoji" not in source:
            shown = "".join(c for c in missing if ord(c) > 0x2000)[:80]
            print(f"   not in source font ({len(missing)}): {shown}")


if __name__ == "__main__":
    main()
