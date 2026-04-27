# 3D Desktop Pet Phase 14 Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Upgrade the `OpenAILLMProvider` to support conversation history (memory). A true companion pet needs to remember what was said a few sentences ago to hold a coherent conversation.

**Architecture:** We will maintain a `List<OpenAIMessage>` inside the provider. Whenever the user sends a message or the AI responds, the message gets appended to the list. We will enforce a maximum token/message limit to avoid exceeding API limits and keep costs low.

**Tech Stack:** Unity 3D, C#

---

### Task 1: Implement Conversation Memory in LLM Provider

**Files:**
- Modify: `Assets/Scripts/AI/OpenAILLMProvider.cs`

- [ ] **Step 1: Update OpenAILLMProvider to use persistent message history**

```csharp
// Update OpenAILLMProvider.cs
```
*Note: We will use a SearchReplace operation to refactor the internal state to hold a list of history messages instead of creating a new array each time.*

- [ ] **Step 2: Commit the memory update**

```bash
git add Assets/Scripts/AI/OpenAILLMProvider.cs
git commit -m "feat: add conversation memory (history context) to OpenAILLMProvider"
```