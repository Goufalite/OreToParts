# OreToParts
Mod for the game Kerbal Space Program to create parts from ore.

Mine ore.

Receive parts!

Introducing a minimalistic mod transforming all ore tanks into converters changing ore into vital parts such as repair kits, solar panels,...

## Craft

The oretotanks.cfg file contains a comma-separated list of parts that can be crafted. The ore price to craft them is the mass of the part (5kg = 5 units of ore). The part will be stored in the ore tank’s new inventory, so keep an eye on it!
The converter can only craft cargo-type parts which have a volume. You can list them in the VAB by pressing the cargo-like button near the action-groups button.

## Duplicate

Place a part to duplicate in the first slot and press Duplicate! Same as above, the ore price is the mass of the part.

## Scrap

Transform the part in the first slot into ore, or throw it to oblivion! Just change the `scrapMode` setting in the cfg file to change the scraping strategy : 

* **always** : destroyes the part and tries to fill all needed ore at maximum cost
* **partial** : tries to fill all needed ore but blocks if a ore container will overflow
* **block** : checks if the recycled ore can be stored

This is an autonomous part module, so you can put it anywhere with an `always` mode to trash everything you don't need, like a "throw to sas" feature!

## Adding my mod’s parts/custom parts.

As far as I tested this, the mod manages unknown parts to craft by putting ??partName?? in the part description. If you want to add your own parts to craft, replace or concatenate the partList attribute with a MM patch. Parts must be comma separated without spaces.

    @PART[*]:HAS[@MODULE[OreToParts]]:AFTER[OreToParts]
    {
    	@MODULE[OreToParts] {
		    @partList = #$partList$,mycommaseparatedparts
    	}
    }

## Managing your own resources (CRP, Simplex Resources,...)

Apply patches to add resources to your containers, add the OreToParts module to the container, and specify the receipe:

    @PART[*]:HAS[@MODULE[OreToParts]]:NEEDS[CommunityResourcePack]:AFTER[OreToParts]
    {
    	@MODULE[OreToParts] {
		    @resources = Ore|0.5,Hydrogen|1
	    }
    }

## Licence

CC-BY-SA 4.0

Last release : https://github.com/Goufalite/OreToParts/releases/

Github : https://github.com/Goufalite/OreToParts

Issues : https://github.com/Goufalite/OreToParts/issues

## Dependencies
* [Module Manager](https://forum.kerbalspaceprogram.com/index.php?/topic/50533-18x-110x-module-manager-414-july-7th-2020-locked-inside-edition/) (hard dependency) - install separately

## Instructions 

* Remove previous versions. 
* Unzip into the GameData folder of your KSP installation, your folder should look like GameData/OreToParts

Please provide a KSP.log file in order for me to help you.


## Localization 

English, French

## Changelog

### 0.3.0.0
* (in progress)
* Scrap part module to transform parts into ore, or simply get rid of an inventory item

### 0.2.0.0
* Added Eva cylinders
* Multiple resources with ratios
* New friendly slider to select a craft part

### 0.1.0.0
* initial release
* ore tanks can convert parts by either crafting them or duplicating them
* ore tanks have different maximum volume
* default parts to craft are repair kits, eva science kits, HG-5 antenna, 1x6 solar panel, eva jetpacks
