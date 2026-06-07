# GanyuEditor User Guide

GanyuEditor is a Unity editor tool for authoring GoldSrc physics data. Its main workflow is: read a GoldSrc `*.mdl` skeleton, configure ragdoll physics in a Unity scene, and export the result as a `*.gpd` file.

## Background: what this physics data is for

In character ragdoll physics, the game usually does not simulate the rendered mesh directly. Instead, it simulates a simplified set of physical objects attached to bones.

### Bone

GoldSrc models are driven by bones. After importing a `*.mdl`, the tool creates a matching bone hierarchy in Unity and adds a `StudioBone` component to each generated bone object.

`StudioBone` stores:

- bone name
- bone index
- world transform
- parent-child hierarchy relationship

The exported `.gpd` uses bone indices to bind physics data back to the original model.

### Body / PhysicsBody

A body is a rigid body in the physics setup. It represents a simulated physical object associated with a bone.

In this tool, once a bone has a `PhysicsBody` component, that bone is considered part of the ragdoll export.

`PhysicsBody` mainly contains:

- the owning bone index
- whether it is an attachment / jiggle body

If `IsAttachment` is checked, the exported rigid body is marked as an attachment body. This is typically used for hair, ornaments, cloth pieces, and other lightweight secondary motion parts.

### Shape / CollisionShape

A shape is the collision representation of a body. Ragdolls usually do not use the full render mesh for collision. They use simple primitives that approximate body parts.

Currently supported:

- `BoxCollisionShapeComponent`: box shape, useful for chest, abdomen, pelvis, and other block-like parts
- `CapsuleCollisionShapeComponent`: capsule shape, useful for head, arms, and legs

Each `PhysicsBody` must have at least one `CollisionShapeComponent`. The tool draws these shapes as scene gizmos so they can be adjusted visually.

### Constraint

A constraint connects two bodies so they can move like a joint.

Currently supported:

- `SphericalConstraintComponent`: point connection with no angular limits
- `HingeConstraintComponent`: hinge joint, suitable for elbows and knees
- `ConeTwistConstraintComponent`: cone-and-twist joint, suitable for shoulders, hips, neck, and spine

The constraint is attached to the current bone, and `ConnectedBody` points to the parent or upstream body that it should connect to.

### GPD file

The `.gpd` file is the exported GoldSrc physics data file. It is an XML document with three main blocks:

- `collision-shape-block`
- `rigidbody-block`
- `constraint-block`

The exporter also writes the model header checksum so the `.gpd` can be matched to the correct `.mdl`.

## Basic workflow

### 1. Import model bones

Menu:

```text
GoldsrcPhysics/ImportStudioModelBones(.mdl)
```

Steps:

1. Open the menu and fill in `ModelPath`.
2. Select a GoldSrc `*.mdl` file.
3. Confirm the dialog.

The tool validates that the file is a GoldSrc Studio Model, then creates a model root object and a full bone hierarchy in the scene.

The root object gets a `ModelInfo` component that stores:

- model path
- model checksum
- default export path, which is the same file path with `.mdl` replaced by `.gpd`

### 2. Auto-build a ragdoll

Menu:

```text
GameObject/SetupRagdoll...
```

This is intended for humanoid characters. Open the wizard, assign all required body-part bone slots, then click `Setup`.

Requirements:

- the model should preferably be in T-pose
- the character should face Unity `-Z`
- all assigned bones must belong to the same model root with `ModelInfo`

The tool will automatically:

- create capsule shapes for head, limbs, and pelvis
- create box shapes for spine and chest
- add `PhysicsBody` to the relevant bones
- create constraints for spine, neck, shoulders, hips, elbows, and knees
- apply a default set of angular limits

### 3. Use bone naming conventions

The `SetupRagdoll` wizard also includes convention-related fields:

- `SelectedConvention`: choose a saved bone naming convention and auto-fill the bone slots
- `Conventions`: list of saved convention names; renaming is allowed, and an empty name means delete
- `SaveCurrent`: whether the current bone assignment should be appended as a new convention when saving
- `SaveConvension`: save conventions into `BoneNameConvensions.xml`

Typical usage:

1. Manually assign bone slots for a model family once.
2. Check `SaveCurrent`.
3. Click `SaveConvension`.
4. Rename the new entry in `Conventions`.
5. For later models with the same naming scheme, just choose `SelectedConvention`.

### 4. Manually adjust shapes

After auto-generation, it is common to inspect and fine-tune the setup in the scene:

- select the bone that has the shape
- adjust `LocalCenter`, `Rotation`, `HalfExtent`, `Radius`, and `Height`
- use gizmos to confirm that each box or capsule covers the intended body part

White gizmos indicate normal bodies. Blue gizmos indicate bodies with `IsAttachment` enabled.

### 5. Manually add constraints

If a bone needs a constraint added manually, right-click the bone and use:

```text
GameObject/GoldsrcPhysics/AddHinge(Auto Parent)
GameObject/GoldsrcPhysics/AddConeTwist(Auto Parent)
```

These commands search upward in the hierarchy for the nearest parent `PhysicsBody` and assign it to `ConnectedBody` automatically.

The selected object must already have both `StudioBone` and `PhysicsBody`.

### 6. Export physics data

Right-click the model root object and use:

```text
GameObject/ExportRagdoll (same path)
```

The export path comes from `ModelInfo.OutputPath`, usually the original `.mdl` path with the extension changed to `.gpd`.

If the target file already exists, the tool asks whether it should be overwritten.

### 7. Load an existing GPD

Right-click the model root object and use:

```text
GameObject/LoadRagdoll (.gpd)...
```

After selecting an existing `.gpd`, the tool imports its bodies, shapes, and constraints back onto the current model bones.

## Notes and caveats

- Do not export from a non-root object. The root object must have `ModelInfo`.
- Every exported physical bone should have `StudioBone`, `PhysicsBody`, and at least one shape.
- `ConnectedBody` should not be null for constraints that are expected to connect to another body.
- Capsules are currently drawn and exported with the X axis as the length axis.
- Coordinates are converted between Unity space and GoldSrc space, so matrix-related code should be changed very carefully.
- `BoneNameConvensions.xml` is user data that stores reusable bone naming conventions.
