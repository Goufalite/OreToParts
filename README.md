# OreToParts
Mod for the game Kerbal Space Program to create parts from ore.

Mine ore.

Receive parts!

Introducing a minimalistic mod transforming all ore tanks into converters changing ore into vital parts such as repair kits, solar panels,...

![Example of OreToParts](https://imgur.com/I5dEh7a.png)

## Craft

The `oretotanks.cfg` file contains a comma-separated list of parts that can be crafted. The ore price to craft them is the mass of the part (5kg = 5 units of ore). The part will be stored in the ore tank’s new inventory, so keep an eye on it!
The converter can only craft cargo-type parts which have a volume. You can list them in the VAB by pressing the cargo-like button near the action-groups button.

## Duplicate

Place a part to duplicate in the first slot and press Duplicate! Same as above, the ore price is the mass of the part.

## Scrap

Transform the part in the first slot into ore, or throw it through the sas! 

## EVA Refuel

[If you deactivated jetpack and cylinder refueling](https://forum.kerbalspaceprogram.com/index.php?/topic/139980-130-community-database-of-module-manager-patches-for-stock-ksp/&do=findComment&comment=4005087), you can use this refueler to transform Ore or Monoprop (or any custom resource) into EVA fuel. 
Just put the parts you want to refuel in the inventory of the part having the refueler. 

Kerbals present in the part containing the refueler will have their jetpacks refueled too!

## Licence

CC-BY-SA 4.0

Last release : https://github.com/Goufalite/OreToParts/releases/

Github : https://github.com/Goufalite/OreToParts

Issues : https://github.com/Goufalite/OreToParts/issues

Wiki and tips : https://github.com/Goufalite/OreToParts/wiki (add parts, manage custom resources, scrap syntax,...)

## Dependencies
* [Module Manager](https://forum.kerbalspaceprogram.com/index.php?/topic/50533-18x-110x-module-manager-414-july-7th-2020-locked-inside-edition/) (hard dependency) - install separately

## Instructions 

* Remove previous versions. 
* Unzip into the GameData folder of your KSP installation, your folder should look like GameData/OreToParts

Please provide a KSP.log file in order for me to help you.

## Localization 

English, French

## Changelog

### 0.3.1.0
* Kerbals in the part containing the EVA refueler will have their jetpacks refueled too!
* Partial EVA parts refueling

### 0.3.0.0
* Code refactoring
* Better syntax for resources
* Scrap parts into ore, or simply get rid of an inventory item
* Refuel EVA jetpacks or cylinders

### 0.2.0.0
* Added Eva cylinders
* Multiple resources with ratios
* New friendly slider to select a craft part

### 0.1.0.0
* initial release
* ore tanks can convert parts by either crafting them or duplicating them
* ore tanks have different maximum volume
* default parts to craft are repair kits, eva science kits, HG-5 antenna, 1x6 solar panel, eva jetpacks
