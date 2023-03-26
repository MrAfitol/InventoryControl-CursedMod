# InventoryControl
[![GitHub release](https://flat.badgen.net/github/release/MrAfitol/InventoryControl-CursedMod)](https://github.com/MrAfitol/InventoryControl/releases/)
![GitHub downloads](https://flat.badgen.net/github/assets-dl/MrAfitol/InventoryControl-CursedMod)

A plugin that allows you to configure the default role inventory

## How download ?
   - *1. Find the SCP SL server config folder*
   
   *("C:\Users\\(user name)\AppData\Roaming\SCP Secret Laboratory\" for windows, "/home/(user name)/.config/SCP Secret Laboratory/" for linux)*
  
   - *2. Find the "PluginAPI" folder there, it contains the "plugins" folder.*
  
   - *3. Select the folder where CursedMod is downloaded (global or (server port)), and go to the path CursedMod\Plugins, and move the plugin to this folder*
   
## Config

```yml
# List of roles, their items, and chance (Do not add a role if you want its inventory to be normal)
inventory:
  ClassD:
    keep_items: false
    items:
      KeycardJanitor: 35
      Painkillers: 80
      Coin: 100
  Scientist:
    keep_items: true
    items:
      Flashlight: 100
      Coin: 90
# List of ranks and their roles items and chances
inventory_rank:
  owner:
    ClassD:
      keep_items: false
      items:
        KeycardScientist: 80
        GunCOM18: 60
        Painkillers: 100
        Coin: 100
    Scientist:
      keep_items: true
      items:
        GunCOM18: 85
        SCP500: 70
        Flashlight: 100
        Coin: 90
```

## Wiki
**Be sure to check out the [Wiki](https://github.com/MrAfitol/InventoryControl-CursedMod/wiki)**
