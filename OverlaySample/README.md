# OverlaySample — Place symbols by program over synoptics, maps and cameras

**Used for:** showing objects whose position or presence is only known at run time, on every client at once: vehicles
or people tracked on a map, alarms placed on a floor plan, notes pinned by an operator, markers over a camera image...
An **overlay** is a symbol that the server places on a synoptic, a geographical map, a camera or a client window, and
that every connected client displays and updates live.

The sample has two parts:

- **OverlayManager**, a standalone WPF application (this project) that lists the overlays of a server in a grid and
  creates, modifies and deletes them through the SDK;
- a ready-made **AppVision configuration** (`Configuration/`) with two demos written as client and server scripts:
  *Overlays 101* (create, move, delete overlays from synoptic buttons) and *Post-it notes* (notes that operators pin on
  a synoptic, stored as user messages).

![OverlayManager connected to the sample configuration: the two overlays of the Overlays 101 demo](overlay-manager.png)

![The same overlays on the synoptic Overlays 101: symbol A, and symbol B showing its parameter](synoptic.png)

**Audience:** partner developers who know C# and the AppVision SDK (`AppServer`, notifications), and have created
symbols and synoptics in the configurator.

---

## How it works

An overlay is an `OverlayItem` (`appData.dll`) stored by the server and identified by its **name**:

| Property | Meaning |
|---|---|
| `Name` | Unique identifier. Updating an overlay with an existing name modifies it; a new name creates one. (`Id` is an obsolete alias.) |
| `Symbol` | What to display: a symbol file of the configuration (`OverlaySample\Symbol A.xaml`), a type (`file.dll#Namespace.Type`) or a XAML string. |
| `Synoptic` | Where: a synoptic file (`OverlaySample\Synoptic 1.xaml`), `@geo` (every geographical synoptic), a camera (`$V.Node.Camera`), or a client window (`@Window`, `@Window.All`, `@Window.<title or number>`). |
| `Position` | Pixels for a synoptic, a camera or a window; **X = longitude, Y = latitude** for `@geo`. |
| `SymbolParameters` | Values for the `%1`, `%2`... placeholders of the symbol, separated by `;`. |
| `SymbolProperties`, `Size`, `ZIndex`, `Layers`, `Recipient`, `Parameters` | Properties set on the symbol (one `Name=Value` per line), size, Z-order, layers, users who see it (`@Profile.x`, `@User.x`...), free data. |

A symbol displayed as an overlay finds its `OverlayItem` in its `Tag` property (see `Symbol A.xaml`).

The API is the same in an SDK application (`AppServer.ClientManager`), a client script and a server script
(`Server.ClientManager`):

| Operation | Method |
|---|---|
| List | `GetOverlays(params string[] filters)` — filters `Name=`, `Synoptic=`, `Symbol=`, `SymbolParameters=`, `Parameters=`, `Layers=`; `*` allowed in `Name=` and `Synoptic=` |
| Create or modify | `UpdateOverlay(OverlayItem item)`, or `UpdateOverlay(name, symbol, synoptic, position[, symbolParameters, ...])` |
| Delete | `RemoveOverlay(name)` |
| Be notified | `OverlayUpdated` event: `(ModifType, OverlayItem)` |

`OverlayUpdated` receives `ModifType.Update` for a creation **and** a modification, `ModifType.Delete` for a
deletion: there is no `Add`.

| Project file | Role |
|---|---|
| `MainWindow.xaml(.cs)` | Connection, overlay grid, create / update / delete |
| `App.xaml.cs` | Displays unhandled exceptions in a message box |
| `Configuration/` | Ready-made AppVision configuration with the two demos (see below) |

**References:** `appSDK.dll`, `appData.dll`, `appCommon.dll` (AppVision `Bin` folder). **Output:** `OverlayManager.exe`.

---

## OverlayManager, step by step

**Connect and subscribe before listing**, so that no change is missed between the list and the first notification:

```csharp
_server.Open("http://localhost");
_server.Login("ADM", password);
_server.ClientManager.OverlayUpdated += Overlay_Updated;
_server.AddFilterNotifications("NotificationType=Overlay");   // only the overlay notifications
_server.StartNotifications(false);

foreach (var overlay in await _server.ClientManager.GetOverlays())
	AddOrReplace(overlay);
```

**Go back to the UI thread.** In an SDK application, notifications arrive on a background thread, and a collection
bound to a `DataGrid` can only change on the UI thread. The SDK swallows the exceptions of event handlers, so a
missing `Dispatcher` call fails silently:

