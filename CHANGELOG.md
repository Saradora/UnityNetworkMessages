# Change Log
All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](http://keepachangelog.com/)
and this project adheres to [Semantic Versioning](http://semver.org/).

## [Unreleased]

//

## [1.1.0] - 2024-01-24

### Added
- Added reflection Invoke with return value

### Changed
- Made logging more generic

## [1.0.0] - 2024-01-22

### Added
- Added Postfix to ``Object.Instantiate()`` methods
- Patched last missing instantiate method ``Object.Instantiate<T>(T original)``
- Added utils for creating prefabs
- Added automatic binding of ConfigDatas

## [0.1.0] - 2024-01-20

### Added
- Patched ``Object.Instantiate(Object original)`` methods to modify original prefabs before cloning
- Patched ``GameObject.AddComponent()`` to automatically add code to any new component
- InjectToComponent Attribute to automatically add any MonoBehaviour to specified component
- Initializer Attribute to simulate Unity's RuntimeInitializeOnLoad
- SceneConstructor Attribute to execute code after a scene is loaded