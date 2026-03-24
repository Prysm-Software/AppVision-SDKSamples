# WpfControlSimple â€” Simple Embeddable WPF Control

## Overview

A minimal WPF `UserControl` designed to be embedded inside an AppVision synoptic.
It demonstrates how a custom control interacts with the AppVision server and
opens a second instance of itself in a floating window.

Features:
- List all variable names from AppVision in a ComboBox.
- Read and display the current value of the selected variable.
- Write a new value back to AppVision.
- Open a second instance via `Sdk.Client.Open()`.

**References:** `appCommon.dll`, `appData.dll`, `appSDK.dll`, `appUI.dll`  
**Output:** `WpfControlSimple.dll` â†’ `AppVision 4\Bin\`

---

## Project Structure

| File | Role |
|---|---|
| `UserControl1.xaml.cs` | Variable read/write, self-open |

---

## Key Concepts

### 1. `Sdk.Server` â€” server interaction

The control must **never** call `Open()` or `Login()` â€” the client host has
already connected `Sdk.Server`.

```csharp
// Read a variable
object val = Sdk.Server.OperandManager.Read("$V.NodeName.VarName");

// Write a variable
Sdk.Server.OperandManager.Write("$V.NodeName.VarName", newValue);
```

`OperandManager.Read/Write` accept the full operand path (`$V.node.var`) and
return / accept `object` values.

### 2. `Sdk.Client` â€” host interaction

Open a new floating window containing this same control:

```csharp
Sdk.Client.Open(
    "WpfControlSimple.dll#WpfControlSimple.UserControl1",
    "Size=320,240"
);
```

Format: `assemblyFileName#fullyQualifiedClassName`.

### 3. Embedding in a synoptic

```
Configurator â€º Synoptics â€º [synoptic] â€º Add object
    Type:      Third-party .Net object
    Assembly:  WpfControlSimple.dll
    Class:     WpfControlSimple.UserControl1
```

### 4. Design-time guard

Always check `Sdk.Server.IsConnected` before querying the server in the
constructor â€” the XAML designer does not have an active connection:

```csharp
if (!Sdk.Server.IsConnected) return;
```

---

## AppVision SDK Reference

### `Sdk.Server` (`appSDK.dll`)

| Member | Description |
|---|---|
| `.IsConnected` | True if the client host is connected |
| `.VariableManager.GetStates()` | Snapshot of all variable values |
| `.OperandManager.Read(path)` | Read any operand value |
| `.OperandManager.Write(path, v)` | Write any operand value |

### `Sdk.Client` (`appSDK.dll`)

| Member | Description |
|---|---|
| `.Open(classRef, parameters)` | Open a control in a new host window |

---

## How to Build and Deploy

1. Build in **Release** configuration.  
2. Copy `WpfControlSimple.dll` â†’ `AppVision 4\Bin\`.  
3. Restart the AppVision client (or reload the synoptic).

---

## Debugging

Attach to the AppVision client process (**Debug â€º Attach to Process**) and set
breakpoints inside the control.  
In the XAML designer the control will appear empty â€” this is expected.

---

## Learning Path

```
WpfControlSimple  â†’  WpfControlAdvanced
Read/Write             IPersistable, ICommandable, IContainer, async loading
```
