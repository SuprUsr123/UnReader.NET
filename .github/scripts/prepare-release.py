import os
run_number = int(os.environ["RELEASE_RUN_NUMBER"])
version = f"1.0.{run_number}"
tag = f"v{version}"

with open(os.environ["GITHUB_ENV"], "a", encoding="utf-8") as output:
    output.write(f"UNREADER_VERSION={version}\n")
    output.write(f"BUILD_LABEL={tag}\n")

with open(os.environ["GITHUB_OUTPUT"], "a", encoding="utf-8") as output:
    output.write(f"version={version}\n")
    output.write(f"tag={tag}\n")
