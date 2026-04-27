# 3D Desktop Pet Phase 16 Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Enhance the pet's interaction so that when the user drags and releases the pet in mid-air, it will fall down to the bottom of the screen (taskbar level) with gravity. It will also prevent the pet from being dragged completely off the screen.

**Architecture:** We will modify `InteractionManager.cs` to add custom gravity logic. When `isDragging` is false, the script will apply downward velocity until the pet's screen Y coordinate reaches the bottom margin. We'll also clamp the `X` and `Y` coordinates during dragging so the pet can't leave the screen boundaries.

**Tech Stack:** Unity 3D, C#

---

### Task 1: Implement Screen Boundary & Gravity Logic

**Files:**
- Modify: `Assets/Scripts/Interaction/InteractionManager.cs`

- [ ] **Step 1: Update InteractionManager script**

```csharp
// Update InteractionManager.cs
```
*Note: We will use SearchReplace to inject gravity and boundary clamping logic into the Update loop.*

- [ ] **Step 2: Commit the gravity update**

```bash
git add Assets/Scripts/Interaction/InteractionManager.cs
git commit -m "feat: add gravity fall and screen boundary clamping to InteractionManager"
```