# STS2 JOI

## Workshop build

Build a local Steam Workshop package:

```sh
scripts/build_workshop_release.sh
```

Override the manifest version:

```sh
scripts/build_workshop_release.sh v1.0.10
```

Output:

- `dist/workshop/Joi-<version>/`: ModUploader workshop upload folder.

The generated folder follows the ModUploader template:

- `workshop.json`: copied from the project root template.
- `image.png`: Workshop preview image.
- `content/Joi.dll`, `content/Joi.pck`, `content/Joi.json`: mod files.

Edit root `workshop.json` before building to set visibility, change notes, tags, and Workshop dependencies. Workshop dependencies must be numeric Steam Workshop item IDs, for example `3737335127` for BaseLib. BaseLib remains a dependency declared in `content/Joi.json`.
