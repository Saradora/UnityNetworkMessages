# Unity Network Messages

C# Library that allows sending messages and events inside Unity games that use NetCode for GameObjects without having 
to create prefabs with NetworkObject components.

Library created mostly for personal usage, I'll add a wiki and some examples at some point.

Network Variables **might** come at some point too.

For now mostly tested on LAN.

## Features

- Global events and messages
- NetworkObject events and messages
- **Vanilla-friendly!**

## F.A.Q.

### How is it "Vanilla-friendly"?

Most Network APIs use NetworkObject prefabs to run code, which for games that force said prefabs compatibility 
(e.g. Lethal Company) means that any client who's trying to join a server that doesn't have the same network prefab
list will get rejected.

In this mod's case, you are free to join a non-modded server!

Obviously, any logic that would require the server's intervention will not work, but the server will be joinable.

## Available classes:

### Global event/messages

These classes have a global identifier, meaning that all messages registered with the same identifier across your mod 
will receive the event/data when called.

- ``GlobalNetEvent``
  - Simple event without data.
- ``GlobalNetMessage``
  - Message that can transmit a string.
- ``GlobalNetMessage<T>``
  - Message that can transmit any unmanaged struct that implements ``IEquatable``. 
  - (e.g. int, float, Vector3, Quaternion, bool, etc)
  - Works with ValueTuples. (if unmanaged)
- ``GlobalNetStructMessage<T>``
  - Message that can transmit any unmanaged struct that implements ``INetworkSerializeByMemcpy``

### Object event/messages

These classes are registered with a NetworkObject, meaning that 2 events with the same identifier but a different NetworkObject
will not communicate between them.

It also means that the events are only valid during the lifespan of the NetworkObject.

- ``ObjectNetEvent``
  - Simple event without data.
- ``ObjectNetMessage``
  - Message that can transmit a string.
- ``ObjectNetMessage<T>``
  - Message that can transmit any unmanaged struct that implements ``IEquatable``.
  - (e.g. int, float, Vector3, Quaternion, bool, etc)
  - Works with ValueTuples. (if unmanaged)
- ``ObjectNetStructMessage<T>``
  - Message that can transmit any unmanaged struct that implements ``INetworkSerializeByMemcpy``