import os
import re

tag = os.environ.get("RELEASE_TAG", "")
if not re.fullmatch(r"v\d+\.\d+\.\d+(?:-[0-9A-Za-z.-]+)?", tag):
    raise SystemExit("Tag must use vMAJOR.MINOR.PATCH, for example v1.21.1 or v1.22.0-pre.1.")

with open(os.environ["GITHUB_ENV"], "a", encoding="utf-8") as output:
    output.write(f"UNREADER_VERSION={tag[1:]}\n")
