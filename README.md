# Entitas Blueprints
[Entitas](https://github.com/sschmid/Entitas-CSharp) is a super fast Entity
Component System (ECS) specifically made for C# and Unity, developed by Simon
Schmid (Wooga) and open-source developers.

Entitas blueprints extend Entitas by adding data blueprints, allowing
developers to create entities driven by data instead of hard-coded values.

Current version based on
[Entitas 0.27.0](https://github.com/sschmid/Entitas-CSharp/releases/tag/0.27.0).

## Setup

1. Download Entitas Blueprints source code files.
1. Download [Entitas 0.27.0](https://github.com/sschmid/Entitas-CSharp/releases/tag/0.27.0).
1. Extract the Entitas and Entitas.CodeGenerator folders to Source\Entitas.Blueprints.

## Development Cycle

New releases of Entitas Blueprints are created using [Semantic Versioning](http://semver.org/). In short:

* Version numbers are specified as MAJOR.MINOR.PATCH.
* MAJOR version increases indicate incompatible changes with respect to the public UnityQuery API.
* MINOR version increases indicate new functionality that are backwards-compatible.
* PATCH version increases indicate backwards-compatible bug fixes.

Each release is built against a specific version of Entitas, but may work with others as well.

## Bugs & Feature Requests

After verifying that you are using the [latest version](https://github.com/npruehs/Entitas-Blueprints-CSharp/releases)
of Entitas Blueprints and having checked whether a [similar issue](https://github.com/npruehs/Entitas-Blueprints-CSharp/issues)
has already been reported, feel free to [open a new issue](https://github.com/npruehs/Entitas-Blueprints-CSharp/issues/new).
In order to help us resolving your problem as fast as possible, please include the following details in your report:

* Steps to reproduce
* What happened?
* What did you expect to happen?

After being able to reproduce the issue, we'll look into fixing it immediately.

## License

Entitas Blueprints is released under the [MIT license](https://github.com/npruehs/Entitas-Blueprints-CSharp/blob/master/README.md).