```csharp
void Overlay_Updated(ModifType @event, OverlayItem item) => Dispatcher.InvokeAsync(() =>
{
	if (@event == ModifType.Delete)
		_overlays.RemoveAll(ov => ov.Name == item.Name);
	else
		AddOrReplace(item);
});
```

**Write through the server.** The grid never changes the local list directly: a committed row is sent with
`UpdateOverlay`, the *Del* key calls `RemoveOverlay` (the `DataGrid.DeleteCommand` is intercepted in
`CommandManager.PreviewExecuted`). The server then notifies every client, this one included, and the notification
updates the grid. The *Name* column can only be typed on a new row: renaming would create a second overlay.

---

## Build and run

1. Build in **Release**. `OverlayManager.exe` needs the AppVision DLLs: run it from the build folder (they are copied
   next to it) or from the AppVision `Bin` folder.
2. Start it, enter the connection string `http://user:password@host` (default `http://ADM:123@localhost`) and click
   **Connect**.
3. Add a row at the bottom of the grid: *Name*, *Symbol* and *Synoptic* propose the symbols and synoptics of the
   configuration (`@geo` for the geographical maps). Type a *Position* (`x,y`, or `longitude,latitude` for `@geo`)
   and press *Enter*: the overlay appears in the clients that display this synoptic.
4. Edit a cell to move or change an overlay; select rows and press *Del* to delete them.

The server must have the overlay option in its licence (synoptic overlays or video overlays); otherwise the overlay
methods do nothing.

---

## The sample configuration

Open it as described in [PlayerSimple](../PlayerSimple/README.md#try-it-with-the-sample-configuration)
(`OverlaySample\Configuration`), generate the client configuration and start a client. The home page has two demos:

**Overlays 101** (`OverlaySample\Synoptic 1.xaml`, client scripts of the buttons)

- *Create overlay A* / *Create overlay B*: `UpdateOverlay("101_A", "OverlaySample\\Symbol A.xaml", "OverlaySample\\Synoptic 1.xaml", position)`
  at a random position; B passes `symbolParameters: "some param"`, displayed by its `%1` placeholder. Click again: the
  same name moves the existing overlay.
- *Remove overlays*: deletes every overlay whose name starts with `101_`. From a client, `RemoveOverlay` takes an
  exact name, so the script lists them first:

  ```csharp
  foreach (var overlay in await Server.ClientManager.GetOverlays("Name=101_*"))
  	Server.ClientManager.RemoveOverlay(overlay.Name);
  ```

  (In a **server** script, `Server.ClientManager.RemoveOverlay("101_*")` accepts the `*` itself.)
- Hover a symbol: its tooltip shows `((OverlayItem)Tag).Name` and `Position`.

**Post-it notes** (`OverlaySample\Post-it Demo.xaml`, symbol `OverlaySample\Post-it.xaml`, server script
`Param\Server\Posit-it.pcs`)

- *Create Post-it note* creates a user message (category *Post-it*) and an overlay named `Event_<message id>` whose
  `SymbolParameters` hold the subject and the text of the note.
- Typing in a note updates the user message; dragging the pin moves the overlay (`UpdateOverlay` with the new
  `Position`); the `x` closes the user message.
- The server script follows the user messages (`OnEventChanged`): it updates the overlay of a modified message, and
  removes the overlay of a closed one, for every client.

Open OverlayManager on the same server at the same time to watch the overlays appear, move and disappear.

---

## Troubleshooting

| Symptom | Cause |
|---|---|
| The grid stays empty, no overlay is displayed | The licence of the server has no overlay option. |
| The grid does not follow the changes made elsewhere | `AddFilterNotifications("NotificationType=Overlay")` / `StartNotifications` missing, or the handler modifies the collection outside the UI thread. |
| An overlay is created but not visible | `Synoptic` is not the exact file name of the displayed synoptic (`folder\file.xaml`), the symbol file does not exist, or `Recipient`/`Layers` hide it. |
| An overlay of `@geo` appears far away | `Position` is `latitude,longitude` instead of **X = longitude, Y = latitude**. |
| `RemoveOverlay("101_*")` from a client removes nothing | Wildcards are only accepted by server scripts: list with `GetOverlays("Name=101_*")` and remove each one. |
| *Login failed* | Wrong user or password in the connection string. |

---

## See also

- [DriverOverlay](../DriverOverlay/README.md) — a driver that moves geo-referenced overlays on a map
- [MapProvider](../MapProvider/README.md) — a custom background for geographical synoptics
- [PlayerMetadata](../PlayerMetadata/README.md) — drawings computed by the client over a camera image
