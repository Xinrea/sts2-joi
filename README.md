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

Outputs:

- `dist/workshop/Joi-<version>/`: Workshop content folder.
- `dist/workshop/Joi-<version>-workshop.zip`: Zip with the same files.

The package contains only `Joi.dll`, `Joi.pck`, `Joi.json`, and `mod_image.png`. BaseLib remains a dependency declared in `Joi.json`.
