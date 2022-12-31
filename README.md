# Jackz Builder for FiveM

This is a port of the Stand mod menu lua script **jackz_vehicle_builder** to fivem.
This is still under development, and may be very buggy.

### Current Features
 * [x] Attaching props, vehicles, entities to a base entity
 * [x] Saving and spawning
 * [x] Saving vehicle data (colors, mods, etc)
 * [x] Editing a saved build
 * [x] Allow changing blip
 * [x] Support spawning build into a fixed world position

### Pending Features
Checkmark indicates that the feature is in progress

 * [ ] Downloading builds from cloud
 * [ ] Support particle fx
 * [x] Allow parenting attachments to other attachments
 * [ ] Test for compatibility with existing stand version builds
 * [ ] Support ped animations
 * [ ] Local server-based cloud (not global cloud)
 * [x] Overlays (build info, attachment info, 'add to build')
 * [ ] Free Edit

# Editing & Building

To edit it, open `jackz_builder.sln` in Visual Studio.

To build it, run `build.cmd`. To run it, run the following commands to make a symbolic link in your server data directory:

```dos
cd /d [PATH TO THIS RESOURCE]
mklink /d X:\cfx-server-data\resources\[local]\jackz_builder dist
```

Afterwards, you can use `ensure jackz_builder` in your server.cfg or server console to start the resource.